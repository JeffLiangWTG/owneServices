using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Messaging
{
	public class IEOutgoingMessageProcessor : BaseOutgoingMessageProcessor
	{
		public IEOutgoingMessageProcessor(LoggingInformation logger)
		: base(logger)
		{
		}

		protected sealed override bool IsBranchFilter => false;

		protected override ZQuery GetNewFilter()
		{
			var filter = new ZQuery(EDIMessageSchema.EM_Status, EDIMessage.Status.Pending);
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationReference, SQLComparisonOperator.NotEqual, ZString.Empty);
			filter.AddToFilter(EDIMessageSchema.EM_EI, null);
			return filter;
		}

		protected override ZQuery MessageFilter => new ZQuery(EDIMessageSchema.EM_ApplicationCode, new[] { EDIMessage.ApplicationCodes.IECustomsExport, EDIMessage.ApplicationCodes.IECustomsImport, EDIMessage.ApplicationCodes.IECustomsUCC5Import, EDIMessage.ApplicationCodes.IECustomsEMCS, EDIMessage.ApplicationCodes.IECustomsNCTS });

		protected override ZString PreProcessMessages(NonDependentEDIMessageCollection readyMessages)
		{
			foreach (EDIMessage message in readyMessages)
			{
				var factory = message.Factory;
				var applicationCode = message.EM_ApplicationCode;
				ZGuid? warehouseCredentialPK = null;
				if (applicationCode == EDIMessage.ApplicationCodes.IECustomsEMCS)
				{
					warehouseCredentialPK = message.EM_GP;
					if (warehouseCredentialPK.Value.IsEmpty)
					{
						LogEMCSCredentialErrorInfo(message);
						message.EM_Status = EDIMessage.Status.Error;
					}
				}

				if (message.EM_Status != EDIMessage.Status.Error)
				{
					var messageType = message.EM_MessageType;
					var webServiceEndPoint = GetWebServiceEndPoint(factory, applicationCode, messageType);
					var interchange = InterchangeCreator.CreateOutgoingInterchange(factory, applicationCode, messageType, message.EM_GB, webServiceEndPoint, message.EM_MessageText, message.EM_ApplicationReference, warehouseCredentialPK);
					if (interchange != null)
					{
						interchange.ContainedMessages.Add(message);
						message.EM_Status = EDIMessage.Status.Sent;
					}
				}
			}
			return ZString.Empty;
		}

		void LogEMCSCredentialErrorInfo(EDIMessage message)
		{
			var declaration = message.EM_LinkedObject as Business.BaseJobDeclaration;
			if (declaration != null)
			{
				if (declaration.JE_CustomsProfile.IsEmpty)
				{
					Logger.LogError(Res.GetString("3AB65D90-50A1-4662-811B-156EB6987525", "Certificate Identifier of EMCS Declaration(PK:{0}) is blank.", declaration.PK));
				}
				else
				{
					Logger.LogError(Res.GetString("F0D0ABF5-5120-4FC7-B761-9C1BC9A6FC8A", "No existing certificate matched for EMCS Declaration(PK:{0})'s Certificate Identifier.", declaration.PK));
				}
			}
		}

		ZString GetWebServiceEndPoint(BusinessObjectFactory factory, ZString applicationCode, ZString messageType)
		{
			return WebServiceEndPointProvider.GetSubmissionURL(factory, applicationCode, messageType);
		}
	}
}
