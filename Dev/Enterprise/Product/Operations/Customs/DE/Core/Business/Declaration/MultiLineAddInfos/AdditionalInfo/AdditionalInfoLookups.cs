using System.Collections;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business
{
	public sealed class AdditionalInfoLookups : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoLookups
	{
		public AdditionalInfoLookups(AdditionalInfo parent) : base(parent)
		{
		}

		public override ICollection CodeList
		{
			get
			{
				var parent = Parent;
				if (parent.ParentIsExitDetail)
				{
					var codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_AI44X;
					return Factory.GetCachedValue($"DE.AdditionalInfoLookups.CodeList_{codeType}", // Cache Key
						() => ZZRefCusCodeListCombinedCollection.GetCachedCollection(
						Factory,
						Core.Constants.CountryCodes.Germany,
						codeType,
						ZDateTime.Today));
				}
				else
				{
					return base.CodeList;
				}
			}
		}

		protected override ZQuery GetAdditionalFilterForUCCAdditionalInformation()
		{
			var subType = Parent.CSI_SubType;
			ZQuery excludeCodesQuery = null;
			if (subType.In(new ZString[] { AdditionalDocTypeList.Codes.TransportDocuments, AdditionalDocTypeList.Codes.AdditionalReference }) && Parent.Parent is JobComInvoiceHeader)
			{
				var excludedCodesList = new List<string> { UniversalReferenceConstants.SupportingDocumentTypes._9ZZX, UniversalReferenceConstants.SupportingDocumentTypes._9ZZY };
				if (subType == AdditionalDocTypeList.Codes.TransportDocuments)
				{
					excludedCodesList.AddRange(new[] { UniversalReferenceConstants.SupportingDocumentTypes.C613, UniversalReferenceConstants.SupportingDocumentTypes.C614, UniversalReferenceConstants.SupportingDocumentTypes.N720 });
				}

				excludeCodesQuery = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.NotEqual, excludedCodesList);
			}
			return excludeCodesQuery;
		}

		protected override ZBool UseUCCAdditionalInfosCore(BusinessObject parent)
		{
			if (Parent.UsedInExportCusClassPartPivot)
			{
				return true;
			}
			else
			{
				return base.UseUCCAdditionalInfosCore(parent);
			}
		}

		protected override ICollection GetUCCAdditionalInformationList(ICanBeImportOrExport importExportParent, ZString direction)
		{
			var refCusCodeListTypes = GetRefCusCodeListTypes(importExportParent, Parent.CSI_SubType);
			if (refCusCodeListTypes.Length > 0 && Parent.UsedInExportCusClassPartPivot)
			{
				return Factory.GetUCCAdditionalInformationList(null, importExportParent.DataGroupingCode, refCusCodeListTypes, (NoResString)"Item", OmitLevelAttribute);
			}
			else
			{
				return base.GetUCCAdditionalInformationList(importExportParent, direction);
			}
		}

		public CodeDescriptionPairList KindList
		{
			get
			{
				if (Parent.ParentAsInvoiceHeader != null)
				{
					return InvoiceHeaderKindList;
				}
				else if (Parent.ParentIsExitDetail)
				{
					return ExitSummaryKindList;
				}
				else
				{
					return InvoiceLineKindList;
				}
			}
		}

		internal ZZRefCusCodeListCombined FullTypeRefCusCode
		{
			get
			{
				var code = Parent.CSI_Code;
				var codeTypes = GetRefCusCodeListTypes(Parent.ImportExportParent, Parent.CSI_SubType);
				string codeType = codeTypes.IsNullOrEmpty() ? UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E : codeTypes[0].ToString();
				var levelAttributeValue = RefCusCodeListLevel;
				return Factory.GetCachedValue($"DE.AdditionalInfoLookups.FullTypeRefCusCode_{levelAttributeValue}_{codeType}_{code}", // Cache Key
					() => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, code, Core.Constants.CountryCodes.Germany, codeType, ZDateTime.Today,
						attributeFilters: new[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.Level, JoinCondition.And, levelAttributeValue) }));
			}
		}

		new AdditionalInfo Parent => (AdditionalInfo)base.Parent;

		protected override ZString[] GetRefCusCodeListTypes(ICanBeImportOrExport importExportParent, ZString csiSubType)
		{
			if (csiSubType == AdditionalDocTypeList.Codes.Authorization)
			{
				return new ZString[] { UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AA44E };
			}
			else
			{
				return base.GetRefCusCodeListTypes(importExportParent, csiSubType);
			}
		}

		string RefCusCodeListLevel => (Parent.ParentAsInvoiceHeader != null) ? RefCusCodeListLevelType.Header : RefCusCodeListLevelType.Item;

		CodeDescriptionPairList InvoiceHeaderKindList
		{
			get
			{
				return Factory.GetCachedValue("DE.AdditionalInfoLookups.InvoiceHeaderKindList", () =>
				{
					var result = new AdditionalDocTypeList();
					result.RemoveCode(AdditionalDocTypeList.Codes.Authorization);
					return result;
				});
			}
		}

		CodeDescriptionPairList InvoiceLineKindList
		{
			get
			{
				return Factory.GetCachedValue("DE.AdditionalInfoLookups.InvoiceLineKindList", () =>
				{
					var result = new AdditionalDocTypeList();
					result.RemoveCode(AdditionalDocTypeList.Codes.TransportDocuments);
					return result;
				});
			}
		}

		CodeDescriptionPairList ExitSummaryKindList => Factory.GetCachedValue("DE.AdditionalInfoLookups.ExitSummaryKindList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(AdditionalDocTypeList.Codes.AdditionalInformation, AdditionalDocTypeList.Descriptions.AdditionalInformation);
			return result;
		});
	}
}
