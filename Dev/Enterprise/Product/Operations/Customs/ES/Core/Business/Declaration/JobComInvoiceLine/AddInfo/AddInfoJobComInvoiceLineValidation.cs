using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class AddInfoJobComInvoiceLineValidation : EU.Business.Declaration.AddInfoJobComInvoiceLineValidation
	{
		public AddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}
		public new AddInfoJobComInvoiceLine Parent => (AddInfoJobComInvoiceLine)base.Parent;

		protected JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)Parent.Parent;

		protected override void CheckZG_T2LItemNumber()
		{
			base.CheckZG_T2LItemNumber();

			var entryInstruction = InvoiceLine.EntryInstruction;
			if (entryInstruction != null && entryInstruction.IsT2C && InvoiceLine.IsImport)
			{
				var t2LItemNumber = Parent.ZG_T2LItemNumber;
				if (t2LItemNumber <= 0 || t2LItemNumber > 999)
				{
					Parent.ZG_T2LItemNumberInfo.AddMessageError(Res.GetString("4FEC1423-A19A-4276-96D6-C9685920A4A7", "T2L Item number must be from 1 to 999"));
				}
			}
		}

		protected override void CheckZG_MethodOfPayment2()
		{
			base.CheckZG_MethodOfPayment2();
			if (InvoiceLine.DestinationStateIsCanaryIsland)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ZG_MethodOfPayment2Info);
			}
		}

		protected override void CheckZG_ExciseExemption()
		{
			base.CheckZG_ExciseExemption();
			if (InvoiceLine.IsImport)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ZG_ExciseExemptionInfo);

				if (!(Parent.ZG_ExciseExemption.IsEmpty || Parent.ZG_ExciseExemption == ExciseExemptionList.Codes.NoExemption) && Parent.ZG_ExciseCode.IsEmpty)
				{
					Parent.ZG_ExciseExemptionInfo.AddWarning(Res.GetString("32E8F2B6-6ECC-45CD-B601-79D03F1BED81", "There is no Excise Code to be exempted from."));
				}
			}
		}

		protected override void CheckZG_REAProductCode()
		{
			base.CheckZG_REAProductCode();
			if (!InvoiceLine.ZG_REAProductCode.IsEmpty && InvoiceLine.DestinationStateIsCanaryIsland)
			{
				//ListValidation.MessageErrorIfInvalidCode(Parent.ZG_REAProductCodeInfo); We haven't a list yet, it will be added later
				if (InvoiceLine.JI_PrimaryPreference != REAProductPreference)
				{
					Parent.ZG_REAProductCodeInfo.AddMessageError(Res.GetString("4AF9A278-32A8-4D90-AB04-C2027E74810E", "REA Product Code should only be declared when Preference = 085"));
				}
			}
		}
		const string REAProductPreference = "085";

		protected override void CheckZG_AIEMType()
		{
			base.CheckZG_AIEMType();
			if (InvoiceLine.DestinationStateIsCanaryIsland)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ZG_AIEMTypeInfo);
			}
		}

		protected override void CheckZG_HasNonRecycledPlastics()
		{
			base.CheckZG_HasNonRecycledPlastics();

			if (InvoiceLine.IsImport)
			{
				ValidateProcedureForNonRecycledPlastic();
			}
		}

		void ValidateProcedureForNonRecycledPlastic()
		{
			var procedure = InvoiceLine.JI_Procedure;

			if (!Parent.ZG_HasNonRecycledPlastics || procedure.IsEmpty)
			{
				return;
			}

			var isProcedureCodeAllowed = Regex.IsMatch(procedure, "^(40|42|61|63|44|49|07)");
			if (!isProcedureCodeAllowed)
			{
				Parent.ZG_HasNonRecycledPlasticsInfo.AddWarning(NonRecycledPlasticFeeNotAllowedForProcedureCode);
			}
		}

		internal static string NonRecycledPlasticFeeNotAllowedForProcedureCode => Res.GetString("022AD3E0-77B1-4127-85AA-C57DED413B0D", "1PL tax (non-recycled plastics) only applies to 40, 42, 61, 63, 44, 49 and 07 Procedure Code [37] ");
	}
}
