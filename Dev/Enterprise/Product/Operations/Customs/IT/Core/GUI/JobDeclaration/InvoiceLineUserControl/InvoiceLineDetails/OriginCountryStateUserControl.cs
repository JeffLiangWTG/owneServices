using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class OriginCountryStateUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember
{
	public OriginCountryStateUserControl()
	{
		InitializeComponent();
		extensions = new DefaultControlExtensionCollection(this);
	}

	#region IExtendedControl Members

	Control IExtendedControl.Host => this;

	IControlExtensionCollection IExtendedControl.Extensions => extensions;

	#endregion

	#region IResourceStringBindingMember Members

	string IResourceStringBindingMember.ResourceStringBindingMember => nameof(JobComInvoiceLine.JI_CountryOfOrigin);

	#endregion

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			extensions.Dispose();
		}

		base.Dispose(disposing);
	}

	readonly DefaultControlExtensionCollection extensions;
}
