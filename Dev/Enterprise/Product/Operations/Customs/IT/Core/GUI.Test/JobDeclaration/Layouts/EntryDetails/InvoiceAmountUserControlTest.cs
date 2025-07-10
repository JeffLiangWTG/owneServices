using System;
using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class InvoiceAmountUserControlTest : EntryDetailsChildUserControlTest<InvoiceAmountUserControl>
{
	protected override IEnumerable<(string, Type)> ControlNames
	{
		get
		{
			yield return ("InvoiceAmountCalcEdit", typeof(ZCalcEdit));
			yield return ("InvoiceCurrencyTextBox", typeof(ZTextBox));
		}
	}

	public void TestInvoiceAmountCalcEditCaption()
	{
		var sut = Control.InvoiceAmountCalcEdit;
		var labelCaptionVisible = new LabelCaptionRenderProvider().GetLabelCaptionVisible(sut);

		AssertNull("InvoiceAmountCalcEdit caption", sut.CaptionResourceString.Caption);
		AssertEquals("InvoiceAmountCalcEdit caption visible", expected: false, labelCaptionVisible);
	}

	public void TestInvoiceCurrencyTextBoxCaption()
	{
		var sut = Control.InvoiceCurrencyTextBox;
		var labelCaptionVisible = new LabelCaptionRenderProvider().GetLabelCaptionVisible(sut);

		AssertNull("InvoiceCurrencyTextBox caption", sut.CaptionResourceString.Caption);
		AssertEquals("InvoiceCurrencyTextBox caption visible", expected: false, labelCaptionVisible);
	}

	public void TestImplementIExtendedControl()
	{
		AssertEquals("Implements IExtendedControl", expected: true, Control is IExtendedControl);
		AssertEquals("Host", Control, Control.Host);
		AssertNotNull("Extensions", Control.Extensions);
	}
}
