using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class AddInfoChildUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		protected readonly BusinessObject bizObj;
		readonly IAddInfoChildUniqueIndexFailureHandlerSupporter supporter;

		public AddInfoChildUniqueIndexFailureHandler(IAddInfoChildUniqueIndexFailureHandlerSupporter bizObj)
		{
			supporter = bizObj;
			this.bizObj = Argument.NotNull(supporter as BusinessObject, nameof(bizObj) + " should be of type " + typeof(BusinessObject).FullName);
		}

		public virtual IEnumerable<string> HandledUniqueIndexNames
		{
			get
			{
				yield return supporter.UniqueIndexName;
			}
		}

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			if (indexName == supporter.UniqueIndexName)
			{
				NotifyUserAndAttemptToResolveAddInfoChildIndex(notifier);
			}
			else
			{
				NotifyUserAndAttemptToResolveCore(notifier, indexName);
			}
		}

		protected virtual void NotifyUserAndAttemptToResolveCore(INotificationHandler notifier, string indexName) { }

		void NotifyUserAndAttemptToResolveAddInfoChildIndex(INotificationHandler notifier) => NotifyUserAndAttemptToResolveAddInfoChildIndex(notifier, GetExistingItem());

		protected void NotifyUserAndAttemptToResolveAddInfoChildIndex(INotificationHandler notifier, IAddInfoChildUniqueIndexFailureHandlerSupporter existingItem)
		{
			if (existingItem != null)
			{
				var message = GenerateMessage(existingItem);
				var parent = supporter.Parent;
				bizObj.Delete();
				_ = parent.AddInfoChild;
				(parent as BusinessObject)?.RefreshBindingIncludingChildren();
				notifier.ReportInformation(message, Res.GetString("{8CCEF68B-97A2-4D60-B19B-C0B2DE325D61}", "Save Error"));
			}
		}

		string GenerateMessage(IAddInfoChildUniqueIndexFailureHandlerSupporter existingItem)
		{
			return Res.GetString("{3BF88DC0-34D3-46C5-9AE2-66395A86BEAB}",
				"{0} has already been created for ({1}) by another user ({2}). Your changes have been merged, please review your changes and save again.",
				bizObj.HumanReadableName,
				GetParentReference(),
				GetLastEditUserAndTime(existingItem));
		}

		protected virtual string GetParentReference() => (supporter.Parent as BusinessObject)?.HumanReadableName ?? ZString.Empty;

		static string GetLastEditUserAndTime(IAddInfoChildUniqueIndexFailureHandlerSupporter existingBizObj)
		{
			return existingBizObj.SystemLastEditUser + " @ " + existingBizObj.SystemLastEditTimeUtc;
		}

		IAddInfoChildUniqueIndexFailureHandlerSupporter GetExistingItem()
		{
			var bizObjType = bizObj.GetType();
			var query = new ZDBOnlyQuery(bizObjType);
			var parent = supporter.Parent;
			var childForeignKeyColumn = parent.ChildForeignKeyColumn;
			query.AddToFilter(childForeignKeyColumn.TableSchema.PK, SQLComparisonOperator.NotEqual, bizObj.PK);
			query.AddToFilter(childForeignKeyColumn, parent.PK);
			return (IAddInfoChildUniqueIndexFailureHandlerSupporter)bizObj.Factory.LoadTop1(bizObjType, query);
		}
	}
}
