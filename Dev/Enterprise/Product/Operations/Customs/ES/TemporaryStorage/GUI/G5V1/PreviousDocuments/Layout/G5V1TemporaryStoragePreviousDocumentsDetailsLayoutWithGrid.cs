using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public class G5V1TemporaryStoragePreviousDocumentsDetailsLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public Type GridUserControlType => typeof(EU.TemporaryStorage.GUI.UCC6TemporaryStoragePreviousDocumentsUserControlWithGrid);

		PanelLayout IPanelLayoutProvider.Layout => CreateLayout();

		static PanelLayout CreateLayout()
		{
			var builder = new EU.TemporaryStorage.GUI.UCC6TemporaryStoragePreviousDocumentsDetailsLayoutBuilder<EU.Business.CusTempStorage.TemporaryStoragePreviousDocument>();
			var euBag = builder.CommonBag;
			var esBag = G5V1TemporaryStoragePreviousDocumentsDetailsControlBag.Instance;
			builder.AddControlBag(esBag);

			builder.AddColumn();
			builder.Add(euBag.TypeCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.ReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(euBag.GoodItemIdentifierCalcEdit, ControlWidthClass.Long);
			builder.Add(esBag.ReferenceNumber2TextBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
