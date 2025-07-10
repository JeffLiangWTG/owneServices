using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business.Testing
{
	class DummyCustomProperty : ICustomProperty
	{
		public DummyCustomProperty(string identifier, object value, Type type)
		{
			this.identifier = identifier;
			this.value = value;
			this.type = type;
		}

		readonly string identifier;
		object value;
		readonly Type type;

		public string Identifier
		{
			get { return identifier; }
		}

		public object GetValue(BusinessObject parent)
		{
			return value;
		}

		public bool TrySetValue(BusinessObject parent, object value)
		{
			this.value = value;
			return true;
		}

		public DynamicBusinessObjectProperty Info
		{
			get { return new DynamicBusinessObjectProperty(type, false, metaData: type == typeof(ZString) ? new DynamicMetaData[] { new MaxLengthImpl(26) } : Array.Empty<DynamicMetaData>()); }
		}

		public void Validate(BusinessObject parent)
		{
		}

		public virtual IEnumerable<ICustomProperty> RelatedProperties { get { return Enumerable.Empty<ICustomProperty>(); } }

		public ICustomColumnDefinition CustomColumnDefinition => null;

		public bool IsDeleted => false;
	}
}
