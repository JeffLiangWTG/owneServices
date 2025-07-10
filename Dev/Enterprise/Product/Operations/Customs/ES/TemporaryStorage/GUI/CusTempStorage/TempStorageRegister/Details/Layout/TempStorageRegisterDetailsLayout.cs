using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public sealed class TempStorageRegisterDetailsLayout : IPanelLayoutProvider
	{
		public TempStorageRegisterDetailsLayout()
		{
			Layout = CreateTempStorageRegisterHeaderLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateTempStorageRegisterHeaderLayout()
		{
			var builder = new TempStorageRegisterDetailsLayoutBuilder<CusTempStorageRegHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.LineNumberCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.LocationOfGoodsTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.GrossWeightUQTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.GrossWeightRemainingCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.GoodsOwnerIdentifierTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.OwnerReferenceNumberTextBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(commonBag.LimitDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.UnionStatusDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.PackageTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.PackagesRemainingCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.PackageMarksTextBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(commonBag.BondAmountRemainingCalculatedCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsStatusDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
