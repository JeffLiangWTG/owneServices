using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationMessageSendingObject : Customs.Business.JobDeclarationMessageSendingObject
	{
		public JobDeclarationMessageSendingObject(CusEntryHeader entry, string messageType)
			: base(entry)
		{
			MessageType = messageType;
		}
		public new CusEntryHeader Header => (CusEntryHeader)base.Header;

		public new class Schema : AutoJobDeclarationMessageSendingObject.Schema
		{
			public const string EntryNumber = "EntryNumber";
		}

		public override ZBool ShouldSend
		{
			get => base.ShouldSend;
			set
			{
				var oldValue = base.ShouldSend;
				base.ShouldSend = value;
				if (oldValue != value && !IsValidationSuspended)
				{
					Validation.ValidateAll();
				}
			}
		}

		public ZString EntryNumber => Header.EntryNumber;

		[ResourceStringData("17CB51E8-819F-4E14-97BE-E88F62D7E756", Caption = "Entry Number")]
		[ResourceStringData("6E9C98AC-A019-45AF-B6EF-664593B167C7", Caption = "Entry Number", MultipleKey = KRJobMessageTypeList.Codes.Export)]
		[ResourceStringData("6B8CC333-A5C4-46A6-93EB-B10A5098F453", Caption = "Entry Number", MultipleKey = KRJobMessageTypeList.Codes.LocalExport)]
		public ZString FormattedEntryNumber => MessageFunctions.DeclarationNumberFormat(EntryNumber);

		protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation() => new JobDeclarationMessageSendingObjectValidation(this);

		public MessageSendingValidation MessageSendingValidation
		{
			get
			{
				if (fMessageSendingValidation == null)
				{
					fMessageSendingValidation = MessageSendingValidation.New(Header.Declaration, GetNewMessageErrorCollector(),
						Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed);
				}
				return fMessageSendingValidation;
			}
		}
		MessageSendingValidation fMessageSendingValidation;

		[ResourceStringData("058273A1-C063-4306-871E-D692A5F96E54", Caption = "Validation Errors")]
		public ZString BizObjValidationMessageErrors => ShouldSend ? GetBizObjValidationMessageErrors() : ZString.Empty;

		ZString GetBizObjValidationMessageErrors()
		{
			return Regex.Replace(MessageSendingValidation.CheckBusinessObjectLevelValidation().NotificationsAsString(), "(?<!\r)\n", "\r\n");
		}

		IEnumerable<INotification> GetNewMessageErrorCollector()
		{
			return new JobDeclarationMessageSendingNotificationCollector(Header.Declaration, new CusEntryHeader[] { Header }).GetMessageErrors();
		}
	}
}
