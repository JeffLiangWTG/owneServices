using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class ExitControlDocumentRequest : CommonDocumentRequest<CusExitReport>
	{
		public ExitControlDocumentRequest(CusExitReport exitReport, ZString certName) : base(exitReport, certName)
		{
			Argument.NotNull(exitReport.Header, nameof(exitReport.Header));
			var consignment = Argument.NotNull(exitReport.Consignment, nameof(exitReport.Consignment));
			mrnCode = Argument.NotNullOrEmpty(consignment.CXC_MovementReference, nameof(consignment.CXC_MovementReference));
		}

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

		protected override ZString GetBGMReference(CusExitReport businessObject) => businessObject.Consignment.CXC_MovementReference;

		protected override ZString GetCSVReference(CusExitReport businessObject) => businessObject.ClearanceReferenceNumber;

		protected override ZBool GetIsTrainingDeclaration()
		{
			var registration = ObjectFactory.Get<IProductRegistration>();
			return registration.Key.DatabaseType != DatabaseTypes.Codes.Production
						&& (!registration.IsWiseTechGlobalInternalSystem()
							|| (bool)businessObject.Header.TrainingEntry);
		}

		protected override EDIMessageCollection GetMessages(CusExitReport businessObject) => businessObject.Messages;

		protected override GlbStaff GetBroker() => businessObject.Header.CustomsAgent;
	}
}
