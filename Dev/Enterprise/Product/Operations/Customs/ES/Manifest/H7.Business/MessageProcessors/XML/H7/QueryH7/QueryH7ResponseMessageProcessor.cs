using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.ConsultaH7V1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class QueryH7ResponseMessageProcessor : H7CommonResponseMessageProcessor<ConsultaH7V1Sal, QueryH7MessagePrettyFormatter>
	{
		public QueryH7ResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}
		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => [DeclarationMessageTypeList.Codes.H7Query];

		protected override string MessageFriendlyNameCore => (NoResString)"Query H7 Response Message Processor";

		protected override void ProcessMessageCore(EDIMessage message, AsycudaBill linkedBusinessObject, ConsultaH7V1Sal provider)
		{
			mrnInfoForBill = provider.Response.MrnInfo.FirstOrDefault(x => x.Mrn == linkedBusinessObject.H7MovementReferenceNumber);
			base.ProcessMessageCore(message, linkedBusinessObject, provider);
		}

		protected override void ProcessAcceptedResponse(ConsultaH7V1Sal response, EDIMessage message, AsycudaBill bill)
		{
			base.ProcessAcceptedResponse(response, message, bill);
			if (mrnInfoForBill != null)
			{
				SetBillStatus(mrnInfoForBill, bill);
				SetReleaseDate(mrnInfoForBill, bill);
				SetDocumentationRequired(mrnInfoForBill, bill);
				SetMovementReferenceNumber(mrnInfoForBill, bill);
				SetClearanceCSV(mrnInfoForBill, bill, message);
			}
		}

		void SetBillStatus(MrnInfoTd mrnInfo, AsycudaBill bill)
		{
			var billStatus = string.Empty;

			switch (mrnInfo.StatusDeclaration)
			{
				case ESH7CustomsStatusList.Codes.PreDeclarationCompleted:
					billStatus = AISEntryStatusList.Codes.Prelodged;
					break;
				case ESH7CustomsStatusList.Codes.DeclarationPendingClearance:
					billStatus = AISEntryStatusList.Codes.Control;
					break;
				case ESH7CustomsStatusList.Codes.DeclarationProcessed:
					billStatus = AISEntryStatusList.Codes.Registered;
					break;
				case ESH7CustomsStatusList.Codes.DeclarationWithRelease:
					billStatus = AISEntryStatusList.Codes.Released;
					break;
				case ESH7CustomsStatusList.Codes.Annulled:
					billStatus = AISEntryStatusList.Codes.Cancelled;
					break;
				case ESH7CustomsStatusList.Codes.PendingH1Declaration:
					billStatus = AISEntryStatusList.Codes.AwaitingSupplementaryDeclaration;
					break;
				case ESH7CustomsStatusList.Codes.SADeclarationWithPaymentLetter:
					billStatus = AISEntryStatusList.Codes.Released;
					break;
				case ESH7CustomsStatusList.Codes.NoRelease:
					billStatus = AISEntryStatusList.Codes.NotReleased;
					break;
				case ESH7CustomsStatusList.Codes.Returned:
					billStatus = Reexported;
					break;
				case ESH7CustomsStatusList.Codes.Invalidated:
					billStatus = AISEntryStatusList.Codes.Invalid;
					break;
				case ESH7CustomsStatusList.Codes.FinalStatus:
					billStatus = AISEntryStatusList.Codes.Released;
					break;
				case ESH7CustomsStatusList.Codes.AbandonmentDestruction:
					billStatus = Abandonment;
					break;
				case ESH7CustomsStatusList.Codes.H7DeclaredInH1Declaration:
					billStatus = AISEntryStatusList.Codes.GeneralNotificationReceived;
					break;
				case ESH7CustomsStatusList.Codes.ReexportationCanceled:
					billStatus = AISEntryStatusList.Codes.CancellationRequested;
					break;
			}

			bill.ABL_BillStatus = billStatus;
		}

		const string Reexported = "REX";

		const string Abandonment = "ABD";

		protected override ZString XsdSchemaEmbeddedResourceName => "CargoWise.Customs.ES.MessageDefinitions.Version1.H7.Incoming.ConsultaH7V1Sal.xsd";

		protected override QueryH7MessagePrettyFormatter GetNewMessagePrettyFormatter(ConsultaH7V1Sal response, EDIMessage message, AsycudaBill bill) => new QueryH7MessagePrettyFormatter(response, mrnInfoForBill);

		MrnInfoTd mrnInfoForBill;
	}
}
