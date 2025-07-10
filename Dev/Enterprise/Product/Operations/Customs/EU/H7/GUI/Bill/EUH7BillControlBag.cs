using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class EUH7BillControlBag : ControlBag
	{
		public static EUH7BillControlBag Instance => billControlBag.Value;

		public EUH7BillControlBag()
		{
			LocalReferenceNumberTextBox = RegisterControl(nameof(EUH7BillFieldsUserControl.LocalReferenceNumberTextBox));
			MovementReferenceNumberTextBox = RegisterControl(nameof(EUH7BillFieldsUserControl.MovementReferenceNumberTextBox));
			LocationOfGoodsUserControl = RegisterControl(nameof(EUH7BillFieldsUserControl.LocationOfGoodsUserControl));
			GoodsValueConvertToLocalCurrencyControl = RegisterControl(nameof(EUH7BillFieldsUserControl.GoodsValueConvertToLocalCurrencyControl));
			AdditionalProcedureDropEdit = RegisterControl(nameof(EUH7BillFieldsUserControl.AdditionalProcedureDropEdit));
			MessageStatusDropEdit = RegisterControl(nameof(EUH7BillFieldsUserControl.MessageStatusDropEdit));
			AdditionalProcedureCodesUserControl = RegisterControl(nameof(EUH7BillFieldsUserControl.AdditionalProcedureCodesUserControl));
			StandAloneDeclarationUserControl = RegisterControl(nameof(EUH7BillFieldsUserControl.StandAloneDeclarationUserControl));
			ContainerUserControl = RegisterControl(nameof(EUH7BillFieldsUserControl.ContainerUserControl));
		}

		public ControlReference LocalReferenceNumberTextBox { get; }
		public ControlReference MovementReferenceNumberTextBox { get; }
		public ControlReference LocationOfGoodsUserControl { get; }
		public ControlReference GoodsValueConvertToLocalCurrencyControl { get; }
		public ControlReference AdditionalProcedureDropEdit { get; }
		public ControlReference MessageStatusDropEdit { get; }
		public ControlReference AdditionalProcedureCodesUserControl { get; }
		public ControlReference StandAloneDeclarationUserControl { get; }
		public ControlReference ContainerUserControl { get; }

		protected override Control CreateTemplate() => new EUH7BillFieldsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		readonly static Lazy<EUH7BillControlBag> billControlBag = new Lazy<EUH7BillControlBag>(() => new EUH7BillControlBag());
	}
}
