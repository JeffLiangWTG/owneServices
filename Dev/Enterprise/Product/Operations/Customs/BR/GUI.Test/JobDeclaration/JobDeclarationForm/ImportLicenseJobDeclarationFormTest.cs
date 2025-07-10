using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class ImportLicenseJobDeclarationFormTest : Customs.GUI.Testing.BaseJobDeclarationFormAbstractTest<JobDeclaration>
	{
		public void TestUnsplitEntryInstructionsOnPreSave()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FixedJobMessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;

			var job = new JobHeader.Loader(declaration).TryLoadOrCreate();
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = Factory.NewWithValidTestData<GlbDepartment>().PK;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Description = "DESCRIPTION";

			var invoice = declaration.Invoices.AddNew();
			for (var idx = 0; idx < Business.CusEntryInstruction.MaximumInvoiceLinesAllowedForImportLicense; idx++)
			{
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_Tariff = "1111111";
			}

			var invoiceLineExceeded = invoice.JobComInvoiceLines.AddNew();
			invoiceLineExceeded.JI_CEI = instruction.PK;
			invoiceLineExceeded.JI_Tariff = "1111111";

			var invoiceLineOtherMergeKey = invoice.JobComInvoiceLines.AddNew();
			invoiceLineOtherMergeKey.JI_CEI = instruction.PK;
			invoiceLineOtherMergeKey.JI_Tariff = "1111111";
			invoiceLineOtherMergeKey.NaladiHs = "1";

			var invoiceLineWithDiffTariff = invoice.JobComInvoiceLines.AddNew();
			invoiceLineWithDiffTariff.JI_CEI = instruction.PK;
			invoiceLineWithDiffTariff.JI_Tariff = "2222222";

			declaration.DoMerge();
			var newInstruction = invoiceLineWithDiffTariff.EntryInstruction;

			using (var form = new JobDeclarationForm(declaration))
			{
				form.FireValidateAllForTest();
				AssertEquals("Precondition: no errors", "", declaration.GetErrors().ToUniqueMessageListString());

				invoiceLineWithDiffTariff.JI_LinePrice = 100m;
				AssertEquals("Must ContinueWithSave", ContinueWithSave.Yes, form.FireSaveButton());
				AssertEquals(false, declaration.HasChanges);

				invoiceLineWithDiffTariff.JI_UsedMaterialRegime = UsedMaterialRegimeList.Codes.Nationalization;
				invoiceLineWithDiffTariff.JI_Tariff = "1111111";
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.No);
				AssertEquals("Must NOT ContinueWithSave", ContinueWithSave.No, form.FireSaveButton());
				AssertEquals("Any change made to an already split Entry Instruction will cause the system to revert the changes. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Not unsplitted", false, newInstruction.IsDeleted);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
				AssertEquals("Must NOT ContinueWithSave", ContinueWithSave.No, form.FireSaveButton());
				AssertEquals("Any change made to an already split Entry Instruction will cause the system to revert the changes. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Unsplitted", true, newInstruction.IsDeleted);
			}
		}

		public void TestRoutingVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.Routing);

				Assert("Routing should not be enabled", !plugIn.Enabled);
			}
		}

		public override ZString MessageTypeForFormBashing => Common.BR.BRJobMessageTypeList.Codes.ImportLicense;

		protected override void SetupDeclarationForSpecificFormBashing(BaseJobDeclarationForm form)
		{
			base.SetupDeclarationForSpecificFormBashing(form);
			currentFormBashingDeclaration.FixedJobMessageType = MessageTypeForFormBashing;
		}

		protected override BaseJobDeclaration CreateDeclarationForPerformanceTest(BusinessObjectFactory factory)
		{
			var result = factory.New<JobDeclaration>();
			result.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			result.JE_MessageType = MessageTypeForFormBashing;
			result.JE_TransportMode = Core.Constants.TransportModes.Sea;
			return result;
		}

		protected override IEnumerable<string> GetMessageSubTypesForFormBashingTest(CodeDescriptionPairList messageSubTypeList)
		{
			if (messageSubTypeList.Count > 0)
			{
				yield return messageSubTypeList[0].Code;
			}
		}
	}
}
