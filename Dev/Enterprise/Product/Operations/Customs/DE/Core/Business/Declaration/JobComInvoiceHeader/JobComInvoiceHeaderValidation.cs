using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class JobComInvoiceHeaderValidation : EU.Business.Declaration.JobComInvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(JobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		protected override void CheckJZ_Weight()
		{
			base.CheckJZ_Weight();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JZ_WeightInfo);
		}

		protected override void CheckJZ_WeightUQ()
		{
			base.CheckJZ_WeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_WeightUQInfo);
			if (Parent.JZ_Weight != ZDecimal.Zero)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_WeightUQInfo);
			}
		}

		protected override void CheckJZ_NetWeight()
		{
			base.CheckJZ_NetWeight();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JZ_NetWeightInfo);
		}

		protected override void CheckJZ_NetWeightUQ()
		{
			base.CheckJZ_NetWeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_NetWeightUQInfo);
			if (Parent.JZ_NetWeight != ZDecimal.Zero)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_NetWeightUQInfo);
			}
		}

		protected override void CheckInvoice(ZPropertyInfo amountInfo, ZPropertyInfo currencyInfo)
		{
			base.CheckInvoice(amountInfo, currencyInfo);
			if (currencyInfo != null && !IsExport)
			{
				if (amountInfo.Value.IsEmpty)
				{
					MessageValidation.CheckEntered(currencyInfo, Res.GetString("BDBAE8E0-2FF0-4031-8BDA-BE4BF7F3FD49", "Please enter a Currency"));
				}
			}
		}

		protected override bool IsJZ_ValuationCodeMandatory => false;

		protected override TypeOfValidationForMissingMandatoryChargesForIncoterm ValidationForMissingMandatoryCharges => TypeOfValidationForMissingMandatoryChargesForIncoterm.Warning;
	}
}
