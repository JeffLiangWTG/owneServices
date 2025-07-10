using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

class VehicleDetailsControlBag : ControlBag
{
	public VehicleDetailsControlBag()
	{
		VinTextBox = RegisterControl(nameof(VehicleDetailsUserControl.VinTextBox));
		BrandTextBox = RegisterControl(nameof(VehicleDetailsUserControl.BrandTextBox));
		ModelTextBox = RegisterControl(nameof(VehicleDetailsUserControl.ModelTextBox));
	}

	public static VehicleDetailsControlBag Instance => instance ??= new VehicleDetailsControlBag();

	[ThreadStatic]
	static VehicleDetailsControlBag instance;

	public ControlReference VinTextBox { get; }
	public ControlReference BrandTextBox { get; }
	public ControlReference ModelTextBox { get; }

	protected override Control CreateTemplate() => new VehicleDetailsUserControl();
}
