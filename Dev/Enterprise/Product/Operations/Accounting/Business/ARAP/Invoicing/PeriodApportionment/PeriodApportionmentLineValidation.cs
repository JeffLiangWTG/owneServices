using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class PeriodApportionmentLineValidation : ZValidation
	{
		public PeriodApportionmentLineValidation(PeriodApportionmentLine parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly PeriodApportionmentLine parent;
		public override Type AutoValidationType => GetType();

		public void ValidateOSAmount()
		{
			ValidateCalculatedProperty(parent.OSAmountInfo);
		}

		protected virtual void CheckOSAmount()
		{
			if (parent.OSAmount < 0)
			{
				parent.OSAmountInfo.AddError(Res.GetString("B55464E6-3573-4EF4-B1F9-6769B30AED6C", "Negative apportionments not allowed"));
			}
		}

		public void ValidateOSTaxNotRecoverable()
		{
			ValidateCalculatedProperty(parent.OSTaxNotRecoverableInfo);
		}

		protected virtual void CheckOSTaxNotRecoverable()
		{
			if (parent.OSTaxNotRecoverable < 0)
			{
				parent.OSTaxNotRecoverableInfo.AddError(Res.GetString("8B877DBB-D08A-4E11-8AB5-A2EBA76B3F47", "Negative apportionments of the taxes not allowed"));
			}
		}

		public override void ValidateAll()
		{
			ValidateOSAmount();
			ValidateOSTaxNotRecoverable();
		}
	}
}
