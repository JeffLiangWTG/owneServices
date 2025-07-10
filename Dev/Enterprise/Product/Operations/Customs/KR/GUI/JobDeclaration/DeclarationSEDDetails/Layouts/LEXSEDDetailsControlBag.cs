using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class LEXSEDDetailsControlBag : ControlBag
	{
		LEXSEDDetailsControlBag()
		{
			this.CustomsOfficeCodeFindBox = RegisterControl(nameof(LEXSEDDetailsUserControl.CustomsOfficeCodeFindBox));
			this.CustomsDivisionCodeFindBox = RegisterControl(nameof(LEXSEDDetailsUserControl.CustomsDivisionCodeFindBox));
			this.SubLocationOfGoodsTextBox = RegisterControl(nameof(LEXSEDDetailsUserControl.SubLocationOfGoodsTextBox));
			this.BondedAreaCodeFindBox = RegisterControl(nameof(LEXSEDDetailsUserControl.BondedAreaCodeFindBox));
			this.CrewCountCalcEdit = RegisterControl(nameof(LEXSEDDetailsUserControl.CrewCountCalcEdit));
			this.BlanketDeclarationDropEdit = RegisterControl(nameof(LEXSEDDetailsUserControl.BlanketDeclarationDropEdit));
			this.DeclarationDateEdit = RegisterControl(nameof(LEXSEDDetailsUserControl.DeclarationDateEdit));
			this.GridUserControl = RegisterControl(nameof(LEXSEDDetailsUserControl.GridUserControl));
		}

		public static LEXSEDDetailsControlBag Instance => instance ?? (instance = new LEXSEDDetailsControlBag());

		[ThreadStatic]
		static LEXSEDDetailsControlBag instance;
		public ControlReference CustomsOfficeCodeFindBox { get; }
		public ControlReference CustomsDivisionCodeFindBox { get; }
		public ControlReference SubLocationOfGoodsTextBox { get; }
		public ControlReference BondedAreaCodeFindBox { get; }
		public ControlReference CrewCountCalcEdit { get; }
		public ControlReference BlanketDeclarationDropEdit { get; }
		public ControlReference DeclarationDateEdit { get; }
		public ControlReference GridUserControl { get; }
		protected override Control CreateTemplate() => new LEXSEDDetailsUserControl();
	}
}
