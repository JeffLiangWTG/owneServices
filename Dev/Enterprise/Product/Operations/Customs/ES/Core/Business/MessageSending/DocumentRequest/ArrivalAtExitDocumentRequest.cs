using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business
{
	public class ArrivalAtExitDocumentRequest : CommonDocumentRequest<CusExitDetail>
	{
		public ArrivalAtExitDocumentRequest(CusExitDetail exitDetail, ZString certName) : base(exitDetail, certName)
		{
			exitHeader = Argument.NotNull(exitDetail.Header, nameof(exitDetail.Header));
			mrnCode = Argument.NotNullOrEmpty(exitDetail.CED_MovementReferenceNumber, nameof(exitDetail.CED_MovementReferenceNumber));
		}
		readonly EU.Business.CusExitControlHeader exitHeader;

		protected override ZInt RequestMissingDocumentsCore()
		{
			var messagesSentCount = ZInt.Zero;

			if (!csvClearance.IsEmpty)
			{
				var ealDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.ArrivalAtExitDoc);

				if (ealDocMissing)
				{
					CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.ArrivalAtExitDoc, csvClearance);
					messagesSentCount++;
				}
			}

			return messagesSentCount;
		}

		protected override ZString GetBGMReference(CusExitDetail businessObject) => businessObject.CED_MovementReferenceNumber;

		protected override ZString GetCSVReference(CusExitDetail businessObject) => businessObject.ZG_CSVClearance;

		protected override ZBool GetIsTrainingDeclaration() => (exitHeader.CEH_Parent as JobDeclaration)?.ZG_IsTrainingDeclaration ?? false;

		protected override EDIMessageCollection GetMessages(CusExitDetail businessObject) => businessObject.Messages;

		protected override GlbStaff GetBroker() => exitHeader.CustomsAgent;
	}
}
