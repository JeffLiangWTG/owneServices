using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Licensing
{
	public class LicenceUsageCollection : ActiveBusinessObjectCollection<LicenceUsageLog>
	{
		public LicenceUsageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Filter

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(base.CreateRelationshipFilter());
			query.AddToFilter(StmActivityLogSchema.S7_ControllerID, SQLComparisonOperator.StartsWith, LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString());
			var company = GlbCompany.CurrentCompany;
			List<ZGuid> pks = new List<ZGuid>();
			pks.Add(company.PK);
			foreach (GlbBranch branch in company.Branches)
			{
				pks.Add(branch.PK);
			}
			query.AddToFilter(StmActivityLogSchema.S7_ParentID, pks);
			return query;
		}

		protected override void SetDefaultsForNewElementCore(LicenceUsageLog newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.S7_ControllerID = LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString();
			newElement.S7_ParentID = GlbBranch.CurrentBranch.PK;
			newElement.S7_ParentTableCode = GlbBranchSchema.Constants.Prefix;
		}

		#endregion
	}
}
