using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public sealed class PreviousDocumentsFieldsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new PreviousDocumentsFieldsUserControl();

		public static PreviousDocumentsFieldsControlBag Instance => instance ?? (instance = new PreviousDocumentsFieldsControlBag());

		[ThreadStatic]
		static PreviousDocumentsFieldsControlBag instance;

		PreviousDocumentsFieldsControlBag()
		{
			CodeDropEdit = RegisterControl(nameof(PreviousDocumentsFieldsUserControl.CodeDropEdit));
			ReferenceTextBox = RegisterControl(nameof(PreviousDocumentsFieldsUserControl.ReferenceTextBox));
			ProcedureDropEdit = RegisterControl(nameof(PreviousDocumentsFieldsUserControl.ProcedureDropEdit));
			Reference2TextBox = RegisterControl(nameof(PreviousDocumentsFieldsUserControl.Reference2TextBox));
			IssueDateEdit = RegisterControl(nameof(PreviousDocumentsFieldsUserControl.IssueDateEdit));
			LineNoCalcEdit = RegisterControl(nameof(PreviousDocumentsFieldsUserControl.LineNoCalcEdit));
			CustomsOfficeCodeFindBox = RegisterControl(nameof(PreviousDocumentsFieldsUserControl.CustomsOfficeCodeFindBox));
			QuantityCalcDropEdit = RegisterControl(nameof(PreviousDocumentsFieldsUserControl.QuantityCalcDropEdit));
			Quantity2CalcDropEdit = RegisterControl(nameof(PreviousDocumentsFieldsUserControl.Quantity2CalcDropEdit));
			Quantity3CalcDropEdit = RegisterControl(nameof(PreviousDocumentsFieldsUserControl.Quantity3CalcDropEdit));
			PackageQuantityCalcDropEdit = RegisterControl(nameof(PreviousDocumentsFieldsUserControl.PackageQuantityCalcDropEdit));
			ItemNumberCalcEdit = RegisterControl(nameof(PreviousDocumentsFieldsUserControl.ItemNumberCalcEdit));
			SubTypeDropEdit = RegisterControl(nameof(PreviousDocumentsFieldsUserControl.SubTypeDropEdit));
		}

		public ControlReference CodeDropEdit { get; }

		public ControlReference ReferenceTextBox { get; }

		public ControlReference ProcedureDropEdit { get; }

		public ControlReference Reference2TextBox { get; }

		public ControlReference IssueDateEdit { get; }

		public ControlReference LineNoCalcEdit { get; }

		public ControlReference CustomsOfficeCodeFindBox { get; }

		public ControlReference QuantityCalcDropEdit { get; }

		public ControlReference Quantity2CalcDropEdit { get; }

		public ControlReference Quantity3CalcDropEdit { get; }

		public ControlReference PackageQuantityCalcDropEdit { get; }

		public ControlReference ItemNumberCalcEdit { get; }

		public ControlReference SubTypeDropEdit { get; }
	}
}
