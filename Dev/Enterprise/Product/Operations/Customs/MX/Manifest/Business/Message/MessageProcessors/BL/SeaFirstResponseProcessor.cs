using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class SeaFirstResponseProcessor : ManifestResponseMessageProcessor
	{
		public SeaFirstResponseProcessor(LoggingInformation logger)
			: base(logger)
		{ }

		protected override ZString ProcessManifestMessageCore(MXMessage message)
		{
			var status = EDIMessageStatusList.Codes.ProcessedOK;

			var linkedBill = message.EM_LinkedObject as AsycudaBill;
			if (linkedBill != null)
			{
				var response = CargoWise.Customs.MX.MessageContracts.MXHelper.GetSeaFirstResponseInformation(message.EM_MessageText, linkedBill.ABL_BillNumber);
				isFailure = response.IsFailure;
				var nodes = MXMessageProcessorHelper.GetFirstResponseInformation(response);
				foreach (var item in nodes)
				{
					tableCreator.WriteRow(new string[] { item.Item1, item.Item2 });
				}

				if (response.IsFailure)
				{
					ProcessRejectedMessage(linkedBill, message, response.MessageInterpretation);
				}
				else
				{
					ProcessAcceptedMessage(linkedBill, message, response.MessageInterpretation, response.IdPet);
				}
			}
			else
			{
				status = EDIMessageStatusList.Codes.Discarded;
			}

			return status;
		}

		void ProcessAcceptedMessage(AsycudaBill linkedBill, MXMessage message, ZString messageInterpretation, ZString idPet)
		{
			linkedBill.ABL_MessageStatus = CustomsStatusList.Codes.AWA;
			linkedBill.CustomsEntryNumber = idPet;
			linkedBill.CustomsEntryNumberType = CusEntryNumberTypes.Mexico.MXSeaCustomsNumber;
			message.EM_MessageInterpretation = messageInterpretation;
		}

		void ProcessRejectedMessage(AsycudaBill linkedBill, MXMessage message, ZString messageInterpretation)
		{
			linkedBill.ABL_MessageStatus = CustomsStatusList.Codes.ERR;

			message.EM_MessageInterpretation = messageInterpretation;
			tableCreator.WriteRow(errorMessage, message.EM_MessageInterpretation);
		}
	}
}
