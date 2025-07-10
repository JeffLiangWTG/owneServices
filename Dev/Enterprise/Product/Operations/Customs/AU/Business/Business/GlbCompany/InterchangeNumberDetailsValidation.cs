using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class InterchangeNumberDetailsValidation : ZValidation
	{
		public InterchangeNumberDetailsValidation(InterchangeNumberDetails parent) : base(parent)
		{
			Parent = parent;
		}

		public override Type AutoValidationType => typeof(InterchangeNumberDetailsValidation);

		InterchangeNumberDetails Parent { get; }

		public override void ValidateAll()
		{
			ValidateNewInterchangeNumber();
		}

		public void ValidateNewInterchangeNumber()
		{
			ValidateCalculatedProperty(Parent.NewInterchangeNumberInfo);
		}

		protected void CheckNewInterchangeNumber()
		{
			if (Parent.NewInterchangeNumber <= Parent.CurrentInterchangeNumber + 100_000)
			{
				Parent.NewInterchangeNumberInfo.AddError(Res.GetString("IncreaseInterchangeNumberValidation|NewInterchangeNumberIsTooSmall", "The new interchange number must be greater than the current interchange number by at least 100,000."));
			}
			else if (Parent.NewInterchangeNumber > Parent.CurrentInterchangeNumber + 2_000_000)
			{
				Parent.NewInterchangeNumberInfo.AddError(Res.GetString("IncreaseInterchangeNumberValidation|NewInterchangeNumberIsTooLarge", "The new interchange number is too large, cannot be 2,000,000 greater than the current interchange number."));
			}
		}
	}
}
