using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot("SystemDefinableCodeDescriptionBool")]
	public sealed class SystemDefinableCodeDescriptionBool : CodeDescriptionBoolDefaultReadonly, ICanDelete
	{
		public SystemDefinableCodeDescriptionBool()
		{
		}

		#region Code

		public override ZString Code
		{
			get { return base.Code; }
			set
			{
				bool differentCode = (base.Code != value);
				base.Code = value;
				if (differentCode)
				{
					SystemDefinableCodeDescriptionBoolCollection parentCollection = ParentCollection;
					if (parentCollection != null && parentCollection.DefaultElement == this)
					{
						parentCollection.SetDefaultCode(value, false);
					}
				}
			}
		}

		#endregion

		#region Bool

		public override ZBool Bool
		{
			get
			{
				bool result = false;
				SystemDefinableCodeDescriptionBoolCollection parentCollection = ParentCollection;
				if (parentCollection != null)
				{
					result = (this == parentCollection.DefaultElement);
				}
				return result;
			}
			set
			{
				SystemDefinableCodeDescriptionBoolCollection parentCollection = ParentCollection;
				if (parentCollection != null)
				{
					if (value)
					{
						parentCollection.DefaultElement = this;
					}
					else if (parentCollection.DefaultElement == this)
					{
						parentCollection.SetDefaultCode("", true);
					}
					BoolInfo.RefreshBinding();
				}
			}
		}

		public override ZPropertyInfo BoolInfo
		{
			get => GetZPropertyInfo(nameof(Bool));
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SystemDefinableCodeDescriptionBool();
		}

		#endregion

		#region Implementation

		protected override void WriteMoreElements(XmlWriter writer)
		{
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
		}

		SystemDefinableCodeDescriptionBoolCollection ParentCollection
		{
			get { return (SystemDefinableCodeDescriptionBoolCollection)GetParentCollection(this, typeof(SystemDefinableCodeDescriptionBoolCollection)); }
		}

		#endregion
	}
}
