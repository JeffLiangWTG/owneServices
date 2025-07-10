using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class VatTypeAndDescriptionUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember
{
	public VatTypeAndDescriptionUserControl()
	{
		InitializeComponent();
		Extensions = new DefaultControlExtensionCollection(this);
	}

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		base.SetDataBinding(dataSource, dataMember);
		Extensions.SetDataBinding(dataSource, dataMember);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Extensions.Dispose();
		}
		base.Dispose(disposing);
	}

	#region IResourceStringBindingMember Members

	string IResourceStringBindingMember.ResourceStringBindingMember => nameof(JobComInvoiceLine.JI_ZZF_NKTaxType);

	#endregion

	#region IExtendedControl Members

	Control IExtendedControl.Host => this;

	IControlExtensionCollection IExtendedControl.Extensions => Extensions;

	IControlExtensionCollection Extensions { get; }

	#endregion
}
