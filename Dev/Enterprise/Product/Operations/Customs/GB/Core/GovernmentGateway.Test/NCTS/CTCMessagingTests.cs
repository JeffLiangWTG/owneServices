using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.GB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NctsHeader = Enterprise.Customs.GB.Business.NctsHeader;
using NctsTransmissionMessageGenerator = Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.NCTS.NctsTransmissionMessageGenerator;

namespace Enterprise.Customs.GB.GovernmentGateway.NCTS.Testing
{
	class CTCMessagingTests : TestCaseWithFactory
	{
		#region CTC/NCTS Messaging switching tests

		public void TestIsCTCFunctionalityValid_GeneratesCTCMessage_IE15()
		{
			var builder = new GatewayApplications.NCTS.NctsMessageBuilder();
			var message = builder.NativeMessage(header, new NctsMessageFunctionSet.DeclarationDataMessage(), errorCollector);
			AssertXMLStartsWith("Expect XML message", "<CC015B>", message);
		}

		public void TestIsCTCFunctionalityValid_GeneratesCTCMessage_IE14()
		{
			var builder = new GatewayApplications.NCTS.NctsMessageBuilder();
			var message = builder.NativeMessage(header, new NctsMessageFunctionSet.DeclarationCancellationRequestMessage("", ""), errorCollector);
			AssertXMLStartsWith("Expect XML message", "<CC014A>", message);
		}

		public void TestIsCTCFunctionalityValid_GeneratesCTCMessage_IE07()
		{
			var builder = new GatewayApplications.NCTS.NctsMessageBuilder();
			var message = builder.NativeMessage(header, new NctsMessageFunctionSet.ArrivalNotificationMessage(), errorCollector);
			AssertXMLStartsWith("Expect XML message", "<CC007A>", message);
		}

		public void TestIsCTCFunctionalityValid_GeneratesCTCMessage_IE44()
		{
			var builder = new GatewayApplications.NCTS.NctsMessageBuilder();
			var message = builder.NativeMessage(header, new NctsMessageFunctionSet.UnloadingRemarksMessage(), errorCollector);
			AssertXMLStartsWith("Expect XML message", "<CC044A>", message);
		}

		#endregion

		#region Messaging

		public void TestFlipOutAllPlaceholders_CC007A()
		{
			SendCTCMessageAndAssertPlaceholderSubstitutions(new NctsMessageFunctionSet.ArrivalNotificationMessage());
		}

		public void TestFlipOutAllPlaceholders_CC014A()
		{
			SendCTCMessageAndAssertPlaceholderSubstitutions(new NctsMessageFunctionSet.DeclarationCancellationRequestMessage("CANCELLATION REASON", ""));
		}

		public void TestFlipOutAllPlaceholders_CC015B()
		{
			SendCTCMessageAndAssertPlaceholderSubstitutions(new NctsMessageFunctionSet.DeclarationDataMessage());
		}

		public void TestFlipOutAllPlaceholders_CC044A()
		{
			SendCTCMessageAndAssertPlaceholderSubstitutions(new NctsMessageFunctionSet.UnloadingRemarksMessage());
		}

		void SendCTCMessageAndAssertPlaceholderSubstitutions(NctsMessageFunctionSet messageFunction)
		{
			var departureOffice = header.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture);
			departureOffice.CY_Data = "GB000060";
			var destinationOffice = header.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDestination);
			destinationOffice.CY_Data = "GB000060";

			var office3 = header.CustomsOffices.AddNew();
			office3.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival;
			office3.CY_Data = "GB000060";

			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", header.Principal, suffix: "", traderTin: "123456789012");
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CO1", header.Consignor, suffix: "", traderTin: "123456789013");
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PE1", header.Consignee, suffix: "", traderTin: "123456789014");
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "TRD", header.DestinationTrader, suffix: "", traderTin: "123456789015");
			var mrn = CusEntryNumber.New(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, header.Branch.Company.GC_RN_NKCountryCode);
			mrn.CE_EntryNum = "MRN123";

			var generator = new NctsTransmissionMessageGenerator(messageFunction);
			var messageManager = new NctsMessageManager(header, generator);
			var shutUp = new SendsMessagesToCustomsShutterUpperer();
			messageManager.SendNctsMessage(shutUp);
			var message = header.Messages[0];
			var messageText = message.EM_MessageText;
			AssertNotContains("{{", messageText);
			AssertNotContains("}}", messageText);
			var pkOfMessage = NctsTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(message);
			AssertContains("{{XML_INTERCHANGEID_PLACEHOLDER}}", "<IntConRefMES11>1</IntConRefMES11>", messageText);
			AssertContains("Application reference", "<AppRefMES14>NCTS</AppRefMES14>", messageText);
			AssertContains("{{XML_MESSAGEID_PLACEHOLDER}}", "<MesIdeMES19>1</MesIdeMES19>", messageText);
			AssertContains("{{XML_SYSCAR_PLACEHOLDER}}", $"<ComAccRefMES21>{pkOfMessage}</ComAccRefMES21>", messageText);
			AssertEquals(ApplicationCodeList.Codes.GbCommonTransitConvention, message.EM_ApplicationCode);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.FillWithValidTestData();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			errorCollector = new ErrorCollector();

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG";
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789011", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var aaaBranch = Factory.New<GlbBranch>();
			aaaBranch.GB_Code = "ABC";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			aaaBranch.GB_OH_OrgProxy = org1.PK;
			header.Declarant.E2_OA_Address = org1.MainAddress.PK;
			header.BH_GB = aaaBranch.PK;

			CreateCredential(header, "GB123456789000", "CTC");
			CreateCredential(header, "GB123456789011", "CTC");
		}

		public static void CreateCredential(NctsHeader header, ZString eori, ZString badge)
		{
			var company = header.Company;
			var wrapper = GBGlbCompanyWrapper.GetWrapper<GBGlbCompanyWrapper>(company);
			var gbbPasword1 = wrapper.GBBPasswordCollection.AddNew();
			gbbPasword1.Badge = badge;
			gbbPasword1.EORI = eori;
			gbbPasword1.Status = PasswordStatusList.Codes.Valid;
			gbbPasword1.IsTokenForNCTS = true;
		}

		NctsHeader header;
		ErrorCollector errorCollector;
	}
}
