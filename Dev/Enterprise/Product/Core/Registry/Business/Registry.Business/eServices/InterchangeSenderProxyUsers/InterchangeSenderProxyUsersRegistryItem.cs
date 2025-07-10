using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class InterchangeSenderProxyUsersRegistryItem : TranslatableRegistryItem<InterchangeSenderProxyUserCollection, InterchangeSenderProxyUserCollection>
	{
		public InterchangeSenderProxyUsersRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, InterchangeSenderProxyUserCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new InterchangeSenderProxyUserRegistryDataType(), storage, options, defaultValue))
		{
		}

		#region Implementation

		public override bool IsTranslatable => false;
		public override IEnumerable<ResourceString> DefaultStrings => Enumerable.Empty<ResourceString>();
		public override int MaxLength => 256;
		public override IEnumerable<string> GetCaptions(InterchangeSenderProxyUserCollection value) => Enumerable.Empty<string>();

		#endregion
	}
	[RegistryEditor("Enterprise.Registry.GUI.InterchangeSenderProxyUserRegistryItemEditor, Enterprise.Registry.GUI")]
	public class InterchangeSenderProxyUserRegistryDataType : NonPersistentBusinessObjectCollectionRegistryDataType<InterchangeSenderProxyUserCollection>
	{
	}
}
