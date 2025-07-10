using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

class TobaccoFieldsControlBag : ControlBag
{
	public static TobaccoFieldsControlBag Instance => instance ?? (instance = new TobaccoFieldsControlBag());

	[ThreadStatic]
	static TobaccoFieldsControlBag instance;

	TobaccoFieldsControlBag()
	{
		MainGroupDropEdit = RegisterControl(nameof(TobaccoFieldsUserControl.MainGroupDropEdit));
		SubGroupDropEdit = RegisterControl(nameof(TobaccoFieldsUserControl.SubGroupDropEdit));
		DesignationTextBox = RegisterControl(nameof(TobaccoFieldsUserControl.DesignationTextBox));
		SequentialNumberIntEdit = RegisterControl(nameof(TobaccoFieldsUserControl.SequentialNumberIntEdit));
		TobaccoBrandDropEdit = RegisterControl(nameof(TobaccoFieldsUserControl.TobaccoBrandDropEdit));
		ReverseNumberTextBox = RegisterControl(nameof(TobaccoFieldsUserControl.ReverseNumberTextBox));
		SpecialUnitOfMeasureDropEdit = RegisterControl(nameof(TobaccoFieldsUserControl.SpecialUnitOfMeasureDropEdit));
		RetailPriceCalcEdit = RegisterControl(nameof(TobaccoFieldsUserControl.RetailPriceCalcEdit));
	}

	protected override Control CreateTemplate() => new TobaccoFieldsUserControl();

	internal ControlReference MainGroupDropEdit { get; }
	internal ControlReference SubGroupDropEdit { get; }
	internal ControlReference DesignationTextBox { get; }
	internal ControlReference SequentialNumberIntEdit { get; }
	internal ControlReference TobaccoBrandDropEdit { get; }
	internal ControlReference ReverseNumberTextBox { get; }
	internal ControlReference SpecialUnitOfMeasureDropEdit { get; }
	internal ControlReference RetailPriceCalcEdit { get; }
}
