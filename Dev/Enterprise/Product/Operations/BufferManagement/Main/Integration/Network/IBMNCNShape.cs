using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMNCNShape : IBusiness, IProposedNetworkEntity, IBufferedItem, IApprovable
	{
		ZString BNS_Name { get; set; }
		ZString BNS_CompletionStatements { get; set; }
		ZString BNS_LayoutData { get; set; }
		ZString BNS_JobType { get; set; }
		ZString BNS_ShapeType { get; set; }
		ZString BNS_GS_NKApprovedBy { get; set; }
		ZGuid BNS_RelatedEntityID { get; set; }
		ZGuid BNS_BNS_RootShape { get; set; }

		bool IsChildOf(IBMNCNShape parentShape);
		void MakeChildOf(IBMNCNShape parentShape);
		void AddFetchHintsForBoardLoad(IEnumerable<IBMNCNShape> shapes);
		void CloneParentProperties(IBMNCNShape parentShape);

		ZDecimal BufferPenetration { get; }
		ZInt PenetratingBufferSizeInMinutes { get; }

		IBMNCNShape RootDiagram { get; }

		void DisconnectRelatedEntity();

		#region Schedule

		ZBool IsBuffered { get; }

		ZDecimal EarliestStartHours { get; }
		ZDecimal EarliestFinishHours { get; }
		ZDecimal LatestStartHours { get; }
		ZDecimal LatestFinishHours { get; }

		ZDecimal FloatHours { get; }
		ZDecimal ExplicitDurationHours { get; }

		ZBool IsCriticalPath { get; }

		ZDateTime EarliestStartTimeUtc { get; }
		ZDateTime EarliestFinishTimeUtc { get; }
		ZDateTime LatestStartTimeUtc { get; }
		ZDateTime LatestFinishTimeUtc { get; }

		ZDateTime ScheduledStartTimeUtc { get; }
		ZDateTime ScheduledFinishTimeUtc { get; }

		#endregion
	}
}
