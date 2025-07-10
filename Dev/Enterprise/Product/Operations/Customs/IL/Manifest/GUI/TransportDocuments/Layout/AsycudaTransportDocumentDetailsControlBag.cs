using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public class AsycudaTransportDocumentDetailsControlBag : ControlBag
	{
		public AsycudaTransportDocumentDetailsControlBag()
		{
			TypeDropEdit = RegisterControl(nameof(AsycudaTransportDocumentDetailsControl.typeDropEdit));
			ReferenceTextBox = RegisterControl(nameof(AsycudaTransportDocumentDetailsControl.referenceTextBox));
		}

		public ControlReference TypeDropEdit { get; }

		public ControlReference ReferenceTextBox { get; }

		public static AsycudaTransportDocumentDetailsControlBag Instance => asycudaTransportDocumentDetailsControlBag.Value;

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<AsycudaTransportDocumentDetailsControlBag> asycudaTransportDocumentDetailsControlBag = new Lazy<AsycudaTransportDocumentDetailsControlBag>(() => new AsycudaTransportDocumentDetailsControlBag());

		protected override Control CreateTemplate() => new AsycudaTransportDocumentDetailsControl();
	}
}
