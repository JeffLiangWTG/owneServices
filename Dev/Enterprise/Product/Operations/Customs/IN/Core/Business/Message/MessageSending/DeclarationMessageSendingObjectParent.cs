using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IN.Business;

public class DeclarationMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent<DeclarationMessageSendingObject>, IMessageSendingObjectParent
{
	public DeclarationMessageSendingObjectParent(JobDeclaration declaration) : base(declaration)
	{
	}

	public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties
	{
		get
		{
			yield return new MessageSendingObjectProperty(AutoJobDeclarationMessageSendingObject.Schema.MessageType, true, 100);
			yield return new MessageSendingObjectProperty(AutoJobDeclarationMessageSendingObject.Schema.LocalReferenceNumber, true, 150);
			yield return new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.LocalReferenceNumberDate, false, 150);
			yield return new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.ShippingBillNumber, false, 150);
			yield return new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.ShippingBillDate, false, 150);
			yield return new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.DeclarationType, false, 150, null, false);
			yield return new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.Description, false, 150, null, false);
			yield return new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.CustomsHouse, false, 150);
		}
	}

	public EDIMessage[] SendAndSaveMessages(MessageSendingContext context)
	{
		Func<DeclarationMessageSendingObject, IMessageSender> sender = null;
		if (ParentDeclaration.IsExport)
		{
			sender = x => (string)x.MessageType switch
			{
				DeclarationMessageTypeList.Codes.GoodsRegistration => new ExportGoodsRegistrationMessageSender(x),
				_ => new ShippingBillMessageSender(x),
			};
		}
		return sender != null ? this.SendAndSaveMessages(sender, context) : Array.Empty<EDIMessage>();
	}

	protected override JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
	{
		return new DeclarationMessageSendingObject((CusEntryHeader)header);
	}

	protected override ZString GetAdditionalWarningsCore()
	{
		var result = ZString.Empty;
		if (SelectedSendingObjects.Any())
		{
			var warnings = CreateNewNotificationCollector().GetWarnings();
			result = Regex.Replace(warnings.ToUniqueMessageListString(), "(?<!\r)\n", "\r\n");
		}
		return result;
	}

	ZString IMessageSendingObjectParent.ValidateBeforeSend() => ParentDeclaration.JE_CustomsOffice.IsEmpty ? Res.GetString("B2FD1A67-A0AF-4BCD-8F91-01ED6183DDC3", "Please select Customs House to send the message electronically to ICEGate.") : ZString.Empty;
}
