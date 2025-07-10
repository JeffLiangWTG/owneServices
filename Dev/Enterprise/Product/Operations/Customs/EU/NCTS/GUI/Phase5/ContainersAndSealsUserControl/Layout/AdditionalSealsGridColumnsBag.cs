using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class AdditionalSealsGridColumnsBag
	{
		public static AdditionalSealsGridColumnsBag Instance => instance ?? (instance = new AdditionalSealsGridColumnsBag());

		[ThreadStatic]
		static AdditionalSealsGridColumnsBag instance;

		public AdditionalSealsGridColumnsBag()
		{
			SequenceNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(CusSeal.Schema.BK_SequenceNumber, 100);
			SealNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(CusSeal.Schema.BK_SealNumber, 135);
		}

		public IGridColumnReference SequenceNumberTextBoxColumn { get; }

		public IGridColumnReference SealNumberTextBoxColumn { get; }
	}
}
