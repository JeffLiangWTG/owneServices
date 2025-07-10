using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[ProvideMetaDataProperty("IsPropertyReadonly", MetaDataTypes.ReadOnly)]
	class CustomBusinessObjectForTest : CustomBusinessObject
	{
		public CustomBusinessObjectForTest(BusinessObjectFactory factory, ICustomPropertyCollection properties)
			: base(factory, null, properties)
		{
		}

		public ICollection<string> ReadOnlyPropertyIdentifiers
		{
			get { return readOnlyPropertyIdentifiers; }
		}

		readonly List<string> readOnlyPropertyIdentifiers = new List<string>();

		protected virtual bool GetIsPropertyReadonly(PropertyDescriptor property)
		{
			return ReadOnlyPropertyIdentifiers.Contains(property.Name)
				|| MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}
	}
}
