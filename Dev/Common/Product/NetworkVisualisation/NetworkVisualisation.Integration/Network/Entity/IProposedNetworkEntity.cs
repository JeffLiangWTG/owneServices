using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.PAVE.Common.Interfaces;

namespace CargoWise.NetworkVisualisation.Integration
{
	public interface IProposedNetworkEntity : INotifyPropertyChanged
	{
		string Name { get; set; }
		string JobNumber { get; }
		string JobName { get; set; }
		bool JobName_ReadOnly { get; }

		string Description { get; }
		string CompletionCriteria { get; set; }

		WorkStatus Status { get; }
		string StatusName { get; }
		string StatusDescription { get; }
		bool IsStartable { get; }

		string EstimateSummary { get; }

		bool IsOnCriticalPath { get; }
		bool CanUnlinkEntity { get; }

		IProposedNetworkEntity Parent { get; }
		IEnumerable<IEntityRelationship> Links { get; }
		IEnumerable<IEntityRelationship> PreRequisiteLinks { get; }
		IEnumerable<IEntityRelationship> PostRequisiteLinks { get; }

		bool IsSameEntity(IProposedNetworkEntity other);
		bool CanDeleteUnderlyingEntity { get; }

		// need this in the interface to be able to mock this method for testing
		// https://stackoverflow.com/questions/1410654/rhinomocks-exceptions-when-stubbing-out-equals-method
		int GetHashCode();
	}
}