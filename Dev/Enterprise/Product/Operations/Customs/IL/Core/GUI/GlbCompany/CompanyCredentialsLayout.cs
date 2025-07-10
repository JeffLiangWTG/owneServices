using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	sealed class CompanyCredentialsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new CompanyCredentialsLayoutBuilder<GlbCompanyWrapper>();
			var ilBag = CompanyCredentialsControlBag.Instance;
			builder.AddControlBag(ilBag);

			builder.AddColumn();
			builder.Add(ilBag.CompanyCredentialsDetailsUserControl, ControlWidthClass.LongControl);

			return builder.Build();
		}
	}
}
