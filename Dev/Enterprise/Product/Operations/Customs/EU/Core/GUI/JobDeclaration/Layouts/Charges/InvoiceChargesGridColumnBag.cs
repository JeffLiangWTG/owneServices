using System;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI;

public sealed class InvoiceChargesGridColumnBag
{
	public static InvoiceChargesGridColumnBag Instance => instance ??= new InvoiceChargesGridColumnBag();

	[ThreadStatic]
	static InvoiceChargesGridColumnBag instance;

	InvoiceChargesGridColumnBag()
	{
		AmountInLocalCurrencyCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(nameof(IEUCommonInvoiceCharge.AmountInLocalCurrency), 80, c =>
		{
			c.CaptionResourceString = Res.GetData("7AA6E614-1B25-4859-ABE5-CA992734DD96", "{0} Value").Format(GlbCompany.CurrentCompany.CustomsCurrency.RX_Code);
		});
		AmountCorrectionCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(nameof(IEUCommonInvoiceCharge.AmountCorrection), 100, c =>
		{
			c.CaptionResourceString = Res.GetData("6F9CCCE7-BB08-44B7-9042-7045998C13BD", "Add/Deduct ({0})").Format(GlbCompany.CurrentCompany.CustomsCurrency.RX_Code);
		});
	}

	public IGridColumnReference AmountInLocalCurrencyCalcEditColumn { get; }

	public IGridColumnReference AmountCorrectionCalcEditColumn { get; }
}
