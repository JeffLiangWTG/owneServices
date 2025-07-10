using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Interfaces.Snapshot;

namespace Enterprise.Customs.FR.Business.Snapshot
{
	public class CusEntryLineSnapshotDataBuilder
	{
		public CusEntryLineSnapshotDataBuilder(ISnapshottedCusEntryLine[] snapshottedCusEntryLineList)
		{
			this.snapshottedCusEntryLineList = snapshottedCusEntryLineList;
		}

		public ZString GetSnapshot()
		{
			var snapshot = GenerateSnapshot();
			return snapshot == null ? ZString.Empty : Extensions.SerializeUsingUtf16(snapshot);
		}

		FrenchEntryLineChildSnapshot GenerateSnapshot()
		{
			var snapshot = new FrenchEntryLineChildSnapshot();

			var myItems = new List<FrenchEntryLineChildSnapshotCusEntryLine>();
			foreach (var snapshottedCusEntryLine in snapshottedCusEntryLineList)
			{
				var newItem = new FrenchEntryLineChildSnapshotCusEntryLine();
				newItem.LineNumber = snapshottedCusEntryLine.LineNumber;

				var myChildData = new List<FrenchEntryLineChildSnapshotCusEntryLineChildData>();
				foreach (var child in snapshottedCusEntryLine.Children)
				{
					var childData = new FrenchEntryLineChildSnapshotCusEntryLineChildData();
					childData.Type = child.Type;
					childData.Code = child.Code;
					childData.Reference = child.Reference;
					myChildData.Add(childData);
				}
				newItem.ChildData = myChildData.ToArray();
				myItems.Add(newItem);
			}
			snapshot.CusEntryLine = myItems.ToArray();
			return snapshot;
		}

		readonly ISnapshottedCusEntryLine[] snapshottedCusEntryLineList;
	}
}
