using Enterprise.Integration;
using Enterprise.MailManager.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class Ms365OAuth2TokenRegistryItem : StronglyTypedRegistryItem<Ms365OAuth2Token>
	{
		public Ms365OAuth2TokenRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, EmailType emailType, StringRegistryItem ms365OAuth2TenantId, StringRegistryItem ms365ApplicationIdForIncoming, BooleanRegistryItem useGraphApiForIncoming, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new Ms365OAuth2TokenRegistryDataType(), new Ms365OAuth2TokenRegistryEditorInfo(), storage, options, new Ms365OAuth2Token(), true))
		{
			EmailType = emailType;
			Ms365OAuth2TenantId = ms365OAuth2TenantId;
			Ms365ApplicationIdForIncoming = ms365ApplicationIdForIncoming;
			UseGraphApiForIncoming = useGraphApiForIncoming;
		}

		public EmailType EmailType { get; private set; }
		public StringRegistryItem Ms365OAuth2TenantId { get; private set; }
		public StringRegistryItem Ms365ApplicationIdForIncoming { get; private set; }
		public BooleanRegistryItem UseGraphApiForIncoming { get; private set; }
	}
}
