using System;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class ShapeAffinityValidation : ZValidation
	{
		public ShapeAffinityValidation(ShapeAffinity parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly ShapeAffinity parent;

		public override Type AutoValidationType
		{
			get { return parent.GetType(); }
		}

		public override void ValidateAll()
		{
			ValidateColor();
			ValidateName();
		}

		public void ValidateColor()
		{
			ValidateCalculatedProperty(parent.ColorInfo);
		}

		protected void CheckColor()
		{
			ListValidation.ErrorIfInvalidCode(parent.ColorInfo);
		}

		public void ValidateName()
		{
			ValidateCalculatedProperty(parent.NameInfo);
		}

		protected void CheckName()
		{
			MandatoryValidation.CheckEntered(parent.NameInfo);
		}

		public void ValidateAllowedConcurrency()
		{
			ValidateCalculatedProperty(parent.AllowedConcurrencyInfo);
		}

		protected void CheckAllowedConcurrency()
		{
			CompareValidation.CheckGreaterThanOrEqualTo(parent.AllowedConcurrencyInfo, 1.0m);
		}
	}
}
