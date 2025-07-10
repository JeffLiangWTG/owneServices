using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public interface IFormedPagesSupporter
	{
		#region Collections

		DocFormedPagesShipmentCollection Shipments { get; }
		DocFormedPagesContainerCollection Containers { get; }
		DocFormedPagesTopLevelPackCollection TopLevelPacks { get; }
		DocPackLinesCollection PackLines { get; }
		DocJobChargeCollection AllCharges { get; }
		DocJobChargeCollection CollectCharges { get; }

		#endregion

		#region Display data

		ZString BOLClause { get; }

		#endregion

		#region Control Properties

		bool DisplayContainers { get; }
		bool HideContainerGrossWeight { get; }
		bool HideContainerTareWeight { get; }
		bool HidePackLinesInContainersSection { get; }

		#endregion

		#region Output

		DocBillOfLadingFormedPageCollection FormedPages { get; }
		ZString[] FollowOnSection { get; }
		ZBool HasFollowOnSection { get; }

		#endregion

		#region Charges Properties

		bool IsOriginal { get; }
		bool IsCopy { get; }
		bool ShouldPrintChargesAsLumpSum { get; }
		bool ShouldPrintTotalCharges { get; }

		#endregion
	}
}
