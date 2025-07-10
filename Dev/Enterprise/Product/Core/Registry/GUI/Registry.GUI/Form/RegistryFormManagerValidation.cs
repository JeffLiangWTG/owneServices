using System;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.GUI
{
	class RegistryFormManagerValidation : ZValidation
	{
		public RegistryFormManagerValidation(RegistryFormManager parent) : base(parent)
		{
			this.Parent = parent;
		}

		public override Type AutoValidationType
		{
			get { return null; }
		}

		public override void ValidateAll()
		{
			// Nothing to validate.
		}

		readonly RegistryFormManager Parent;

		#region Properties

		#region Override Default

		public void ValidateOverrideDefault()
		{
			ValidateCalculatedProperty(Parent.OverrideDefaultInfo);
		}

		protected void CheckOverrideDefault()
		{
			if (Parent.ValidationErrorMessage != null)
			{
				Parent.OverrideDefaultInfo.AddError(Parent.ValidationErrorMessage);
			}
		}

		#endregion

		#endregion
	}
}
