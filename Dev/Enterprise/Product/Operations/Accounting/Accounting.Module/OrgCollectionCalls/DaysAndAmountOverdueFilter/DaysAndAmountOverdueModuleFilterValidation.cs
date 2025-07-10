using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module
{
	public class DaysAndAmountOverdueModuleFilterValidation : ModuleTextFilterValidation
	{
		public DaysAndAmountOverdueModuleFilterValidation(DaysAndAmountOverdueModuleFilter parent)
			: base(parent)
		{
		}

		new DaysAndAmountOverdueModuleFilter Parent
		{
			get { return (DaysAndAmountOverdueModuleFilter)base.Parent; }
		}

		public override Type AutoValidationType
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		#region AmountOverdue

		public void ValidateAmountOverdue()
		{
			ValidateCalculatedProperty(Parent.AmountOverdueInfo);
		}

		protected void CheckAmountOverdue()
		{
			TypeValidation.CheckValidDecimal(Parent.AmountOverdueInfo, 19, 4);
		}

		#endregion

		#region DaysOverdue

		public void ValidateDaysOverdue()
		{
			ValidateCalculatedProperty(Parent.DaysOverdueInfo);
		}

		protected override void CheckProperty()
		{
			base.CheckProperty();
			if (Parent.DaysOverdue < 0)
			{
				Parent.DaysOverdueInfo.AddError(Res.GetString("ed966ead-e3da-49b2-ab73-ee73c8612fde", "Days Overdue should be greater or equal to 0."));
			}
		}

		#endregion

		#region AndOrDecider

		public void ValidateAndOrDecider()
		{
			ValidateCalculatedProperty(Parent.AndOrDeciderInfo);
		}

		protected void CheckAndOrDecider()
		{
			if (!Parent.AndOrDeciderList.ContainsCode(Parent.AndOrDecider))
			{
				Parent.AndOrDeciderInfo.AddError(Res.GetString("d5dc357f-674b-4d08-95ba-af7691ccea25", "Choose a valid code."));
			}
		}

		#endregion

		#region Implementation

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAmountOverdue();
			ValidateAndOrDecider();
			ValidateDaysOverdue();
		}

		#endregion
	}
}
