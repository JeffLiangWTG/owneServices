using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.ExitControl.GUI
{
	public sealed class HeaderDetailsLayout : IPanelLayoutProvider
	{
		public HeaderDetailsLayout()
		{
			Layout = CreateHeaderDetailsLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateHeaderDetailsLayout()
		{
			var builder = new EU.ExitControl.GUI.HeaderDetailsLayoutBuilder<Business.CusExitHeader>();
			var commonBag = builder.CommonBag;
			var esBag = HeaderDetailsControlBag.Instance;
			builder.AddControlBag(esBag);

			builder.AddColumn();
			builder.Add(commonBag.BranchGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ExporterOrgAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.CarrierAddressWithContactControl, ControlWidthClass.Auto);
			builder.Add(esBag.BrokerCodeFindBox, ControlWidthClass.Auto);
			builder.Add(esBag.CertificateDropEdit, ControlWidthClass.Auto);
			builder.Add(esBag.TrainingCheckBox, ControlWidthClass.Long);

			builder.SetVisibility(esBag.TrainingCheckBox, h => ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem());

			return builder.Build();
		}
	}
}
