using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public static class Extensions
	{
		public static bool CheckManifestHeaderHasBeenCreated(this ForwardingConsol consol, string countryCode, string manifestType = null)
		{
			var result = false;
			if (consol.IsInDatabase)
			{
				var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
				headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ParentId, consol.PK);
				headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, SQLComparisonOperator.NotEqual, new string[] { AsycudaManifestHeader.ApplicationCode_ZAOutturnGateInOut, ApplicationCodeTypeList.Codes.TRETrade });
				headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, countryCode);
				if (!string.IsNullOrEmpty(manifestType))
				{
					headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ManifestType, manifestType);
				}
				result = consol.Factory.ExistsInDatabase(AsycudaManifestHeader.Schema.TableName, headerQuery);
			}
			return result;
		}

		public static void RefreshLabels(this ZGrid grid)
		{
			grid?.Extensions.Get<IAutomaticLabelExtension>()?.Refresh();
		}
	}
}
