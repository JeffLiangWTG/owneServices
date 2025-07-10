using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public class AsycudaTransportDocumentsWithGridLayout : IPanelLayoutWithGridProvider
	{
		public AsycudaTransportDocumentsWithGridLayout()
		{
			asycudaTransportDocumentsLayout = CreateAsycudaTransportDocumentsLayout();
		}

		public Type GridUserControlType => typeof(AsycudaTransportDocumentsGridControl);

		public PanelLayout Layout => asycudaTransportDocumentsLayout;

		PanelLayout CreateAsycudaTransportDocumentsLayout()
		{
			var builder = new AsycudaTransportDocumentDetailsBuilder();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.TypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceTextBox, ControlWidthClass.Long);
			return builder.Build();
		}

		readonly PanelLayout asycudaTransportDocumentsLayout;
	}
}
