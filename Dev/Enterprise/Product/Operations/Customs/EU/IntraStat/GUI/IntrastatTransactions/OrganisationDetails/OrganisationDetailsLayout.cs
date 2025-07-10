
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	public sealed class OrganisationDetailsLayout : IPanelLayoutProvider
	{
		public OrganisationDetailsLayout()
		{
			Layout = CreateDetailsLayout();
		}

		public PanelLayout Layout { get; }

		PanelLayout CreateDetailsLayout()
		{
			var builder = new OrganisationDetailsLayoutBuilder<Business.CusIntrastatHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.SupplierOrganisationControl, ControlWidthClass.LongControl);
			builder.Add(commonBag.ConsigneeOrganisationControl, ControlWidthClass.LongControl);

			return builder.Build();
		}
	}
}
