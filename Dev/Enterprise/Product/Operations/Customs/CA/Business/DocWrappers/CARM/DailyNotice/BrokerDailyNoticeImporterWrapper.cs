using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class BrokerDailyNoticeImporterWrapper : NonPersistentBusinessObject
	{
		public BrokerDailyNoticeImporterWrapper(CusStatementLineGroup lineGroup, BusinessObjectFactory factory) : base(factory)
		{
			this.lineGroup = lineGroup;
			this.factory = factory;
		}
		readonly CusStatementLineGroup lineGroup;
		readonly BusinessObjectFactory factory;

		public ZString LegalName => lineGroup.B10_ImporterName;

		public ZString ProgramAccountNum => lineGroup.B10_ImporterCustomsID;

		public BusinessObjectCollectionWrapper<DailyNoticeDetailsWrapper> DailyNoticeDetails
		{
			get
			{
				var list = new List<DailyNoticeDetailsWrapper>();
				lineGroup.StatementLines
					.ToList().ForEach(x => list.Add(new DailyNoticeDetailsWrapper(x, factory)));
				return new BusinessObjectCollectionWrapper<DailyNoticeDetailsWrapper>(list);
			}
		}
	}
}
