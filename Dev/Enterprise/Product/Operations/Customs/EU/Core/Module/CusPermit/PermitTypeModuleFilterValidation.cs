using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Module
{
	public class PermitTypeModuleFilterValidation : Customs.Module.PermitTypeModuleFilterValidation
	{
		public PermitTypeModuleFilterValidation(Customs.Module.PermitTypeModuleFilter parent) : base(parent)
		{
		}

		protected new PermitTypeModuleFilter Parent => (PermitTypeModuleFilter)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateProperty3();
		}

		public void ValidateProperty3()
		{
			ValidateCalculatedProperty(Parent.Property3Info);
		}

		protected virtual void CheckProperty3()
		{
			ListValidation.WarnIfInvalidCode(Parent.Property3Info);
		}
	}
}
