using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ValuationDeclarationTemplateDetailsControlBag : ControlBag
	{
		ValuationDeclarationTemplateDetailsControlBag()
		{
			ValuationCodeDropEdit = RegisterControl(nameof(ValuationDeclarationTemplateDetailsControlBag.ValuationCodeDropEdit));
			CustomsOfficeCodeFindBox = RegisterControl(nameof(ValuationDeclarationTemplateDetailsControlBag.CustomsOfficeCodeFindBox));
			DepartmentCodeFindBox = RegisterControl(nameof(ValuationDeclarationTemplateDetailsControlBag.DepartmentCodeFindBox));
			PONoTextBox = RegisterControl(nameof(ValuationDeclarationTemplateDetailsControlBag.PONoTextBox));
			PODateEdit = RegisterControl(nameof(ValuationDeclarationTemplateDetailsControlBag.PODateEdit));
			ServiceCodeFindBox = RegisterControl(nameof(ValuationDeclarationTemplateDetailsControlBag.ServiceCodeFindBox));
		}

		public static ValuationDeclarationTemplateDetailsControlBag Instance => instance ?? (instance = new ValuationDeclarationTemplateDetailsControlBag());
		[ThreadStatic]
		static ValuationDeclarationTemplateDetailsControlBag instance;

		public ControlReference ValuationCodeDropEdit { get; }
		public ControlReference CustomsOfficeCodeFindBox { get; }
		public ControlReference DepartmentCodeFindBox { get; }
		public ControlReference PONoTextBox { get; }
		public ControlReference PODateEdit { get; }
		public ControlReference ServiceCodeFindBox { get; }

		protected override Control CreateTemplate() => new ValuationDeclarationTemplateDetailsUserControl();
	}
}
