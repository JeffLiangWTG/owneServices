using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using SupportingDocSendingObject = Enterprise.Customs.Business.SupportingDocSendingObject;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(SupportingDocSendingForm))]
public class SupportingDocSendingFormTest : ZFormBasherTest
{
	protected override bool AllowSaveOnFormForTestHasChanges => false;

	protected override bool AllowHasChangesOnFormOpen => true;

	protected override Form GetFormToBashCore()
	{
		return new SupportingDocSendingForm(new JobDeclarationSupportingDocSendingObjectParent(Declaration));
	}

	public void TestFormHeading()
	{
		using (var form = new SupportingDocSendingForm(new JobDeclarationSupportingDocSendingObjectParent(Declaration)))
		{
			AssertEquals("Send Accompanying Documents to Customs", form.FormHeading);
		}
	}

	public void TestColumns()
	{
		using (var form = new SupportingDocSendingForm(new JobDeclarationSupportingDocSendingObjectParent(Declaration)))
		{
			var messageSendingObjectsGrid = form.Controls.Find("MessageSendingObjectsGrid", true)[0] as ZGrid;
			var columnStyles = messageSendingObjectsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				UserControlTestHelper.AssertColumnStyles<ZGuidDropEditColumnStyleInfo>(columnStyles, SupportingDocSendingObject.Schema.EDoc, 0);
				UserControlTestHelper.AssertColumnStyles<ZDropEditColumnStyleInfo>(columnStyles, SupportingDocSendingObject.Schema.DocumentType, 1);
				UserControlTestHelper.AssertColumnStyles<ZDropEditColumnStyleInfo>(columnStyles, SupportingDocSendingObject.Schema.LocalReferenceNumber, 2, caption: "Entry (MRN)");
				UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(columnStyles, SupportingDocSendingObject.Schema.CaseNumber, 3, caption: "Reference");
				UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(columnStyles, SupportingDocSendingObject.Schema.EDocFileSizeInMB, 4);
			});
		}
	}

	JobDeclaration Declaration
	{
		get
		{
			if (declaration == null)
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.Invoices.AddNew().InvoiceLines.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.MovementReferenceNumberSetter("MRN1");
			}
			return declaration;
		}
	}
	JobDeclaration declaration;
}
