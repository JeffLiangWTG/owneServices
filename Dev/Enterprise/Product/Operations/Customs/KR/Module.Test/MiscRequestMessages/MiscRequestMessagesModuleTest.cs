using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(MiscRequestMessagesModule))]
	sealed class MiscRequestMessagesModuleTest : ZModuleBasherWithFetchHintsTest
	{
		protected override ZFilterModule CreateModuleForFetchHintsTest() => new MiscRequestMessagesModule();
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.KR.MiscRequestMessages;
		protected override void SetupDataForFetchHintsTest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType("CUSDP", "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSOF", "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSDP", "20", "내륙기지통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			for (int idx = 0; idx < 20; idx++)
			{
				CreateMiscRequestMessageForFetchHintTest(idx, ElectronicDocumentTypeList.Codes._5AC);
			}

			for (int idx = 20; idx < 40; idx++)
			{
				CreateMiscRequestMessageForFetchHintTest(idx, ElectronicDocumentTypeList.Codes._5SG);
			}
			Factory.Save();
		}
		void CreateMiscRequestMessageForFetchHintTest(int idx, string messageType)
		{
			var request = Factory.New<CusMiscRequestHeader>();
			request.CMR_Status = "OAC";
			request.CMR_MessageType = messageType;
			request.CMR_JobNumber = "MSC000000" + idx.ToString("00");
			request.CMR_RequestDate = ZDateTime.Today.AddDays(idx + 1);
			request.CMR_CustomsOffice = "01020";
			request.CMR_RequestDetails = "Test" + idx.ToString();
			request.CMR_GB = GlbBranch.CurrentBranch.PK;
			request.CMR_GS_NKBroker = GlbStaff.CurrentUser.GS_Code;
			var requestLine = request.RequestLines.AddNew();
			requestLine.CML_EntryType = "EXP";
			requestLine.CML_EntryNumber = "12345" + idx.ToString("00");
			var cusEntryNum = request.CreateCusEntryNumber();
			cusEntryNum.CE_EntryNum = "4061522000000" + idx.ToString("00");
			cusEntryNum.CE_IssueDate = ZDateTime.Today;
			cusEntryNum.CE_ExpiryDate = ZDateTime.Today.AddDays(1);
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var factory = collection.Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType("CUSDP", "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSOF", "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSDP", "20", "내륙기지통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			factory.Save();

			var request = factory.New<CusMiscRequestHeader>();
			request.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;
			request.CMR_JobNumber = "11598210000001U";
			request.CMR_GB = GlbBranch.CurrentBranch.PK;
			request.CMR_CustomsOffice = "01020";
			request.CMR_GS_NKBroker = "BRK";
			request.CMR_RequestDate = ZDateTime.Today;
			request.CMR_RequestDetails = "Request Reason";
			request.CMR_Status = "OST";
			factory.Save();
		}

		public void TestNewMenu()
		{
			using (var module = new MiscRequestMessagesModuleForTest())
			{
				var menuItems = module.GetNewStandardMenuItems().FindByText("New").MenuItems;
				AssertNotNull(menuItems.FindByText("5AC - (EXP) Application for Extended Office Hours"));
				AssertNotNull(menuItems.FindByText("5GW - (IMP) Application for Extended Office Hours"));
				AssertNotNull(menuItems.FindByText("5SG - (IMP) Final Price Period Extension Application"));
			}
		}

		public void TestAllowNew()
		{
			using (var module = new MiscRequestMessagesModule())
			{
				Assert(!module.AllowNew);
			}
		}

		sealed class MiscRequestMessagesModuleForTest : MiscRequestMessagesModule
		{
			public new MenuItem[] GetNewStandardMenuItems() => base.GetNewStandardMenuItems();
		}
	}
}
