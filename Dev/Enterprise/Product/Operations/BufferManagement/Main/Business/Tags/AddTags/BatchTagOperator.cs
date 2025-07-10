using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public static class BatchTagOperator
	{
		public static bool TryAddTag(ITagMagnitude tag, IEnumerable<ITagable> items, out string failureMessage, bool showSecurityDialog = true)
		{
			return TryActionTag(AddTagAction, tag, items, TagActionType.AddTag, out failureMessage, showSecurityDialog);
		}

		public static bool TryRemoveTag(ITagMagnitude tag, IEnumerable<ITagable> items, out string failureMessage, bool showSecurityDialog = true)
		{
			return TryActionTag(RemoveTagAction, tag, items, TagActionType.RemoveTag, out failureMessage, showSecurityDialog);
		}

		public static bool TryRemoveTag(ITagMagnitude tag, IEnumerable<ITagable> items, bool showSecurityDialog = true)
		{
			string notUsed;
			return TryActionTag(RemoveTagAction, tag, items, TagActionType.RemoveTag, out notUsed, showSecurityDialog);
		}

		static bool TryActionTag(Func<ITagMagnitude, ITagable, bool, ITagOperationResult> action, ITagMagnitude tag, IEnumerable<ITagable> items, TagActionType actionType, out string failureMessage, bool showSecurityDialog = true)
		{
			Argument.NotNull(tag, nameof(tag));
			Argument.NotNull(action, nameof(action));
			Argument.NotNull(items, nameof(items));
			var itemsArray = items.ToArray();
			var unsuccessfullyActionedItems = new List<Tuple<ITagable, ITagOperationResult>>(itemsArray.Length);

			var workQueue = tag as WorkQueue;
			if (workQueue != null)
			{
				foreach (var item in itemsArray)
				{
					if (item is ProcessHeader)
					{
						workQueue.Factory.AddFetchHint(ProcessHeaderSchema.PK, item.PK);
					}
				}
			}

			var tagMagnitudeFactory = tag.Factory;

			foreach (var item in itemsArray)
			{
				var processHeader = item as ProcessHeader;

				if (processHeader != null)
				{
					processHeader.AddDeepFetchHintForParentType(tagMagnitudeFactory);
				}

				var result = action(tag, item, showSecurityDialog) ?? throw new NullReferenceException("Result should not be null");
				if (!result.WasSuccessful)
				{
					unsuccessfullyActionedItems.Add(Tuple.Create(item, result));
				}
			}

			failureMessage = GetFailureMessage(tag, itemsArray, unsuccessfullyActionedItems, actionType);
			return unsuccessfullyActionedItems.Count == 0;
		}

		static string GetFailureMessage(ITagMagnitude tag, ITagable[] itemsArray, List<Tuple<ITagable, ITagOperationResult>> unsuccessfullyActionedItems, TagActionType actionType)
		{
			string failureMessage;
			if (unsuccessfullyActionedItems.Count > 0)
			{
				string entityAction;
				if (tag is WorkQueue)
				{
					entityAction = actionType == TagActionType.AddTag ? Res.GetString("820DCE00-B7CC-42C0-B150-CF730529F478", "be added to the queue")
						: Res.GetString("7D7159E7-49B4-48DB-9A7A-A8F0EB106EAA", "be removed from the queue");
				}
				else
				{
					entityAction = actionType == TagActionType.AddTag ? Res.GetString("36EB787A-7EBD-4C07-8BDC-79FA7743BDEA", "have the tag applied")
						: Res.GetString("A741E225-2F49-4223-AFD5-515CB85B9C4F", "have the tag removed");
				}

				failureMessage = itemsArray.Length != unsuccessfullyActionedItems.Count
					? Res.GetString("c3bf5023-e3ed-4a56-97ce-c715b8c873d8", "Some items could not {0}.", entityAction)
					: itemsArray.Length == 1
						? Res.GetString("6ffe9f70-9a24-4dbd-ab7c-6dc501b684f0", "The selected item could not {0}.", entityAction)
						: Res.GetString("900f5414-8cff-4cf4-bb47-51a87a42474b", "The selected items could not {0}.", entityAction);

				failureMessage += System.Environment.NewLine + System.Environment.NewLine + string.Join(System.Environment.NewLine, unsuccessfullyActionedItems.Select(x => string.Format(CultureInfo.InvariantCulture, "{0} ({1})", x.Item2.Message, x.Item1.Description)));
			}
			else
			{
				failureMessage = null;
			}

			return failureMessage;
		}

		static ITagOperationResult AddTagAction(ITagMagnitude tag, ITagable item, bool showSecurityDialog = true)
		{
			return item.AddTag(tag, showSecurityDialog);
		}

		static ITagOperationResult RemoveTagAction(ITagMagnitude tag, ITagable item, bool showSecurityDialog = true)
		{
			return item.RemoveTag(tag, showSecurityDialog);
		}
	}
}
