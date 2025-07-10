using Enterprise.Customs.JP.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public class CompanyCredentialsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout
		{
			get
			{
				var builder = new MasterFiles.GUI.CompanyCredentialsLayoutBuilder<JPGlbCompanyWrapper>();
				var jpControlBag = CompanyCredentialsControlBag.Instance;
				var commonBag = builder.CommonBag;
				builder.AddControlBag(jpControlBag);

				builder.AddColumn();
				builder.Add(commonBag.ICS2CredentialUserControl, ControlWidthClass.LongControl);
				builder.Add(jpControlBag.NaccsMailboxGroupBox, ControlWidthClass.LongControl);

				return builder.Build();
			}
		}
	}
}
