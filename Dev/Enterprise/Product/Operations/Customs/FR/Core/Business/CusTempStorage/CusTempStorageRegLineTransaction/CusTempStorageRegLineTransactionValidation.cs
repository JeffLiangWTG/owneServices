using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageRegLineTransactionValidation : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionValidation
	{
		public CusTempStorageRegLineTransactionValidation(CusTempStorageRegLineTransaction parent) : base(parent)
		{
		}

		new CusTempStorageRegLineTransaction Parent => (CusTempStorageRegLineTransaction)base.Parent;

		protected override void CheckSRT_GrossWeight()
		{
			base.CheckSRT_GrossWeight();
			var parent = Parent;

			if (parent.SRT_GrossWeight == 0)
			{
				parent.SRT_GrossWeightInfo.AddWarning(Res.GetString("A254FDCA-88D3-4F71-9020-2BDC7D0D50EB", "Gross Weight in KGs should be greater than zero."));
			}
		}

		protected override void CheckSRT_PackageQty()
		{
			base.CheckSRT_PackageQty();

			var parent = Parent;
			MandatoryValidation.MessageErrorIfNotEntered(parent.SRT_PackageQtyInfo);

			var sumPackageQty = parent.RegLine?.CusTempStorageRegLineTransactions.Sum(t => t.SRT_PackageQty) ?? 0;
			if (sumPackageQty < 0)
			{
				parent.SRT_PackageQtyInfo.AddError(Res.GetString("47CA47A0-A5F6-4EB1-8C8A-60E8BF2AD854", "The sum of Package Qty. across all transaction lines should not be negative."));
			}
		}

		protected override void CheckSRT_ReferenceType()
		{
			base.CheckSRT_ReferenceType();
			ListValidation.MessageErrorIfInvalidCode(Parent.SRT_ReferenceTypeInfo);
		}

		protected override void CheckSRT_Reference()
		{
			base.CheckSRT_Reference();

			var parent = Parent;
			if (!parent.SRT_ReferenceType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.SRT_ReferenceInfo);
			}
		}
	}
}
