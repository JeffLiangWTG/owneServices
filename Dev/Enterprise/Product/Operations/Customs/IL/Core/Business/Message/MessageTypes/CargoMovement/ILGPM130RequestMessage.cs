using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IL.Business
{
	public class ILGPM130RequestMessage : ILEDIRequestMessage
	{
		public ILGPM130RequestMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ILMessageTypeList.Codes.GPM;
			EM_MessageSubType = ILEDIMessageSubTypeList.Codes.GatepassMovementRequest;
		}

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			base.GetNumberFountainNumbersAndFillInPlaceHolders();

			FillInGatePassNumberPlaceHolders();
		}

		protected override MessageDataObjectBase GenerateMessageDataObject() => new ILGPM130RequestMessageDataObject(this);

		void FillInGatePassNumberPlaceHolders()
		{
			const string gatePassNumber = "gatepassNumber";
			const string gatePassNumberDefaultSection = $"<{gatePassNumber}>0</{gatePassNumber}>";

			if (EM_LinkedObject is IGatePassMovementProviderFactory providerFactory && providerFactory.GatePassMovementProvider is IGatePassMovementProvider gatePassMovementProvider)
			{
				if (!gatePassMovementProvider.BusinessObject.IsInDatabase || gatePassMovementProvider.MessageReferenceNumber.IsEmpty)
				{
					gatePassMovementProvider.MessageReferenceNumber = Env.NumberFountains.ILGatePassMovementNumber.GetNextFormatted(gatePassMovementProvider.Factory);
				}

				var messageText = EM_MessageText;
				if (messageText.IndexOf(gatePassNumberDefaultSection, StringComparison.InvariantCulture) != -1)
				{
					EM_MessageText = messageText.Replace(gatePassNumberDefaultSection, $"<{gatePassNumber}>{gatePassMovementProvider.MessageReferenceNumber}</{gatePassNumber}>");
				}
			}
		}
	}
}
