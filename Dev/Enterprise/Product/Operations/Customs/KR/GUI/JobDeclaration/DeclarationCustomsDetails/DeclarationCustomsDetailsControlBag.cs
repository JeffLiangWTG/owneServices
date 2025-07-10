using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class DeclarationCustomsDetailsControlBag : ControlBag
	{
		[ThreadStatic]
		static DeclarationCustomsDetailsControlBag instance;

		public static DeclarationCustomsDetailsControlBag Instance => instance ?? (instance = new DeclarationCustomsDetailsControlBag());

		protected override Control CreateTemplate() => new DeclarationCustomsDetailsUserControl();

		DeclarationCustomsDetailsControlBag()
		{
			CustomsOfficeCodeFindBox = RegisterControl(nameof(DeclarationCustomsDetailsUserControl.CustomsOfficeCodeFindBox));
			DepartmentCodeFindBox = RegisterControl(nameof(DeclarationCustomsDetailsUserControl.DepartmentCodeFindBox));
			DepartureCountryCodeFindBox = RegisterControl(nameof(DeclarationCustomsDetailsUserControl.DepartureCountryCodeFindBox));
			ContainerPackDropEdit = RegisterControl(nameof(DeclarationCustomsDetailsUserControl.ContainerPackDropEdit));
			BondedAreaCodeFindBox = RegisterControl(nameof(DeclarationCustomsDetailsUserControl.BondedAreaCodeFindBox));
			LocationIDInBondedAreaTextBox = RegisterControl(nameof(DeclarationCustomsDetailsUserControl.LocationIDInBondedAreaTextBox));
			UnderbondMovementArrivalDateEdit = RegisterControl(nameof(DeclarationCustomsDetailsUserControl.UnderbondMovementArrivalDateEdit));
			CustomsBrokerCommentUserControl = RegisterControl(nameof(DeclarationCustomsDetailsUserControl.CustomsBrokerCommentUserControl));
			SouthNorthTradeTypeDropEdit = RegisterControl(nameof(DeclarationCustomsDetailsUserControl.SouthNorthTradeTypeDropEdit));
			GoldTradeTransactionYNDropEdit = RegisterControl(nameof(DeclarationCustomsDetailsUserControl.GoldTradeTransactionYNDropEdit));
		}

		public ControlReference CustomsOfficeCodeFindBox { get; }
		public ControlReference DepartmentCodeFindBox { get; }
		public ControlReference DepartureCountryCodeFindBox { get; }
		public ControlReference ContainerPackDropEdit { get; }
		public ControlReference BondedAreaCodeFindBox { get; }
		public ControlReference LocationIDInBondedAreaTextBox { get; }
		public ControlReference UnderbondMovementArrivalDateEdit { get; }
		public ControlReference CustomsBrokerCommentUserControl { get; }
		public ControlReference SouthNorthTradeTypeDropEdit { get; }
		public ControlReference GoldTradeTransactionYNDropEdit { get; }
	}
}
