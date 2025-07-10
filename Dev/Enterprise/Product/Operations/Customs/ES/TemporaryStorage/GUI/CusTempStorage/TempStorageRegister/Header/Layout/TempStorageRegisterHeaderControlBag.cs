using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public sealed class TempStorageRegisterHeaderControlBag : ControlBag
	{
		public static TempStorageRegisterHeaderControlBag Instance => tempStorageRegisterHeaderControlBag.Value;
		public TempStorageRegisterHeaderControlBag()
		{
			InternalReferenceTextBox = RegisterControl(nameof(TempStorageRegisterHeaderUserControl.InternalReferenceTextBox));
			StatusDropEdit = RegisterControl(nameof(TempStorageRegisterHeaderUserControl.StatusDropEdit));
			PreviousReferenceTypeDropEdit = RegisterControl(nameof(TempStorageRegisterHeaderUserControl.PreviousReferenceTypeDropEdit));
			PresentationDateEdit = RegisterControl(nameof(TempStorageRegisterHeaderUserControl.PresentationDateEdit));
			ArrivalDateEdit = RegisterControl(nameof(TempStorageRegisterHeaderUserControl.ArrivalDateEdit));
			PreviousReferenceNumberTextBox = RegisterControl(nameof(TempStorageRegisterHeaderUserControl.PreviousReferenceNumberTextBox));
			DDTNumberUserControl = RegisterControl(nameof(TempStorageRegisterHeaderUserControl.DDTNumberUserControl));
		}

		public ControlReference InternalReferenceTextBox { get; }

		public ControlReference StatusDropEdit { get; }

		public ControlReference PreviousReferenceTypeDropEdit { get; }

		public ControlReference PresentationDateEdit { get; }

		public ControlReference ArrivalDateEdit { get; }

		public ControlReference PreviousReferenceNumberTextBox { get; }

		public ControlReference DDTNumberUserControl { get; }

		protected override Control CreateTemplate() => new TempStorageRegisterHeaderUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<TempStorageRegisterHeaderControlBag> tempStorageRegisterHeaderControlBag = new Lazy<TempStorageRegisterHeaderControlBag>(() => new TempStorageRegisterHeaderControlBag());
	}
}
