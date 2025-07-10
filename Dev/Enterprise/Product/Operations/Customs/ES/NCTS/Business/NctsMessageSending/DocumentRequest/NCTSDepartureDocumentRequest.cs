using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NCTSDepartureDocumentRequest : CommonDocumentRequest<NctsHeader>
	{
		public NCTSDepartureDocumentRequest(NctsHeader nctsHeader, ZString certName) : base(nctsHeader, certName)
		{
			mrnCode = Argument.NotNullOrEmpty(nctsHeader.MovementReferenceNumber, nameof(nctsHeader.MovementReferenceNumber));
		}

		protected override ZInt RequestMissingDocumentsCore()
		{
			var messagesSentCount = ZInt.Zero;

			if (!csvClearance.IsEmpty)
			{
				var clearanceDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.NCTSDepartureTADDoc);

				if (clearanceDocMissing)
				{
					CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.NCTSDepartureTADDoc, mrnCode);
					messagesSentCount++;
				}
			}

			return messagesSentCount;
		}

		protected override ZString GetBGMReference(NctsHeader businessObject) => businessObject.BH_JobReference;

		protected override ZString GetCSVReference(NctsHeader businessObject) => businessObject.ClearanceReferenceNumber;

		protected override EDIMessageCollection GetMessages(NctsHeader businessObject) => businessObject.Messages;

		protected override ZBool GetIsTrainingDeclaration()
		{
			var registration = ObjectFactory.Get<IProductRegistration>();
			return registration.Key.DatabaseType != DatabaseTypes.Codes.Production
						&& (!registration.IsWiseTechGlobalInternalSystem()
							|| (bool)businessObject.TrainingEntry);
		}

		protected override GlbStaff GetBroker() => businessObject.IsArrivalMovement ? businessObject.ArrivalMovementHeader.CusAgent : businessObject.MovementHeader.CusAgent;
	}
}
