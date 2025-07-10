using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class Import934MessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle()
		{
			var builder = new Import934MessageSendingFormBuilder();
			var list = builder.GetColumnStyles();

			AssertEquals(list.Length, 3);
			AssertEquals(nameof(KR.Business.JobDeclarationMessageSendingObject.ShouldSend), list[0].ColumnName);
			AssertEquals(nameof(KR.Business.JobDeclarationMessageSendingObject.FormattedEntryNumber), list[1].ColumnName);
			AssertEquals(nameof(ValuationDeclarationMessageSendingObject.ValuationCode), list[2].ColumnName);
		}

		public void TestGetAdditionalTabPages()
		{
			var builder = new Import934MessageSendingFormBuilder();
			var tabPages = builder.GetAdditionalTabPages(null);
			AssertNotNull(tabPages);
			AssertEquals(1, tabPages.Length);
			AssertType(typeof(ImportValuationDeclarationUserControl), tabPages[0].Controls[0]);
			AssertEquals("Valuation Declaration", tabPages[0].CaptionResourceString.Caption);
			AssertEquals("ImportValuationDeclarationUserControl", tabPages[0].Controls[0].Name);

			foreach (var control in tabPages)
			{
				control.Dispose();
			}
		}
	}

	[TestedType(typeof(MessageSendingActionForm))]
	sealed class Import934MessageSendingActionFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var parent = JobDeclarationMessageSendingObjectParent.GetJobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._934, MessageFunctions.MessageFunctionCode.Original);

			return new MessageSendingActionForm(parent, new Import934MessageSendingFormBuilder());
		}
		public override void TestMinimumSizeNotTooBig()
		{
			using Form form = GetFormToBash();
			int num = ControlDpiScalingHelper.ScaleToCurrentDpiX(1366);
			int num2 = ControlDpiScalingHelper.ScaleToCurrentDpiY(900);
			int num3 = ControlDpiScalingHelper.ScaleToCurrentDpiY(43);
			int num4 = num;
			int num5 = checked(num2 - num3);
			Assertion.Assert("Form min size too wide (" + form.MinimumSize.Width + ") for the screen. Should be less than or equal to " + num4, form.MinimumSize.Width <= num4);
			Assertion.Assert("Form min size too high (" + form.MinimumSize.Height + ") for the screen. Should be less than or equal to " + num5, form.MinimumSize.Height <= num5);
		}
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var list = new ValuationCodeList();
			foreach (CodeDescriptionPair item in list)
			{
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_ValuationCode = item.Code;
				var invoiceLine = invoice.InvoiceLines.AddNew();

				var entry = declaration.ActiveEntryHeaders.AddNew();
				var entryLine = entry.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
			}
		}
		JobDeclaration declaration;
	}
}
