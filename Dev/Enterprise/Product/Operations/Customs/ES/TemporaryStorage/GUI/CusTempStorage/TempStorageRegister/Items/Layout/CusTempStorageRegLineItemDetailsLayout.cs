using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public class CusTempStorageRegLineItemDetailsLayout : IPanelLayoutProvider
	{
		public CusTempStorageRegLineItemDetailsLayout()
		{
			Layout = CreateTempStorageRegisterPremisesLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateTempStorageRegisterPremisesLayout()
		{
			var builder = new CusTempStorageRegLineItemDetailsLayoutBuilder<CusTempStorageRegHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.GoodsItemNumberCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.TariffTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CusC4NumberTextBox, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
