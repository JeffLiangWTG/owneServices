using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class DV1DetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new DV1DetailsUserControl();

		public static DV1DetailsControlBag Instance => instance ?? (instance = new DV1DetailsControlBag());

		[ThreadStatic]
		static DV1DetailsControlBag instance;

		DV1DetailsControlBag()
		{
			RelationshipDropEdit = RegisterControl(nameof(DV1DetailsUserControl.RelationshipDropEdit));
			RelationDetailsTextBox = RegisterControl(nameof(DV1DetailsUserControl.RelationDetailsTextBox));
			PriceInfluenceDropEdit = RegisterControl(nameof(DV1DetailsUserControl.PriceInfluenceDropEdit));
			RestrictionsDropEdit = RegisterControl(nameof(DV1DetailsUserControl.RestrictionsDropEdit));
			ConsiderationDropEdit = RegisterControl(nameof(DV1DetailsUserControl.ConsiderationDropEdit));
			RestrictionsConsiderationTextBox = RegisterControl(nameof(DV1DetailsUserControl.RestrictionsConsiderationTextBox));
			RoyalitiesLicenceDropEdit = RegisterControl(nameof(DV1DetailsUserControl.RoyalitiesLicenceDropEdit));
			RoyalitiesLicenceDetailsTextBox = RegisterControl(nameof(DV1DetailsUserControl.RoyalitiesLicenceDetailsTextBox));
			ResaleDropEdit = RegisterControl(nameof(DV1DetailsUserControl.ResaleDropEdit));
			ResaleDetailsTextBox = RegisterControl(nameof(DV1DetailsUserControl.ResaleDetailsTextBox));
			CustomsDecisionNumberTextBox = RegisterControl(nameof(DV1DetailsUserControl.CustomsDecisionNumberTextBox));
			CloseApproximationDropEdit = RegisterControl(nameof(DV1DetailsUserControl.CloseApproximationDropEdit));
			ContractNumberTextBox = RegisterControl(nameof(DV1DetailsUserControl.ContractNumberTextBox));
			ContractDateDateEdit = RegisterControl(nameof(DV1DetailsUserControl.ContractDateDateEdit));
		}

		public ControlReference RelationshipDropEdit { get; }
		public ControlReference RelationDetailsTextBox { get; }
		public ControlReference PriceInfluenceDropEdit { get; }
		public ControlReference RestrictionsDropEdit { get; }
		public ControlReference ConsiderationDropEdit { get; }
		public ControlReference RestrictionsConsiderationTextBox { get; }
		public ControlReference RoyalitiesLicenceDropEdit { get; }
		public ControlReference RoyalitiesLicenceDetailsTextBox { get; }
		public ControlReference ResaleDropEdit { get; }
		public ControlReference ResaleDetailsTextBox { get; }
		public ControlReference CustomsDecisionNumberTextBox { get; }
		public ControlReference CloseApproximationDropEdit { get; }
		public ControlReference ContractNumberTextBox { get; }
		public ControlReference ContractDateDateEdit { get; }
	}
}
