using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public sealed class ArrivalNotificationDetailsControlBag : ControlBag
	{
		ArrivalNotificationDetailsControlBag()
		{
			ArrivalGoodsLocationZCodeFindBox = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.ArrivalGoodsLocationCodeFindBox));
			RepresentativeTraderZDocAddressControl = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.RepresentativeTraderZDocAddressControl));
			BrokerCodeFindBox = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.BrokerCodeFindBox));
			CertificateDropEdit = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.CertificateDropEdit));
			AdditionalArrivalNotificationDetailsUserControl = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.AdditionalArrivalNotificationDetailsUserControl));
			TrainingCheckBox = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.TrainingCheckBox));
		}

		protected override Control CreateTemplate() => new ArrivalNotificationDetailsUserControl();

		public static ArrivalNotificationDetailsControlBag Instance => instance ?? (instance = new ArrivalNotificationDetailsControlBag());

		[ThreadStatic]
		static ArrivalNotificationDetailsControlBag instance;

		public ControlReference ArrivalGoodsLocationZCodeFindBox { get; }

		public ControlReference RepresentativeTraderZDocAddressControl { get; }

		public ControlReference BrokerCodeFindBox { get; }

		public ControlReference CertificateDropEdit { get; }

		public ControlReference AdditionalArrivalNotificationDetailsUserControl { get; }

		public ControlReference TrainingCheckBox { get; }
	}
}
