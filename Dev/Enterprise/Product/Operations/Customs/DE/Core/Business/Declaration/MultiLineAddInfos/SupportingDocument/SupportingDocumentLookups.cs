using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;
using RefCusCodeListTypes = Enterprise.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.DE.Business
{
	public class SupportingDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentLookups
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
				var parent = Parent;
				if (parent.Parent is ICanBeImportOrExport importExportParent)
				{
					var parentIsInvoiceHeader = parent.ParentIsInvoiceHeader;
					var levelAttributeValue = parentIsInvoiceHeader ? RefCusCodeListAttributes.Value.Header : RefCusCodeListAttributes.Value.Item;

					if (importExportParent.IsExport)
					{
						var excludedCodesList = parentIsInvoiceHeader ? new[] { SupportingDocumentTypes._9ZZX, SupportingDocumentTypes._9ZZY, SupportingDocumentTypes.C612, SupportingDocumentTypes.N820, SupportingDocumentTypes.N821, SupportingDocumentTypes.N822, SupportingDocumentTypes.N952 } :
							new[] { SupportingDocumentTypes._9ZZX, SupportingDocumentTypes._9ZZY };
						var excludeCodesQuery = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.NotEqual, excludedCodesList);
						var codeList = CusSupportingInfoHelper.GetDocumentCodesFilteredByLevelAttributes(Factory, excludeCodesQuery, new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection },
							levelAttributeValue);
						return codeList;
					}
					else if (importExportParent.IsImport)
					{
						return CusSupportingInfoHelper.GetDocumentCodesFilteredByLevelAttributes(Factory, new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection },
							levelAttributeValue);
					}
				}

				return base.CodeList;
			}
		}

		public CodeDescriptionPairList AvailabilityList => Factory.GetCachedValue<AvailabilityList>();

		public CodeDescriptionPairList CustomsUQList
		{
			get
			{
				if (Parent.ImportExportParent.IsExport)
				{
					return Factory.GetCachedValue<SupportingDocumentsCustomsUQList>();
				}
				else
				{
					return CachedCustomsUQList;
				}
			}
		}

		public CodeDescriptionPairList CustomsUQ2List
		{
			get
			{
				var parent = Parent;
				if (parent.ImportExportParent.IsExport && parent.RefCusCode.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit))
				{
					return new CodeDescriptionPairList();
				}
				else
				{
					return CachedCustomsUQList;
				}
			}
		}

		CodeDescriptionPairList CachedCustomsUQList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today, includeParentDataGrouping: false);
	}
}
