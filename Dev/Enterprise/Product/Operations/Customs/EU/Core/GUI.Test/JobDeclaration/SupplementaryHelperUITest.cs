using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class SupplementaryHelperUITest : TestCaseWithFactory
	{
		public void TestNoEligibleEntries()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			const string expectedMessage = "No suitable simplified entry could be found on this declaration. This may mean that there are no entries that are deemed a simplified entry, or none that are customs cleared, or none that have an entry number or MRN.";

			SupplementaryHelperUI.NewRelatedDeclaration(declaration);
			AssertEquals("NewRelatedDeclaration", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			SupplementaryHelperUI.NewEntryInstruction(declaration);
			AssertEquals("NewEntryInstruction", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			SupplementaryHelperUI.ReUseEntryInstruction(declaration);
			AssertEquals("ReUseEntryInstruction", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestNewRelatedDeclaration()
		{
			var declaration = CreateDeclarationForTest();
			var forms = SupplementaryHelperUI.NewRelatedDeclaration(declaration);
			AssertEquals("forms.Count", 1, forms.Count);
			AssertType<JobDeclarationForm>("form type", forms[0]);
			forms[0].Dispose();
		}

		public void TestNewEntryInstruction()
		{
			var declaration = CreateDeclarationForTest();
			SupplementaryHelperUI.NewEntryInstruction(declaration);
			AssertContains("NewRelatedDeclaration", "Entry Instruction created", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestReUseEntryInstruction()
		{
			var declaration = CreateDeclarationForTest();
			SupplementaryHelperUI.ReUseEntryInstruction(declaration);
			AssertContains("NewRelatedDeclaration", "Entry Instruction updated", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		JobDeclaration CreateDeclarationForTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var ei1 = declaration.CustomsEntryInstructions.AddNew();
			ei1.CEI_Style = "I1";

			var invHeader1 = declaration.Invoices.AddNew();
			invHeader1.JZ_Remarks = "Header1";
			var invLine11 = invHeader1.InvoiceLines.AddNew();
			invLine11.JI_InvoiceQuantity = 11;
			invLine11.JI_CEI = ei1.PK;

			declaration.DoMerge();
			var eh1 = declaration.CustomsEntryHeaders.Single(x => x.CH_CEI_Instruction == ei1.PK);

			eh1.CH_EntryStatus = EntryStatusList.Codes.Clear;
			eh1.MovementReferenceNumberSetter("MRN0001");
			Factory.Save();

			var cusCodeHelper = new UniversalReferenceTestDataHelper(Factory);
			var cusCodeType = cusCodeHelper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsStatus, RefCusCodeListTypes.Codes.CustomsStatus);
			var cusCodeList = cusCodeHelper.CreateNewOrGetExistingCusCodeList(declaration.CountryCode, RefCusCodeListTypes.Codes.CustomsStatus, EntryStatusList.Codes.Clear, EntryStatusList.Descriptions.Clear, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var cusCodeListAttribute = cusCodeHelper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeList.PK, Universal.RefCusCodeListAttributeTypes.Codes.CustomsCleared, "true");
			Factory.Save();

			return declaration;
		}
	}
}
