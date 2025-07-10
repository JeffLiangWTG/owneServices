using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public class G5V1TemporaryStorageUserControlBag : ControlBag
	{
		public static G5V1TemporaryStorageUserControlBag Instance => g5V1TemporaryStorageControlBag.Value;
		G5V1TemporaryStorageUserControlBag()
		{
			DocumentsTabControl = RegisterControl(nameof(G5V1TemporaryStorageUserControl.DocumentsTabControl));
			DestinationCustomsOfficeCodeFindBox = RegisterControl(nameof(G5V1TemporaryStorageUserControl.DestinationCustomsOfficeCodeFindBox));
			ManualLocationOfGoodsCodeFindBox = RegisterControl(nameof(G5V1TemporaryStorageUserControl.ManualLocationOfGoodsCodeFindBox));
			DestinationLocationOfGoodsUserControl = RegisterControl(nameof(G5V1TemporaryStorageUserControl.DestinationLocationOfGoodsUserControl));
			CertificateDropEdit = RegisterControl(nameof(G5V1TemporaryStorageUserControl.CertificateDropEdit));
			TransportDocumentTypeDropEdit = RegisterControl(nameof(G5V1TemporaryStorageUserControl.TransportDocumentTypeDropEdit));
			TransportDocumentTextBox = RegisterControl(nameof(G5V1TemporaryStorageUserControl.TransportDocumentTextBox));
			DeclarationDetailsGroupBox = RegisterControl(nameof(G5V1TemporaryStorageUserControl.DeclarationDetailsGroupBox));
			TrainingCheckBox = RegisterControl(nameof(G5V1TemporaryStorageUserControl.TrainingCheckBox));
			GuaranteeGroupBox = RegisterControl(nameof(G5V1TemporaryStorageUserControl.GuaranteeGroupBox));
			UnionGoodsCheckBox = RegisterControl(nameof(G5V1TemporaryStorageUserControl.UnionGoodsCheckBox));
			IsSimplifiedCheckBox = RegisterControl(nameof(G5V1TemporaryStorageUserControl.IsSimplifiedCheckBox));
			MovementOfContainersOnlyCheckBox = RegisterControl(nameof(G5V1TemporaryStorageUserControl.MovementOfContainersOnlyCheckBox));
		}

		public ControlReference DocumentsTabControl { get; }

		public ControlReference DestinationCustomsOfficeCodeFindBox { get; }

		public ControlReference ManualLocationOfGoodsCodeFindBox { get; }

		public ControlReference DestinationLocationOfGoodsUserControl { get; }

		public ControlReference CertificateDropEdit { get; }

		public ControlReference IsSimplifiedCheckBox { get; }

		public ControlReference TransportDocumentTypeDropEdit { get; }

		public ControlReference TransportDocumentTextBox { get; }

		public ControlReference DeclarationDetailsGroupBox { get; }

		public ControlReference TrainingCheckBox { get; }

		public ControlReference GuaranteeGroupBox { get; }

		public ControlReference MovementOfContainersOnlyCheckBox { get; }

		public ControlReference UnionGoodsCheckBox { get; }

		protected override Control CreateTemplate() => new G5V1TemporaryStorageUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<G5V1TemporaryStorageUserControlBag> g5V1TemporaryStorageControlBag = new Lazy<G5V1TemporaryStorageUserControlBag>(() => new G5V1TemporaryStorageUserControlBag());
	}
}
