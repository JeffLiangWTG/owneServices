using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class ArrivalNotificationDetailsControlBag : ControlBag
	{
		public ArrivalNotificationDetailsControlBag()
		{
			MrnTextBox = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.MrnTextBox));
			LocalReferenceNumberTextBox = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.LocalReferenceNumberTextBox));
			DestinationCustomsOfficeCodeCodeFindBox = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.DestinationCustomsOfficeCodeCodeFindBox));
			ArrivalDateDateTimeOffsetEdit = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.ArrivalDateDateTimeOffsetEdit));
			AuthorizationCodeDropEdit = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.AuthorizationCodeDropEdit));
			NumberCodeFindBox = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.NumberCodeFindBox));
			OwnerZGuidFindBox = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.OwnerZGuidFindBox));
			CarnetTotalPagesDropEdit = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.CarnetTotalPagesDropEdit));
			DischargeTypeDropEdit = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.DischargeTypeDropEdit));
			DestinationTraderDocAddressControl = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.DestinationTraderDocAddressControl));
			LocationOfGoodsUserControl = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.LocationOfGoodsUserControl));
			IncidentFlagDropEdit = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.IncidentFlagDropEdit));
			OverrideFreightDetailsCheckBox = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.OverrideFreightDetailsCheckBox));
			CommunicationLanguageDropEdit = RegisterControl(nameof(CommunicationLanguageDropEdit));
			TransportMeansLabel = RegisterControl(nameof(TransportMeansLabel));
			TransportAtArrivalTypeDropEdit = RegisterControl(nameof(TransportAtArrivalTypeDropEdit));
			TransportAtArrivalIDTextBox = RegisterControl(nameof(TransportAtArrivalIDTextBox));
			TransportNationalityCodeFindBox = RegisterControl(nameof(TransportNationalityCodeFindBox));
			StateOfSealsDropEdit = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.StateOfSealsDropEdit));
			AdditionalTextTextBox = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.AdditionalTextTextBox));
			NationalInfoSeparatorUserControl = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.NationalInfoSeparatorUserControl));
			GoodsLocationFromAuthorizationCodeFindBox = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.GoodsLocationFromAuthorizationCodeFindBox));
		}

		public static ArrivalNotificationDetailsControlBag Instance => instance ?? (instance = new ArrivalNotificationDetailsControlBag());

		[ThreadStatic]
		static ArrivalNotificationDetailsControlBag instance;

		public ControlReference MrnTextBox { get; }

		public ControlReference LocalReferenceNumberTextBox { get; }

		public ControlReference DestinationCustomsOfficeCodeCodeFindBox { get; }

		public ControlReference ArrivalDateDateTimeOffsetEdit { get; }

		public ControlReference AuthorizationCodeDropEdit { get; }

		public ControlReference NumberCodeFindBox { get; }

		public ControlReference OwnerZGuidFindBox { get; }

		public ControlReference CarnetTotalPagesDropEdit { get; }

		public ControlReference DischargeTypeDropEdit { get; }

		public ControlReference DestinationTraderDocAddressControl { get; }

		public ControlReference LocationOfGoodsUserControl { get; }

		public ControlReference IncidentFlagDropEdit { get; }

		public ControlReference OverrideFreightDetailsCheckBox { get; }

		public ControlReference CommunicationLanguageDropEdit { get; }

		public ControlReference TransportMeansLabel { get; }

		public ControlReference TransportAtArrivalTypeDropEdit { get; }

		public ControlReference TransportAtArrivalIDTextBox { get; }

		public ControlReference TransportNationalityCodeFindBox { get; }

		public ControlReference StateOfSealsDropEdit { get; }

		public ControlReference AdditionalTextTextBox { get; }

		public ControlReference NationalInfoSeparatorUserControl { get; }

		public ControlReference GoodsLocationFromAuthorizationCodeFindBox { get; }

		protected override Control CreateTemplate() => new ArrivalNotificationDetailsUserControl();
	}
}
