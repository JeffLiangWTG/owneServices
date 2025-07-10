using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.ValueProviders
{
	public class MacroValueProviderMap : NonPersistentBusinessObject, IObsoleteValidation
	{
		readonly ZString typeName;
		readonly ZString usage;
		readonly ZString description;

		public MacroValueProviderMap(ZString usage, ZString description)
			: this(ZString.Empty, usage, description)
		{
		}

		public MacroValueProviderMap(ZString typeName, ZString usage, ZString description)
		{
			this.typeName = typeName;
			this.usage = usage;
			this.description = description;
		}

		public ZString TypeName => typeName;
		public ZPropertyInfo TypeNameInfo => GetZPropertyInfo(nameof(TypeName));

		public ZString Usage => usage;
		public ZPropertyInfo UsageInfo => GetZPropertyInfo(nameof(Usage));

		public ZString Description => description;
		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));
	}
}
