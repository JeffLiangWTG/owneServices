using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class RatesPrioritiesCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new RatesPriorities this[int i]
		{
			get { return (RatesPriorities)Elements[i]; }
		}

		protected override bool AllowSort
		{
			get { return false; }
		}

		public new RatesPriorities AddNew()
		{
			return (RatesPriorities)base.AddNew();
		}

		public RatesPriorities AddNew(RatingDebtorOrgTypes orgType, RatingJobTypes jobType = RatingJobTypes.ALL)
		{
			var priority = AddNew();
			priority.OrganizationType = orgType.ToString();
			priority.JobType = jobType.ToString();
			return priority;
		}

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RatesPriorities();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RatesPrioritiesCollection();
		}

		public static RatesPrioritiesCollection GetDefault(string registryName)
		{
			var result = new RatesPrioritiesCollection();

			switch (registryName)
			{
				case RatingDataRegistry.ImportPrepaidPrioritiesRegistryName:
					result.AddNew(RatingDebtorOrgTypes.AG);
					result.AddNew(RatingDebtorOrgTypes.CNR);
					break;

				case RatingDataRegistry.ExportPrepaidPrioritiesRegistryName:
					result.AddNew(RatingDebtorOrgTypes.LC);
					result.AddNew(RatingDebtorOrgTypes.CNR);
					result.AddNew(RatingDebtorOrgTypes.LCBK);
					break;

				case RatingDataRegistry.CrossTradePrepaidPrioritiesRegistryName:
					result.AddNew(RatingDebtorOrgTypes.LC);
					result.AddNew(RatingDebtorOrgTypes.AG);
					result.AddNew(RatingDebtorOrgTypes.LCBK);
					result.AddNew(RatingDebtorOrgTypes.CNR);
					break;

				case RatingDataRegistry.DomesticPrepaidPrioritiesRegistryName:
					result.AddNew(RatingDebtorOrgTypes.CNR);
					result.AddNew(RatingDebtorOrgTypes.LC);
					result.AddNew(RatingDebtorOrgTypes.AG);
					break;

				case RatingDataRegistry.ImportCollectPrioritiesRegistryName:
					result.AddNew(RatingDebtorOrgTypes.LC);
					result.AddNew(RatingDebtorOrgTypes.CNE);
					result.AddNew(RatingDebtorOrgTypes.LCBK);
					break;

				case RatingDataRegistry.ExportCollectPrioritiesRegistryName:
					result.AddNew(RatingDebtorOrgTypes.AG);
					result.AddNew(RatingDebtorOrgTypes.CNE);
					break;

				case RatingDataRegistry.CrossTradeCollectPrioritiesRegistryName:
					result.AddNew(RatingDebtorOrgTypes.LC);
					result.AddNew(RatingDebtorOrgTypes.AG);
					result.AddNew(RatingDebtorOrgTypes.LCBK);
					result.AddNew(RatingDebtorOrgTypes.CNE);
					break;

				case RatingDataRegistry.DomesticCollectPrioritiesRegistryName:
					result.AddNew(RatingDebtorOrgTypes.CNE);
					result.AddNew(RatingDebtorOrgTypes.LC);
					result.AddNew(RatingDebtorOrgTypes.AG);
					break;

				case RatingDataRegistry.GatewayPrepaidPrioritiesRegistryName:
					result.AddNew(RatingDebtorOrgTypes.SAG);
					result.AddNew(RatingDebtorOrgTypes.CNR);
					break;

				case RatingDataRegistry.GatewayCollectPrioritiesRegistryName:
					result.AddNew(RatingDebtorOrgTypes.RAG);
					result.AddNew(RatingDebtorOrgTypes.CNE);
					break;
			}

			return result;
		}
	}
}
