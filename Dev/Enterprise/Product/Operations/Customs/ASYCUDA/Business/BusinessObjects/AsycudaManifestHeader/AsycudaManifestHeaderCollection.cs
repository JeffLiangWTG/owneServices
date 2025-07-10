using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public sealed class AsycudaManifestHeaderCollection : DependentBusinessObjectCollection<AsycudaManifestHeader, ForwardingConsol>
	{
		public AsycudaManifestHeaderCollection(ForwardingConsol master)
			: base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => AsycudaManifestHeaderSchema.AMA_ParentId;

		public AsycudaManifestHeader GetHeader(string countryCode, string manifestType)
		{
			return !string.IsNullOrWhiteSpace(countryCode)
				? this.Cast<AsycudaManifestHeader>()
					.FirstOrDefault(c => c.AMA_RN_NKCountry == countryCode && c.AMA_ManifestType == manifestType)
				: null;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, SQLComparisonOperator.NotEqual, AsycudaManifestHeader.ApplicationCode_ZAOutturnGateInOut);

			if (Master.IsInDatabase)
			{
				CountryHelper.AddManifestCompanyFilterInSpecifiedCountry(query);
			}
			else
			{
				query.FetchOnlyFromLocalCache = true;
			}
			query.ReLoadExistingRows = true;

			return query;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			var header = (AsycudaManifestHeader)child;
			header.AMA_ParentTableCode = Master.TablePrefix;
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			((AsycudaManifestHeader)dependent).AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;
		}

		protected override bool AllowNewCore => false;
	}
}
