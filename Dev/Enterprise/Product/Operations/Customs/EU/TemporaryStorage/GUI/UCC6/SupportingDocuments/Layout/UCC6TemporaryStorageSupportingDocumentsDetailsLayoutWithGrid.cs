using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStorageSupportingDocumentsDetailsLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public Type GridUserControlType => typeof(UCC6TemporaryStorageSupportingDocumentsUserControlWithGrid);

		PanelLayout IPanelLayoutProvider.Layout => CreateLayout();

		PanelLayout CreateLayout()
		{
			var builder = new UCC6TemporaryStorageSupportingDocumentsDetailsLayoutBuilder();
			var commonBag = builder.CommonBag;
			builder.AddColumn();
			builder.Add(commonBag.CodeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Long);
			return builder.Build();
		}
	}
}
