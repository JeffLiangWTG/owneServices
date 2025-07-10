using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class LPCOMessageSendingObjectParent : BaseMessageSendingObjectParent<LPCOMessageSendingObject>, IMessageSendingObjectParent
	{
		public LPCOMessageSendingObjectParent(CusLPCOHeader lpcoHeader) : base(lpcoHeader.Factory)
		{
			LPCOHeader = Argument.NotNull(lpcoHeader, nameof(lpcoHeader));
		}

		public static class Schema
		{
			public const string BrokerCode = "BrokerCode";
		}

		public readonly CusLPCOHeader LPCOHeader;

		protected override void SetDefaultValues()
		{
			BrokerCode = GlbStaff.CurrentUser.GS_Code;
		}

		public override BusinessObject TopLevelBusinessObject => LPCOHeader;

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.None;

		protected override NonPersistentBusinessObjectCollection<LPCOMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var collection = new LPCOMessageSendingObjectCollection(Factory);
			collection.Add(new LPCOMessageSendingObject(this));
			return collection;
		}

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

		readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
		{
			new MessageSendingObjectProperty(nameof(LPCOMessageSendingObject.MessageType), true, 100),
			new MessageSendingObjectProperty(nameof(LPCOMessageSendingObject.SubmittedDate), true, 140),
			new MessageSendingObjectProperty(nameof(LPCOMessageSendingObject.CustomsStatus), true, 100),
			new MessageSendingObjectProperty(nameof(LPCOMessageSendingObject.Reason), true, 140),
			new MessageSendingObjectProperty(nameof(LPCOMessageSendingObject.Requirement), true, 140),
			new MessageSendingObjectProperty(nameof(LPCOMessageSendingObject.PermitNumber), true, 100),
			new MessageSendingObjectProperty(nameof(LPCOMessageSendingObject.MessageStatusDescription), false, 180),
			new MessageSendingObjectProperty(nameof(LPCOMessageSendingObject.NewEffectiveDate), true, 120),
			new MessageSendingObjectProperty(nameof(LPCOMessageSendingObject.Message), true, 120),
		};

		#region BrokerCode

		[List(nameof(Lookups) + "." + nameof(LPCOMessageSendingObjectParentLookups.BrokerList))]
		[MaxLength(GlbStaff.Schema.GS_CodeMaxLength)]
		public ZString BrokerCode
		{
			get { return fBrokerCode; }
			set
			{
				SetNonPersistentPropertyValue(BrokerCodeInfo, ref fBrokerCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateBrokerCode();
				}
			}
		}
		ZString fBrokerCode;

		public ZPropertyInfo BrokerCodeInfo => GetZPropertyInfo(Schema.BrokerCode);

		public GlbExternalPassword_CCT BrokerCertificate => BRGlbStaffWrapper.Get(Broker)?.GetCCTPassword();

		public GlbStaff Broker => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, fBrokerCode);

		#endregion

		#region Lookups

		public LPCOMessageSendingObjectParentLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = new LPCOMessageSendingObjectParentLookups(this);
				}
				return fLookups;
			}
		}

		LPCOMessageSendingObjectParentLookups fLookups;

		#endregion

		public LPCOMessageSendingObjectParentValidation Validation => GetNewValidation();

		protected virtual LPCOMessageSendingObjectParentValidation GetNewValidation() => new LPCOMessageSendingObjectParentValidation(this);
	}
}
