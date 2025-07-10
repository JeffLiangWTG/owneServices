using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

public class TempStorageRegTransactionNewLayout : IPanelLayoutProvider
{
	public TempStorageRegTransactionNewLayout()
	{
		Layout = CreateTempStorageRegTransactionNewLayout();
	}

	PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	PanelLayout CreateTempStorageRegTransactionNewLayout()
	{
		var builder = new TempStorageRegTransactionNewLayoutBuilder<CusTempStorageRegLineTransactionFormEditable>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.PhysicalInOutDateDateEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.TransactionDateDateEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.GrossWeightCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.PackageQtyCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.InternalReferenceTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.InternalReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.ReferenceTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ReferenceTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.CommentsTextBox, ControlWidthClass.Long);

		return builder.Build();
	}
}
