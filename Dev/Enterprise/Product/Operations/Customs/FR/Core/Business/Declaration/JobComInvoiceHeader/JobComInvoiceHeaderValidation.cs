using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class JobComInvoiceHeaderValidation : EU.Business.Declaration.JobComInvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(JobComInvoiceHeader parent)
			: base(parent)
		{
			currentinvoiceHeader = parent;
		}

		readonly JobComInvoiceHeader currentinvoiceHeader;
		public new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		protected override void CheckJZ_IncoTermPlace()
		{
			base.CheckJZ_IncoTermPlace();

			if (!Parent.JZ_IncoTermPlace.IsEmpty)
			{
				var refPlace = Parent.JZ_IncoTermPlace;
				var invoicesHeaderList = Parent.Entries.Where(x => x.InvoiceHeaders.Contains(currentinvoiceHeader)).FirstOrDefault()?.InvoiceHeaders;
				if (invoicesHeaderList != null)
				{
					foreach (JobComInvoiceHeader invoiceHeader in invoicesHeaderList)
					{
						if (invoiceHeader.JZ_IncoTermPlace != refPlace)
						{
							Parent.JZ_IncoTermPlaceInfo.AddMessageError(Res.GetString("E158C514-B5B7-416C-9046-55A89223803D", "All Agreed places of an Entry must be equal"));
						}
					}
				}
			}
		}

		protected override void CheckJZ_Calc_BalanceCore()
		{
			if (!(ValidationDecider is IInvoiceHeaderValidationDecider decider)
				|| !decider.IsRuleTNAT_078Active
				|| Parent.CusEntryInstructions.Any(x => !x.CEI_SubStyle.EqualsAny(EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF, EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationOfSimplifiedDeclarationsCoveredByCAndF))
				|| !Parent.IsProvisonalAmountAuthorised)
			{
				base.CheckJZ_Calc_BalanceCore();
			}
		}

		protected override void CheckJZ_InvoiceAmount()
		{
			base.CheckJZ_InvoiceAmount();

			var invoice = Parent;
			if (invoice.Validation.ValidationDecider is IInvoiceHeaderValidationDecider validationDecider
				&& validationDecider.IsRuleNAT_240Active
				&& invoice.HasFreeGoods
				&& invoice.JZ_InvoiceAmount > 0)
			{
				invoice.JZ_InvoiceAmountInfo.AddMessageError(Res.GetString("1bbdf20f-57f5-426a-be6a-cd2382beebff", "[NAT_240] Invoice Amount must be equal to 0 EUR when Additional Code 0097 (free goods) is selected in one of the Invoice Lines."));
			}
		}

		protected override bool IsJZ_ValuationCodeUsed => false;

		protected override ZBool RequireJZ_IncoTermPlaceMandatory => !Parent.JZ_IncoTerm.IsEmpty;

		protected override bool IsJZ_ValuationCodeMandatory => false;

		protected override bool IncoTermRequired => true;
	}
}
