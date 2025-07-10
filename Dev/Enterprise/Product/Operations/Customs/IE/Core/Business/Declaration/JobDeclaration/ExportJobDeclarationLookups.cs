using System.Collections;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExportJobDeclarationLookups : JobDeclarationLookups
	{
		public ExportJobDeclarationLookups(JobDeclaration parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList LocationQualifierList
		{
			get
			{
				return Factory.GetCachedValue("IE.ExportJobDeclationsLookups.LocationQualifierList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair("U", "UN/LOCODE");
					return result;
				});
			}
		}

		public override ICollection GoodsDestination
		{
			get
			{
				ICollection result;
				var parent = Parent;
				var declarationType = parent.JE_EntryStyle.ToUpperInvariant();
				if (declarationType == EntryStyleListExport.Codes.ExportToSpecialTerritory)
				{
					result = Factory.GetCachedValue("Enterprise.Customs.IE.Business.Declaration.GoodsDestination|Export|CO", () =>
					{
						CodeDescriptionPairList codeDescriptionPairList = new CodeDescriptionPairList();
						var refCountryCollection = new RefCountryCollection(Factory, new ZQuery(RefCountrySchema.RN_EconomicGrouping, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN));
						refCountryCollection.ApplySort(RefCountrySchema.Constants.RN_Code, ListSortDirection.Ascending);
						foreach (RefCountry country in refCountryCollection)
						{
							codeDescriptionPairList.AddPair(country.Code, country.Description);
						}
						return codeDescriptionPairList;
					});
				}
				else
				{
					result = base.GoodsDestination;
				}
				return result;
			}
		}

		protected override ZString GoodsDestinationCodeType => UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country;

		protected override ZString GoodsOriginCodeType => UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country;

		public override CodeDescriptionPairList MessageSubTypeList => Factory.GetCachedValue<MessageSubTypeListExp>();

		public override CodeDescriptionPairList EntryStatusList => Factory.GetCachedValue<AESEntryStatusList>();

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<IELogicalStatusList>();
	}
}
