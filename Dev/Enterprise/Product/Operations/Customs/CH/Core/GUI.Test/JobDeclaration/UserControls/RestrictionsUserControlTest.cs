using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

class RestrictionsUserControlTest : TestCaseWithFactory
{
	public void TestGridLayout() => CombineAssertions(() =>
	{
		using (var form = new JobDeclarationForm(declaration))
		using (var userControl = new RestrictionsUserControl())
		{
			form.Controls.Add(userControl);
			var columnStyles = userControl.RestrictionsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			AssertEquals("# of columns", 4, columnStyles.Length);
			UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(columnStyles, Restriction.Schema.CSI_LineNo, 0);
			UserControlTestHelper.AssertColumnStyles<ZDropEditColumnStyleInfo>(columnStyles, Restriction.Schema.CSI_Code, 1);
			UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(columnStyles, Restriction.Schema.CSI_ReferenceNumber, 2, characterCasing: CharacterCasing.Normal);
			UserControlTestHelper.AssertColumnStyles<ZDropEditColumnStyleInfo>(columnStyles, Restriction.Schema.CSI_Description, 3);
		}
	});

	public void TestTabPages()
	{
		using (var form = new JobDeclarationForm(declaration))
		using (var userControl = new RestrictionsUserControl())
		{
			AssertSame(userControl.ItemDetailsTabPage, userControl.RestrictionDetailTabControl.TabPages[0]);
			AssertSame(userControl.AdditionalInformationTabPage, userControl.RestrictionDetailTabControl.TabPages[1]);
		}
	}

	public void TestDetailControls()
	{
		using (var form = new JobDeclarationForm(declaration))
		using (var userControl = new RestrictionsUserControl())
		{
			form.Controls.Add(userControl);
			AssertControl(userControl.ItemNumberIntEdit, nameof(Restriction.CSI_LineNo), true);
			AssertControl(userControl.PermitNumberTextBox, nameof(Restriction.CSI_ReferenceNumber), false, characterCasing: CharacterCasing.Normal);
			AssertEquals($"{userControl.PermitExceptionReasonDropEdit.Name}.BindTo", nameof(Restriction.CSI_Description), userControl.PermitExceptionReasonDropEdit.BindTo);
			AssertEquals($"{userControl.CodeDropEdit.Name}.BindTo", nameof(Restriction.CSI_Code), userControl.CodeDropEdit.BindTo);
			AssertEquals($"{userControl.PermitOwnerDocAddressControl.Name}.BindTo", nameof(Restriction.PermitOwnerDocAddress), userControl.PermitOwnerDocAddressControl.BindTo);
			AssertEquals($"{userControl.PermitOwnerDocAddressControl.Name}.BindToOrganisations", $"{nameof(JobDeclaration.FilteredInvoiceLines)}.{nameof(JobComInvoiceLine.Restrictions)}.{nameof(Restriction.Lookups)}.{nameof(RestrictionLookups.PermitOwnerList)}", userControl.PermitOwnerDocAddressControl.BindToOrganisations);
			AssertControl(userControl.PermitOwnerIdentificationTextBox, nameof(Restriction.PermitOwnerIdentification), false);
			AssertEquals($"{userControl.PermitOwnerOverrideCheckBox.Name}.BindTo", nameof(Restriction.OverrideIdentification), userControl.PermitOwnerOverrideCheckBox.BindTo);
			AssertEquals($"{userControl.PermitOwnerOverrideCheckBox.Name}.ReadOnly", false, userControl.PermitOwnerOverrideCheckBox.ReadOnly);
			AssertControl(userControl.OverriddenIdentificationTextBox, nameof(Restriction.CSI_ReferenceNumber2), false);
		}
		void AssertControl(ZTextBox control, string expectedBindTo, bool expectedReadOnly, CharacterCasing? characterCasing = null)
		{
			AssertEquals($"{control.Name}.BindTo", expectedBindTo, control.BindTo);
			AssertEquals($"{control.Name}.ReadOnly", expectedReadOnly, control.ReadOnly);
			if (characterCasing != null)
			{
				AssertEquals($"{control.Name}.CharacterCasing", characterCasing, control.CharacterCasing);
			}
		}
	}

	public void TestAdditionalInformationGrid() => CombineAssertions(() =>
	{
		using (var form = new JobDeclarationForm(declaration))
		using (var userControl = new RestrictionsUserControl())
		{
			form.Controls.Add(userControl);
			var columnStyles = userControl.RestrictionAdditionalInformationGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			AssertEquals("# of columns", 4, columnStyles.Length);
			UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(columnStyles, RestrictionAdditionalInformation.Schema.CY_Order, 0);
			UserControlTestHelper.AssertColumnStyles<ZDropEditColumnStyleInfo>(columnStyles, RestrictionAdditionalInformation.Schema.CY_Code, 1);
			UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(columnStyles, nameof(RestrictionAdditionalInformation.CY_CodeDescription), 2);
			UserControlTestHelper.AssertColumnStyles<ZMultiControlColumnStyleInfo>(columnStyles, RestrictionAdditionalInformation.Schema.CY_Data, 3, characterCasing: CharacterCasing.Normal);

			var cyDataColumnStyle = columnStyles.FirstOrDefault(x => x.ColumnName == RestrictionAdditionalInformation.Schema.CY_Data) as ZMultiControlColumnStyleInfo;
			AssertEquals("CY_Data FieldTypeColumnName", nameof(RestrictionAdditionalInformation.CY_DataFieldType), cyDataColumnStyle.FieldTypeColumnName);
		}
	});

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.Invoices.AddNew().InvoiceLines.AddNew().Restrictions.AddNew();
	}
	JobDeclaration declaration;
}
