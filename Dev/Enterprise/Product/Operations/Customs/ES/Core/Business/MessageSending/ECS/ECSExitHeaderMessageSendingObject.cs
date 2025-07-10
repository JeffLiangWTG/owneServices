using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.MessageSending
{
	public class ECSExitHeaderMessageSendingObject : Customs.Business.BaseMessageSendingObject
	{
		public ECSExitHeaderMessageSendingObject(CusExitDetail cusExitDetail)
			: base()
		{
			ExitDetail = Argument.NotNull(cusExitDetail, nameof(cusExitDetail));
		}
		public CusExitDetail ExitDetail { get; }

		public static class Schema
		{
			public const string ArrivalNotificationDate = "ArrivalNotificationDate";
			public const string Status = "Status";
			public const string MRN = "MRN";
		}

		#region New Properties

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.ES.Business.MessageSending.ECSMessageSendingObject|MRN", ShortCaption = "MRN", Caption = "MRN")]
		public ZString MRN => ExitDetail.CED_MovementReferenceNumber;
		public ZPropertyInfo MRNInfo => GetZPropertyInfo(Schema.MRN);

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.ES.Business.MessageSending.ECSMessageSendingObject|ArrivalNotificationDate", ShortCaption = "Arr. Notification Date", Caption = "Arrival Notification Date")]
		public ZDate ArrivalNotificationDate => ExitDetail.CED_ArrivalNotificationDate.Date;
		public ZPropertyInfo ArrivalNotificationDateInfo => GetZPropertyInfo(Schema.ArrivalNotificationDate);

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.ES.Business.MessageSending.ECSMessageSendingObject|MessageStatus", ShortCaption = "Status", Caption = "Status")]
		public ZString Status => ExitDetail.CED_Status;
		public ZPropertyInfo MessageStatusInfo => GetZPropertyInfo(Schema.Status);

		#endregion

		protected override bool ShouldSend_ReadOnly => ExitDetail.CED_Status != ZString.Empty && ExitDetail.CED_Status != MessageStatusList.Codes.SentAndRejected;
	}
}
