using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Brazil
{
	public class BrazilEInvoiceAPICommandList : CodeDescriptionPairList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string GenerateInvoiceRequest = EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
			public const string GenerateCancellationRequest = EInvoiceAPICommandList.Codes.GenerateCancellationRequest;
		}

		public BrazilEInvoiceAPICommandList()
		{
			AddPair(Codes.GenerateInvoiceRequest, Res.GetString("adbe5e97-efee-474f-985a-6650f0afc0cd", "Generate Invoice Request"));
			AddPair(Codes.GenerateCancellationRequest, Res.GetString("09343ad5-378d-4410-9ffd-574773f7fcc3", "Generate Cancellation Request"));
		}

		public static ZString GetMessageType(TransactionInfo transactionInfo)
			=> (transactionInfo.IsCancelled ?? false) ? Codes.GenerateCancellationRequest : Codes.GenerateInvoiceRequest;
	}
}
