using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class BaseAdditionalInfoLookups : Customs.Business.CusSupportingInfoLookups
	{
		public BaseAdditionalInfoLookups(BaseAdditionalInfo parent)
			: base(parent)
		{
		}

		new protected BaseAdditionalInfo Parent => (BaseAdditionalInfo)base.Parent;

		public override ICollection CodeList
		{
			get
			{
				var importExportParent = Parent.ImportExportParent;

				if (importExportParent is BusinessObject bo && !bo.IsDeleted)
				{
					var direction = GetDirection(importExportParent);
					return GetAdditionalInformationList(importExportParent, direction);
				}

				return new CodeDescriptionPairList();
			}
		}

		public override CodeDescriptionPairList SubTypeList => Factory.GetCachedValue<AdditionalInfoSubTypeList>();

		public virtual CodeDescriptionPairList GetAdditionalInformationList(ICanBeImportOrExport importExportParent, ZString direction)
		{
			return Factory.GetAdditionalInformationList(importExportParent.DataGroupingCode, importExportParent.Level, direction);
		}

		protected ZString GetDirection(ICanBeImportOrExport importExportParent)
		{
			return (importExportParent.IsExport && importExportParent.IsImport)
						? UniversalReferenceConstants.RefCusCodeListDirectionType.Both
						: importExportParent.IsExport
							? UniversalReferenceConstants.RefCusCodeListDirectionType.Export
							: UniversalReferenceConstants.RefCusCodeListDirectionType.Import;
		}
	}
}
