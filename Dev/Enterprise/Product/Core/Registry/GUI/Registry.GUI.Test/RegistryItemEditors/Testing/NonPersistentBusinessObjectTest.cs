using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class NonPersistentBusinessObjectTest : NonPersistentBusinessObject, IObsoleteValidation, IRegistryBusiness
	{
		[CargoWise.ComponentModel.MaxLength(10)]
		public ZString Property
		{
			get { return fProperty; }
			set
			{
				CheckMaximumLength(PropertyInfo, value);
				fProperty = value;
				PropertyInfo.RefreshBinding();
			}
		}
		ZString fProperty;

		public ZPropertyInfo PropertyInfo
		{
			get { return GetZPropertyInfo(nameof(Property)); }
		}

		public Image ImageProperty
		{
			get { return fImageProperty; }
			set
			{
				fImageProperty = value;
				RefreshBinding();
			}
		}
		Image fImageProperty;

		#region IRegistryBusiness Members

		public IRegistryBusiness Clone(FallbackLevel currentFallbackLevel, BusinessObjectFactory factory)
		{
			NonPersistentBusinessObjectTest result = new NonPersistentBusinessObjectTest();
			result.Property = Property;
			result.ImageProperty = ImageProperty;

			return result;
		}

		public FallbackLevel CurrentFallbackLevel
		{
			get { return null; }
			set { }
		}

		#endregion
	}
}
