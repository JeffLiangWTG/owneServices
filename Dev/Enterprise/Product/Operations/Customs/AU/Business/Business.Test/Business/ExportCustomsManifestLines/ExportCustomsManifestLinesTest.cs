using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ExportCustomsManifestLines))]
	sealed class ExportCustomsManifestLinesTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetTopBusinessObject()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var controllerFactory = System.Reflection.Assembly.Load("Enterprise.ZArchitecture.GUI").GetType("Enterprise.ZArchitecture.Modules.ZControllerFactory").GetField("Instance").GetValue(null);
				var controller = controllerFactory.GetType().GetMethod("GetControllerForBizo").Invoke(controllerFactory, new[] { Line.GetTopBusinessObject() });
				AssertNotNull("We got the controller, so we got the Form", controller);
				AssertEquals("Controller type", "Enterprise.Customs.AU.Module.AirCTOHawbExportController", controller.GetType().FullName);
			}
		}

		public void TestChangingEL_AirWayBillUpdateEL_CAN()
		{
			var anotherFactory = new BusinessObjectFactory();
			var gbMawb = anotherFactory.New<Customs.Business.CusMAWB>();
			gbMawb.CM_ApplicationCode = "CUK";
			var gbHawb1 = gbMawb.ChildBills.AddNew();
			gbHawb1.CS_HAWB = "~HAWBTran1~";
			gbHawb1.CS_ApplicationCode = "CUK";

			var gbHawb2 = anotherFactory.New<Customs.Business.CusHAWB>();
			gbHawb2.CS_HAWB = "~HAWBTran1~";
			gbHawb2.CS_ApplicationCode = "CUK";

			var cusHAWBTranshipment = anotherFactory.NewWithValidTestData<CusHAWB>();
			cusHAWBTranshipment.CS_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			cusHAWBTranshipment.CS_HAWB = "~HAWBTran~";
			cusHAWBTranshipment.CS_TranshipmentEntryNum = "~TranNum~";

			anotherFactory.Save();

			var header = Factory.NewWithValidTestData<ExportCustomsManifestHeader>();
			var line = header.Lines.AddNew();
			line.EL_TypeOfCAN = CANType.ContingencyCustomsAuthorityNumber.Code;
			line.EL_AirWayBill = cusHAWBTranshipment.CS_HAWB;
			AssertEquals(CANType.CustomsAuthorityNumber.Code, line.EL_TypeOfCAN);
			AssertEquals("~TranNum~", line.EL_CAN);

			var cusHAWBTranshipment1 = Factory.NewWithValidTestData<CusHAWB>();
			cusHAWBTranshipment1.CS_HAWB = "~HAWBTran1~";
			cusHAWBTranshipment1.CS_TranshipmentEntryNum = "~TranNum1~";
			line.EL_AirWayBill = cusHAWBTranshipment1.CS_HAWB;
			AssertEquals("EL_CAN should not change", "~TranNum~", line.EL_CAN);

			var line1 = header.Lines.AddNew();
			line1.EL_AirWayBill = "~NotExistingHAWB~";
			AssertEquals("", line1.EL_CAN);

			var line2 = header.Lines.AddNew();
			var cusHAWB = Factory.NewWithValidTestData<CusHAWB>();
			cusHAWB.CS_HAWB = "~HAWB~";
			line2.EL_AirWayBill = cusHAWB.CS_HAWB;
			AssertEquals("", line2.EL_CAN);
		}

		public void TestSupportedAddresses()
		{
			IDocAddresses addresses = Factory.New<ExportCustomsManifestLines>();

			AssertEquals("just a reminder to update this test when new addresses are added", 2, addresses.SupportedAddressTypes.Count);
			AssertCollectionContains(DocAddressType.ConsignorDocumentaryAddress, addresses.SupportedAddressTypes);
			AssertCollectionContains(DocAddressType.ConsigneeDocumentaryAddress, addresses.SupportedAddressTypes);
		}

		public void TestJobInvoicingSecurity()
		{
			IJobInvoicingPlugIn invoicing = Factory.New<ExportCustomsManifestLines>();
			AssertEquals("AuditSecurity", Env.Security.AUCustomsAirCTOExportAuditBilling.LookupKey, invoicing.InvoicingSupporter.AuditSecurity.LookupKey);
			AssertEquals("InvoicingSecurity", Env.Security.AUCustomsAirCTOExportJobInvoicing.LookupKey, invoicing.InvoicingSupporter.JobInvoicingSecurity.LookupKey);
		}

		public void TestJobInvoicingConsignorConsignee()
		{
			OrgHeader owner = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();

			ExportCustomsManifestLines line = Factory.New<ExportCustomsManifestLines>();
			line.EL_OH_Owner = owner.PK;
			line.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			line.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			IJobInvoicingPlugIn invoicing = line;
			AssertEquals("Consignor", consignor, invoicing.InvoicingSupporter.Consignor);
			AssertEquals("Consignee", consignee, invoicing.InvoicingSupporter.Consignee);
		}

		public void TestJobInvoicingConsumerType()
		{
			IJobInvoicingPlugIn invoicing = Factory.New<ExportCustomsManifestLines>();
			AssertEquals("ConsumerType", JobInvoicingConsumerTypes.CTOCusExportHAWB, invoicing.InvoicingSupporter.ConsumerType);
		}

		public void TestJobInvoicingPorts()
		{
			ZDateTime now = ZDateTime.Now;

			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_DepartureDate = now;
			header.ED_RL_NKPortOfDeparture = "AUBNE";
			header.ED_RL_NKPortOfDestination = "SGSIN";

			IJobInvoicingPlugIn invoicing = header.Lines.AddNew();

			AssertEquals("Origin", "AUBNE", invoicing.InvoicingSupporter.Origin.Code);
			AssertEquals("ETD", now, invoicing.InvoicingSupporter.ETD);
			AssertEquals("Destination", "SGSIN", invoicing.InvoicingSupporter.Destination.Code);
			AssertEquals("ETA", ZDateTime.Empty, invoicing.InvoicingSupporter.ETA);
		}

		public void TestJobInvoicingMisc()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			ExportCustomsManifestLines line = header.Lines.AddNew();
			line.EL_AirWayBill = "08155555625";
			line.EL_UserReferenceNum = "RefNum";

			IJobInvoicingPlugIn invoicing = line;

			AssertEquals("MasterBillNumber", "", invoicing.InvoicingSupporter.MasterBillNumber);
			AssertEquals("HouseBillNumber", "08155555625", invoicing.InvoicingSupporter.HouseBillNumber);
			AssertEquals("TransportMode", Core.Constants.TransportModes.Air, invoicing.InvoicingSupporter.TransportMode);
			AssertEquals("JobNumber", "RefNum", invoicing.JobNumber);
		}

		public void TestJobInvoicingEditSecurity()
		{
			IJobInvoicingPlugIn testJob = Factory.New<ExportCustomsManifestLines>();
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestJobInvoicingDefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = Factory.New<ExportCustomsManifestLines>();
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		public void TestValidation()
		{
			AssertEquals("ValidationType", typeof(ExportCustomsManifestLinesValidation), Line.Validation.GetType());
		}

		public void TestLookups()
		{
			AssertEquals("LookupsType", typeof(ExportCustomsManifestLinesLookups), Line.Lookups.GetType());
		}

		public void TestIsCANLine()
		{
			Line.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
			Assert(Line.IsCANLine);
			Line.EL_TypeOfCAN = CANType.Exemptions.EXLV.Code;
			Assert(!Line.IsCANLine);
		}

		public void TestIsExemptLine()
		{
			Line.EL_TypeOfCAN = CANType.Exemptions.EXLV.Code;
			Assert(Line.IsExemptLine);
			Line.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
			Assert(!Line.IsExemptLine);
		}

		public void TestIsPersonalEffectsOrLowValue()
		{
			Line.EL_TypeOfCAN = CMRExportExemptionCodes.EXDC.Code;
			Assert(!Line.IsPersonalEffectsOrLowValue);
			Line.EL_TypeOfCAN = CMRExportExemptionCodes.EXPE.Code;
			Assert(Line.IsPersonalEffectsOrLowValue);
			Line.EL_TypeOfCAN = CMRExportExemptionCodes.EXLV.Code;
			Assert(Line.IsPersonalEffectsOrLowValue);
			Line.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
			Assert(!Line.IsPersonalEffectsOrLowValue);
		}

		public void TestHeader()
		{
			AssertEquals("Header", Header, Line.Header);
		}

		public void TestIsCCANLine()
		{
			Line.EL_TypeOfCAN = CANType.ContingencyCustomsAuthorityNumber.Code;
			Assert(Line.IsCCANLine);
			Line.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
			Assert(!Line.IsCCANLine);
		}

		public void TestBlankCANIfWeChangeToAnExemption()
		{
			Line.EL_TypeOfCAN = CANType.ContingencyCustomsAuthorityNumber.Code;
			Line.EL_CAN = "12345";
			Line.EL_TypeOfCAN = CANType.Exemptions.EXTI.Code;
			AssertEquals("CAN", ZString.Empty, Line.EL_CAN);
			Line.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
			Line.EL_CAN = "12345";
			Line.EL_TypeOfCAN = CANType.Exemptions.EXTI.Code;
			AssertEquals("CAN", ZString.Empty, Line.EL_CAN);
		}

		public void TestOwnerProxy()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsConsignor = true;
			org.OH_FullName = "foo bar";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.Australia);
			cusCode.OK_RN_NKCodeCountry = GlbBranch.CurrentBranch.Country.Code;
			cusCode.OK_CustomsRegNo = "spam eggs";

			AssertEquals(false, Line.EL_GoodsOwnerInfo.ReadOnly);
			AssertEquals(false, Line.EL_GoodsOwnerPartyIDInfo.ReadOnly);
			AssertEquals("", Line.EL_GoodsOwner);
			AssertEquals("", Line.EL_GoodsOwnerPartyID);

			Line.EL_OH_Owner = org.PK;
			AssertEquals(true, Line.EL_GoodsOwnerInfo.ReadOnly);
			AssertEquals(true, Line.EL_GoodsOwnerPartyIDInfo.ReadOnly);
			AssertEquals("foo bar", Line.EL_GoodsOwner);
			AssertEquals("spam eggs", Line.EL_GoodsOwnerPartyID);

			org.CustomsClientID = "ccid";
			cusCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia);
			Line.EL_OH_Owner = org.PK;
			AssertEquals(true, Line.EL_GoodsOwnerInfo.ReadOnly);
			AssertEquals(true, Line.EL_GoodsOwnerPartyIDInfo.ReadOnly);
			AssertEquals("foo bar", Line.EL_GoodsOwner);
			AssertEquals("ccid", Line.EL_GoodsOwnerPartyID);

			Line.EL_OH_Owner = ZGuid.Empty;
			AssertEquals(false, Line.EL_GoodsOwnerInfo.ReadOnly);
			AssertEquals(false, Line.EL_GoodsOwnerPartyIDInfo.ReadOnly);
			AssertEquals("foo bar", Line.EL_GoodsOwner);
			AssertEquals("ccid", Line.EL_GoodsOwnerPartyID);

			cusCode.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;
			Line.EL_OH_Owner = org.PK;
			AssertEquals("spam eggs", Line.EL_GoodsOwnerPartyID);
		}

		public void TestDefaultTypeOfCAN()
		{
			AssertEquals(CANType.CustomsAuthorityNumber.Code, Line.EL_TypeOfCAN);
		}

		public void TestIsAirCTO()
		{
			AirCTOExportCustomsManifestHeader airCTOHeader = Factory.New<AirCTOExportCustomsManifestHeader>();
			airCTOHeader.ED_ManifestType = AirManifestTypeList.Codes.CtoReceivalRemovalStandAlone;
			ExportCustomsManifestLines lines = airCTOHeader.Lines.AddNew();
			AssertEquals(true, lines.IsCTO);
		}

		public void TestIsNotAirCTO()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			ExportCustomsManifestLines lines = header.Lines.AddNew();
			AssertEquals(false, lines.IsCTO);
		}

		public void TestValidationForNotAirCTO()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			ExportCustomsManifestLines lines = header.Lines.AddNew();
			AssertNotNull(lines.Validation);
			AssertEquals(typeof(ExportCustomsManifestLinesValidation), lines.Validation.GetType());
		}

		public void TestMessages()
		{
			AssertNotNull(Line.Messages);
			AssertEquals(Line.Messages, Line.Messages);
			AssertEquals(true, Line.IsRegisteredEditableChildObject(Line.Messages));
			AssertEquals(true, Line.Messages.IsLoaded);
		}

		public void TestDetailForAir()
		{
			Header.ED_TransportMode = Core.Constants.TransportModes.Air;
			Line.EL_AirWayBill = "11221122";
			Line.EL_CAN = "CANCAN";
			Line.EL_GoodsDescription = "Foo";
			Line.EL_GoodsOwner = "bar";
			Line.EL_GoodsOwnerPartyID = "spam";
			Line.EL_NumberOfPackages = 17;
			Line.EL_RN_NKCountryOfDestination = Core.Constants.CountryCodes.NewZealand;
			Line.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
			Line.EL_UserReferenceNum = "eggs";

			string expectedDetail = @"Reference: eggs
CAN: CANCAN
AirWay Bill: 11221122
Destination Country/Region: NZ
Number of Packages: 17
Goods Description: Foo
Goods Owner: bar
Goods Owner Party ID: spam";

			AssertMultilineASCIIEquals("detail", expectedDetail, ((ICMRMessageRespondee)Line).Details);
		}

		public void TestDetailForAirBlank()
		{
			Header.ED_TransportMode = Core.Constants.TransportModes.Air;
			string expectedDetail = @"";
			AssertMultilineASCIIEquals("detail", expectedDetail, ((ICMRMessageRespondee)Line).Details);
		}

		public void TestDetailForSea()
		{
			Header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			Line.EL_CAN = "CANCAN";
			Line.EL_GoodsDescription = "Foo";
			Line.EL_GoodsOwner = "bar";
			Line.EL_GoodsOwnerPartyID = "spam";
			Line.EL_NumberOfPackages = 17;
			Line.EL_NumberOfContainers = 3;
			Line.EL_RN_NKCountryOfDestination = Core.Constants.CountryCodes.NewZealand;
			Line.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
			Line.EL_UserReferenceNum = "eggs";

			string expectedDetail = @"Reference: eggs
CAN: CANCAN
Destination Country/Region: NZ
Number of Containers: 3
Number of Packages: 17
Goods Description: Foo
Goods Owner: bar
Goods Owner Party ID: spam";

			AssertMultilineASCIIEquals("detail", expectedDetail, ((ICMRMessageRespondee)Line).Details);
		}

		public void TestDetailForSeaBlank()
		{
			Header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			string expectedDetail = @"";
			AssertMultilineASCIIEquals("detail", expectedDetail, ((ICMRMessageRespondee)Line).Details);
		}

		public void TestShortDescription()
		{
			Line.EL_CAN = "foofoo";
			AssertEquals("CAN: foofoo", ((ICMRMessageRespondee)Line).ShortDescription);
		}

		public void TestShortDescriptionBlank()
		{
			Line.EL_CAN = "";
			AssertEquals("", ((ICMRMessageRespondee)Line).ShortDescription);
		}

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent exportCustomsManifestLines = Factory.New<ExportCustomsManifestLines>();
			Assert(exportCustomsManifestLines.AllowInvoiceDeletion);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<ExportCustomsManifestHeader>();
			return header.Lines.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<ExportCustomsManifestHeader>();
			return header.Lines.AddNew();
		}

		ExportCustomsManifestHeader header;
		ExportCustomsManifestHeader Header => header ?? (header = Factory.New<ExportCustomsManifestHeader>());

		ExportCustomsManifestLines line;
		ExportCustomsManifestLines Line => line ?? (line = Header.Lines.AddNew());
	}
}
