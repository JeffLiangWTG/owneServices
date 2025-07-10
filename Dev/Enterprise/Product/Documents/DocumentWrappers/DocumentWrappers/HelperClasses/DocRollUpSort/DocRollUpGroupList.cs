using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.RollUpSort;

namespace Enterprise.DocumentWrappers.HelperClasses.DocRollUpSort
{
	public abstract class BaseDocRollUpGroupList<TID, TDocLineList> : List<DocRollUpGroup<TID, TDocLineList>>
		where TID : IZType
		where TDocLineList : ISortableDocLineList
	{
		readonly BusinessObjectFactory ReadonlyFactory = new();

		public TDocLineList GetGroup(bool needsRollUp, TID groupId, Func<TDocLineList> getNewLines)
		{
			DocRollUpGroup<TID, TDocLineList> group = null;

			if (needsRollUp)
			{
				foreach (var innerGroup in this)
				{
					if (innerGroup.ID.Equals(groupId))
					{
						group = innerGroup;
						break;
					}
				}
			}

			if (group == null)
			{
				var newLines = GetNewLine(ReadonlyFactory);
				group = new DocRollUpGroup<TID, TDocLineList>(newLines, needsRollUp, groupId);
				Add(group);
			}

			return group.Collection;
		}

		public bool HasGroupThatNeedsRollUp
		{
			get
			{
				var result = false;

				foreach (var group in this)
				{
					if (group.NeedsRollUp)
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		public TDocLineList GetLinesThatDontNeedRollUp()
		{
			var result = GetNewLine(ReadonlyFactory);

			foreach (var group in this)
			{
				if (!group.NeedsRollUp)
				{
					foreach (var docLine in group.Collection)
					{
						result.Add(docLine);
					}
				}
			}

			return result;
		}

		protected abstract TDocLineList GetNewLine(BusinessObjectFactory factory);
	}
}
