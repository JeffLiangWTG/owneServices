using System;

namespace CargoWise.EntityFramework.Testing
{
	public class DummyNonPersistentBusinessObjectValidation : ZValidation
	{
		public DummyNonPersistentBusinessObjectValidation(BusinessObject parent)
			: base(parent)
		{
		}

		public override Type AutoValidationType
		{
			get { return typeof(DummyNonPersistentBusinessObjectValidation); }
		}

		public override void ValidateAll()
		{
		}
	}
}
