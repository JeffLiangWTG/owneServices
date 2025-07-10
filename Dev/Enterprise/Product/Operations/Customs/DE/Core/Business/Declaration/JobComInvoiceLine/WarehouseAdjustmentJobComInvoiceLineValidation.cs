using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class WarehouseAdjustmentJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public WarehouseAdjustmentJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckOutwardMRN()
		{
			base.CheckOutwardMRN();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.OutwardMRNInfo);
		}

		protected override void CheckOutwardDecisiveDate()
		{
			base.CheckOutwardDecisiveDate();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.OutwardDecisiveDateInfo);
			if (Parent.OutwardDecisiveDate.Date > ZDate.Today)
			{
				Parent.OutwardDecisiveDateInfo.AddMessageError(Res.GetString("925DE624-D7A8-4B59-A21A-47318CB371BA", "Decisive Date cannot be greater than current date."));
			}
		}

		protected override void CheckJI_Procedure()
		{
			CheckJI_ProcedureForImportAndWarehouseAdjustment();
		}

		protected override void CheckAdditionalProcedureCodesAsString()
		{
		}

		protected override void CheckJI_Tariff()
		{
		}

		protected override void CheckJI_Description()
		{
		}

		protected override void CheckJI_CountryOfOrigin()
		{
		}

		protected override void CheckJI_PrimaryPreference()
		{
		}

		protected override void CheckJI_LinePrice()
		{
		}

		protected override void CheckJI_PartNo()
		{
		}

		protected override void CheckJI_RH_NKCommodity_Code()
		{
		}

		protected override void CheckJI_ConcessionOrder()
		{
		}

		protected override void CheckJI_Weight()
		{
		}

		protected override void CheckJI_WeightUQ()
		{
		}

		protected override void CheckJI_NetWeight()
		{
		}

		protected override void CheckJI_NetWeightUQ()
		{
		}

		protected override void CheckJI_CustomsQuantity()
		{
		}

		protected override void CheckJI_CustomsUnitQty()
		{
		}

		protected override void CheckJI_CustomsSecondQuantity()
		{
		}

		protected override void CheckJI_CustomsSecondUnitQty()
		{
		}

		protected override void CheckJI_CustomsThirdQuantity()
		{
		}

		protected override void CheckJI_CustomsThirdUnitQty()
		{
		}

		protected override void CheckJI_BondedWhsQuantity()
		{
			var parent = Parent;
			var targetInfo = parent.JI_BondedWhsQuantityInfo;
			var propertyDescription = parent.BondedWhsQuantityAdjustmentResourceDataString.Caption;

			MandatoryValidation.MessageErrorIfNotEntered(targetInfo, propertyDescription);

			if (!IsNegativeQuantityAllowed(parent.Factory, parent.JI_Procedure.Left(2)))
			{
				MandatoryValidation.MessageErrorIfIsNegative(targetInfo, propertyDescription);
			}

			if (!parent.JI_BondedWhsQuantity.IsInteger && parent.Factory.IsIntegerRequiredUnitOfQuantity(parent.JI_BondedWhsUnitQty))
			{
				targetInfo.AddMessageError(Res.GetString("8EB6C43C-BA9D-41B2-B2C5-AA559AA55CB0", "{0} must be integer.", propertyDescription));
			}
		}

		protected override void CheckJI_BondedWhsUnitQty()
		{
			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.JI_BondedWhsUnitQtyInfo, parent.BondedWhsUnitQuantityAdjustmentResourceDataString.Caption);
		}

		protected override void CheckJI_PreviousEntryNumber()
		{
			base.CheckJI_PreviousEntryNumber();

			var parent = Parent;
			var previousEntryNumber = parent.JI_PreviousEntryNumber;
			var targetInfo = parent.JI_PreviousEntryNumberInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

			var formatErrorDescription = RegistrationNumberValidationHelper.ValidateRegistrationNumberLengthAndMrnFormat(previousEntryNumber, parent.Factory);
			if (!string.IsNullOrWhiteSpace(formatErrorDescription))
			{
				targetInfo.AddMessageError(formatErrorDescription);
			}
		}

		protected override void CheckJI_PreviousEntryLineNumber()
		{
			base.CheckJI_PreviousEntryLineNumber();

			var parent = Parent;
			if (parent.JI_PreviousEntryLineNumber.IsEmpty)
			{
				parent.JI_PreviousEntryLineNumberInfo.AddMessageError(Res.GetString("DE37EB21-9255-4903-B22E-023EEA52A400", "Inbound Reg. Pos. cannot be zero."));
			}
		}

		static bool IsNegativeQuantityAllowed(BusinessObjectFactory factory, ZString procedureCode) => NegativeQuantityAllowedProcedureCodes(factory).Contains(procedureCode);

		static ImmutableHashSet<string> NegativeQuantityAllowedProcedureCodes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("B4EA80C7-B3BB-4CE4-8CF7-7496E42AA7F9", () => ImmutableHashSet.Create(
				CustomsProcedureCodeList.Import.ProcedureCode._14,
				CustomsProcedureCodeList.Import.ProcedureCode._15,
				CustomsProcedureCodeList.Import.ProcedureCode._16,
				CustomsProcedureCodeList.Import.ProcedureCode._17
			));
		}
	}
}
