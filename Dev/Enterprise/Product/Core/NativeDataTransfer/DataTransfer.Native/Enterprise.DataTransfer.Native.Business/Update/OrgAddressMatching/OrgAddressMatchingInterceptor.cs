using System.Linq;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Native.Business.Update.OrgAddressMatching.Converters;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.DataTransfer.Native.Utils.Models;
using Enterprise.MasterFiles.DataTransfer.OrgMatching;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgAddressMatching
{
	public class OrgAddressMatchingInterceptor : MIDOrgMatchingInterceptor
	{
		public OrgAddressMatchingInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices)
		{ }

		public OrgAddressMatchingSetting Setting
		{
			get { return (OrgAddressMatchingSetting)InterceptorSetting; }
		}

		public override void Invoke(IEntitySet entitySet)
		{
			entitySet.Root.DepthFirstTraversal((entity, relative) => { MatchOrgAddressBySingleOrgCusCode(entity); });

			Function(entitySet);
		}

		public void MatchOrgAddressBySingleOrgCusCode(IEntity entity)
		{
			if (entity.TableName == "OrgAddress")
			{
				if (entity.Parents.IsEmpty() && entity.Properties.IsEmpty()
						&& entity.ChildrenCollection.Where(c => c.EntityName == "OrgCusCode").Take(2).Count() == 1)
				{
					var orgMatchingData = new EntityToOrgHeaderMatchingConverter(factory).Convert(entity);

					try
					{
						var orgAddress = new OrganisationMatcher(factory, IfUnmatched.ReturnNull).GetMatchingAddress(orgMatchingData, null, false);
						if (orgAddress != null)
						{
							entity.InternalPK = orgAddress.PK.ToGuid();
							entity["Code"] = orgAddress.OA_Code;
						}
						else
						{
							CreateMIDOrganizationIfNecessary(entity);
						}
					}
					finally
					{
						orgMatchingData.Delete();
					}
				}

				foreach (var orgCusCodeEntity in entity.ChildrenCollection.Where(c => c.EntityName == "OrgCusCode").ToList())
				{
					entity.ChildrenCollection.Remove(orgCusCodeEntity);
				}
			}
		}
	}
}
