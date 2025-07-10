using System;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStoragePreviousDocumentsDetailsLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public Type GridUserControlType => typeof(UCC6TemporaryStoragePreviousDocumentsUserControlWithGrid);

		PanelLayout IPanelLayoutProvider.Layout => CreateLayout();

		static PanelLayout CreateLayout()
		{
			var builder = new UCC6TemporaryStoragePreviousDocumentsDetailsLayoutBuilder<TemporaryStoragePreviousDocument>();

			var euBag = UCC6TemporaryStoragePreviousDocumentsDetailsControlBag.Instance;

			builder.AddControlBag(euBag);
			builder.AddColumn();
			builder.Add(euBag.TypeCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.ReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(euBag.GoodItemIdentifierCalcEdit, ControlWidthClass.Long);

			builder.SetVisibility(euBag.GoodItemIdentifierCalcEdit, x => !x.TemporaryStorageHeader.IsTransfer && !x.TemporaryStorageHeader.IsDeconsolidation, x => x.TemporaryStorageHeader.AMA_MessageTypeInfo);

			return builder.Build();
		}
	}
}
