using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class IMDJobComInvoiceLineValidation : CMRImportJobComInvoiceLineValidation
	{
		public IMDJobComInvoiceLineValidation(JobComInvoiceLine line)
			: base(line)
		{
		}

		public override CodeDescriptionPairList LineValuationBasisList
		{
			get { return Parent.AddInfo.Lookups.ValuationBasisListForCMR; }
		}

		public override CodeDescriptionPairList InstrumentTypeList
		{
			get { return Parent.AddInfo.Lookups.ZA_InstrumentType_List; }
		}

		public override CodeDescriptionPairList InstrumentCodeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var instruments = new CMRInstrumentCollection(Parent.Factory);

				var effectiveDate = Parent.AddInfo.EffectiveDutyDate.IsValid ? Parent.AddInfo.EffectiveDutyDate : ZDateTime.Today;

				var endDateFilter = new ZQuery(CMRInstrumentSchema.IN_EndDate, null);
				endDateFilter.AddToFilter(JoinCondition.Or, CMRInstrumentSchema.IN_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);

				var revocationDateFilter = new ZQuery(CMRInstrumentSchema.IN_RevocationDate, null);
				revocationDateFilter.AddToFilter(JoinCondition.Or, CMRInstrumentSchema.IN_RevocationDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);

				var filter = new ZQuery(CMRInstrumentSchema.IN_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
				filter.AddToFilter(CMRInstrumentSchema.IN_Type, Parent.AddInfo.ZA_InstrumentType_Hidden);
				filter.AddToFilter(endDateFilter);
				filter.AddToFilter(revocationDateFilter);

				instruments.LoadWithMoreFiltering(new ZQuery(filter));

				foreach (CMRInstrument currentInstrument in instruments)
				{
					result.AddPair(currentInstrument.IN_Number);
				}

				return result;
			}
		}

		protected override int AllowedDescriptionLength
		{
			get { return 250; }
		}

		protected override void CheckJI_OP()
		{
			base.CheckJI_OP();
			Parent.AddInfo.Validation.ValidateZA_WRL();
		}

		protected override void CheckJI_CountryOfOrigin()
		{
			if (Parent.AggregatedZA_ORG.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CountryOfOriginInfo);
			}
			else
			{
				base.CheckJI_CountryOfOrigin();
			}
		}

		protected override void CheckJI_CustomsQuantity()
		{
			if (!Parent.JI_CustomsQuantityInfo.ReadOnly)
			{
				base.CheckJI_CustomsQuantity();
				if (Parent.AddInfo != null && Parent.AddInfo.ZA_LCTI == "Y" && Parent.JI_CustomsQuantity != 1)
				{
					Parent.JI_CustomsQuantityInfo.AddMessageError("Customs Quantity must be 1 when Luxury Car Tax Payable (LCTI) is set to \"Y\".");
				}
				if (Parent.JI_CustomsQuantity > 9999999999.99999m)
				{
					Parent.JI_CustomsQuantityInfo.AddMessageError("Customs Quantity must not be > 9,999,999,999.99999");
				}
			}
			Parent.AddInfo.Validation.ValidateZA_WRL();
		}

		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();
			Parent.AddInfo.Validation.ValidateZA_WRL();
		}

		protected override void CheckJI_OH_Supplier()
		{
			base.CheckJI_OH_Supplier();

			if (Parent.Declaration.IsExWarehouse && (Parent.Supplier?.GetCustomsClientID().IsEmpty ?? false))
			{
				Parent.JI_OH_SupplierInfo.AddWarning(SupplierRequiresCIDWarning);
			}
		}

		internal const string SupplierRequiresCIDWarning = "The Supplier must have a CID code.";
	}
}
