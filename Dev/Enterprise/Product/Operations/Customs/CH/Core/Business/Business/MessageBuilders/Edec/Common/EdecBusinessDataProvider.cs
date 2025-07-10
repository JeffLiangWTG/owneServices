using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class EdecBusinessDataProvider : IEdecBusiness
{
	protected EdecBusinessDataProvider(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}
	protected readonly CusEntryHeader entryHeader;

	protected JobDeclaration Declaration => entryHeader.Declaration;

	public virtual string CustomsAccount => null;

	public virtual string VATAccount => null;

	protected virtual OrgHeader VATOrganisation => null;

	public string VATNumber => (VATOrganisation?.GetVATNumber()).ReturnDefaultValueIfNullOrEmpty(null);

	public bool? VATSuffix
	{
		get
		{
			bool? vatPrefix = null;

			var vatNumber = VATNumber;
			if (vatNumber != null)
			{
				vatPrefix = VATOrganisation.HasValidNumber(OrgCusCode.CodeTypes.VATCode);
			}

			return vatPrefix;
		}
	}

	public string InvoiceCurrencyType
	{
		get
		{
			string currencyType;

			var invoice = entryHeader.InvoiceHeaders.OrderByDescending(d => d.JZ_InvoiceAmountInLocalCurrency).FirstOrDefault();
			var currency = invoice?.JZ_RX_NKInvoice_Currency ?? ZString.Empty;
			switch (currency)
			{
				case "":
					currencyType = "0";
					break;
				case Core.Constants.CurrencyCodes.Switzerland:
					currencyType = "1";
					break;
				case Core.Constants.CurrencyCodes.EuropeanUnion:
					currencyType = "2";
					break;
				case Core.Constants.CurrencyCodes.BulgariaNew:
				case Core.Constants.CurrencyCodes.CzechRepublic:
				case Core.Constants.CurrencyCodes.Denmark:
				case Core.Constants.CurrencyCodes.Hungary:
				case Core.Constants.CurrencyCodes.Latvia:
				case Core.Constants.CurrencyCodes.Lithuania:
				case Core.Constants.CurrencyCodes.Poland:
				case Core.Constants.CurrencyCodes.RomaniaNew:
				case Core.Constants.CurrencyCodes.Sweden:
					currencyType = "3";
					break;
				case Core.Constants.CurrencyCodes.UnitedStates:
					currencyType = "4";
					break;
				default:
					currencyType = "5";
					break;
			}

			return currencyType;
		}
	}

	public string Incoterms
	{
		get
		{
			var incoterms = Declaration.JE_ShipmentIncoTerm;

			switch (incoterms)
			{
				case Core.Constants.IncoTerms.FreeCarrierSeller:
				case Core.Constants.IncoTerms.FreeCarrierBuyer:
					incoterms = Core.Constants.IncoTerms.FreeCarrier;
					break;
			}

			return incoterms;
		}
	}

	public virtual string CompanyNumberTaxPayer => null;
}
