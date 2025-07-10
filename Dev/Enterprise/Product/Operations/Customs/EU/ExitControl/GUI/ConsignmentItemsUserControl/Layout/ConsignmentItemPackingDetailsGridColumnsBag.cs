using System;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed class ConsignmentItemPackingDetailsGridColumnsBag
	{
		public static ConsignmentItemPackingDetailsGridColumnsBag Instance => instance ?? (instance = new ConsignmentItemPackingDetailsGridColumnsBag());

		[ThreadStatic]
		static ConsignmentItemPackingDetailsGridColumnsBag instance;

		ConsignmentItemPackingDetailsGridColumnsBag()
		{
			var packagePropertyPrefix = nameof(CusExitConsignmentPivot.Package) + "+";
			PackageSequenceTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(
				packagePropertyPrefix + CusExitConsignmentPackage.Schema.CXP_Sequence,
				50);

			PackageTypeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(
				packagePropertyPrefix + CusExitConsignmentPackage.Schema.CXP_PackageType,
				60);

			PackageQuantityCalEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(
				packagePropertyPrefix + CusExitConsignmentPackage.Schema.CXP_Quantity,
				100, c => c.BindToDecimalPlaces = null);

			PackageMarksAndNumbersStatusDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(
				packagePropertyPrefix + CusExitConsignmentPackage.Schema.CXP_MarksAndNumbersStatus,
				50);

			PackageMarksAndNumbersTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(
				packagePropertyPrefix + CusExitConsignmentPackage.Schema.CXP_MarksAndNumbers,
				100);

			ContainerGuidDropEditColumn = new GridColumnReference<ZGuidDropEditColumnStyleInfo>(
				CusExitConsignmentPivot.Schema.CNP_CXN_Container,
				112,
				c => c.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode);
		}

		public IGridColumnReference PackageSequenceTextBoxColumn { get; }
		public IGridColumnReference PackageTypeDropEditColumn { get; }
		public IGridColumnReference PackageQuantityCalEditColumn { get; }
		public IGridColumnReference PackageMarksAndNumbersTextBoxColumn { get; }
		public IGridColumnReference PackageMarksAndNumbersStatusDropEditColumn { get; }
		public IGridColumnReference ContainerGuidDropEditColumn { get; }
	}
}
