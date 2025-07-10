using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

sealed class TemporaryStorageUserControlBag : ControlBag
{
	public static TemporaryStorageUserControlBag Instance => temporaryStorageControlBag.Value;

	TemporaryStorageUserControlBag()
	{
		AccountNameDropEdit = RegisterControl(nameof(TemporaryStorageUserControl.AccountNameDropEdit));
		RepresentativeQualificationDropEdit = RegisterControl(nameof(TemporaryStorageUserControl.RepresentativeQualificationDropEdit));
	}

	public ControlReference AccountNameDropEdit { get; }

	public ControlReference RepresentativeQualificationDropEdit { get; }

	protected override Control CreateTemplate() => new TemporaryStorageUserControl();

	[WTG.StaticAnalysis.Annotation.ThreadSafe]
	static readonly Lazy<TemporaryStorageUserControlBag> temporaryStorageControlBag = new Lazy<TemporaryStorageUserControlBag>(() => new TemporaryStorageUserControlBag());
}
