using System;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CusContainerValidation : Customs.Business.CusContainerValidation
	{
		public CusContainerValidation(CusContainer parent)
			: base(parent)
		{
		}

		public new CusContainer Parent
		{
			get { return (CusContainer)base.Parent; }
		}

		#region CheckCO_ContainerNumber
		protected override void CheckCO_ContainerNumber()
		{
			base.CheckCO_ContainerNumber();
			if (Parent.Messages.Count > 0 && Parent.CO_ContainerNumberInfo.OriginalValue.ToString() != Parent.CO_ContainerNumberInfo.Value.ToString())
			{
				String sentMsgSubType = "";
				EDIMessage lastMessage = Parent.Messages[0];
				foreach (EDIMessage message in Parent.Messages)
				{
					if (message.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
					{
						sentMsgSubType = message.EM_MessageSubType;
					}

					lastMessage = message;
				}
				if (lastMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
				{
					if (lastMessage.EM_MessageSubType == "SSM")
					{
						Parent.CO_ContainerNumberInfo.AddError(CommonContainerValidation.CannotChangeContainerWhileWaitingForAReplyMustCancelToo);
					}
					else
					{
						Parent.CO_ContainerNumberInfo.AddError(CommonContainerValidation.CannotChangeContainerWhileWaitingForAReplyToCancellation);
					}
				}
				else
				{
					if (sentMsgSubType == "SSM" && lastMessage.EM_MessageSubType == PRAConstants.MessageAcknowledged)
					{
						Parent.CO_ContainerNumberInfo.AddError(CommonContainerValidation.CannotChangeContainerWithoutCancellingPRAFirst);
					}
				}
			}

			var dec = Parent.Declaration;
			if (dec != null)
			{
				var consol = dec.RelevantConsol;
				if (consol != null && !consol.Containers.HasContainer(Parent.CO_ContainerNumber))
				{
					Parent.CO_ContainerNumberInfo.AddWarning("This container does not exist in the freight system.");
				}
			}
		}
		#endregion
	}
}
