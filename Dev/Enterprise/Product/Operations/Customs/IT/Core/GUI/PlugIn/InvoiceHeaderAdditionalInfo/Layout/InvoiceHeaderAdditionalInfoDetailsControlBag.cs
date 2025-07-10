using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public class InvoiceHeaderAdditionalInfoDetailsControlBag : ControlBag
{
	protected InvoiceHeaderAdditionalInfoDetailsControlBag()
	{
		DescriptionTextBox = RegisterControl(nameof(InvoiceHeaderAdditionalInfoDetailsUserControl.DescriptionTextBox));
	}

	public static InvoiceHeaderAdditionalInfoDetailsControlBag Instance => instance ?? (instance = new InvoiceHeaderAdditionalInfoDetailsControlBag());

	[ThreadStatic]
	static InvoiceHeaderAdditionalInfoDetailsControlBag instance;

	protected override Control CreateTemplate() => new InvoiceHeaderAdditionalInfoDetailsUserControl();

	public ControlReference DescriptionTextBox { get; }
}
