using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business.Testing
{
	class CustomPropertyForTest : ICustomProperty
	{
		public object GetValue(BusinessObject parent)
		{
			return value;
		}

		public bool TrySetValue(BusinessObject parent, object value)
		{
			this.value = value;
			return true;
		}

		object value;

		public void Validate(BusinessObject parent) { }

		public string Identifier { get; set; }

		public DynamicBusinessObjectProperty Info { get; set; } = new DynamicBusinessObjectProperty(typeof(object), true);

		public virtual IEnumerable<ICustomProperty> RelatedProperties { get { return Enumerable.Empty<ICustomProperty>(); } }

		public ICustomColumnDefinition CustomColumnDefinition => null;

		public bool IsDeleted => false;
	}
}
