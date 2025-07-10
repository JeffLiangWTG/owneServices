using System;
using System.Collections.Generic;

namespace Enterprise.Scheduler.GraphEngine
{
	public interface IQueueState
	{
		string TableCode { get; }
		Guid ParentID { get; }
		Guid Identifier { get; }
		IEnumerable<string> Keys { get; }
		string Status { get; }
		string OrderInfo { get; }
		bool HasChanges { get; }
		bool IsInDatabase { get; }
		void UpdateStatus(string newStatus);
		Guid ChainID { get; }
		bool StatusChanged { get; }
		void SetChainId(Guid id);
		bool ChainIdChanged { get; }
		void SetAsDatabaseSynced();
		string GetParentDetails();
		string GetFrontQueueDetails();
	}
}
