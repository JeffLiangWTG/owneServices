using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CL.Manifest.Business
{
	static class CLInterchangeHelper
	{
		internal static ZString GetInterchangeMessageTo(ZString messageType)
		{
			var result = ZString.Empty;
			var messageSending = CLCustomsDataRegistry.Instance.CLSMSMessageSending.Value;
			if (messageSending != null)
			{
				result = messageSending.ApplicationNodeName;
			}
			return result;
		}

		internal static ZString GetOutingMessageFileName(EDIMessage message)
		{
			var fileName = ZString.Empty;

			if (XmlUtils.IsValidXml(message.EM_MessageText))
			{
				var freeMax4chr = ZString.Empty;
				var docType = ZString.Empty;
				var version = ZString.Empty;
				var messageNumber = message.EM_MessageNum;

				if (message.EM_MessageType == MessageTypes.Codes.CHA || message.EM_MessageType == MessageTypes.Codes.CHB)
				{
					var bl = MessageBuilderHelper.GetBLRequest(message.EM_MessageText);
					if (bl != null)
					{
						var nature = bl.Operation;
						if (nature != null)
						{
							freeMax4chr = (ZString)(nature == WrappersConstants.OperationType.I ? ShipmentTypeList.Codes.Import23 :
								nature == WrappersConstants.OperationType.S ? ShipmentTypeList.Codes.Export22 :
								nature == WrappersConstants.OperationType.Tr ? ShipmentTypeList.Codes.Transit24 :
								nature == WrappersConstants.OperationType.Trb ? ShipmentTypeList.Codes.Transhipment28 :
								string.Empty);
							docType = bl.Type;
							version = bl.Version;
						}
					}
				}
				else if (message.EM_MessageType == MessageTypes.Codes.CHC)
				{
					var blCancel = MessageBuilderHelper.GetBLCancelRequest(message.EM_MessageText);
					if (blCancel != null)
					{
						freeMax4chr = CLMessageConstants.Cancelation;
						docType = blCancel.Type;
						version = blCancel.Version;
					}
				}
				else if (message.EM_MessageType == MessageTypes.Codes.CHD || message.EM_MessageType == MessageTypes.Codes.CHE)
				{
					var awb = MessageBuilderHelper.GetAWBRequest(message.EM_MessageText);
					var nature = awb.Operation;
					if (nature != null)
					{
						freeMax4chr = (ZString)(nature == WrappersConstants.OperationType.I ? ShipmentTypeList.Codes.Import23 :
							nature == WrappersConstants.OperationType.S ? ShipmentTypeList.Codes.Export22 :
							string.Empty);
						docType = awb.Type;
						version = awb.Version;
					}
				}
				else if (message.EM_MessageType == MessageTypes.Codes.CHF)
				{
					var awbCancel = MessageBuilderHelper.GetAWBCancelRequest(message.EM_MessageText);
					if (awbCancel != null)
					{
						freeMax4chr = CLMessageConstants.Cancelation;
						docType = awbCancel.Type;
						version = awbCancel.Version;
					}
				}

				fileName = FileNameBuilder(freeMax4chr, docType, version, messageNumber);
			}

			return fileName;
		}

		static ZString FileNameBuilder(ZString freeMax4chr, ZString docType, ZString version, ZString messageNumber)
		{
			var fileName = ZString.Empty;
			if (!freeMax4chr.IsEmpty && !docType.IsEmpty && !version.IsEmpty && !messageNumber.IsEmpty)
			{
				ZStringBuilder stringBuilder = new ZStringBuilder();

				stringBuilder.Append(freeMax4chr);
				stringBuilder.Append(CLMessageConstants.FileNameSeparator);
				stringBuilder.Append(docType);
				stringBuilder.Append(CLMessageConstants.FileNameSeparator);
				stringBuilder.Append(version);
				stringBuilder.Append(CLMessageConstants.FileNameSeparator);
				stringBuilder.Append(messageNumber);
				stringBuilder.Append(CLMessageConstants.FileNameExtension);

				fileName = stringBuilder.ToString();
			}
			return fileName;
		}
	}
}
