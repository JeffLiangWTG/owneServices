using System.IO;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AirCargoValueObjectDataAdapter))]
	sealed class AirCargoValueObjectDataAdapterTest : BaseCargoXmlDataAdapterTest<CusMAWB, Xsd.Consol>
	{
		public void TestImportFromXml()
		{
			InitialiseForImportTest();
			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			adapter.ImportFromValueObject(cusMAWB, consols.Consol[0], ImportContext);
			AssertMAWBDetails(cusMAWB);
			AssertEquals(1, cusMAWB.ChildBills.Count);
			AssertHAWBDetails(cusMAWB.ChildBills[0]);
		}

		public void TestImportMAWB()
		{
			InitialiseForImportTest();
			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			adapter.ImportMAWB(cusMAWB, consols.Consol[0], ImportContext);
			AssertMAWBDetails(cusMAWB);
		}

		public void TestImportHAWB()
		{
			InitialiseForImportTest();
			CusHAWB cusHAWB = Factory.New<CusHAWB>();
			adapter.ImportHAWB(cusHAWB, consols.Consol[0].Shipments[0], ImportContext);
			AssertHAWBDetails(cusHAWB);
		}

		[TestDate(2006, 12, 12)]//CMR Date
		public void TestImportHAWBFreightCollect()
		{
			InitialiseForImportTest();
			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			consols.Consol[0].Shipments[0].Declaration.PaymentTerms = "COL";
			CusHAWB cusHAWB = Factory.New<CusHAWB>();
			cusHAWB.CS_CM = cusMAWB.PK;
			adapter.ImportHAWB(cusHAWB, consols.Consol[0].Shipments[0], ImportContext);
			AssertEquals(CMRMethodsOfPayment.Codes.Collect, cusHAWB.CS_FreightPrepaidCollect);
		}

		protected override ValueObjectDataAdapter<CusMAWB, Xsd.Consol> GetNewBizObjXmlDataAdapter() => new AirCargoValueObjectDataAdapter();

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
			=> new BusinessObjectAndExpectedOutputFileName(Factory.New<CusMAWB>(), GetEmbeddedResourcePath("AirCargoExample.xml"), ValidationKind.None, "Empty MasterBill");

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
			=> new BusinessObjectAndExpectedOutputFileName(PopulatedCusMAWB, embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("AUPopulatedAirCargo.xml")), ValidationKind.Xsd | ValidationKind.FactorySave, "Populated AirCargo");

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => System.Array.Empty<BusinessObjectAndExpectedOutputFileName>();

		void AssertMAWBDetails(CusMAWB cusMAWB)
		{
			AssertEquals(new ZDateTime(2005, 10, 10), cusMAWB.CM_ArrivalDate);
			AssertEquals("QF123", cusMAWB.CM_FlightNo);
			AssertEquals("08133333333", cusMAWB.CM_MAWB);
			AssertEquals("AUSYD", cusMAWB.CM_RL_NKDischargePort);
			AssertEquals("AUBNE", cusMAWB.CM_RL_NKFirstArrivalPort);
			AssertEquals("SGSIN", cusMAWB.CM_RL_NKLoadPort);

			StmNoteCollection notes = (StmNoteCollection)cusMAWB.Notes.GetAllNotes();
			AssertEquals(1, notes.Count);
			StmNote note = notes[0];
			AssertEquals(true, note.ST_IsCustomDescription);
			AssertEquals("DuplicateHAWBs", note.ST_Description);
			AssertEquals("Their are duplicate Hawbs", note.ST_NoteDataAsText);
		}

		void AssertHAWBDetails(CusHAWB cusHAWB)
		{
			AssertEquals("HouseBill", cusHAWB.CS_HAWB);
			AssertEquals("SGSIN", cusHAWB.CS_RL_NKOrigin);
			AssertEquals("AUSYD", cusHAWB.CS_RL_NKDestination);
			AssertEquals("08144444444", cusHAWB.CS_MasterHouseBill);
			AssertEquals(10m, cusHAWB.CS_Weight);
			AssertEquals("KG", cusHAWB.CS_WeightUQ);
			AssertEquals(15m, cusHAWB.CS_ChargableWeight);
			AssertEquals(1230.12m, cusHAWB.CS_GoodsValue);
			AssertEquals("ZAR", cusHAWB.CS_RX_NKGoodsCurrency);
			AssertEquals("GoodsDescription", cusHAWB.CS_GoodsDescription);
			AssertEquals(false, cusHAWB.CS_IsSurplus);
			AssertEquals(CMRMethodsOfPayment.Codes.PrepaidOnly, cusHAWB.CS_FreightPrepaidCollect);
			AssertEquals("DOC", cusHAWB.CS_ShipmentType);
			AssertEquals((short)10, (short)cusHAWB.CS_PiecesManifested);
			AssertEquals("STD", cusHAWB.CS_RS_NK_ServiceLevel);

			AssertEquals("Sydney", cusHAWB.CS_ConsigneeCity);
			AssertEquals("ConsigneeContactName", cusHAWB.CS_ConsigneeContactName);
			AssertEquals("ConsigneeName", cusHAWB.CS_ConsigneeName);
			AssertEquals("9637660", cusHAWB.CS_ConsigneePhone);
			AssertEquals("7551", cusHAWB.CS_ConsigneePostcode);
			AssertEquals("NSW", cusHAWB.CS_ConsigneeState);
			AssertEquals("Street", cusHAWB.CS_ConsigneeStreet);
			AssertEquals("Street2", cusHAWB.CS_ConsigneeStreet2);
			AssertEquals("AU", cusHAWB.CS_RN_NKConsigneeCountry);

			AssertEquals("Singapore", cusHAWB.CS_ConsignorCity);
			AssertEquals("ConsignorContactName", cusHAWB.CS_ConsignorContactName);
			AssertEquals("ConsignorName", cusHAWB.CS_ConsignorName);
			AssertEquals("963766", cusHAWB.CS_ConsignorPhone);
			AssertEquals("7550", cusHAWB.CS_ConsignorPostcode);
			AssertEquals("Jurong", cusHAWB.CS_ConsignorState);
			AssertEquals("Pasir", cusHAWB.CS_ConsignorStreet);
			AssertEquals("Gudang", cusHAWB.CS_ConsignorStreet2);
			AssertEquals("SG", cusHAWB.CS_RN_NKConsignorCountry);

			StmNoteCollection notes = (StmNoteCollection)cusHAWB.Notes.GetAllNotes();
			AssertEquals(1, notes.Count);
			StmNote note = notes[0];
			AssertEquals(true, note.ST_IsCustomDescription);
			AssertEquals("MasterBillNumberWarning", note.ST_Description);
			AssertEquals("This Masterbill number is not valid for whatever reason", note.ST_NoteDataAsText);

			ZQuery importerFilter = new ZQuery(OrgPatternMatchAddressSchema.P3_ParentID, cusHAWB.PK);
			importerFilter.AddToFilter(OrgPatternMatchAddressSchema.P3_ParentTableCode, ((ZString)CusHAWBSchema.PK.Name).Left(2));
			importerFilter.AddToFilter(OrgPatternMatchAddressSchema.P3_AddressType, OrgPatternMatchAddress.Constants.AddressType.AirCargoImporter);
			var importer = Factory.LoadTop1<OrgPatternMatchAddress>(importerFilter);

			AssertEquals("ImporterName", importer.P3_CompanyName);
			AssertEquals("ImporterAddress", importer.P3_Address1);
			AssertEquals("ImporterAddress2", importer.P3_Address2);
			AssertEquals("Chatswood", importer.P3_City);
			AssertEquals("NSW", importer.P3_State);
			AssertEquals("7552", importer.P3_PostCode);
			AssertEquals("9763766", importer.P3_Phone);
			AssertEquals("ImporterContactName", importer.P3_ContactName);
		}

		void InitialiseForImportTest()
		{
			consols = PopulatedConsols1;
			adapter = new AirCargoValueObjectDataAdapter();
		}
		Xsd.Consols consols;
		AirCargoValueObjectDataAdapter adapter;

		Xsd.Consols PopulatedConsols1
		{
			get
			{
				Xsd.Consols result;

				using (Stream stream = new FileStream(embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("AUPopulatedAirCargo.xml")), FileMode.Open, FileAccess.Read))
				{
					XmlTextReader xmlTextReader = new XmlTextReader(stream);
					XmlValueObjectSerializer xmlValueObjectSerializer = new XmlValueObjectSerializer(typeof(Xsd.Consols));
					result = (Xsd.Consols)xmlValueObjectSerializer.Deserialize(xmlTextReader);
				}

				return result;
			}
		}

		ValueObjectImportContext ImportContext
		{
			get
			{
				if (fImportContext == null)
				{
					fImportContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
				}
				return fImportContext;
			}
		}
		ValueObjectImportContext fImportContext;

		CusMAWB PopulatedCusMAWB
		{
			get
			{
				if (fCusMAWB == null)
				{
					fCusMAWB = Factory.New<CusMAWB>();
					fCusMAWB.CM_ArrivalDate = new ZDateTime(2005, 10, 10);
					fCusMAWB.CM_FlightNo = "QF123";
					fCusMAWB.CM_MAWB = "08133333333";
					fCusMAWB.CM_RL_NKDischargePort = "AUSYD";
					fCusMAWB.CM_RL_NKFirstArrivalPort = "AUBNE";
					fCusMAWB.CM_RL_NKLoadPort = "SGSIN";

					CusHAWB cusHAWB = fCusMAWB.ChildBills.AddNew();

					cusHAWB.CS_HAWB = "HouseBill";
					cusHAWB.CS_RL_NKOrigin = "SGSIN";
					cusHAWB.CS_RL_NKDestination = "AUSYD";
					cusHAWB.CS_MasterHouseBill = "08144444444";
					cusHAWB.CS_Weight = 10m;
					cusHAWB.CS_WeightUQ = "KG";
					cusHAWB.CS_ChargableWeight = 15m;
					cusHAWB.CS_GoodsValue = 1230.12m;
					cusHAWB.CS_RX_NKGoodsCurrency = "ZAR";
					cusHAWB.CS_GoodsDescription = "GoodsDescription";
					cusHAWB.CS_IsSurplus = false;
					cusHAWB.CS_FreightPrepaidCollect = "PPD";
					cusHAWB.CS_ShipmentType = "DOC";
					cusHAWB.CS_PiecesManifested = 10;
					cusHAWB.CS_RS_NK_ServiceLevel = "STD";

					cusHAWB.CS_ConsigneeCity = "Sydney";
					cusHAWB.CS_ConsigneeContactName = "ConsigneeContactName";
					cusHAWB.CS_ConsigneeName = "ConsigneeName";
					cusHAWB.CS_ConsigneePhone = "9637660";
					cusHAWB.CS_ConsigneePostcode = "7551";
					cusHAWB.CS_ConsigneeState = "NSW";
					cusHAWB.CS_ConsigneeStreet = "Street";
					cusHAWB.CS_ConsigneeStreet2 = "Street2";
					cusHAWB.CS_RN_NKConsigneeCountry = "AU";

					cusHAWB.CS_ConsignorCity = "Singapore";
					cusHAWB.CS_ConsignorContactName = "ConsignorContactName";
					cusHAWB.CS_ConsignorName = "ConsignorName";
					cusHAWB.CS_ConsignorPhone = "963766";
					cusHAWB.CS_ConsignorPostcode = "7550";
					cusHAWB.CS_ConsignorState = "Jurong";
					cusHAWB.CS_ConsignorStreet = "Pasir";
					cusHAWB.CS_ConsignorStreet2 = "Gudang";
					cusHAWB.CS_RN_NKConsignorCountry = "SG";
				}
				return fCusMAWB;
			}
		}

		CusMAWB fCusMAWB;
	}
}
