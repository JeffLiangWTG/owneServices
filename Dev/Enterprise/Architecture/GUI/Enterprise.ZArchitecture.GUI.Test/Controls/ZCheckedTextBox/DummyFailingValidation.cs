using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class DummyFailingValidation : DummyBizoValidation
	{
		public DummyFailingValidation(DummyWithValidationErrors parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly DummyWithValidationErrors parent;

		public override Type AutoValidationType
		{
			get { return typeof(DummyFailingValidation); }
		}

		public void ValidatePropertyWithErrors()
		{
			ValidateCalculatedProperty(parent.PropertyWithErrorsInfo);
		}

		protected void CheckPropertyWithErrors()
		{
			parent.PropertyWithErrorsInfo.AddError("Error!");
		}

		public override void ValidateAll()
		{
			throw new NotImplementedException();
		}
	}
}
