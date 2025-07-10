using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AE.Manifest.Business;

public class MessageChooserItemLookups : ZLookups
{
	public MessageChooserItemLookups(MessageChooserItem parent) : base(parent)
	{
	}

	public CodeDescriptionPairList EntryTypeList => new EntryTypes();

	public CodeDescriptionPairList SubjectCodeList => new SubjectCodes();
}
