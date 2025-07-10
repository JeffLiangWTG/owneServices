using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class FTAMessageSendingObjectValidation : JobDeclarationMiscMessageSendingObjectCoreValidation
	{
		public FTAMessageSendingObjectValidation(FTAMessageSendingObject parent) : base(parent)
		{
		}

		ZString MessageType => Parent.MessageType;
		CusEntryHeader Header => Parent.Header;
		protected new FTAMessageSendingObject Parent => (FTAMessageSendingObject)base.Parent;

		protected override void CheckShouldSend()
		{
			base.CheckShouldSend();

			if (Parent.ShouldSend)
			{
				if (Header.GetOriginalFTAType() != MessageType)
				{
					switch (MessageType)
					{
						case ElectronicDocumentTypeList.Codes._5SC:
							Parent.ShouldSendInfo.AddError(Res.GetString("DB0BD723-2B43-4C21-B1D5-943EC8906923", "You have selected a 5SC message, but there are some invoice lines which require a detailed FTA message(DHR)."));
							break;
						case ElectronicDocumentTypeList.Codes._DHR:
							Parent.ShouldSendInfo.AddError(Res.GetString("12E682F8-D1F0-4C3A-BAC3-F5790C5A2810", "You have selected a DHR message, but none of invoice lines have required country of origins."));
							break;
					}
				}
			}
		}
	}
}
