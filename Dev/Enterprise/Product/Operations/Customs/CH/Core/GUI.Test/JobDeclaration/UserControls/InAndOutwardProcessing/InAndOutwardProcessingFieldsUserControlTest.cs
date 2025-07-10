using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(InAndOutwardProcessingFieldsUserControl))]
sealed class InAndOutwardProcessingFieldsUserControlTest : TestCaseWithFactory
{
	public void TestControls() => CombineAssertions(() =>
	{
		AssertEquals("SubTypeDropEdit", nameof(JobComInvoiceLine.InAndOutwardProcessing) + "+" + nameof(InAndOutwardProcessing.CSI_SubType), UserControl.SubTypeDropEdit.BindTo);
		AssertEquals("CodeDropEdit", nameof(JobComInvoiceLine.InAndOutwardProcessing) + "+" + nameof(InAndOutwardProcessing.CSI_Code), UserControl.CodeDropEdit.BindTo);
		AssertEquals("ProcedureDropEdit", nameof(JobComInvoiceLine.InAndOutwardProcessing) + "+" + nameof(InAndOutwardProcessing.CSI_Procedure), UserControl.ProcedureDropEdit.BindTo);
		AssertEquals("IssuerTypeDropEdit", nameof(JobComInvoiceLine.InAndOutwardProcessing) + "+" + nameof(InAndOutwardProcessing.CSI_IssuerType), UserControl.IssuerTypeDropEdit.BindTo);
		AssertEquals("StatusCheckBox", nameof(JobComInvoiceLine.InAndOutwardProcessing) + "+" + nameof(InAndOutwardProcessing.Repair), UserControl.StatusCheckBox.BindTo);
		AssertEquals("DescriptionTextBox", nameof(JobComInvoiceLine.InAndOutwardProcessing) + "+" + nameof(InAndOutwardProcessing.CSI_Description), UserControl.DescriptionTextBox.BindTo);
		AssertEquals("CustomsOfficeCodeFindBox", nameof(JobComInvoiceLine.InAndOutwardProcessing) + "+" + nameof(InAndOutwardProcessing.CSI_CustomsOffice), UserControl.CustomsOfficeCodeFindBox.BindTo);
	});

	protected override void TearDown()
	{
		base.TearDown();
		UserControl.Dispose();
	}

	InAndOutwardProcessingFieldsUserControl UserControl => userControl ??= new InAndOutwardProcessingFieldsUserControl();
	InAndOutwardProcessingFieldsUserControl userControl;
}

