using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Module
{
	public class CustomsOfficeFilterValidation : ModuleTextFilterValidation
	{
		public CustomsOfficeFilterValidation(ModuleTextBaseFilter parent) : base(parent)
		{
		}

		public void ValidatePurpose()
		{
			ValidateCalculatedProperty(Parent.PurposeInfo);
		}
		protected virtual void CheckPurpose()
		{
			ListValidation.WarnIfInvalidCode(Parent.PurposeInfo, Parent.PurposeList);
		}

		#region Implementation

		new CustomsOfficeFilter Parent => (CustomsOfficeFilter)base.Parent;

		#endregion
	}
}
