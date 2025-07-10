using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public abstract class BaseSingleMessageSendingObject : BaseMessageSendingObject, IMessageSendingObject, IMessageSendingObjectParent
	{
		public BaseSingleMessageSendingObject(BusinessObject parent) : base(parent.Factory)
		{
			Parent = Argument.NotNull(parent, nameof(parent));
		}

		public readonly BusinessObject Parent;

		public static class Schema
		{
			public const string MessageType = "MessageType";
			public const string BrokerCode = "BrokerCode";
		}

		#region BrokerCode

		[List(nameof(Lookups) + "." + nameof(SingleMessageSendingObjectLookups.BrokerList))]
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

		public GlbStaff Broker => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, BrokerCode);

		public GlbExternalPassword_CCT BrokerCertificate => BRGlbStaffWrapper.Get(Broker)?.GetCCTPassword();

		#endregion

		#region MessageType

		public ZString MessageType
		{
			get { return fMessageType; }
			set
			{
				SetNonPersistentPropertyValue(MessageTypeInfo, ref fMessageType, value);
			}
		}
		ZString fMessageType;

		public ZPropertyInfo MessageTypeInfo => GetZPropertyInfo(Schema.MessageType);

		#endregion

		#region Lookups

		public SingleMessageSendingObjectLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = new SingleMessageSendingObjectLookups(this);
				}
				return fLookups;
			}
		}

		SingleMessageSendingObjectLookups fLookups;

		#endregion

		#region Validation

		public SingleMessageSendingObjectValidation Validation => new SingleMessageSendingObjectValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		#endregion

		public IEnumerable<BaseMessageSendingObject> SelectedSendingObjects => new[] { this };

		public BusinessObject MessageAttachee => Parent;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ShouldSend = true;
		}

		public virtual ZString GetApplicationReference() => ZString.Empty;

		public virtual ZGuid GetGlbExternalPasswordPK() => BrokerCertificate?.PK ?? ZGuid.Empty;

		public virtual ZString GetMessageOwner() => ZString.Empty;

		public abstract ZString GetMessageTypeForEDIMessage();

		public abstract ZString GetMessageText();
	}
}
