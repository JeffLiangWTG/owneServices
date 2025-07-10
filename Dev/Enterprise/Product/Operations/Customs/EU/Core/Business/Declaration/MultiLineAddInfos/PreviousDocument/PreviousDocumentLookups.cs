using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class PreviousDocumentLookups : Customs.Business.CusSupportingInfoLookups
	{
		public PreviousDocumentLookups(PreviousDocument parent)
			: base(parent)
		{
		}

		protected new PreviousDocument Parent => (PreviousDocument)base.Parent;

		public virtual ICollection ReferenceList => new CodeDescriptionPairList();

		public override CodeDescriptionPairList SubTypeList => Factory.GetCachedValue<PreviousDocumentClassList>();

		public override ICollection CodeList
		{
			get
			{
				var codeType = ZString.Empty;
				var countryCode = ZString.Empty;
				if (Parent.ImportExportParent is ICanBeImportOrExport canBeImportOrExport)
				{
					countryCode = canBeImportOrExport.DataGroupingCode;
					if (canBeImportOrExport.IsImport)
					{
						codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection;
					}
					else if (canBeImportOrExport.IsExport)
					{
						codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection;
					}
				}
				return codeType.IsEmpty || countryCode.IsEmpty ?
					new CodeDescriptionPairList() :
					Factory.GetCachedValue("CodeList_RefCusCodeListTypes_" + codeType + "_" + countryCode, () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddRange(ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, countryCode, codeType, ZDateTime.Today));
						result.SortByDescription();
						return result;
					});
			}
		}
	}
}
