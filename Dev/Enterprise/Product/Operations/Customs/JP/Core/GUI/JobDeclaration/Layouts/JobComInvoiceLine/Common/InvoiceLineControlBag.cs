using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI;

public class InvoiceLineControlBag : ControlBag
{
	InvoiceLineControlBag()
	{
		NACCSCodeDropEdit = RegisterControl(nameof(InvoiceLineTemplate.NACCSCodeDropEdit));
		CustomsQuantityCalcDropEdit = RegisterControl(nameof(InvoiceLineTemplate.CustomsQuantityCalcDropEdit));
		CustomsSecondQuantityCalcDropEdit = RegisterControl(nameof(InvoiceLineTemplate.CustomsSecondQuantityCalcDropEdit));
		EntryInstructionGuidDropEdit = RegisterControl(nameof(InvoiceLineTemplate.EntryInstructionGuidDropEdit));
		UnitPriceCalcEdit = RegisterControl(nameof(InvoiceLineTemplate.UnitPriceCalcEdit));
		VolumeCalcDropEdit = RegisterControl(nameof(InvoiceLineTemplate.VolumeCalcDropEdit));
		WeightCalcDropEdit = RegisterControl(nameof(InvoiceLineTemplate.WeightCalcDropEdit));
		InvoiceQuantityCalcDropEdit = RegisterControl(nameof(InvoiceLineTemplate.InvoiceQuantityCalcDropEdit));
		LinePriceCurrencyCalcFindBox = RegisterControl(nameof(InvoiceLineTemplate.LinePriceCurrencyCalcFindBox));
	}

	public ControlReference NACCSCodeDropEdit { get; }
	public ControlReference EntryInstructionGuidDropEdit { get; }
	public ControlReference CustomsQuantityCalcDropEdit { get; }
	public ControlReference CustomsSecondQuantityCalcDropEdit { get; }
	public ControlReference UnitPriceCalcEdit { get; }
	public ControlReference VolumeCalcDropEdit { get; }
	public ControlReference WeightCalcDropEdit { get; }
	public ControlReference InvoiceQuantityCalcDropEdit { get; }
	public ControlReference LinePriceCurrencyCalcFindBox { get; }

	public static InvoiceLineControlBag Instance => instance ??= new InvoiceLineControlBag();

	[ThreadStatic]
	static InvoiceLineControlBag instance;

	protected override Control CreateTemplate() => new InvoiceLineTemplate();
}
