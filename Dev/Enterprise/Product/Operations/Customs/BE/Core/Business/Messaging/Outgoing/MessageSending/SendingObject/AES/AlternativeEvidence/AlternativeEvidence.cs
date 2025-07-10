using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business;

public class AlternativeEvidence(BusinessObjectFactory factory) : JobDeclarationAlternativeEvidence(factory)
{
	[List(nameof(Lookups) + "." + nameof(AlternativeEvidenceLookups.TransportDocumentTypeList))]
	public override ZString DocType
	{
		get => base.DocType;
		set
		{
			var oldValue = DocType;
			base.DocType = value;
			if (oldValue != DocType && !Reference.IsEmpty && Reference_ReadOnly)
			{
				Reference = ZString.Empty;
			}
		}
	}

	protected override bool Reference_ReadOnly => !(DocTypeRefCusCodeList?.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference) ?? false);

	protected override JobDeclarationAlternativeEvidenceValidation GetNewValidation() => new AlternativeEvidenceValidation(this);

	protected override JobDeclarationAlternativeEvidenceLookups GetNewLookups() => new AlternativeEvidenceLookups(this);
}
