using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyBusinessObjectWithCustomProperties : DummyBusinessObject, ICustomFieldProvider
	{
		public DummyBusinessObjectWithCustomProperties(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			DummyCustomProperty prop1 = new DummyCustomProperty("STR", new ZString("String"), typeof(ZString));
			DummyCustomProperty prop2 = new DummyCustomProperty("BOO", new ZBool(true), typeof(ZBool));
			DummyCustomProperty prop3 = new DummyCustomProperty("DAT", new ZDateTime(2020, 1, 1), typeof(ZDateTime));
			DummyCustomProperty prop4 = new DummyCustomProperty("INT", new ZInt(6), typeof(ZInt));

			customBusinessObject = new CustomBusinessObject(this, CustomPropertyCollectionBuilder.GetCustomProperties(new ICustomProperty[] { prop1, prop2, prop3, prop4 }));
		}

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			return customBusinessObject;
		}

		readonly CustomBusinessObject customBusinessObject;
	}
}
