using Enterprise.Customs.CH.Business;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.DeclarationActivation.Business;

public class DeclarationActivationReportLookups(DeclarationActivationReport parent) : CusExitReportLookups(parent)
{
	new DeclarationActivationReport Parent => (DeclarationActivationReport)base.Parent;

	public CodeDescriptionPairList ActivationTypeList => CommonLookups.ActivationTypeList(Factory);

	public CodeDescriptionPairList CommunicationLanguageList => CommonLookups.CommunicationLanguageList(Factory); 

	public CodeDescriptionPairList TransportModeList => CommonLookups.TransportTypeList(Factory);

	public CodeDescriptionPairList TransportTypeList => CommonLookups.TransportModeList(Factory);

	public CodeDescriptionPairList DeclarationTimeCodeList => CommonLookups.DeclarationTimeCodeList(Parent);

	public CodeDescriptionPairList MessageStatusList => CommonLookups.MessageStatusList(Factory);

	public CodeDescriptionPairList CustomsStatusList => CommonLookups.CustomsStatusList(Factory);

	public ZZRefCusCodeListCombinedCollection CustomsOfficeList => CommonLookups.CustomsOfficeList(Parent);

	public CodeDescriptionPairList AuthorizationsList => CommonLookups.ExportAuthorizationsList(Parent, Parent.IsEdecActivation, GlbCompany.CurrentCompany.GC_OH_OrgProxy);

	public CodeDescriptionPairList NextProcedureList => CommonLookups.NextProcedureList(Parent);
}
