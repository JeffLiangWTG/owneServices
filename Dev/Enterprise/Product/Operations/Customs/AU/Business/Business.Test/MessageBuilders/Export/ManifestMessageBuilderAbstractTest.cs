using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class ManifestMessageBuilderAbstractTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertNotNull("ManifestMessageBuilder", NewMessageBuilder(Common.MessageBuilders.MessageSubTypes.Create, CreateTestAirFreightConsol()));
		}

		public void TestCreateMessage()
		{
			consol = CreateTestSeaFCLFreightConsol();
			var builder = NewMessageBuilder(Common.MessageBuilders.MessageSubTypes.Create, consol);
			var messageBuilderResult = builder.PopulateMessages();

			foreach (var builderResult in messageBuilderResult.GetBuilderResults())
			{
				AssertNotNull("Failed to create a message", builderResult.Message.EM_MessageText);
			}
		}

		protected abstract IManifestMessageBuilder NewMessageBuilder(Common.MessageBuilders.MessageSubTypes messageSubType, object data);

		protected ForwardingConsol consol;

		protected ForwardingConsol CreateTestAirFreightConsol()
		{
			var result = Factory.New<ForwardingConsol>();
			result.JK_TransportMode = Core.Constants.TransportModes.Air;

			var transport = result.Transports[0];
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_RL_NKLoadPort = "AUSYD";
			result.JK_UniqueConsignRef = "EJF000121";
			EnsureQantasAirLineCodeSet();
			transport.JW_VoyageFlight = "QF112";
			result.JK_MasterBillNum = "081-00000011";

			transport.JW_ATD = new ZDateTime(new DateTime(2003, 11, 1, 7, 23, 0));

			return result;
		}

		protected ForwardingConsol CreateTestSeaLCLFreightConsol()
		{
			var result = Factory.New<ForwardingConsol>();
			result.JK_TransportMode = Core.Constants.TransportModes.Sea;
			result.JK_TransportMode = Core.Constants.ContainerModes.LCL;

			return result;
		}

		protected ForwardingConsol CreateTestSeaFCLFreightConsol()
		{
			var result = Factory.New<ForwardingConsol>();
			result.JK_TransportMode = Core.Constants.TransportModes.Sea;
			result.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			result.JK_UniqueConsignRef = "EJF001111";
			result.JK_MasterBillNum = "180-14653505";

			var transport = result.Transports[0];
			transport.JW_VoyageFlight = "EF330";
			var testVessel = Factory.New<RefVessel>();
			testVessel.RV_Code = "SS TITANIC";
			var titanicOwner = Factory.New<OrgHeader>();
			titanicOwner.MainAddress.OA_Address1 = "ICEBERG CENTRAL";
			titanicOwner.OH_Code = "TITANIC";
			titanicOwner.OH_FullName = "AIN T GOING DOWN";
			testVessel.RV_OH = titanicOwner.PK;
			transport.JW_Vessel = "SS TITANIC";
			AssertEquals("TestVessel Code", "SS TITANIC", result.Vessel.RV_Code);

			transport.JW_RL_NKDiscPort = "USLAX";
			result.JK_RL_NKPortOfFirstArrival = "PHJOL";
			transport.JW_RL_NKLoadPort = "AUMEL";

			transport.JW_ATD = new ZDateTime(2003, 11, 14);

			return result;
		}

		protected ForwardingConsol CreateTestCoLoadFreightConsol()
		{
			var result = CreateTestSeaFCLFreightConsol();
			result.JK_RL_NKPortOfFirstArrival = "USLAX";

			var transport = result.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_ATD = new ZDateTime(new DateTime(2003, 12, 01, 23, 59, 00));
			//Result.JK_TotalDocumentedWeight = 84.1m;

			var newContainer = Factory.New<CommonContainer>();
			newContainer.JC_ContainerNum = "1273819201O";
			newContainer.JC_SealNum = "876";
			result.Containers.Add(newContainer);

			var coLoadMaster = CommonShipment.New(Factory);
			var coLoadSub1 = CommonShipment.New(Factory);
			var coLoadSub2 = CommonShipment.New(Factory);
			coLoadSub1.JS_JS_ColoadMasterShipment = coLoadMaster.PK;
			coLoadSub2.JS_JS_ColoadMasterShipment = coLoadMaster.PK;
			result.Shipments.Add(coLoadMaster);

			coLoadSub1.JS_HouseBill = "SUB-2";
			coLoadSub1.JS_OuterPacks = 1;
			coLoadSub1.JS_ActualWeight = 42.1m;
			coLoadSub1.JS_UnitOfWeight = "KG";
			coLoadSub1.JS_GoodsDescription = "Sub Electronics";
			coLoadSub1.JS_RL_NKDestination = "THBKK";
			AssertNotNull("Failed to set destination", coLoadSub1.Destination);
			CreateECNForShipment(coLoadSub1, DecNumber2);
			result.Shipments.Add(coLoadSub1);

			coLoadSub2.JS_HouseBill = "SUB-3";
			coLoadSub2.JS_OuterPacks = 1;
			coLoadSub2.JS_ActualWeight = 42.0m;
			coLoadSub2.JS_UnitOfWeight = "KG";
			coLoadSub2.JS_GoodsDescription = "More Electronics Co-Loaded";
			coLoadSub2.JS_RL_NKDestination = "THBKK";
			CreateECNForShipment(coLoadSub2, DecNumber3);
			result.Shipments.Add(coLoadSub2);

			coLoadMaster.JS_ActualWeight = 84.1m;
			coLoadMaster.JS_UnitOfWeight = "KG";

			Factory.Save();
			coLoadSub2.Consols.Load();
			coLoadSub1.Consols.Load();
			return result;
		}

		protected void AddShipmentsToConsol(ForwardingConsol consol)
		{
			AddShipmentToConsol(consol, Housebill1, 5.6m);
		}

		protected CommonShipment shipmentAddedToConsol;
		protected void AddShipmentToConsol(ForwardingConsol consol, ZString housebill, ZDecimal weight)
		{
			shipmentAddedToConsol = CommonShipment.New(Factory);
			shipmentAddedToConsol.JS_HouseBill = housebill;
			shipmentAddedToConsol.JS_ActualWeight = weight;
			shipmentAddedToConsol.Consols.Add(consol);

			CreateECNForShipment(shipmentAddedToConsol, DecNumber1);
		}

		protected CusEntryNumber shipmentECN;
		protected void CreateECNForShipment(CommonShipment houseBill, string decNumber)
		{
			// Create Customs Permit
			shipmentECN = houseBill.CusEntryNumbers.AddNew();
			shipmentECN.CE_ParentID = houseBill.PK;
			shipmentECN.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			shipmentECN.CE_EntryType = CusEntryNumberTypes.Australia.ECN;
			shipmentECN.CE_EntryNum = decNumber;
		}

		protected ZString GetElementFromMessage(ZString[] messages, int message, int segment, int element)
		{
			var segments = messages[message].Split('\'');
			var elements = segments[segment].Split('+');
			return elements[element];
		}

		protected void EnsureQantasAirLineCodeSet()
		{
			var factory = new BusinessObjectFactory();
			var qantas = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "QANAIR");

			var qantasAirline = factory.LoadFromNaturalKey<RefAirline>(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, "081");
			if (qantasAirline == null)
			{
				qantasAirline = factory.NewWithValidTestData<RefAirline>();
				qantasAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "081";
				qantasAirline.RM_TwoCharacterCode = "QF";
			}

			qantas.MiscServ.OM_RM_Airline = qantasAirline.PK;
			AssertEquals("Qantas airline code", "QF", qantas.MiscServ.Airline.RM_TwoCharacterCode);
			qantas.Factory.Save();
		}

		protected ZString GetValueFromMessage(ZString[] messages, int message, int segment, int element, int value)
		{
			var values = GetElementFromMessage(messages, message, segment, element);
			return values.Split(':')[value];
		}

		const string Housebill1 = "W2002290013";
		const string DecNumber1 = "2M033281796NBC";
		const string DecNumber2 = "2M1121111111BB";
		const string DecNumber3 = "2M9999999999KK";
	}
}
