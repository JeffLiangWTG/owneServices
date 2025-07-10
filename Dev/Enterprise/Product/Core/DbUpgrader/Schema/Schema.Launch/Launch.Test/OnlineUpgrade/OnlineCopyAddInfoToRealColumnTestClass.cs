using System.Collections.Generic;
using System.Linq;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformations.Transforms;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing
{
	class OnlineCopyAddInfoToRealColumnTestClass : OnlineCopyAddInfoToRealColumn<CopyAddInfoToRealColumnTestClass>
	{
		public OnlineCopyAddInfoToRealColumnTestClass(IUpgradeManager manager, string dbBeingUpgraded, string templateDb)
			: base(manager, dbBeingUpgraded, templateDb)
		{
		}

		protected override string UserDescription => "Populate JobComInvoiceLine (JI_ValuationDateOverride, JI_CustomDate4, JI_OrderNumber, JI_CustomDecimal1) columns from dbo.JobComInvoiceLine JI_AddInfo";
		protected override IEnumerable<Mapping> GetAllMappings()
		{
			return new[] { Mapping.New<CopyAddInfoToRealColumnTestClass>(new VersionLabel(1, 0)) }.Union(base.GetAllMappings());
		}
	}
}
