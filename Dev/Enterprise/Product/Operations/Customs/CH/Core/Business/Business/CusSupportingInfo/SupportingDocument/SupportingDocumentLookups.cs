using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class SupportingDocumentLookups : Customs.Business.CusSupportingInfoLookups
{
	public SupportingDocumentLookups(SupportingDocument parent)
		: base(parent)
	{
	}
	protected new SupportingDocument Parent => (SupportingDocument)base.Parent;

	public override ICollection CodeList
	{
		get
		{
			ZString listType = ZString.Empty;

			var parent = Parent.Parent;
			if (parent?.JobDeclaration != null)
			{
				if (parent.JobDeclaration.IsImport)
				{
					listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
				}
				else if (parent.JobDeclaration.IsExportOrExportDeclarationActivation)
				{
					listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
				}
			}

			return listType.IsEmpty
				? new ZZRefCusCodeListCombinedCollection(Factory)
				: ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Switzerland, listType, parent?.EffectiveAssessmentDate ?? ZDateTime.Today);
		}
	}

	public CodeDescriptionPairList ValidOriginDocumentCodes => RefCusCodeListLoader.GetOriginDocumentCodes(Factory, Parent.Parent?.EffectiveAssessmentDate ?? ZDateTime.Today);
}
