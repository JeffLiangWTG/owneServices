using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CH.Business;

public class PreviousDocumentLookups : Customs.Business.CusSupportingInfoLookups
{
	public PreviousDocumentLookups(PreviousDocument parent) : base(parent) { }

	protected new PreviousDocument Parent => (PreviousDocument)base.Parent;

	public override ICollection CodeList
	{
		get
		{
			var codeType = ZString.Empty;
			var declaration = Parent?.Parent?.JobDeclaration;
			if (declaration != null)
			{
				codeType = declaration.IsImport ? Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection : Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection;
			}
			return codeType.IsEmpty ?
				new ZZRefCusCodeListCombinedCollection(Factory) :
				ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Switzerland, codeType, Parent.Parent?.EffectiveAssessmentDate ?? ZDateTime.Today);
		}
	}
}
