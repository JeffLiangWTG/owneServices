using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentCustomsInformation))]
	class ForwardingShipmentCustomsInformationTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestCustomsStatus()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var mawb = (BusinessObject)Factory.New<AU.ICusMAWB>();
			var hawb = (BusinessObject)Factory.New<AU.ICusHAWB>();
			hawb[CusHAWBSchema.CS_CM] = mawb.PK;
			hawb[CusHAWBSchema.CS_MsgStatus] = "WTO";
			hawb[CusHAWBSchema.CS_CustomsStatus] = "HLD";
			hawb[CusHAWBSchema.CS_JS] = shipment.PK;
			Factory.Save();

			var customsInformation = new ForwardingShipmentCustomsInformation(shipment);

			NUnit.Framework.Assert.That(customsInformation.CustomsMessageStatus, Is.EqualTo("WTO").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.CustomsCargoStatus, Is.EqualTo("HLD").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustomsCargoStatusDefaults()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var customsInformation = new ForwardingShipmentCustomsInformation(shipment);

			NUnit.Framework.Assert.That(customsInformation.CustomsCargoStatus.IsEmpty, Is.True, "Should default to empty");
		}

		[ExpectNoExceptions]
		public void TestCustomsMessageStatusDefaults()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var customsInformation = new ForwardingShipmentCustomsInformation(shipment);

			NUnit.Framework.Assert.That(customsInformation.CustomsMessageStatus.IsEmpty, Is.True, "Should default to empty");
		}

		[ExpectNoExceptions]
		public void TestCargoStatusDefaults()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var customsInformation = new ForwardingShipmentCustomsInformation(shipment);

			NUnit.Framework.Assert.That(customsInformation.CRLStatus.IsEmpty, Is.True, "Should default to empty");
			NUnit.Framework.Assert.That(customsInformation.SEBillStatus.IsEmpty, Is.True, "Should default to empty");
			NUnit.Framework.Assert.That(customsInformation.HLDOrEXMStatus.IsEmpty, Is.True, "Should default to empty");
		}

		[ExpectNoExceptions]
		public void TestENSStatusDefaults()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var customsInformation = new ForwardingShipmentCustomsInformation(shipment);

			NUnit.Framework.Assert.That(customsInformation.ENSStatus.IsEmpty, Is.True, "Should default to empty");
		}

		[ExpectNoExceptions]
		public void TestEXPStatusDefaults()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var customsInformation = new ForwardingShipmentCustomsInformation(shipment);

			NUnit.Framework.Assert.That(customsInformation.EXPStatus.IsEmpty, Is.True, "Should default to empty");
		}

		[ExpectNoExceptions]
		public void TestITStatusDefaults()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var customsInformation = new ForwardingShipmentCustomsInformation(shipment);

			NUnit.Framework.Assert.That(customsInformation.ITStatus.IsEmpty, Is.True, "Should default to empty");
		}

		[ExpectNoExceptions]
		public void TestACIStatusDefaults()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var customsInformation = new ForwardingShipmentCustomsInformation(shipment);

			NUnit.Framework.Assert.That(customsInformation.ACICargoStatus.IsEmpty, Is.True, "Should default to empty");
			NUnit.Framework.Assert.That(customsInformation.ACIMessageStatus.IsEmpty, Is.True, "Should default to empty");
		}

		[ExpectNoExceptions]
		public void TestEntryStatusDescription()
		{
			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var shipment = Factory.New<ForwardingShipment>();
			var customsInformation = new ForwardingShipmentCustomsInformation(shipment);

			NUnit.Framework.Assert.That(customsInformation.EntryStatusDescription, Is.EqualTo(ZString.Empty), "No Attached Declaration");

			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_JS = shipment.PK;

			Factory.Save();

			customsInformation = new ForwardingShipmentCustomsInformation(shipment);
			NUnit.Framework.Assert.That(customsInformation.EntryStatusDescription, Is.EqualTo("Not Sent").Using(CustomComparers.TypeComparison), "One Attached Declaration, not sent");

			declaration.JE_EntryStatus = "CLR";
			customsInformation = new ForwardingShipmentCustomsInformation(shipment);
			NUnit.Framework.Assert.That(customsInformation.EntryStatusDescription, Is.EqualTo("Declaration Clear (Replacement)").Using(CustomComparers.TypeComparison), "One Attached Declaration, clear amendment");
		}

		[ExpectNoExceptions]
		public void TestGetAFRBillStatusForShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var customsInformation = new ForwardingShipmentCustomsInformation(shipment);

			shipment.JS_HouseBill = "HB1";
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "There is no AFR done for this shipment");
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "There is no AFR done for this shipment");

			var header = Factory.New<JP.AFR.IJPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill1 = Factory.New<JP.AFR.IJPAFRBills>();
			bill1.JPB_JPH_Header = header.PK;
			bill1.JPB_BillNumber = "XXX";
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison));

			bill1.JPB_BillNumber = "HB1";
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison));

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "JPTKY";
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo(AFRBillCustomsStatusList.Codes.NotRegistered).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo(AFRBillCustomsStatusList.Descriptions.NotRegistered).Using(CustomComparers.TypeComparison));

			bill1.JPB_BillNumber = "xxHB1";
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison));

			bill1.JPB_BillNumber = "xxxxHB1";
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo(AFRBillCustomsStatusList.Codes.NotRegistered).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo(AFRBillCustomsStatusList.Descriptions.NotRegistered).Using(CustomComparers.TypeComparison));

			bill1.JPB_BillNumber = "xxxxHB1";
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo(AFRBillCustomsStatusList.Codes.Registered).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo(AFRBillCustomsStatusList.Descriptions.Registered).Using(CustomComparers.TypeComparison));

			var bill2 = Factory.New<JP.AFR.IJPAFRBills>();
			bill2.JPB_JPH_Header = header.PK;
			bill2.JPB_BillNumber = "HB1";
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo("Multiple").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo("There are multiple bills with different statuses").Using(CustomComparers.TypeComparison));

			bill2.JPB_BillNumber = "xxHB1";
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo(AFRBillCustomsStatusList.Codes.Registered).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo(AFRBillCustomsStatusList.Descriptions.Registered).Using(CustomComparers.TypeComparison));

			bill2.JPB_BillNumber = "ttttHB1";
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo("Multiple").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo("There are multiple bills with different statuses").Using(CustomComparers.TypeComparison));

			bill2.JPB_BillNumber = "ttttHB1";
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo(AFRBillCustomsStatusList.Codes.Registered).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo(AFRBillCustomsStatusList.Descriptions.Registered).Using(CustomComparers.TypeComparison));

			bill1.JPB_ReleaseStatus = string.Empty;
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo(AFRBillCustomsStatusList.Codes.NotRegistered).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo(AFRBillCustomsStatusList.Descriptions.NotRegistered).Using(CustomComparers.TypeComparison));

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_RL_NKDischargePort = "JPTKY";
			consol2.Shipments.Add(shipment);
			var header2 = Factory.New<JP.AFR.IJPAFRHeader>();
			header2.JPH_ParentId = consol2.PK;
			header2.JPH_ParentTableCode = consol.TablePrefix;
			var bill3 = Factory.New<JP.AFR.IJPAFRBills>();
			bill3.JPB_JPH_Header = header2.PK;
			bill3.JPB_BillNumber = "HB1";
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo(AFRBillCustomsStatusList.Codes.NotRegistered).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo(AFRBillCustomsStatusList.Descriptions.NotRegistered).Using(CustomComparers.TypeComparison));

			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo(AFRBillCustomsStatusList.Codes.NotRegistered).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo(AFRBillCustomsStatusList.Descriptions.NotRegistered).Using(CustomComparers.TypeComparison));

			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo("Multiple").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo("There are multiple bills with different statuses").Using(CustomComparers.TypeComparison));

			bill3.JPB_BillNumber = "xxxxHB1";
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo("Multiple").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo("There are multiple bills with different statuses").Using(CustomComparers.TypeComparison));

			bill3.JPB_BillNumber = "xxxHB1";
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo(AFRBillCustomsStatusList.Codes.NotRegistered).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo(AFRBillCustomsStatusList.Descriptions.NotRegistered).Using(CustomComparers.TypeComparison));

			var header3 = Factory.New<JP.AFR.IJPAFRHeader>();
			var bill4 = Factory.New<JP.AFR.IJPAFRBills>();
			bill4.JPB_JPH_Header = header3.PK;
			bill4.JPB_BillNumber = "HB1";
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo(AFRBillCustomsStatusList.Codes.NotRegistered).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo(AFRBillCustomsStatusList.Descriptions.NotRegistered).Using(CustomComparers.TypeComparison));

			bill4.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo(AFRBillCustomsStatusList.Codes.NotRegistered).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo(AFRBillCustomsStatusList.Descriptions.NotRegistered).Using(CustomComparers.TypeComparison));

			bill4.JPB_BillNumber = "xxxHB1";
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatus, Is.EqualTo(AFRBillCustomsStatusList.Codes.NotRegistered).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.AFRBillStatusDescription, Is.EqualTo(AFRBillCustomsStatusList.Descriptions.NotRegistered).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		[TestDate(2050, 01, 01)]
		public void TestDestinationGoodsValueAndExchangeRate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(Constants.CurrencyCodes.UnitedStates))
			{
				var exchangeRate_06 = Factory.New<RefExchangeRate>();
				exchangeRate_06.RE_RX_NKExCurrency = Constants.CurrencyCodes.Canada;
				exchangeRate_06.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRate_06.RE_StartDate = new ZDateTime(2024, 06, 01);
				exchangeRate_06.RE_ExpiryDate = new ZDateTime(2024, 06, 30);
				exchangeRate_06.RE_ExRateType = Constants.ExchangeRateTypes.Code.CustomsRate;
				exchangeRate_06.RE_SellRate = 0.736431m;

				var exchangeRate_07 = Factory.New<RefExchangeRate>();
				exchangeRate_07.RE_RX_NKExCurrency = Constants.CurrencyCodes.Canada;
				exchangeRate_07.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRate_07.RE_StartDate = new ZDateTime(2024, 07, 01);
				exchangeRate_07.RE_ExpiryDate = new ZDateTime(2024, 07, 31);
				exchangeRate_07.RE_ExRateType = Constants.ExchangeRateTypes.Code.CustomsRate;
				exchangeRate_07.RE_SellRate = 0.727696m;
				Factory.Save();

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_GoodsValue = 1000m;
				shipment.JS_RX_NKGoodsValueCurr = Constants.CurrencyCodes.UnitedStates;

				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "USPHL";

				var customsInformation = new ForwardingShipmentCustomsInformation(shipment);
				NUnit.Framework.Assert.That(customsInformation.DestinationGoodsValue, Is.EqualTo(1000m).Using(CustomComparers.TypeComparison), "Destination Goods Value In USD");
				NUnit.Framework.Assert.That(customsInformation.DestinationCurrencyCode, Is.EqualTo(Constants.CurrencyCodes.UnitedStates).Using(CustomComparers.TypeComparison), "Destination Goods Currency In USD");
				NUnit.Framework.Assert.That(customsInformation.DestinationExchangeRate, Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "Destination Exchange Rate In USD");

				shipment.JS_RL_NKDestination = "CAVAN";
				NUnit.Framework.Assert.That(customsInformation.DestinationGoodsValue, Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "Destination Goods Value In CAD, no exchange rate found");
				NUnit.Framework.Assert.That(customsInformation.DestinationCurrencyCode, Is.EqualTo(Constants.CurrencyCodes.Canada).Using(CustomComparers.TypeComparison), "Destination Goods Currency In CAD");
				NUnit.Framework.Assert.That(customsInformation.DestinationExchangeRate, Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "Destination Exchange Rate In CAD, no exchange rate found");

				shipment.JS_E_DEP = new ZDateTime(2024, 06, 15);
				NUnit.Framework.Assert.That(customsInformation.DestinationGoodsValue, Is.EqualTo(1357.9m).Using(CustomComparers.TypeComparison), "Destination Goods Value In CAD, exchange rate found in June");
				NUnit.Framework.Assert.That(customsInformation.DestinationCurrencyCode, Is.EqualTo(Constants.CurrencyCodes.Canada).Using(CustomComparers.TypeComparison), "Destination Goods Currency In CAD");
				NUnit.Framework.Assert.That(customsInformation.DestinationExchangeRate, Is.EqualTo(1.3579m).Using(CustomComparers.TypeComparison), "Destination Exchange Rate In CAD, exchange rate found in June");

				var consol = Factory.New<ForwardingConsol>();
				consol.Shipments.Add(shipment);
				consol.Transports[0].JW_ETD = new ZDateTime(2024, 07, 30);
				NUnit.Framework.Assert.That(customsInformation.DestinationGoodsValue, Is.EqualTo(1374.2m).Using(CustomComparers.TypeComparison), "Destination Goods Value In CAD, exchange rate found in July");
				NUnit.Framework.Assert.That(customsInformation.DestinationCurrencyCode, Is.EqualTo(Constants.CurrencyCodes.Canada).Using(CustomComparers.TypeComparison), "Destination Goods Currency In CAD");
				NUnit.Framework.Assert.That(customsInformation.DestinationExchangeRate, Is.EqualTo(1.3742m).Using(CustomComparers.TypeComparison), "Destination Exchange Rate In CAD, exchange rate found in July");
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var shipment = Factory.New<ForwardingShipment>();
			return new ForwardingShipmentCustomsInformation(shipment);
		}
	}
}
