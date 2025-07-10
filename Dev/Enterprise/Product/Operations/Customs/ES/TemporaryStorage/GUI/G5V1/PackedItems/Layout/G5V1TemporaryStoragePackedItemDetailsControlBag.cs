using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public class G5V1TemporaryStoragePackedItemDetailsControlBag : ControlBag
	{
		public G5V1TemporaryStoragePackedItemDetailsControlBag()
		{
			UCRTextBox = RegisterControl(nameof(G5V1TemporaryStoragePackedItemDetailsControl.UCRTextBox));
			PresentationDateEdit = RegisterControl(nameof(G5V1TemporaryStoragePackedItemDetailsControl.PresentationDateEdit));
			MissingCheckBox = RegisterControl(nameof(G5V1TemporaryStoragePackedItemDetailsControl.MissingCheckBox));
		}

		public ControlReference UCRTextBox { get; }

		public ControlReference PresentationDateEdit { get; }

		public ControlReference MissingCheckBox { get; }

		public static G5V1TemporaryStoragePackedItemDetailsControlBag Instance => g5V1TemporaryStoragePackedItemDetailsControlBag.Value;

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<G5V1TemporaryStoragePackedItemDetailsControlBag> g5V1TemporaryStoragePackedItemDetailsControlBag = new Lazy<G5V1TemporaryStoragePackedItemDetailsControlBag>(() => new G5V1TemporaryStoragePackedItemDetailsControlBag());
		protected override Control CreateTemplate() => new G5V1TemporaryStoragePackedItemDetailsControl();
	}
}
