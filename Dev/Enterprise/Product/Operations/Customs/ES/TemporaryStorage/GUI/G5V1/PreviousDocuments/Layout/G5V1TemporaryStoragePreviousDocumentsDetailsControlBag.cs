using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public sealed class G5V1TemporaryStoragePreviousDocumentsDetailsControlBag : ControlBag
	{
		G5V1TemporaryStoragePreviousDocumentsDetailsControlBag()
		{
			ReferenceNumber2TextBox = RegisterControl(nameof(G5V1TemporaryStoragePreviousDocumentsDetailsUserControl.ReferenceNumber2TextBox));
		}

		protected override Control CreateTemplate() => new G5V1TemporaryStoragePreviousDocumentsDetailsUserControl();

		public static G5V1TemporaryStoragePreviousDocumentsDetailsControlBag Instance => uCC6TemporaryStoragePreviousDocumentsDetailsControlBag.Value;

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<G5V1TemporaryStoragePreviousDocumentsDetailsControlBag> uCC6TemporaryStoragePreviousDocumentsDetailsControlBag
			= new (() => new G5V1TemporaryStoragePreviousDocumentsDetailsControlBag());

		public ControlReference ReferenceNumber2TextBox { get; }
	}
}
