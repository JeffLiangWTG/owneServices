using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraString : CodeDescriptionBoolDisallowNewCodeReadOnly
	{
		public ZString String1
		{
			get => string1;
			set
			{
				if (string1 != value)
				{
					SetNonPersistentPropertyValue(String1Info, ref string1, value);
				}
			}
		}

		ZString string1;

		public virtual ZPropertyInfo String1Info
		{
			get => GetZPropertyInfo(nameof(String1));
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraString
		{
			String1 = String1,
			IsCodeReadOnly = IsCodeReadOnly,
			CodeMaxLength = CodeMaxLength,
			SystemDefined = SystemDefined,
			Description = Description,
			Bool = Bool
		};
	}
}
