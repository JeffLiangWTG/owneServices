using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
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
			var builder = new HeaderDetailsLayoutBuilder<Business.CusExitHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.BranchGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.BrokerCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ExporterOrgAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.CarrierAddressWithContactControl, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
