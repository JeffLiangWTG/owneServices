using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Manifest.Business;

public class ManifestMessageSendingObjectLookups : ZLookups
{
	public ManifestMessageSendingObjectLookups(ManifestMessageSendingObject parent) : base(parent)
	{
	}

	public new ManifestMessageSendingObject Parent => (ManifestMessageSendingObject)base.Parent;

	public CodeDescriptionPairList MessageTypes =>
		Parent.Parent.Messages.Count == 0
		? GetFreshMessageType()
		: Factory.GetCachedValue<ManifestMessageTypeList>();

	CodeDescriptionPairList GetFreshMessageType()
	{
		return Factory.GetCachedValue("IN.Manifest.MessageSendingObject.Fresh", () =>
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(ManifestMessageTypeList.Codes.Fresh, ManifestMessageTypeList.Descriptions.Fresh);
			list.DefaultCode = ManifestMessageTypeList.Codes.Fresh;
			return list;
		});
	}
}
