using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Utility.Testing
{
	public static class TransactionLineTestHelper
	{
		public static void SinchronizeChargeAndWipAccrualAmounts(TransactionLine line)
		{
			if (line.AL_AG.IsEmpty)
			{
				line.AL_AG = line.Factory.NewWithValidTestData<AccGLHeader>().PK;
			}
			if (line.RelatedJobCharge != null)
			{
				if (line.AL_LineType == TransactionLineTypes.WIP)
				{
					line.RelatedJobCharge.JR_LocalSellAmt = line.AL_LocalExTaxAmount;
					if (line.RelatedJobCharge.JR_OSSellExRate == 1)
					{
						line.RelatedJobCharge.JR_OSSellAmt = line.RelatedJobCharge.JR_LocalSellAmt;
					}
				}
				if (line.AL_LineType == TransactionLineTypes.Accrual)
				{
					line.RelatedJobCharge.JR_LocalCostAmt = line.AL_LocalExTaxAmount;
					if (line.RelatedJobCharge.JR_OSCostExRate == 1)
					{
						line.RelatedJobCharge.JR_OSCostAmt = line.RelatedJobCharge.JR_LocalCostAmt;
					}
				}
			}
		}
	}
}
