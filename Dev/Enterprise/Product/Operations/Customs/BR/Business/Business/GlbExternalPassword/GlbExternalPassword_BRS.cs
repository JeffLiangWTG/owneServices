using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class GlbExternalPassword_BRS : GlbExternalPassword, IMessageAttachee
	{
		public new class Schema : GlbExternalPassword.Schema
		{
			public const string StatusDescription = "StatusDescription";
		}

		public GlbExternalPassword_BRS(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implement

		protected override ZString HumanReadableNameCore => Res.GetString("F7D484B5-1D6A-4C56-850F-5B8D17ACB508", "Event Subscription");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		public static class StatusReasons
		{
			public const string AwaitingResponse = "Awaiting Response";
			public const string CancelationRejected = "Cancelation Rejected";
			public const string Canceled = "Canceled";
			public const string NotSent = "Not Sent";
			public const string SubscriptionRejected = "Subscription Rejected";
			public const string Subscribed = "Subscribed";
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.BRS;
		}

		public new GlbExternalPasswordValidation_BRS Validation => (GlbExternalPasswordValidation_BRS)GetNewValidation();

		protected override GlbExternalPasswordValidation GetNewValidation()
		{
			return new GlbExternalPasswordValidation_BRS(this);
		}

		protected override GlbExternalPasswordLookups GetNewLookups() => new GlbExternalPasswordLookups_BRS(this);

		public new GlbExternalPasswordLookups_BRS Lookups => (GlbExternalPasswordLookups_BRS)base.Lookups;

		#endregion

		#region Properties

		public ZString StatusDescription => GP_StatusReason.IsEmpty ? (ZString)StatusReasons.NotSent : GP_StatusReason;

		[MaxLength(10)]
		public override ZString GP_MailBoxID
		{
			get => base.GP_MailBoxID;
			set => base.GP_MailBoxID = value;
		}

		[ReadOnly(true)]
		public override ZString GP_PasswordStatus
		{
			get => base.GP_PasswordStatus;
			set => base.GP_PasswordStatus = value;
		}

		[List(nameof(Lookups) + "." + nameof(GlbExternalPasswordLookups_BRS.EventIdCodeDescriptionPairList))]
		[MaxLength(40)]
		[ReadOnlyMember(nameof(IsSubscriptionSent))]
		public override ZString GP_UserID
		{
			get => base.GP_UserID;
			set => base.GP_UserID = value;
		}

		public ZDateTime SubmittedDate => Factory.GetCached(ref cachedSubmittedDate, GetSubmittedDate);
		CachedProperty<ZDateTime> cachedSubmittedDate;

		ZDateTime GetSubmittedDate() => Factory.LoadTop1<EDIMessage>(GetSubmittedDateQuery())?.EM_MessageDateTime ?? ZDateTime.Empty;

		ZQuery GetSubmittedDateQuery()
		{
			var query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, TableName);
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, PK);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.SUB);
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.Original);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.BRCustoms);
			query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Sent);
			query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name;

			return query;
		}

		#endregion

		public bool IsSubscriptionSent => GP_StatusReason == GlbExternalPassword_BRS.StatusReasons.Subscribed || GP_StatusReason == GlbExternalPassword_BRS.StatusReasons.AwaitingResponse;

		ZGuid IMessageAttachee.BranchPK => Company?.FirstActiveBranch?.PK ?? ZGuid.Empty;
	}
}
