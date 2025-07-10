using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.UserControls.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing;

[TestedType(typeof(ImportInvoiceLineTemplate))]
sealed class ImportInvoiceLineTemplateTest : BaseInvoiceLineTemplateTest
{
	protected override ZString DeclarationMessageType => JPJobMessageTypeList.Codes.Import;

	protected override ZUserControl GetControlForTest() => new ImportInvoiceLineTemplate();

	public void TestImportControls()
	{
		var declaration = Factory.New<JobDeclaration>();
		using var control = GetControlForTest();
		control.SetDataBinding(declaration, "");

		CombineAssertions(() =>
		{
			var certOfOriginField = control.FindSingle<ZTextBox>(_ => control.BindingSource.GetBindingMember(_).EndsWith("JI_Procedure"));
			AssertEquals("JI_Procedure is Enabled", true, certOfOriginField?.Enabled);

			TestHelper.AssertControlExists(control, "CertificateOfOriginPanel", "");

			var preferenceDropEdit = control.FindSingle<ZDropEdit>(_ => control.BindingSource.GetBindingMember(_).EndsWith("JI_Calc_Preference"));
			AssertEquals("JI_Calc_Preference is Enabled", true, preferenceDropEdit?.Enabled);

			var originCertifierDropEdit = control.FindSingle<ZDropEdit>(_ => control.BindingSource.GetBindingMember(_).EndsWith("JI_Calc_OriginCertifier"));
			AssertEquals("JI_Calc_OriginCertifier is Enabled", true, originCertifierDropEdit?.Enabled);

			var certificateOfOriginCertifierDropEdit = control.FindSingle<ZDropEdit>(_ => control.BindingSource.GetBindingMember(_).EndsWith("JI_Calc_CertificateOfOriginCertifier"));
			AssertEquals("JI_TradeControlOrderAppendix is Enabled", true, certificateOfOriginCertifierDropEdit?.Enabled);

			var dutyRateFormulaField = control.FindSingle<ZTextBox>(_ => control.BindingSource.GetBindingMember(_).EndsWith("JI_DutyRateFormula"));
			AssertEquals("JI_DutyRateFormula is Enabled", true, dutyRateFormulaField?.Enabled);

			var tariffFindBox = control.FindSingle<Universal.GUI.TariffFindBox>("TariffFindBox");
			AssertEndsWith("Tariff is to bind to JI_FormattedTariff", "JI_FormattedTariff", control.BindingSource.GetBindingMember(tariffFindBox));
			AssertEquals("TariffFindBox should show description box", true, tariffFindBox.ShowDescriptionBox);
			AssertContainsExactElementsInAnyOrder("Selection Style can be tariff or 6-character nomenclature", new List<SelectionStyle> { SelectionStyle.Tariff, SelectionStyle.Subheading }, tariffFindBox.SelectNomenclatureModes);

			var storageTypeDropEdit = control.FindSingle<ZDropEdit>("StorageTypeDropEdit");
			AssertEndsWith("StorageTypeDropEdit is to bind to JI_StorageType", "JI_StorageType", control.BindingSource.GetBindingMember(storageTypeDropEdit));
		});
	}
}

