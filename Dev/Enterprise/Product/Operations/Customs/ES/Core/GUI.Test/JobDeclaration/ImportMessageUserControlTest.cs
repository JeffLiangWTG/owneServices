using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing;
public class ImportMessageUserControlTest : MessageUserControlTest
{
	public void TestEntryBoundGridReadOnly()
	{
		var entry = Factory.New<CusEntryHeader>();

		declaration.CustomsEntryHeaders.Add(entry);
		using (var form = new ZForm(declaration))
		using (var control = new ImportMessageUserControl(declaration))
		{
			form.Controls.Add(control);
			form.SetDataBinding(declaration, ".");
			form.Show();

			AssertEquals(false, control.EntriesBoundGrid.ReadOnly);
		}
	}

	public override void TestSetupEntryHeaderColumns()
	{
		base.TestSetupEntryHeaderColumns();

		using (var form = new ZForm(declaration))
		using (var userControl = new ImportMessageUserControl(declaration))
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			CombineAssertions(() =>
			{
				var t2lClearenceColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.T2CMovementReferenceNumber];
				AssertNotNull("User control should have T2L Clearence column", t2lClearenceColumn);
				AssertEquals("T2LClearence column name is correct", "T2L Clearance MRN", t2lClearenceColumn.ColumnStyle.HeaderText);

				var csvImportCertificateColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ZG_CSVImportCertificate];
				AssertNotNull("User control should have CSV Import Certificate column", csvImportCertificateColumn);
				AssertEquals("CSV Import Certificate column name is correct", "CSV Import Certificate", csvImportCertificateColumn.ColumnStyle.HeaderText);

				var exportMRNColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ZG_ExportMRN];
				AssertNotNull("User control should have Export MRN column", exportMRNColumn);
				AssertEquals("Export MRN column name is correct", "Export MRN", exportMRNColumn.ColumnStyle.HeaderText);

				var limitPaymentDateColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ZG_LimitPaymentDate];
				AssertNotNull("User control should have Limit payment date column", limitPaymentDateColumn);
				AssertEquals("Limit payment date column name is correct", "Limit payment date", limitPaymentDateColumn.ColumnStyle.HeaderText);

				var atcLimitPaymentDateColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ZG_ATCLimitPaymentDate];
				AssertNotNull("User control should have ATC Limit payment date column", atcLimitPaymentDateColumn);
				AssertEquals("ATC Limit payment date column name is correct", "ATC Limit payment date", atcLimitPaymentDateColumn.ColumnStyle.HeaderText);

				var paymentProofNumberColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ZG_PaymentProofNumber];
				AssertNotNull("User control should have Payment Proof Number column", paymentProofNumberColumn);
				AssertEquals("Payment Proof Number column name is correct", "Payment Proof Number", paymentProofNumberColumn.ColumnStyle.HeaderText);

				var atcPaymentProofNumberColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ZG_ATCPaymentProofNumber];
				AssertNotNull("User control should have ATC Payment Proof Number column", atcPaymentProofNumberColumn);
				AssertEquals("ATC Payment Proof Number column name is correct", "ATC Payment Proof Number", atcPaymentProofNumberColumn.ColumnStyle.HeaderText);

				var parallelColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ZG_Parallel];
				AssertNotNull("User control should have Parallel column", parallelColumn);
				AssertEquals("Parallel column name is correct", "Parallel", parallelColumn.ColumnStyle.HeaderText);
			});
		}
	}

	public void TestEntryLineAdditionalDataUserControl()
	{
		using (var form = new ZForm(declaration))
		using (var userControl = new ImportMessageUserControl(declaration))
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();
			AssertType(typeof(ImportEntryLineAdditionalDataUserControl), userControl.FindSingle<EntryLineAdditionalDataUserControl>("EntryLineAdditionalDataUserControl"));
		}
	}

	public void TestGetEntryLineAdditionalData_IsUCC6EntryLineAdditionalDataUserControl()
	{
		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		using (var form = new ZForm(declaration))
		using (var userControl = new ImportMessageUserControl(declaration))
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			AssertType(typeof(UCC6EntryLineAdditionalDataUserControl), userControl.FindSingle<EntryLineAdditionalDataUserControl>("EntryLineAdditionalDataUserControl"));
		}
	}

	public void TestGetEntryLineAdditionalData_IsNotUCC6EntryLineAdditionalDataUserControl()
	{
		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		using (var form = new ZForm(declaration))
		using (var userControl = new ImportMessageUserControl(declaration))
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			AssertType(typeof(ImportEntryLineAdditionalDataUserControl), userControl.FindSingle<EntryLineAdditionalDataUserControl>("EntryLineAdditionalDataUserControl"));
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
	}
	JobDeclaration declaration;

	protected override MessageUserControl GetControlToTest() => new ImportMessageUserControl(declaration);
}
