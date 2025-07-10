using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BE.Business;

public class MessageVersionRegistryItem : StronglyTypedRegistryItem<MessageVersionRegistryCollection>
{
	public MessageVersionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, MessageVersionRegistryCollection defaultValue)
		: base(new RegistryItemImpl(name, category, caption, hint, new MessageVersionDataType(), storage, RegistryOptions.Default, defaultValue))
	{
	}

	public ZString GetTargetSystemName(ZString domainCode)
	{
		return domainCode.IsEmpty ? ZString.Empty : Value.Cast<MessageVersionRegistry>().FirstOrDefault(v => v.DomainCode == domainCode)?.TargetSystemName ?? ZString.Empty;
	}
}
