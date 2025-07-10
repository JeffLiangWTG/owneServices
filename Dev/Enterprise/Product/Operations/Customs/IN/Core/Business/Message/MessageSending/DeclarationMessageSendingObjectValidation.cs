using Enterprise.Customs.Business;

namespace Enterprise.Customs.IN.Business;

public class DeclarationMessageSendingObjectValidation : JobDeclarationMessageSendingObjectValidation
{
	public DeclarationMessageSendingObjectValidation(DeclarationMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	new DeclarationMessageSendingObject Parent => (DeclarationMessageSendingObject)base.Parent;

	protected override void CheckMessageType()
	{
		base.CheckMessageType();
		var parent = Parent;
		var instruction = parent.Header.EntryInstruction;
		bool isSbNumberOrDateMissing = (instruction?.ShippingBillNumber.IsEmpty ?? true) || (instruction?.ShippingBillDate.IsEmpty ?? true);

		if (parent.ShouldSend && !parent.MessageType.IsEmpty && parent.MessageType != DeclarationMessageTypeList.Codes.Fresh && isSbNumberOrDateMissing)
		{
			parent.MessageTypeInfo.AddError(Res.GetString("A890CEFE-2BD2-4721-8E6F-789B4DBE2380", "SB Number and Date is required."));
		}
	}
}
