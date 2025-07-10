using System;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	public class DecimalEffectiveDateValidation : ZValidation
	{
		public DecimalEffectiveDateValidation(DecimalEffectiveDate parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly DecimalEffectiveDate parent;

		#region PreviousValue

		public void ValidatePreviousValue()
		{
			ValidateCalculatedProperty(parent.PreviousValueInfo);
		}

		protected virtual void CheckPreviousValue()
		{
		}

		#endregion

		#region NewValue

		public void ValidateNewValue()
		{
			ValidateCalculatedProperty(parent.NewValueInfo);
		}

		protected virtual void CheckNewValue()
		{
		}

		#endregion

		#region EffectiveDate

		public void ValidateEffectiveDate()
		{
			ValidateCalculatedProperty(parent.EffectiveDateInfo);
		}

		protected virtual void CheckEffectiveDate()
		{
		}

		#endregion

		public override void ValidateAll()
		{
			using (((ISingleElementListInternal)parent).SuspendListChanged())
			{
				ValidatePreviousValue();
				ValidateNewValue();
				ValidateEffectiveDate();
			}
		}

		public override Type AutoValidationType => typeof(DecimalEffectiveDateValidation);
	}
}
