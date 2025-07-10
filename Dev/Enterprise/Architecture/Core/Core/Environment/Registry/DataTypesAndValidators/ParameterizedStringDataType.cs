using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class ParameterizedStringDataType : RegistryDataType<ResourceString>
	{
		public ParameterizedStringDataType(ParameterizedStringRegistryItem registryItem, ResourceString defaultValue)
			: base(RegistryDataTypes.Codes.String, defaultValue)
		{
			this.registryItem = registryItem;
		}

		protected override byte[] SerialiseCore(ResourceString value)
		{
			return new StringRegistryDataType().Serialise(value.ToStringWithParameters(Res.DefaultLanguage));
		}

		protected override ResourceString DeserialiseCore(byte[] value)
		{
			return registryItem.Deserialise(new StringRegistryDataType().Deserialise(value));
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new ParameterizedStringRegistryItemEditorInfo();
		}

		readonly ParameterizedStringRegistryItem registryItem;

		public override bool IsDefaultValueImmutable => true;
	}
}
