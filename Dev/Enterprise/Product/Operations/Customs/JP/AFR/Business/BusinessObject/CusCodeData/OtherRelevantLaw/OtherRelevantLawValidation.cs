using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class OtherRelevantLawValidation : Customs.Business.CusCodeDataValidation
	{
		const int MaxOtherRelevantLawCount = 5;

		public OtherRelevantLawValidation(OtherRelevantLaw parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
		}

		protected override void CheckCY_Data()
		{
			var targetInfo = Parent.CY_DataInfo;

			if (Parent.CY_Data.IsEmpty)
			{
				targetInfo.AddWarning(ValidationConstants.Bill.EmptyCusCodeDataWillNotBeIncluded);
			}
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
		}

		protected override void CheckCY_Order()
		{
			base.CheckCY_Order();
			if (Parent.CY_Order > MaxOtherRelevantLawCount)
			{
				Parent.CY_OrderInfo.AddWarning(ValidationConstants.Bill.CusCodeExceedingMaximumNumber(Parent.HumanReadableName, MaxOtherRelevantLawCount));
			}
			ValidateCY_Data();
		}

		protected new OtherRelevantLaw Parent
		{
			get { return (OtherRelevantLaw)base.Parent; }
		}
	}
}
