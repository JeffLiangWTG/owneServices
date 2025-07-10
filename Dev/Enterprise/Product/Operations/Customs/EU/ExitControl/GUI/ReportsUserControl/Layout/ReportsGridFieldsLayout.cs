using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed class ReportsGridFieldsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public ReportsGridFieldsLayout()
		{
			Layout = CreateLayout();
		}

		static PanelLayout CreateLayout()
		{
			var builder = new ReportsGridFieldsLayoutBuilder<Business.CusExitReport>();

			var euBag = ReportsGridFieldsControlBag.Instance;

			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(euBag.ConsignmentGuidDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.OfficeOfExitCodeFindBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(euBag.TransportIDTextBox, ControlWidthClass.Auto);
			builder.Add(euBag.TransportNationalityDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.TransportTypeDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
