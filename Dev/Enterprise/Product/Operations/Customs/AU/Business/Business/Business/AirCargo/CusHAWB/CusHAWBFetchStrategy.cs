using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusHAWBFetchStrategy(CusHAWB cusHAWB)
			: base(cusHAWB)
		{
		}

		protected new CusHAWB BusinessObject
		{
			get { return (CusHAWB)base.BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			Factory.AddFetchHint(CusMAWBSchema.Constants.TableName, BusinessObject.CS_CM);
			base.FetchForLoadCore();
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			if (columns.Any(x => x.ColumnName == "CMRCargoStatus+Description"))
			{
				Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
			}
		}

		protected override void FetchForFactorySaveCore()
		{
			base.FetchForFactorySaveCore();
			if (BusinessObject.ShouldAttachOrphanedCARSTs)
			{
				Factory.AddFetchHint(typeof(CMRCARSTMessage), BusinessObject.GetUnattachedHouseCARSTSQuery());
			}
		}
	}
}
