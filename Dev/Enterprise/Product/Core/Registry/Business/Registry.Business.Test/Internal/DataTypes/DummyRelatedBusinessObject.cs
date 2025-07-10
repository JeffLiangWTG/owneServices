using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Registry.Business.Testing
{
	public class DummyRelatedBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		ZString fProperty;
		[MaxLength(100)]
		public ZString Property
		{
			get { return fProperty; }
			set
			{
				CheckMaximumLength(PropertyInfo, value);
				fProperty = value;
				if (!IsValidationSuspended)
				{
					ValidateProperty();
				}
			}
		}

		public ZPropertyInfo PropertyInfo
		{
			get { return GetZPropertyInfo(nameof(Property)); }
		}

		public void ValidateProperty()
		{
			if (Property == "invalid")
			{
				PropertyInfo.AddError("Invalid string entered");
			}
		}
	}
}
