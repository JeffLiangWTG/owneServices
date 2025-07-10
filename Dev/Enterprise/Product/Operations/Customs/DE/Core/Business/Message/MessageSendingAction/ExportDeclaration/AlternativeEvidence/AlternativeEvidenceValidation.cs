using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants.RefCusCodeListAttributes;

namespace Enterprise.Customs.DE.Business
{
	public class AlternativeEvidenceValidation(AlternativeEvidence parent) : JobDeclarationAlternativeEvidenceValidation(parent)
	{
		protected new AlternativeEvidence Parent => (AlternativeEvidence)base.Parent;

		protected override void CheckEvidenceType()
		{
			base.CheckEvidenceType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.EvidenceTypeInfo);
		}

		protected override void CheckDocType()
		{
			base.CheckDocType();
			var docTypeNotMandatoryForTypes = new ZString[] { AlternativeEvidenceTypeList.Codes._16, AlternativeEvidenceTypeList.Codes._30, AlternativeEvidenceTypeList.Codes._31 };
			var targetInfo = Parent.DocTypeInfo;
			if (!Parent.EvidenceType.In(docTypeNotMandatoryForTypes))
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
		}

		protected override void CheckReference()
		{
			var docTypeRefCusCodeList = Parent.DocTypeRefCusCodeList;
			if (docTypeRefCusCodeList != null && docTypeRefCusCodeList.HasAttribute(Name.Reference, Value.Yes))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ReferenceInfo);
			}
		}
	}
}
