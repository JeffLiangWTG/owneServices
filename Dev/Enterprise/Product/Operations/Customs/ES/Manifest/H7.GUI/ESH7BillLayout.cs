using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public class ESH7BillLayout : IPanelLayoutProvider
	{
		public ESH7BillLayout()
		{
			BillDetails = CreateBillDetailsLayout();
		}

		public PanelLayout Layout => BillDetails;

		PanelLayout BillDetails { get; }

		PanelLayout CreateBillDetailsLayout()
		{
			var builder = new BillLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			var euH7BillControlBag = EUH7BillControlBag.Instance;
			builder.AddControlBag(euH7BillControlBag);
			var esH7BillControlBag = ESH7AsycudaBillControlBag.Instance;
			builder.AddControlBag(esH7BillControlBag);

			builder.AddColumn();
			builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
			builder.Add(euH7BillControlBag.AdditionalProcedureDropEdit, ControlWidthClass.Long);
			builder.Add(esH7BillControlBag.H7MovementReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(esH7BillControlBag.G3MovementReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
			builder.Add(euH7BillControlBag.LocationOfGoodsUserControl, ControlWidthClass.Long);
			builder.Add(euH7BillControlBag.MessageStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.BillStatusDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(esH7BillControlBag.G3LocalReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(common.MarksAndNumbersTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.UCRNumberTextBox, ControlWidthClass.Long);
			builder.Add(euH7BillControlBag.GoodsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(esH7BillControlBag.DocumentationRequiredTextBox, ControlWidthClass.Long);
			builder.Add(euH7BillControlBag.StandAloneDeclarationUserControl, ControlWidthClass.Long);

			builder.SetCaption(euH7BillControlBag.AdditionalProcedureDropEdit, _ => Res.GetData("41aa51e8-2ac8-40b2-9590-12b845e943c9", "Add. Procedure(s)"));
			builder.SetCaption(common.GrossWeightCalcDropEdit, _ => Res.GetData("fc00dc75-8995-4744-aa24-b00285547e11", "Gross Mass"));
			builder.SetCaption(euH7BillControlBag.LocationOfGoodsUserControl, _ => Res.GetData("462dfcc4-957a-4263-ad96-d935a1254059", "Location of Goods"));

			return builder.Build();
		}
	}
}
