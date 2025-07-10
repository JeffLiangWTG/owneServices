using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.MasterFiles.Integration.DocRollUpSort;

namespace Enterprise.DocumentWrappers.HelperClasses.DocRollUpSort
{
	public abstract partial class BaseDocRollUpper<TID, TDescription, TDocLineList>
		where TID : IZType
		where TDescription : IZType
		where TDocLineList : ISortableDocLineList
	{
		protected BaseDocRollUpper(IDocHeader header, BusinessObjectFactory factory, TDocLineList docLines)
		{
			DocHeader = header;
			Factory = factory;
			DocLines = docLines;
		}

		protected IDocHeader DocHeader { get; }

		protected BusinessObjectFactory Factory { get;  }

		TDocLineList DocLines { get; }

		protected abstract TID GetGroupId(IDocLine line);

		protected abstract TDescription GetDescriptionForRolledUpLine(TDocLineList group, TID groupId);

		protected virtual void ProcessRolledUpLine(IRolledUpDocLine rolledUpLine, DocRollUpGroup<TID, TDocLineList> group)
		{
		}

		public abstract TDocLineList GetNewLines(BusinessObjectFactory factory);

		public TDocLineList RollUp()
		{
			var result = GetNewLines(Factory);

			var groups = GetGroups();

			OnBeforeRollUp(groups);

			foreach (var group in groups)
			{
				if (group.NeedsRollUp)
				{
					if (group.Collection.Count > 0)
					{
						var rolledUpLine = RollUpGroup(group.Collection, group.ID);
						ProcessRolledUpLine(rolledUpLine, group);
						if (rolledUpLine != null)
						{
							result.Add(rolledUpLine);
						}
					}
				}
				else
				{
					foreach (var item in group.Collection)
					{
						result.Add(item);
					}
				}
			}

			return result;
		}

		protected abstract IRolledUpDocLine RollUpGroup(TDocLineList group, TID groupId);

		List<TID> GetGroupIds()
		{
			var result = new List<TID>();

			foreach (IDocLine line in DocLines)
			{
				var groupIdCandidate = GetGroupId(line);

				if (!groupIdCandidate.IsEmpty && groupIdCandidate.IsValid && !result.Contains(groupIdCandidate))
				{
					result.Add(groupIdCandidate);
				}
			}

			return result;
		}

		BaseDocRollUpGroupList<TID, TDocLineList> GetGroups()
		{
			var result = GetDocRollUpGroupList();
			var groupIds = GetGroupIds();

			foreach (IDocLine docLine in DocLines)
			{
				var lineGroupId = GetGroupId(docLine);

				if (lineGroupId.IsEmpty)
				{
					var collectionInGroup = result.GetGroup
					(
						needsRollUp: false,
						lineGroupId,
						() => GetNewLines(Factory)
					);
					collectionInGroup.Add(docLine);
				}
				else
				{
					var linePreventGrouping = docLine.PreventGrouping;
					foreach (var groupId in groupIds)
					{
						if (lineGroupId.Equals(groupId))
						{
							var collectionInGroup = result.GetGroup
							(
								needsRollUp: !linePreventGrouping,
								groupId,
								() => GetNewLines(Factory)
							);

							if (!collectionInGroup.Contains(docLine))
							{
								collectionInGroup.Add(docLine);
							}
						}
					}
				}
			}

			return result;
		}

		protected abstract BaseDocRollUpGroupList<TID, TDocLineList> GetDocRollUpGroupList();

		protected virtual void OnBeforeRollUp(BaseDocRollUpGroupList<TID, TDocLineList> allGroups)
		{
		}
	}
}
