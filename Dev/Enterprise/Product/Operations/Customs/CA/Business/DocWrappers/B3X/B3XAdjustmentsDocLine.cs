using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class B3XAdjustmentsDocLine : AdjustmentsDocLine
	{
		public B3XAdjustmentsDocLine(bool isEmpty = false) : base(isEmpty)
		{
		}

		protected override AdjustmentsDocLine CreateNewLine()
		{
			return new B3XAdjustmentsDocLine();
		}

		protected override AdjustmentsDocLine GetFirstLine(JobComInvoiceLine invoiceLine, bool isAccounted = false)
		{
			var result = base.GetFirstLine(invoiceLine, isAccounted);
			if (isAccounted)
			{
				result.CustomsDuties *= -1;
				result.ValueForDuty *= -1;
				result.SIMAAssessment *= -1;
				result.ExciseTax *= -1;
				result.ValueForTax *= -1;
				result.GST *= -1;
			}
			return result;
		}

		protected override AdjustmentsDocLine GetSubsequentContinualDutyLine(DutyAndTax duty, bool isAccounted = false)
		{
			var result = base.GetSubsequentContinualDutyLine(duty, isAccounted);
			if (isAccounted)
			{
				result.CustomsDuties *= -1;
			}
			return result;
		}

		protected override ZString GetTariff(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.JI_Tariff;
		}
	}
}
