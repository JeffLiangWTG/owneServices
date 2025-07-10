using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class SupportingDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentLookups
	{
		public SupportingDocumentLookups(SupportingDocument parent)
			: base(parent)
		{
		}

		protected new SupportingDocument Parent => (SupportingDocument)base.Parent;

		public override CodeDescriptionPairList UnitOfQuantityList
		{
			get
			{
				return Factory.GetCachedValue($"Customs.FR.SupportingDocumentLookups_{(Parent.ImportExportParent.IsImport ? "IMP" : "EXP")}_{Parent.CSI_Code}", () =>
				{
					var result = new CodeDescriptionPairList();
					if (Parent.CurrentCSI_CodeContainsUQMapTypeAttribute)
					{
						var quantityUnitsMapType = Parent.CurrentUQMapType;
						var quantityUnitsMap = ZZRefCusMapCombined.GetCW1CodeToCustomsCodeMapping(Factory, Core.Constants.CountryCodes.France, quantityUnitsMapType, ZDateTime.Today);
						quantityUnitsMap.ForEach(x => result.AddPair(x.Key, x.Value));
					}
					return result;
				});
			}
		}

		public override ICollection CodeList
		{
			get
			{
				var parent = Parent;
				if (parent.Declaration?.IsUCC6 ?? false)
				{
					if (parent.Declaration.IsImport)
					{
						return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection }, ZDateTime.Today, null, false);
					}
				}
				else
				{
					var importExportParent = parent.ImportExportParent as BusinessObject;
					if (importExportParent != null && !importExportParent.IsDeleted)
					{
						return Factory.GetSupportingDocumentList(Parent.ImportExportParent.DataGroupingCode, parent.ParentDirection, "", GetAdditionalCodeListAttributeFilters());
					}
				}

				return new CodeDescriptionPairList();
			}
		}

		public IEnumerable<ZString> GetIsD48CodeList(ZString codeType, ZDateTime dateForDuty)
			=> Factory.GetCachedValue($"IsD48CodeList_FR_{codeType}_{dateForDuty}", () =>
			{
				var list = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
					Core.Constants.CountryCodes.France,
					new[] { codeType },
					dateForDuty,
					new[]
					{
						new RefCusCodeListAttributeFilter(
							UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48,
							JoinCondition.And,
							new ZString[] { Customs.Business.YesNoList.Codes.Yes })
					});
				list.Load();
				return list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code);
			});

		public IEnumerable<ZString> GetIsDTPCodeList(ZString codeType, ZDateTime dateForDuty)
			=> Factory.GetCachedValue($"IsDTPCodeList_FR_{codeType}_{dateForDuty}", () =>
			{
				var list = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
					Core.Constants.CountryCodes.France,
					new[] { codeType },
					dateForDuty,
					new[]
					{
						new RefCusCodeListAttributeFilter(
							UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsDTP,
							JoinCondition.And,
							new ZString[] { Customs.Business.YesNoList.Codes.Yes })
					});
				list.Load();
				return list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code);
			});

		public IEnumerable<ZString> GetIsODSCodeList(ZString codeType, ZDateTime dateForDuty)
			=> Factory.GetCachedValue($"IsODSCodeList_FR_{codeType}_{dateForDuty}", () =>
			{
				var list = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
					Core.Constants.CountryCodes.France,
					new[] { codeType },
					dateForDuty,
					new[]
					{
						new RefCusCodeListAttributeFilter(
							UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsODS,
							JoinCondition.And,
							new ZString[] { Customs.Business.YesNoList.Codes.Yes })
					});
				list.Load();
				return list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code);
			});
	}
}
