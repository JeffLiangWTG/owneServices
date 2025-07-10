using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class ExitControlMessageSendingObject : BaseMessageSendingObject
	{
		public ExitControlMessageSendingObject(CusExitReport messagingObject) : base(messagingObject.Factory)
		{
			this.MessagingObject = messagingObject;
		}
		public readonly CusExitReport MessagingObject;

		[ResourceStringData("DFFFEBE3-D92E-4909-A6B3-B9E81ACFFCDA", Caption = "Type")]
		public ZString Type => MessagingObject.CER_Type;

		public ZPropertyInfo TypeInfo => GetZPropertyInfo(nameof(Type));

		[ResourceStringData("EEDEC31A-698E-461F-A61B-07C5DD4A8372", Caption = "Transport ID")]
		public ZString TransportID => MessagingObject.CER_TransportID;

		public ZPropertyInfo TransportIDInfo => GetZPropertyInfo(nameof(TransportID));

		[ResourceStringData("5267C926-3208-4E23-9821-6717E2C29A4F", Caption = "Location")]
		public ZString Location => MessagingObject.CER_Location;

		public ZPropertyInfo LocationInfo => GetZPropertyInfo(nameof(Location));

		[ResourceStringData("693B0614-BBD7-4496-9B35-1A5DD0060EBF", Caption = "Departure Date + Time")]
		public virtual ZDateTime DateTime => MessagingObject.CER_DateTime.ToLocalZDateTime();

		public ZPropertyInfo DateTimeInfo => GetZPropertyInfo(nameof(DateTime));

		[ResourceStringData("B4A83B8F-2153-4CA8-9F81-45B9E855DE4A", Caption = "Office of Exit")]
		public ZString ExitOffice => MessagingObject.CER_OfficeOfExit;

		public ZPropertyInfo ExitOfficeInfo => GetZPropertyInfo(nameof(ExitOffice));

		[ResourceStringData("B66D2700-F1E8-4E96-826C-90FE12150804", Caption = "MRN")]
		public ZString MRN => MessagingObject.Consignment?.CXC_MovementReference ?? string.Empty;

		public ZPropertyInfo MRNInfo => GetZPropertyInfo(nameof(MRN));

		[ResourceStringData("F9770DB0-B859-424F-AA4F-38DC886DC1E3", Caption = "Customs Status")]
		public ZString CustomsStatus => MessagingObject.CER_Status;

		public ZPropertyInfo CustomsStatusInfo => GetZPropertyInfo(nameof(CustomsStatus));

		[ResourceStringData("096DA0D8-DCD9-4AE3-9F1A-92D8B22A341C", Caption = "Message Status")]
		public ZString MessageStatus => MessagingObject.CER_MessageStatus;

		public virtual ZString MessageType { get; set; }

		public ZPropertyInfo MessageStatusInfo => GetZPropertyInfo(nameof(MessageStatus));

		[ResourceStringData("04AF5F16-431B-4A5A-81C0-2713CB6BDD8F", Caption = "MRN/LRN")]
		public ZString MRN_LRN => MessagingObject.Consignment?.MessageDescriptionForEdocs ?? ZString.Empty;

		public ZPropertyInfo MRN_LRNInfo => GetZPropertyInfo(nameof(MRN_LRN));
	}
}
