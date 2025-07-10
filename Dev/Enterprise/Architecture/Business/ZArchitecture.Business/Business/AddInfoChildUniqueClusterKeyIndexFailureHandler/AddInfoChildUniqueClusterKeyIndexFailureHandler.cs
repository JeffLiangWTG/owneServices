using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class AddInfoChildUniqueClusterKeyIndexFailureHandler : AddInfoChildUniqueIndexFailureHandler
	{
		public AddInfoChildUniqueClusterKeyIndexFailureHandler(IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter supporter)
			: base(supporter)
		{
			this.supporter = supporter;
		}
		readonly IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter supporter;

		public override IEnumerable<string> HandledUniqueIndexNames
		{
			get
			{
				yield return supporter.UniqueClusterIndexName;
				foreach (var indexName in base.HandledUniqueIndexNames)
				{
					yield return indexName;
				}
			}
		}
		protected override void NotifyUserAndAttemptToResolveCore(INotificationHandler notifier, string indexName)
		{
			if (indexName == supporter.UniqueClusterIndexName)
			{
				NotifyUserAndAttemptToResolveAddInfoChildIndex(notifier, GetExistingItem());
			}
			else
			{
				base.NotifyUserAndAttemptToResolveCore(notifier, indexName);
			}
		}

		IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter GetExistingItem()
		{
			var bizObjType = bizObj.GetType();
			var query = new ZDBOnlyQuery(bizObjType);
			var clusterKeyMaster = supporter.ClusterKeyMaster;
			var clusterKeyColumn = supporter.ClusterKeyColumn;
			query.AddToFilter(clusterKeyColumn.TableSchema.PK, SQLComparisonOperator.NotEqual, bizObj.PK);
			query.AddToFilter(clusterKeyColumn, clusterKeyMaster.ClusterKeyPty.Value);
			return (IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter)bizObj.Factory.LoadTop1(bizObjType, query);
		}
	}
}
