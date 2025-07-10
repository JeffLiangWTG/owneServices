using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

public class UCC6TemporaryStoragePackedItemDetailsControlBag : ControlBag
{
	public UCC6TemporaryStoragePackedItemDetailsControlBag()
	{
		RegistrationNoTextBox = RegisterControl(nameof(UCC6TemporaryStoragePackedItemDetailsControl.RegistrationNoTextBox));
		ReleaseDateEdit = RegisterControl(nameof(UCC6TemporaryStoragePackedItemDetailsControl.ReleaseDateEdit));
	}

	public ControlReference RegistrationNoTextBox { get; }

	public ControlReference ReleaseDateEdit { get; }

	public static UCC6TemporaryStoragePackedItemDetailsControlBag Instance => uCC6TemporaryStoragePackedItemDetailsControlBag.Value;

	[WTG.StaticAnalysis.Annotation.ThreadSafe]
	static readonly Lazy<UCC6TemporaryStoragePackedItemDetailsControlBag> uCC6TemporaryStoragePackedItemDetailsControlBag = new Lazy<UCC6TemporaryStoragePackedItemDetailsControlBag>(() => new UCC6TemporaryStoragePackedItemDetailsControlBag());

	protected override Control CreateTemplate() => new UCC6TemporaryStoragePackedItemDetailsControl();
}
