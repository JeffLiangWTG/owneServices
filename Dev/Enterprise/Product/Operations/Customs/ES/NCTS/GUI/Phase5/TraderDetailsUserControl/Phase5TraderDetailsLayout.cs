using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public sealed class Phase5TraderDetailsLayout : IPanelLayoutProvider
	{
		public Phase5TraderDetailsLayout()
		{
			Layout = CreateDepartureDetailsLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateDepartureDetailsLayout()
		{
			var builder = new TraderDetailsLayoutBuilder<Business.NctsHeader>();
			var commonBag = EU.NCTS.GUI.TraderDetailsControlBag.Instance;
			var esBag = TraderDetailsControlBag.Instance;
			builder.AddControlBag(commonBag);
			builder.AddControlBag(esBag);

			builder.AddColumn();
			builder.Add(commonBag.PrincipalDocAddressControl, ControlWidthClass.LongControl);
			builder.Add(commonBag.ConsignorDocAddressControl, ControlWidthClass.LongControl);
			builder.Add(commonBag.ConsigneeDocAddressControl, ControlWidthClass.LongControl);
			builder.Add(commonBag.RepresentativeDocAddressControl, ControlWidthClass.LongControl);

			builder.Add(esBag.BrokerCodeFindBox, ControlWidthClass.Long);
			builder.Add(esBag.CertificateDropEdit, ControlWidthClass.Long);
			builder.Add(esBag.TrainingCheckBox, ControlWidthClass.Long);

			builder.AddControlBehaviour(commonBag.RepresentativeDocAddressControl, new DocAddressControlDisplayModeCompactWithOverrideBehaviour());
			builder.SetVisibility(esBag.BrokerCodeFindBox, h => !h.IsPhaseStatusTNN);
			builder.SetVisibility(esBag.CertificateDropEdit, h => !h.IsPhaseStatusTNN);
			builder.SetVisibility(esBag.TrainingCheckBox, h => ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem() && !h.IsPhaseStatusTNN);

			return builder.Build();
		}
	}
}
