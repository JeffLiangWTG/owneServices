using CargoWise.Types;
using Enterprise.Customs.Common.JP;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing;

[TestedType(typeof(InvoiceLineTemplate))]
sealed class InvoiceLineTemplateTest : BaseInvoiceLineTemplateTest
{
	protected override ZString DeclarationMessageType => JPJobMessageTypeList.Codes.Export;

	protected override ZUserControl GetControlForTest() => new InvoiceLineTemplate();

	public void TestControls()
	{
		using var control = GetControlForTest();
		CombineAssertions(() =>
		{
			var nACCSCodeDropEdit = control.FindSingle<ZDropEdit>("NACCSCodeDropEdit");
			AssertEndsWith("NACCS Code Act is to bind to JI_NACCSCode", "JI_NACCSCode", control.BindingSource.GetBindingMember(nACCSCodeDropEdit));

			var customsSecondQuantityCalcDropEdit = control.FindSingle<ZCalcDropEdit>("CustomsSecondQuantityCalcDropEdit");
			AssertEquals("English Caption", "Custom Qty 2", customsSecondQuantityCalcDropEdit?.CaptionResourceString.Caption);
			AssertEndsWith("Qty is to bind to JI_CustomsSecondQuantity", "JI_CustomsSecondQuantity", customsSecondQuantityCalcDropEdit?.BindToAmount);
			AssertEndsWith("Unit list is to bind to .Lookups+CustomsUQList", "Lookups+CustomsUQList", customsSecondQuantityCalcDropEdit?.BindToList);
			AssertEndsWith("Unit is to bind to JI_CustomsSecondUnitQty", "JI_CustomsSecondUnitQty", customsSecondQuantityCalcDropEdit?.BindToUnit);

			var customsQuantityCalcDropEdit = control.FindSingle<ZCalcDropEdit>("CustomsQuantityCalcDropEdit");
			Assert("Minimum width of CustomsQuantityCalcDropEdit", customsQuantityCalcDropEdit.MinimumSize.Width >= 160);
			Assert(!customsQuantityCalcDropEdit.AllowNegative);

			var entryInstructionGuidDropEdit = control.FindSingle<ZGuidDropEdit>("EntryInstructionGuidDropEdit");
			AssertEndsWith("EntryInstructionGuidDropEdit is to bind to JI_CEI", "JI_CEI", control.BindingSource.GetBindingMember(entryInstructionGuidDropEdit));

			var unitPriceCalcEdit = control.FindSingle<ZCalcEdit>("UnitPriceCalcEdit");
			AssertEndsWith("UnitPriceCalcEdit is to bind to UnitPrice", "UnitPrice", control.BindingSource.GetBindingMember(unitPriceCalcEdit));
			Assert(!unitPriceCalcEdit.AllowNegative);

			var volumeCalcDropEdit = control.FindSingle<ZCalcDropEdit>("VolumeCalcDropEdit");
			AssertEndsWith("Amount is to bind to JI_Volume", "JI_Volume", volumeCalcDropEdit?.BindToAmount);
			AssertEndsWith("Unit list is to bind to VolumeUQList", "Lookups+VolumeUQList", volumeCalcDropEdit?.BindToList);
			AssertEndsWith("Unit is to bind to JI_VolumeUQ", "JI_VolumeUQ", volumeCalcDropEdit?.BindToUnit);
			Assert(!volumeCalcDropEdit.AllowNegative);

			var weightCalcDropEdit = control.FindSingle<ZCalcDropEdit>("WeightCalcDropEdit");
			AssertEndsWith("Amount is to bind to JI_Weight", "JI_Weight", weightCalcDropEdit?.BindToAmount);
			AssertEndsWith("Unit list is to bind to WeightUQList", "Lookups+WeightUQList", weightCalcDropEdit?.BindToList);
			AssertEndsWith("Unit is to bind to JI_WeightUQ", "JI_WeightUQ", weightCalcDropEdit?.BindToUnit);
			Assert(!weightCalcDropEdit.AllowNegative);

			var invoiceQuantityCalcDropEdit = control.FindSingle<ZCalcDropEdit>("InvoiceQuantityCalcDropEdit");
			AssertEndsWith("Amount is to bind to JI_InvoiceQuantity", "JI_InvoiceQuantity", invoiceQuantityCalcDropEdit.BindToAmount);
			AssertEndsWith("Unit is to bind to JI_InvoiceUQ", "JI_InvoiceUQ", invoiceQuantityCalcDropEdit.BindToUnit);
			Assert(!invoiceQuantityCalcDropEdit.AllowNegative);

			var linePriceCurrencyCalcFindBox = control.FindSingle<ZCalcFindBox>("LinePriceCurrencyCalcFindBox");
			AssertEndsWith("Amount is to bind to JI_LinePrice", "JI_LinePrice", linePriceCurrencyCalcFindBox.BindToAmount);
			AssertEndsWith("Unit list is to bind to CurrencyList", "Lookups+CurrencyList", linePriceCurrencyCalcFindBox.BindToList);
			AssertEndsWith("Unit is to bind to JI_RX_NKLinePriceCurr", "JI_RX_NKLinePriceCurr", linePriceCurrencyCalcFindBox.BindToUnit);
			Assert(!linePriceCurrencyCalcFindBox.AllowNegative);
		});
	}
}
