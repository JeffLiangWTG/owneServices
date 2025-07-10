using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Manifest.Business
{
	public class AsycudaPackedItem : ASYCUDA.Business.AsycudaPackedItem
	{
		public AsycudaPackedItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString API_Tariff
		{
			get { return base.API_Tariff; }
			set
			{
				var oldValue = base.API_Tariff;
				base.API_Tariff = value;
				if (!IsCopying && oldValue != value)
				{
					SetDescriptionFromTariff();
				}
			}
		}

		void SetDescriptionFromTariff()
		{
			if (API_GoodsDescription.IsEmpty && !API_Tariff.IsEmpty)
			{
				ZString tariffDescription;

				var applicationBusinessProvider = Header.ApplicationBusinessProvider;
				var date = ZDateTime.Today;
				var includeSectionHeadings = false;
				var includeChapterHeading = false;

				var tariffView = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(applicationBusinessProvider.PackedItemTariffDataGrouping, applicationBusinessProvider.PackedItemTariffType, API_Tariff, date);
				if (tariffView != null)
				{
					tariffDescription = tariffView.FullTariffDescription(date, includeSectionHeadings: includeSectionHeadings, includeChapterHeading: includeChapterHeading, useTariffPreferredLanguage: true);
				}
				else
				{
					tariffDescription = GetTariffNomenclatureDescription(applicationBusinessProvider.PackedItemTariffDataGrouping, date, includeSectionHeadings, includeChapterHeading);
				}

				API_GoodsDescription = !tariffDescription.IsEmpty ? tariffDescription.Truncate(API_GoodsDescriptionInfo.MaxLength).ToUpper() : tariffDescription;
			}
		}

		ZString GetTariffNomenclatureDescription(ZString dataGrouping, ZDateTime date, bool includeSectionHeadings, bool includeChapterHeading)
		{
			var tariffDescription = ZString.Empty;

			var nomenclatureGroup = GetNomenclatureGroupForTariff(dataGrouping);
			if (nomenclatureGroup != null)
			{
				var tariffLanguageManager = new TariffPreferredLanguageManager();
				var language = tariffLanguageManager.IsDefaultLanguage ? GlbStaff.CurrentUser.Language : tariffLanguageManager.Language;

				tariffDescription = RefCusNomenclatureLanguage.GetFullDescriptionForCompositeKey(Factory, nomenclatureGroup.ZZ5_CompositeKey, dataGrouping, nomenclatureGroup.ZZ5_ZZ9_NKNomenclatureGroupType, date, includeSectionHeadings, includeChapterHeading, TranslationHelper.GetLanguageCode(language));
				if (tariffDescription.IsEmpty)
				{
					tariffDescription = RefCusNomenclatureGroup.GetFullDescriptionForCompositeKey(base.Factory, nomenclatureGroup.ZZ5_CompositeKey, dataGrouping, nomenclatureGroup.ZZ5_ZZ9_NKNomenclatureGroupType, date, includeSectionHeadings, includeChapterHeading);
				}
			}
			return tariffDescription;
		}

		RefCusNomenclatureGroup GetNomenclatureGroupForTariff(ZString dataGroupingCode)
		{
			var dataGroupings = RefDataGrouping.GetDataGroupingIncludingParent(Factory, dataGroupingCode);

			var dataGroupingsSubQuery = new ZDBOnlyQuery(typeof(RefCusNomenclatureGroup));
			var subQuery = new ZDBOnlyQuery(typeof(RefCusNomenclatureGroup));
			subQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_ZZZ_NKDataGrouping, dataGroupings);
			dataGroupingsSubQuery.AddToFilter(subQuery, JoinCondition.Or);

			var query = new ZDBOnlyQuery(typeof(RefCusNomenclatureGroup));
			query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_Value, API_Tariff);
			query.AddToFilter(dataGroupingsSubQuery, JoinCondition.And);
			return Factory.LoadTop1<RefCusNomenclatureGroup>(query);
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
		public new AsycudaPack Pack => (AsycudaPack)base.Pack;
		public new AsycudaPackedItemValidation Validation => (AsycudaPackedItemValidation)base.Validation;
		protected override ManifestBase.AsycudaPackedItemValidation GetNewValidation() => new AsycudaPackedItemValidation(this);
	}
}
