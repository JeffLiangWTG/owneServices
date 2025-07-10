using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business;

public sealed class ExportEntryMessageSendingActionLookups : BEJobDeclarationMessageSendingObjectLookups<ExportEntryMessageSendingAction>
{
	public ExportEntryMessageSendingActionLookups(ExportEntryMessageSendingAction action) : base(action)
	{
	}

	public CustomsOfficeCodeCollection ExitCustomsOfficeList => CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, Core.Constants.CountryCodes.Belgium, EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExitInland);

	public CodeDescriptionPairList ExitTypeList => new CodeDescriptionPairList();

	public CodeDescriptionPairList SecurityTypeList => Factory.GetCachedValue<BEExportSecurityTypeList>();

	public override CodeDescriptionPairList EntryTypeList
	{
		get
		{
			var variant = Parent.Variant;
			return Factory.GetCachedValue($"419058b4-fbc5-468f-b3bc-6d2db86f6419|EntryTypeList|{variant}", () => BEExportEntryTypeList.GetExportEntryTypeList(variant));
		}
	}
}
