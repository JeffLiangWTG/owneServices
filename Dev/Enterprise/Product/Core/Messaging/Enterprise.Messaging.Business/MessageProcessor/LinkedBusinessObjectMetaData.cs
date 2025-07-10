using System;
using CargoWise.Types;

namespace Enterprise.Messaging.Business.MessageProcessor
{
	public sealed class LinkedBusinessObjectMetaData
	{
		public static LinkedBusinessObjectMetaData New(ZString linkTableName, ZGuid linkUniqueID, ZGuid branchPk, ZString jobNumber)
		{
			LinkedBusinessObjectMetaData result;
			if (linkTableName.IsEmpty && linkUniqueID.IsEmpty && branchPk.IsEmpty && jobNumber.IsEmpty)
			{
				result = Empty;
			}
			else
			{
				result = new LinkedBusinessObjectMetaData(linkTableName, linkUniqueID, branchPk, jobNumber);
			}
			return result;
		}

		public LinkedBusinessObjectMetaData(ZString linkTableName, ZGuid linkUniqueID, ZGuid branchPk, ZString jobNumber)
		{
			LinkTableName = linkTableName;
			LinkUniqueID = linkUniqueID;
			BranchPk = branchPk;
			JobNumber = jobNumber;
		}

		public static LinkedBusinessObjectMetaData Empty => empty ??= new(ZString.Empty, ZGuid.Empty, ZGuid.Empty, ZString.Empty);

		public ZString LinkTableName { get; }

		public ZGuid LinkUniqueID { get; }

		public ZGuid BranchPk { get; }

		public ZString JobNumber { get; }

		public override string ToString() => base.ToString() + $" (LinkTableName: {LinkTableName}, LinkUniqueID: {LinkUniqueID}, BranchPk: {BranchPk}, JobNumber: {JobNumber})";

		public override bool Equals(object obj)
		{
			if (obj is LinkedBusinessObjectMetaData other)
			{
				return LinkTableName.Equals(other.LinkTableName)
					&& LinkUniqueID.Equals(other.LinkUniqueID)
					&& BranchPk.Equals(other.BranchPk)
					&& JobNumber.Equals(other.JobNumber);
			}

			return false;
		}

		public override int GetHashCode()
		{
			if (hashCode == -1)
			{
				hashCode =
					LinkTableName.GetHashCode() ^
					LinkUniqueID.GetHashCode() ^
					BranchPk.GetHashCode() ^
					JobNumber.GetHashCode();
			}
			return hashCode;
		}

		int hashCode = -1;

		[ThreadStatic]
		static LinkedBusinessObjectMetaData empty;
	}
}
