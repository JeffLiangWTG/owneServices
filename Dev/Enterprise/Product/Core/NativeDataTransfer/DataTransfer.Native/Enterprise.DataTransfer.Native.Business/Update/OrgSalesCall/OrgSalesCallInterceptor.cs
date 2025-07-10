using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.DataTransfer.Native.Utils.Models;
using Enterprise.Environment;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgSalesCall
{
	public class OrgSalesCallInterceptor : BaseInterceptor
	{
		public OrgSalesCallInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices)
		{
			factory = setting.Context.ObjectFactory;
		}
		readonly BusinessObjectFactory factory;

		public OrgSalesCallSetting Setting => (OrgSalesCallSetting)InterceptorSetting;

		public override void Invoke(IEntitySet entitySet)
		{
			entitySet.Root.DepthFirstTraversal((entity, relative) => { CreateNewCommunicationIdWhenInserting(entity); });
			Function(entitySet);
		}

		public void CreateNewCommunicationIdWhenInserting(IEntity entity)
		{
			if (entity.TableName == "OrgSalesCall")
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
						var orgSalesCallRecord = factory.Load<MasterFiles.Business.OrgSalesCall>(new ZGuid(entityPK));
						if (orgSalesCallRecord == null)
						{
							shouldUseFountain = true;
						}
						else if (!orgSalesCallRecord.OQ_CommunicationID.Equals(entity["CommunicationID"]))
						{
							entity["CommunicationID"] = orgSalesCallRecord.OQ_CommunicationID;
						}
					}
					else
					{
						shouldUseFountain = true;
					}
				}

				if (shouldUseFountain)
				{
					entity["CommunicationID"] = Env.NumberFountains.CommunicationID.GetNextFormatted(factory);
				}
			}
		}
	}
}
