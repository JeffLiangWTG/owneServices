using System;

namespace CargoWise.EntityFramework.Testing
{
	internal class ConcreteZValidation : ZValidation
	{
		public ConcreteZValidation()
			: this(new NonPersistentDummy())
		{
		}

		public ConcreteZValidation(BusinessObject businessObject)
			: base(businessObject)
		{
		}

		public override Type AutoValidationType
		{
			get { return null; }
		}

		public override void ValidateAll()
		{
		}
	}
}
