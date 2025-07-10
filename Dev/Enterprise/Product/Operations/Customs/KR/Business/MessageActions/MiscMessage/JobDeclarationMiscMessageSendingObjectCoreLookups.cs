using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationMiscMessageSendingObjectCoreLookups : ZLookups
	{
		public JobDeclarationMiscMessageSendingObjectCoreLookups(JobDeclarationMiscMessageSendingObjectCore parent)
			: base(parent)
		{
		}

		new JobDeclarationMiscMessageSendingObjectCore Parent => (JobDeclarationMiscMessageSendingObjectCore)base.Parent;

		public CodeDescriptionPairList AmendmentReasonCodeList
		{
			get
			{
				CodeDescriptionPairList result = null;
				switch (Parent.MessageType)
				{
					case ElectronicDocumentTypeList.Codes._5AS:
						result = Factory.GetCachedValue<ExportAmendmentReasonCodeList>();
						break;
					case ElectronicDocumentTypeList.Codes._DKJ:
						result = Factory.GetCachedValue<ExportDeclarationwithdrawReasonCodeList>();
						break;
					case ElectronicDocumentTypeList.Codes._5DR:
					case ElectronicDocumentTypeList.Codes._5DS:
						result = Factory.GetCachedValue<LocalExportAmendmentReasonCodeList>();
						break;
					case ElectronicDocumentTypeList.Codes._5FE:
						result = Factory.GetCachedValue<ImportDeclarationModifyReasonCodeList>();
						break;
					default:
						result = new CodeDescriptionPairList();
						break;
				}
				return result;
			}
		}

		public CodeDescriptionPairList FaultPartyList
		{
			get
			{
				CodeDescriptionPairList result = null;
				switch (Parent.MessageType)
				{
					case ElectronicDocumentTypeList.Codes._5AS:
					case ElectronicDocumentTypeList.Codes._DKJ:
						result = Factory.GetCachedValue<ExportImputationReasonCodeList>();
						break;
					case ElectronicDocumentTypeList.Codes._5FE:
						result = Factory.GetCachedValue<ImputationReasonCodeList>();
						break;
					default:
						result = new CodeDescriptionPairList();
						break;
				}
				return result;
			}
		}

		public CodeDescriptionPairList PenaltyExemptionReasonCodeList => Factory.GetCachedValue<PenaltyExemptionReasonCodeList>();
		public CodeDescriptionPairList PenaltyExemption5UAOnlyCodeList => Factory.GetCachedValue("PenaltyExemption5UAOnlyCodeList", () => Messaging.PenaltyExemptionReasonCodeList.GetPenaltyExemption5UAOnlyCodeList());
	}
}
