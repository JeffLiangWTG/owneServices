using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class TempStoragePremisesDetailsControlBag : ControlBag
	{
		TempStoragePremisesDetailsControlBag()
		{
			CodeTextBox = RegisterControl(nameof(TempStoragePremisesDetailsUserControl.CodeTextBox));
			DescriptionTextBox = RegisterControl(nameof(TempStoragePremisesDetailsUserControl.DescriptionTextBox));
			TypeDropEdit = RegisterControl(nameof(TempStoragePremisesDetailsUserControl.TypeDropEdit));
			AuthorizationNumberCodeFindBox = RegisterControl(nameof(TempStoragePremisesDetailsUserControl.AuthorizationNumberCodeFindBox));
			AuthorizationOwnerGuidFindBox = RegisterControl(nameof(TempStoragePremisesDetailsUserControl.AuthorizationOwnerGuidFindBox));
			PremisesAddressAddressControl = RegisterControl(nameof(TempStoragePremisesDetailsUserControl.PremisesAddressAddressControl));
			CustomsLocationCodeFindBox = RegisterControl(nameof(TempStoragePremisesDetailsUserControl.CustomsLocationCodeFindBox));
		}

		protected override Control CreateTemplate() => new TempStoragePremisesDetailsUserControl();

		public static TempStoragePremisesDetailsControlBag Instance => tempStorageRegPremisesControlBag.Value;

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<TempStoragePremisesDetailsControlBag> tempStorageRegPremisesControlBag = new (() => new TempStoragePremisesDetailsControlBag());

		public ControlReference CodeTextBox { get; }

		public ControlReference DescriptionTextBox { get; }

		public ControlReference TypeDropEdit { get; }

		public ControlReference AuthorizationNumberCodeFindBox { get; }

		public ControlReference AuthorizationOwnerGuidFindBox { get; }

		public ControlReference PremisesAddressAddressControl { get; }

		public ControlReference CustomsLocationCodeFindBox { get; }
	}
}
