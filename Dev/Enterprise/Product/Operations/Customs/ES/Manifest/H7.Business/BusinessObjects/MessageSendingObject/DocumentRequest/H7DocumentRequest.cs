using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public sealed class H7DocumentRequest : CommonDocumentRequest<AsycudaBill>
	{
		public H7DocumentRequest(AsycudaBill bill, ZString certName) : base(bill, certName)
		{
			mrnCode = Argument.NotNullOrEmpty(bill.H7MovementReferenceNumber, nameof(bill.H7MovementReferenceNumber));
		}

		protected override ZInt RequestMissingDocumentsCore()
		{
			var messagesSentCount = ZInt.Zero;

			if (!csvClearance.IsEmpty)
			{
				var clearanceDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.H7ClearanceDoc);

				if (clearanceDocMissing)
				{
					CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.H7ClearanceDoc, csvClearance);
					messagesSentCount++;
				}
			}

			return messagesSentCount;
		}

		protected override GlbStaff GetBroker() => businessObject.Header.CustomsAgent;

		protected override ZString GetBGMReference(AsycudaBill businessObject) => businessObject.ABL_BillNumber;

		protected override ZString GetCSVReference(AsycudaBill businessObject) => businessObject.ClearanceReferenceNumber;

		protected override EDIMessageCollection GetMessages(AsycudaBill businessObject) => businessObject.Messages;

		protected override ZBool GetIsTrainingDeclaration()
		{
			var registration = ObjectFactory.Get<IProductRegistration>();
			return registration.Key.DatabaseType != DatabaseTypes.Codes.Production
						&& (!registration.IsWiseTechGlobalInternalSystem() || (bool)businessObject.Header.TrainingEntry);
		}
	}
}
