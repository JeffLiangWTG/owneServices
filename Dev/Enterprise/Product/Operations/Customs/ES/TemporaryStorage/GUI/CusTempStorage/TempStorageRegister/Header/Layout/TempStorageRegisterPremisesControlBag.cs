using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public sealed class TempStorageRegisterPremisesControlBag : ControlBag
	{
		public static TempStorageRegisterPremisesControlBag Instance => tempStorageRegisterPremisesControlBag.Value;
		public TempStorageRegisterPremisesControlBag()
		{
			PremisesCodeTextBox = RegisterControl(nameof(TempStorageRegisterPremisesUserControl.PremisesCodeTextBox));
			PremisesDescriptionTextBox = RegisterControl(nameof(TempStorageRegisterPremisesUserControl.PremisesDescriptionTextBox));
			LocationDropEdit = RegisterControl(nameof(TempStorageRegisterPremisesUserControl.LocationDropEdit));
		}

		public ControlReference PremisesCodeTextBox { get; }

		public ControlReference PremisesDescriptionTextBox { get; }

		public ControlReference LocationDropEdit { get; }

		protected override Control CreateTemplate() => new TempStorageRegisterPremisesUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<TempStorageRegisterPremisesControlBag> tempStorageRegisterPremisesControlBag = new Lazy<TempStorageRegisterPremisesControlBag>(() => new TempStorageRegisterPremisesControlBag());
	}
}
