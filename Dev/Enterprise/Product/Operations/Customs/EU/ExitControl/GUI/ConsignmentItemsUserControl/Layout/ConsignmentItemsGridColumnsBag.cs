using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed class ConsignmentItemsGridColumnsBag
	{
		public static ConsignmentItemsGridColumnsBag Instance => instance ?? (instance = new ConsignmentItemsGridColumnsBag());

		[ThreadStatic]
		static ConsignmentItemsGridColumnsBag instance;

		ConsignmentItemsGridColumnsBag()
		{
			LineNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(CusExitConsignmentItem.Schema.CCI_LineNumber, 80);
			GrossMassCalEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(CusExitConsignmentItem.Schema.CCI_GrossMass, 100,
				c => c.BindToDecimalPlaces = null);
			NetMassCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(CusExitConsignmentItem.Schema.CCI_NetMass, 100,
				c => c.BindToDecimalPlaces = null);
			UniqueConsignmentReferenceTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(
				CusExitConsignmentItem.Schema.CCI_UniqueConsignmentReference, 140,
				c => c.CharacterCasing = CharacterCasing.Upper);
			UniqueConsignmentReferenceStatusDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(
				CusExitConsignmentItem.Schema.CCI_UniqueConsignmentReferenceStatus, 50);
			DiscrepancyStatusDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(
				CusExitConsignmentItem.Schema.CCI_DiscrepancyStatus, 50);
		}

		public IGridColumnReference UniqueConsignmentReferenceTextBoxColumn { get; }
		public IGridColumnReference LineNumberTextBoxColumn { get; }
		public IGridColumnReference GrossMassCalEditColumn { get; }
		public IGridColumnReference NetMassCalcEditColumn { get; }
		public IGridColumnReference UniqueConsignmentReferenceStatusDropEditColumn { get; }
		public IGridColumnReference DiscrepancyStatusDropEditColumn { get; }
	}
}
