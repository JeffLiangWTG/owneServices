using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStoragePackageDetailsControlBag : ControlBag
	{
		public UCC6TemporaryStoragePackageDetailsControlBag()
		{
			ContainerDropEdit = RegisterControl(nameof(UCC6TemporaryStoragePackageDetailsControl.containerPKGuidDropEditWithFixedWidth));
			PackUQDropEdit = RegisterControl(nameof(UCC6TemporaryStoragePackageDetailsControl.packUQDropEditWithFixedWidth));
			PackQtyCalcEdit = RegisterControl(nameof(UCC6TemporaryStoragePackageDetailsControl.packQtyCalcEdit));
			MarksAndNumberTextBox = RegisterControl(nameof(UCC6TemporaryStoragePackageDetailsControl.marksAndNumbersTextBox));
		}

		public ControlReference ContainerDropEdit { get; }

		public ControlReference PackUQDropEdit { get; }

		public ControlReference PackQtyCalcEdit { get; }

		public ControlReference MarksAndNumberTextBox { get; }

		public static UCC6TemporaryStoragePackageDetailsControlBag Instance => uCC6TemporaryStoragePackageDetailsControlBag.Value;

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<UCC6TemporaryStoragePackageDetailsControlBag> uCC6TemporaryStoragePackageDetailsControlBag = new Lazy<UCC6TemporaryStoragePackageDetailsControlBag>(() => new UCC6TemporaryStoragePackageDetailsControlBag());
		protected override Control CreateTemplate() => new UCC6TemporaryStoragePackageDetailsControl();
	}
}
