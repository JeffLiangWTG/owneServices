using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed class SealsGridColumnsBag
	{
		public static SealsGridColumnsBag Instance => instance ?? (instance = new SealsGridColumnsBag());

		[ThreadStatic]
		static SealsGridColumnsBag instance;

		SealsGridColumnsBag()
		{
			SealNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(CusExitSeal.Schema.BK_SealNumber, 120, c =>
			{
				c.CharacterCasing = CharacterCasing.Upper;
			});
			SequenceNumberCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(CusExitSeal.Schema.BK_SequenceNumber, 75, e =>
			{
				e.BindToDecimalPlaces = null;
			});
			UnloadingStatusDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(CusExitSeal.Schema.BK_UnloadingState, 75);
		}

		public IGridColumnReference SealNumberTextBoxColumn { get; }
		public IGridColumnReference SequenceNumberCalcEditColumn { get; }
		public IGridColumnReference UnloadingStatusDropEditColumn { get; }
	}
}
