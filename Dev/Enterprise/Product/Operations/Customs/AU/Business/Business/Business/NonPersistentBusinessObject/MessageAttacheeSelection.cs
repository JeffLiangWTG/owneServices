using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public enum MessageAttacheeMessageType { Original, Amend, Withdraw }

	public interface IMessageAttachee
	{
		BusinessObjectFactory Factory { get; }
		ZString UserFriendlyCode { get; }
		ZBool IsValidToSendThisMessageType(MessageAttacheeMessageType messageType);
	}

	public class MessageAttacheeSelection : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MessageAttacheeSelection(IMessageAttachee messageAttachee)
			: base(messageAttachee.Factory)
		{
			this.MessageAttachee = messageAttachee;
		}

		#region Bindable Properties

		public ZString UserFriendlyCode
		{
			get { return MessageAttachee.UserFriendlyCode; }
		}

		public ZPropertyInfo UserFriendlyCodeInfo
		{
			get { return GetZPropertyInfo(nameof(UserFriendlyCode)); }
		}

		public ZBool ShouldSendNow
		{
			get { return fShouldSendNow; }
			set { SetNonPersistentPropertyValue(ShouldSendNowInfo, ref fShouldSendNow, value); }
		}
		ZBool fShouldSendNow;

		public ZPropertyInfo ShouldSendNowInfo
		{
			get { return GetZPropertyInfo(nameof(ShouldSendNow)); }
		}

		#endregion

		#region Implementation

		public readonly IMessageAttachee MessageAttachee;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ShouldSendNow = true;
		}

		#endregion
	}
}
