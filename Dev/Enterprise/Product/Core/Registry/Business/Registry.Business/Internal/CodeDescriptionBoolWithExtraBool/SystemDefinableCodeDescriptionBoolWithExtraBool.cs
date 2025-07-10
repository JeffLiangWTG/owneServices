using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot("SystemDefinableCodeDescriptionBoolWithExtraBool")]
	public sealed class SystemDefinableCodeDescriptionBoolWithExtraBool : CodeDescriptionBoolWithExtraBool
	{
		public SystemDefinableCodeDescriptionBoolWithExtraBool()
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
					SystemDefinableCodeDescriptionBoolWithExtraBoolCollection parentCollection = ParentCollection;
					if (parentCollection != null && parentCollection.DefaultElement == this)
					{
						parentCollection.SetDefaultCode(value, false);
					}
				}
			}
		}

		#endregion

		protected override int MaxDescriptionLength
		{
			get { return 35; }
		}

		#region Bool

		public override ZBool Bool
		{
			get
			{
				bool result = false;
				SystemDefinableCodeDescriptionBoolWithExtraBoolCollection parentCollection = ParentCollection;
				if (parentCollection != null)
				{
					result = (this == parentCollection.DefaultElement);
				}
				return result;
			}
			set
			{
				SystemDefinableCodeDescriptionBoolWithExtraBoolCollection parentCollection = ParentCollection;
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
			return new SystemDefinableCodeDescriptionBoolWithExtraBool();
		}

		#endregion

		#region Implementation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			SystemDefined = bool.Parse(reader.ReadElementString(Schema.SystemDefined));
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.SystemDefined, SystemDefined.ToString());
		}

		protected override bool UseDefaultSystemDefinedHandlingForXml
		{
			get { return false; }
		}

		SystemDefinableCodeDescriptionBoolWithExtraBoolCollection ParentCollection
		{
			get { return (SystemDefinableCodeDescriptionBoolWithExtraBoolCollection)GetParentCollection(this, typeof(SystemDefinableCodeDescriptionBoolWithExtraBoolCollection)); }
		}

		#endregion
	}
}
