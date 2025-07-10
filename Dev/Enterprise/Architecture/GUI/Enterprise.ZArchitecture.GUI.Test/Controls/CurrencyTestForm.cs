using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class CurrencyTestForm : ZChildForm
	{
		public CurrencyTestForm(BusinessObject bizObj)
			: base(bizObj)
		{
		}

		public ZCalcFindBox CalcFindBox;
		public ZTextBox TextBox;

		protected override void InitializeComponent()
		{
			base.InitializeComponent();
			CalcFindBox = new ZCalcFindBox();
			CalcFindBox.Name = "CalcFindBox";
			CalcFindBox.BindToAmount = DummyBusinessObject.Schema.Z0_AnotherDecimal;
			CalcFindBox.BindToUnit = DummyBusinessObject.Schema.Z0_Guid;
			CalcFindBox.BindToList = "Collection";
			CalcFindBox.Location = new Point(20, 20);

			TextBox = new ZTextBox();
			TextBox.Name = "TextBox";
			TextBox.BindTo = DummyBusinessObject.Schema.Z0_Code;
			TextBox.Location = new Point(20, 50);

			Controls.Add(CalcFindBox);
			Controls.Add(TextBox);
		}
	}
}
