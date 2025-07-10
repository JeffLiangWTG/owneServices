using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.DataTransfer.Native.Utils.Models;
using Enterprise.Environment;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgOpportunity
{
	public class OrgOpportunityInterceptor : BaseInterceptor
	{
		public OrgOpportunityInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices)
					: base(setting, sessionServices)
		{
			factory = setting.Context.ObjectFactory;
		}
		readonly BusinessObjectFactory factory;

		public OrgOpportunitySetting Setting
		{
			get { return (OrgOpportunitySetting)InterceptorSetting; }
		}

		public override void Invoke(IEntitySet entitySet)
		{
			entitySet.Root.DepthFirstTraversal((entity, relative) => { CreateNewOpportunityId(entity); });

			Function(entitySet);
		}

		public void CreateNewOpportunityId(IEntity entity)
		{
			if (entity.TableName == "OrgOpportunity")
			{
				var shouldUseFountain = false;

				if (entity.Action == EntityAction.INSERT)
				{
					shouldUseFountain = true;
				}
				else if (entity.Action == EntityAction.UPDATE || entity.Action == EntityAction.MERGE)
				{
					var entityPK = entity.GetPropertyOrBlankString("PK");
					if (!string.IsNullOrEmpty(entityPK))
					{
						var opportunity = factory.Load<MasterFiles.Business.OrgOpportunity>(new ZGuid(entityPK));
						if (opportunity == null)
						{
							shouldUseFountain = true;
						}
						else if (!opportunity.P8_OpportunityID.Equals(entity["OpportunityID"]))
						{
							entity["OpportunityID"] = opportunity.P8_OpportunityID;
						}
					}
					else
					{
						shouldUseFountain = true;
					}
				}

				if (shouldUseFountain)
				{
					entity["OpportunityID"] = Env.NumberFountains.SalesOpportunityID.GetNextFormatted(factory);
				}
			}
		}
	}
}
