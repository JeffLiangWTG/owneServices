using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business
{
	public class AlternativeEvidence(BusinessObjectFactory factory) : JobDeclarationAlternativeEvidence(factory)
	{
		#region Properties

		[List(nameof(Lookups) + "." + nameof(AlternativeEvidenceLookups.AlternativeEvidenceTypeList))]
		public override ZString EvidenceType
		{
			get => base.EvidenceType;
			set => base.EvidenceType = value;
		}

		[List(nameof(Lookups) + "." + nameof(AlternativeEvidenceLookups.TransportDocumentTypeList))]
		public override ZString DocType
		{
			get => base.DocType;
			set
			{
				base.DocType = value;

				if (!Reference.IsEmpty && Reference_ReadOnly)
				{
					Reference = ZString.Empty;
				}
			}
		}

		protected override bool Reference_ReadOnly => !(DocTypeRefCusCodeList?.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference) ?? false);

		#endregion

		#region Lookups

		protected override JobDeclarationAlternativeEvidenceLookups GetNewLookups() => new AlternativeEvidenceLookups(this);

		#endregion

		protected override JobDeclarationAlternativeEvidenceValidation GetNewValidation() => new AlternativeEvidenceValidation(this);
	}
}
