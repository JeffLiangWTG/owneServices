using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Module
{
	public class StatementFilterStripLookups
	{
		public StatementFilterStripLookups(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public GlbCompanyCollection Companies => new GlbCompanyCollection(factory);

		public CodeDescriptionPairList StatusList => factory.GetCachedValue<StatementStatusList>();

		public ConsigneeCollection Consignees => new ConsigneeCollection(factory);

		public CodeDescriptionPairList DirectionList => factory.GetCachedValue<StatementEntryTypeImpExpList>();
	}
}
