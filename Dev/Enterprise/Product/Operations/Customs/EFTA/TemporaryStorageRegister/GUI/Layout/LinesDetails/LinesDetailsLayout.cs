using System;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;

public sealed class LinesDetailsLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => lineDetailsLayout.Value;

	readonly Lazy<PanelLayout> lineDetailsLayout = new(CreateLineDetailsLayout);

	static PanelLayout CreateLineDetailsLayout()
	{
		var builder = new LinesDetailsCommonLayoutBuilder<CusTempStorageRegHeader>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.LineNumberCalcEdit, ControlWidthClass.Long);
		builder.Add(commonBag.LocationofGoodsTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.OwnerReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.GoodsDescriptionTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.CustodianEORIBranchUserControl, ControlWidthClass.Long);
		builder.Add(commonBag.DisposalEntitledTraderEORIBranchUserControl, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(commonBag.LimitDateEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.PackagesRemainingCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.PackageTypeDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.OwnerReferenceTypeDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsStatusDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.UnionStatusDropEdit, ControlWidthClass.Auto);

		return builder.Build();
	}
}
