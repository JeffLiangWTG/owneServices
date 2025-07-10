using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ImportAddInfoJobComInvoiceLineValidation : AddInfoJobComInvoiceLineValidation
	{
		public ImportAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckZG_CessionFlag()
		{
			base.CheckZG_CessionFlag();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_CessionFlagInfo);
		}

		static readonly ImmutableArray<string> ConcessionsThatDoNotRequireNetPrice =
			ImmutableArray.Create(
				CustomsProcedureCodeList.Import.Concession._E01,
				CustomsProcedureCodeList.Import.Concession._E02
			);

		protected override void CheckZG_NetPrice()
		{
			base.CheckZG_NetPrice();

			var parent = Parent;
			var invoiceLine = InvoiceLine;
			var propertyInfo = parent.ZG_NetPriceInfo;
			var netPrice = parent.ZG_NetPrice;
			MandatoryValidation.MessageErrorIfIsNegative(propertyInfo);

			if (netPrice > invoiceLine.JI_LinePrice)
			{
				propertyInfo.AddMessageError(Res.GetString("02C60189-E68B-4708-AF44-6974F1EB6FDC", "Net. Price must be less or equal [42] Price."));
			}

			if (invoiceLine.InvoiceHeader.IsHighValueOvrd)
			{
				if (!ConcessionsThatDoNotRequireNetPrice.Contains(invoiceLine.Concession))
				{
					MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
				}

				var discountCharge = invoiceLine.Charges.Cast<InvoiceLineCharge>().FirstOrDefault(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.Discount);
				var discountChargeValue = discountCharge != null ? discountCharge.J7_Amount : 0;
				if (invoiceLine.JI_LinePrice != netPrice + discountChargeValue)
				{
					propertyInfo.AddWarning(Res.GetString("81980824-8267-461E-B983-95907BA247DD", $"The sum of Net Price and Discount Value (Charges Code '{Common.CustomsChargeTypeList.Codes.Discount}') is not equal to Price."));
				}
			}
		}

		protected override void CheckZG_EconomicConditions()
		{
			base.CheckZG_EconomicConditions();
			var line = InvoiceLine;
			var instruction = line.EntryInstruction;
			if (instruction != null && instruction.EnabledInwardProcessing)
			{
				if (instruction.CEI_SimplifiedGrantAuthorization == SimplifiedGrantAuthorizationList.Codes.J)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_EconomicConditionsInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.ZG_EconomicConditionsInfo);
			}
		}

		protected override void CheckZG_IdentificationMeansType()
		{
			base.CheckZG_IdentificationMeansType();
			var line = InvoiceLine;
			var instruction = line.EntryInstruction;
			if (instruction != null && instruction.EnabledInwardProcessing)
			{
				if (instruction.CEI_SimplifiedGrantAuthorization == SimplifiedGrantAuthorizationList.Codes.J)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_IdentificationMeansTypeInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.ZG_IdentificationMeansTypeInfo);
			}
		}

		protected override void CheckZG_QuotaQty()
		{
			base.CheckZG_QuotaQty();
			if (!InvoiceLine.JI_ConcessionOrder.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(InvoiceLine.ZG_QuotaQtyInfo);
			}
		}

		protected override void CheckZG_QuotaUQ()
		{
			base.CheckZG_QuotaUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_QuotaUQInfo);
			if (Parent.ZG_QuotaQty > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_QuotaUQInfo);
			}
		}
	}
}
