using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationProductDetailsProvider : IDeclarationProductDetails
	{
		public DeclarationProductDetailsProvider(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		public static DeclarationProductDetailsProvider New(JobComInvoiceLine invoiceLine) => invoiceLine == null ? null : new DeclarationProductDetailsProvider(invoiceLine);

		readonly JobComInvoiceLine invoiceLine;

		public string ProductDescription => invoiceLine.FullGoodsDescription;

		public string InvoiceQuantityUQDescription => invoiceLine.InvoiceUQDescInPortugueseBrazil;

		public decimal InvoiceQuantity => invoiceLine.JI_InvoiceQuantity;

		public decimal ProductValueAmount
		{
			get
			{
				var result = decimal.Zero;
				if (invoiceLine.JI_InvoiceQuantity > 0)
				{
					result = Utilities.Round(invoiceLine.JI_Calc_FOB / invoiceLine.JI_InvoiceQuantity, 2);
				}
				return result;
			}
		}

		public decimal CustomsValueAmount
		{
			get
			{
				var result = decimal.Zero;
				if (invoiceLine.JI_InvoiceQuantity > 0)
				{
					result = Utilities.Round(invoiceLine.JI_Calc_InvAmount / invoiceLine.JI_InvoiceQuantity, 7);
				}
				return result;
			}
		}
	}
}
