using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Netting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;
using XmlWriter = Enterprise.UniversalDataBuss.XmlIO.XmlWriting.XmlWriter;

namespace Enterprise.Accounting.DataTransfer.Universal.Netting
{
	public class UniversalTransactionReExporter
	{
		public UniversalTransactionReExporter(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			this.Factory = factory;
			this.Logger = logger;
		}

		readonly BusinessObjectFactory Factory;
		readonly IXmlImportLogger Logger;

		public bool ReExport<T>(IEDIMessage message, ITopLevelDataObject dataObject, T nettingReceivableTransaction)
			where T : BusinessObject
		{
			var streamWrappers = new List<IDeliveryStreamWrapper>();

			var stream = new CargoWise.IO.Shim.SubStreamableStream();
			var result = false;
			try
			{
				var universalTransaction = dataObject as UniversalTransaction;
				ZString receiverEHubID = NettingHelper.GetRecipienteHubIDFromUniversalTransaction(universalTransaction);
				if (!receiverEHubID.IsEmpty)
				{
					var communicationModes = GetCommunicationModes(receiverEHubID, message.Branch.Company.OrganisationPK);

					var xmlWriter = new XmlWriter();
					dataObject.DataContext.SetWorkflowInfo(new WorkflowInfo()
					{
						RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.IDB }, new RecipientRoleDetail() { Type = RecipientRoleType.WNS } }
					});
					xmlWriter.WriteXML(dataObject, stream, UniversalXmlSchema.Version_2011_11.Namespace);

					streamWrappers.Add(new DeliveryStreamWrapperUXML(stream, EntityInfo.New(nettingReceivableTransaction)));

					var context = CreateContext(nettingReceivableTransaction);
					var delivery = new EDIMessageDelivery(string.Empty);
					var deliveryResult = delivery.DeliverBatch(context, communicationModes[0], streamWrappers.ToArray());

#if DEBUG
					UniversalTransactionAfterReExportForTest = universalTransaction;
#endif

					result = deliveryResult.Succeeded;
				}
			}
			catch (IncorrectDataSetupException ex)
			{
				Logger.Log(Enterprise.Integration.LogType.Error, ex.Message);
			}
			finally
			{
				if (stream != null)
				{
					stream.Dispose();
				}
			}

			return result;
		}

		List<EDICommunicationsMode> GetCommunicationModes(ZString receiverEHubID, ZGuid nettingCentreOrgPK)
		{
			var nettingCentre = Factory.Load<OrgHeader>(nettingCentreOrgPK);
			if (nettingCentre != null)
			{
				var communicationModes = nettingCentre.EDICommunicationsModes
									 .Cast<EDICommunicationsMode>()
									 .Where(x =>
										 x.EK_Module == EDICommunicationsMode.Modules.Netting
											 && x.EK_FileFormat == EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction
											 && x.EK_Destination == receiverEHubID)
									 .ToList();

				if (communicationModes.Count == 0)
				{
					throw new IncorrectDataSetupException(Res.GetString("836a253c-9e06-43a2-a01a-594597467743", "No EDI Communications setup found for E Hub ID: '{0}' in Netting Center Organization.", receiverEHubID));
				}
				return communicationModes;
			}

			return new List<EDICommunicationsMode>();
		}

		DeliveryContext CreateContext<T>(T nettingReceivableTransaction) where T : BusinessObject
		{
			return new DeliveryContext(Factory)
			{
				ParentInfo = EntityInfo.New(nettingReceivableTransaction),
				ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging,
				MessageTypeCode = EDIMessageTypeList.Codes.XDC,
				MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalTransaction,
				Notifications = new ErrorLoggerWrapper(Logger)
			};
		}

		class ErrorLoggerWrapper : INotifications
		{
			readonly IXmlImportLogger logger;

			public ErrorLoggerWrapper(IXmlImportLogger logger)
			{
				this.logger = logger;
			}

			public void Add(INotification notification) => logger?.Log(Enterprise.Integration.LogType.Information, notification.Message);
		}

#if DEBUG
		public UniversalTransaction UniversalTransactionAfterReExportForTest;
#endif
	}
}
