using System.Linq;
using CargoWise.ComponentModel;

namespace Enterprise.Customs.CN.Business
{
	public class EnterpriseQualificationValidation : Customs.Business.CusCodeDataValidation
	{
		public EnterpriseQualificationValidation(EnterpriseQualification parent)
			: base(parent)
		{
		}

		new EnterpriseQualification Parent => (EnterpriseQualification)base.Parent;

		internal IValidationModeProvider ValidationModeProvider => (Parent.Parent as CusEntryInstruction)?.JobDeclaration;

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			Parent.CY_DataInfo.AddNotificationIfNotEntered(ValidationModeProvider);

			if (!Parent.CY_Code.IsEmpty && !Parent.CY_Data.IsEmpty)
			{
				var instruction = Parent.Parent as CusEntryInstruction;
				if (instruction != null && instruction.EnterpriseQualifications.Cast<EnterpriseQualification>().Count(x => x.CY_Code == Parent.CY_Code && x.CY_Data == Parent.CY_Data) > 1)
				{
					Parent.CY_DataInfo.AddNotification(Res.GetString("188F6ED2-1EF4-451F-8755-EA1A4C0ABE69", "The type and number are duplicated."), ValidationModeProvider);
				}
			}
		}
	}
}
