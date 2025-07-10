using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageRegLineTransactionValidation : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionValidation
	{
		public CusTempStorageRegLineTransactionValidation(CusTempStorageRegLineTransaction parent) : base(parent)
		{
		}

		CusTempStorageRegLineTransaction RegLineTransaction => Parent as CusTempStorageRegLineTransaction;

		protected override void CheckSRT_GrossWeight()
		{
			base.CheckSRT_GrossWeight();

			var transactionType = RegLineTransaction.SRT_TransactionType;
			var isWeightMandatory = transactionType == TransactionTypes.Codes.OpeningBalance
									|| (transactionType == TransactionTypes.Codes.Adjustment && RegLineTransaction.SRT_PackageQty == 0);

			if (isWeightMandatory)
			{
				MandatoryValidation.CheckNotZero(Parent.SRT_GrossWeightInfo);
			}
		}

		protected override void CheckSRT_PackageQty()
		{
			base.CheckSRT_PackageQty();

			var transactionType = RegLineTransaction.SRT_TransactionType;
			var isPackQtyMandatory = transactionType == TransactionTypes.Codes.OpeningBalance
								|| (transactionType == TransactionTypes.Codes.Adjustment && RegLineTransaction.SRT_GrossWeight == 0);

			if (isPackQtyMandatory)
			{
				MandatoryValidation.CheckNotZero(Parent.SRT_PackageQtyInfo);
			}

			if (transactionType == TransactionTypes.Codes.OpeningBalance)
			{
				MandatoryValidation.CheckNotNegative(Parent.SRT_PackageQtyInfo);
			}
			else
			{
				var regLine = RegLineTransaction.RegLine;
				if (regLine is CusTempStorageRegLine cusTempStorageRegLine)
				{
					var sumPackageQty = cusTempStorageRegLine.CalculatePackageQtySumFromTransactions();

					if (sumPackageQty < 0)
					{
						Parent.SRT_PackageQtyInfo.AddError(Res.GetString("134467f3-764d-4f1b-b56d-32124554dd82", "The sum of Package Qty. across all transaction lines should not be negative."));
					}
				}
			}
		}

		protected override void CheckSRT_Reference()
		{
			base.CheckSRT_Reference();

			if (!Parent.SRT_ReferenceType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SRT_ReferenceInfo);
			}
		}

		protected override void CheckSRT_ReferenceType()
		{
			base.CheckSRT_ReferenceType();
			ListValidation.MessageErrorIfInvalidCode(Parent.SRT_ReferenceTypeInfo);
		}
	}
}
