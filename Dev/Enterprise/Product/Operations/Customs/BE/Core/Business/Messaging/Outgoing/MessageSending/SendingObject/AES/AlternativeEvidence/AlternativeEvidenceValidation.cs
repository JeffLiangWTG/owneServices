using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.BE.Business.UniversalReferenceConstants.RefCusCodeListAttributes;

namespace Enterprise.Customs.BE.Business;

public class AlternativeEvidenceValidation(AlternativeEvidence parent) : JobDeclarationAlternativeEvidenceValidation(parent)
{
	protected new AlternativeEvidence Parent => (AlternativeEvidence)base.Parent;

	protected override void CheckDocType()
	{
		base.CheckDocType();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.DocTypeInfo);
	}

	protected override void CheckReference()
	{
		base.CheckReference();
		var docTypeRefCusCodeList = Parent.DocTypeRefCusCodeList;
		if (docTypeRefCusCodeList != null && docTypeRefCusCodeList.HasAttribute(Name.Reference, Value.Yes))
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ReferenceInfo);
		}
	}
}
