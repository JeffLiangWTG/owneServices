using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.Customs.IT.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.H7.GUI;
public class ITH7BillLayouts : IPanelLayoutProvider
{
	public ITH7BillLayouts()
	{
		BillDetails = CreateBillDetailsLayout();
	}

	public PanelLayout Layout => BillDetails;

	PanelLayout BillDetails { get; }

	PanelLayout CreateBillDetailsLayout()
	{
		var builder = new BillLayoutBuilder<AsycudaBill>();
		var common = builder.CommonBag;
		var billControlBag = EUH7BillControlBag.Instance;
		builder.AddControlBag(billControlBag);

		builder.AddColumn();
		builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
		builder.Add(billControlBag.AdditionalProcedureDropEdit, ControlWidthClass.Long);
		builder.Add(billControlBag.MovementReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
		builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
		builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
		builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
		builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
		builder.Add(billControlBag.LocationOfGoodsUserControl, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(billControlBag.LocalReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
		builder.Add(common.MarksAndNumbersTextBox, ControlWidthClass.Long);
		builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
		builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
		builder.Add(common.UCRNumberTextBox, ControlWidthClass.Long);
		builder.Add(billControlBag.StandAloneDeclarationUserControl, ControlWidthClass.Long);
		builder.Add(billControlBag.GoodsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
		builder.Add(common.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
		builder.Add(common.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);

		builder.SetCaption(billControlBag.AdditionalProcedureDropEdit, _ => Res.GetData("58857d42-e854-4deb-adae-5405f1439eae", "Add. Procedure(s)"));
		builder.SetCaption(billControlBag.LocationOfGoodsUserControl, _ => Res.GetData("d43cba27-2053-40ae-b95c-c02a93acfcee", "Location of Goods"));
		builder.SetCaption(common.GrossWeightCalcDropEdit, _ => Res.GetData("45dbd998-f0cc-4645-be73-6cebb9afe1ff", "Gross Mass"));

		return builder.Build();
	}
}
