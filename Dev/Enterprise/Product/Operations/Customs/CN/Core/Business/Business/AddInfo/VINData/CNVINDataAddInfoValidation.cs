//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoVINDataAddInfoValidation
//
//    This class should be used for overriding validation in AutoVINDataAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CN.Business
{
	public class CNVINDataAddInfoValidation : AutoCNVINDataAddInfoValidation
	{
		public CNVINDataAddInfoValidation(AutoCNVINDataAddInfo parent) : base(parent)
		{
		}

		internal IValidationModeProvider ValidationModeProvider => (Parent.Parent as VINData)?.Parent;

		protected override void CheckXC_VIN()
		{
			base.CheckXC_VIN();
			var vin = Parent.XC_VIN;
			if (!vin.IsEmpty && (vin.Length != 17 || !vin.IsLettersAndNumbersOnlyOrEmpty))
			{
				Parent.XC_VINInfo.AddNotification(Res.GetString("1e11630f-89c8-4af9-9b4e-9038692d3869", "VIN Number should be 17 alphanumeric."), ValidationModeProvider);
			}
		}

		protected override void CheckXC_ChassisNo()
		{
			base.CheckXC_ChassisNo();
			var no = Parent.XC_ChassisNo;
			if (!no.IsEmpty && (no.Length != 20 || !no.IsLettersAndNumbersOnlyOrEmpty))
			{
				Parent.XC_ChassisNoInfo.AddNotification(Res.GetString("7ed5ca74-4d51-4f68-a1f7-fb86794545ff", "Chassis Number should be 20 alphanumeric."), ValidationModeProvider);
			}
		}
	}
}
