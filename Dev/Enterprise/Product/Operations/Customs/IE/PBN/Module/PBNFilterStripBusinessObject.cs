using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.PBN.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.PBN.Module
{
	public class PBNFilterStripBusinessObject : ASYCUDA.Module.AsycudaPreBoardingNotificationFilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String Constant")]
		public static class PBNFilterConstants
		{
			public const string PBNIds = "PBN ids";
			public const string ManifestNature = "Manifest Nature";
		}

		public PBNFilterStripBusinessObject() : base()
		{
		}

		public PBNFilterStripBusinessObject(bool enableCountryFilter = true) : base(enableCountryFilter)
		{
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new PBNFilterStripBusinessObject(shouldAddCountryFilter);

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();

			var pbnIdFilter = result.AddTextFilter(PBNFilterConstants.PBNIds, GetPBNIdQuery);
			pbnIdFilter.Category = FilterCategories.NumbersAndReferences;
			pbnIdFilter.MultilingualDescription = ResString.GetMultilingualString("PBNFilterStripBusinessObject|PBNIdFilter", PBNFilterConstants.PBNIds);

			var manifestNatureFilter = result.AddTextFilter(PBNFilterConstants.ManifestNature, GetManifestNatureQuery);
			manifestNatureFilter.Category = FilterCategories.NumbersAndReferences;
			manifestNatureFilter.MultilingualDescription = ResString.GetMultilingualString("PBNFilterStripBusinessObject|ManifestNature", PBNFilterConstants.ManifestNature);

			return result;
		}

		ZDBOnlyQuery GetPBNIdQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			// TODO: Implement this method in a future WI
			var result = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			return result;
		}

		ZDBOnlyQuery GetManifestNatureQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			// TODO: Implement this method in a future WI
			var result = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			return result;
		}
	}
}
