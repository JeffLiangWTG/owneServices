using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

public class UCC6TemporaryStoragePreviousDocumentsDetailsLayoutWithGrid : IPanelLayoutWithGridProvider
{
	public Type GridUserControlType => typeof(EU.TemporaryStorage.GUI.UCC6TemporaryStoragePreviousDocumentsUserControlWithGrid);

	PanelLayout IPanelLayoutProvider.Layout => CreateLayout();

	static PanelLayout CreateLayout()
	{
		var builder = new EU.TemporaryStorage.GUI.UCC6TemporaryStoragePreviousDocumentsDetailsLayoutBuilder<TemporaryStoragePreviousDocument>();
		var euBag = EU.TemporaryStorage.GUI.UCC6TemporaryStoragePreviousDocumentsDetailsControlBag.Instance;

		builder.AddControlBag(euBag);
		builder.AddColumn();
		builder.Add(euBag.TypeCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.ReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(euBag.GoodItemIdentifierCalcEdit, ControlWidthClass.Long);
		builder.Add(euBag.PackageCalcDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.QuantityCalcDropEdit, ControlWidthClass.Long);

		builder.SetVisibility(euBag.GoodItemIdentifierCalcEdit, x => !x.TemporaryStorageHeader.IsTransfer && !x.TemporaryStorageHeader.IsDeconsolidation, x => x.TemporaryStorageHeader.AMA_MessageTypeInfo);
		builder.SetCaption(euBag.PackageCalcDropEdit, x => DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStoragePreviousDocument), nameof(TemporaryStoragePreviousDocument.CSI_PackQty)));

		return builder.Build();
	}
}
