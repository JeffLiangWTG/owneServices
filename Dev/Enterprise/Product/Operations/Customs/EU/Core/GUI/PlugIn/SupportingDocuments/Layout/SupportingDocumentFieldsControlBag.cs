using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public sealed class SupportingDocumentFieldsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new SupportingDocumentFieldsControl();

		[ThreadStatic]
		static SupportingDocumentFieldsControlBag instance;
		public static SupportingDocumentFieldsControlBag Instance => instance ?? (instance = new SupportingDocumentFieldsControlBag());

		SupportingDocumentFieldsControlBag()
		{
			CodeCodeFindBox = RegisterControl(nameof(SupportingDocumentFieldsControl.CodeCodeFindBox));
			ValueCalcEdit = RegisterControl(nameof(SupportingDocumentFieldsControl.ValueCalcEdit));
			DateOfIssueDateEdit = RegisterControl(nameof(SupportingDocumentFieldsControl.DateOfIssueDateEdit));
			UnitOfQuantity2TextBox = RegisterControl(nameof(SupportingDocumentFieldsControl.UnitOfQuantity2TextBox));
			ReferenceNumberTextBox = RegisterControl(nameof(SupportingDocumentFieldsControl.ReferenceNumberTextBox));
			CurrencyCodeFindBox = RegisterControl(nameof(SupportingDocumentFieldsControl.CurrencyCodeFindBox));
			UnitOfQuantityTextBox = RegisterControl(nameof(SupportingDocumentFieldsControl.UnitOfQuantityTextBox));
			UnitOfQuantityDropEdit = RegisterControl(nameof(SupportingDocumentFieldsControl.UnitOfQuantityDropEdit));
			QuantityCalcDropEdit = RegisterControl(nameof(SupportingDocumentFieldsControl.QuantityCalcDropEdit));
			Quantity2CalcEdit = RegisterControl(nameof(SupportingDocumentFieldsControl.Quantity2CalcEdit));
			QuantityCalcEdit = RegisterControl(nameof(SupportingDocumentFieldsControl.QuantityCalcEdit));
			DateOfExpiryDateEdit = RegisterControl(nameof(SupportingDocumentFieldsControl.DateOfExpiryDateEdit));
			ReferenceNumberCodeFindBox = RegisterControl(nameof(SupportingDocumentFieldsControl.ReferenceNumberCodeFindBox));
			StatusDropEdit = RegisterControl(nameof(SupportingDocumentFieldsControl.StatusDropEdit));
			AdditionalDescriptionTextBox = RegisterControl(nameof(SupportingDocumentFieldsControl.AdditionalDescriptionTextBox));
			DocumentLineNoCalcEdit = RegisterControl(nameof(SupportingDocumentFieldsControl.DocumentLineNoCalcEdit));
		}

		public ControlReference CodeCodeFindBox { get; }
		public ControlReference ValueCalcEdit { get; }
		public ControlReference DateOfIssueDateEdit { get; }
		public ControlReference UnitOfQuantity2TextBox { get; }
		public ControlReference ReferenceNumberTextBox { get; }
		public ControlReference CurrencyCodeFindBox { get; }
		public ControlReference UnitOfQuantityTextBox { get; }
		public ControlReference UnitOfQuantityDropEdit { get; }
		public ControlReference QuantityCalcDropEdit { get; }
		public ControlReference Quantity2CalcEdit { get; }
		public ControlReference QuantityCalcEdit { get; }
		public ControlReference DateOfExpiryDateEdit { get; }
		public ControlReference ReferenceNumberCodeFindBox { get; }
		public ControlReference StatusDropEdit { get; }
		public ControlReference AdditionalDescriptionTextBox { get; }
		public ControlReference DocumentLineNoCalcEdit { get; }
	}
}
