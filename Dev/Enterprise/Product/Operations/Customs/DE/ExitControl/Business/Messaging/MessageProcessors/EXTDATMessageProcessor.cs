using System.Linq;
using System.Text;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class EXTDATMessageProcessor : ExportMessageProcessor<AesInboundEDIMessage<IEXTDAT>, IEXTDAT>
	{
		public EXTDATMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("9D1FCBF9-3E90-4A11-951C-F5530D06F3B7", "Export EXTDAT Message Processor");

		protected override BusinessObject GetLinkedObject(AesInboundEDIMessage<IEXTDAT> message) =>
			GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AesInboundEDIMessage<IEXTDAT> message)
		{
			var dataProvider = message.DataProvider;

			message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;

			var cusExitReport = (CusExitReport)message.EM_LinkedObject;
			var consignment = cusExitReport.Consignment;

			if (cusExitReport.CER_Type == DEExitReportTypeList.Codes.Presentation)
			{
				cusExitReport.CER_Status = A0116ATLASStatusCodeList.Codes._310;
				cusExitReport.CER_Type = DEExitReportTypeList.Codes.Transfer;
				cusExitReport.CER_MessageStatus = LogicalStatusList.Codes.Accepted;
				consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._310;
				cusExitReport.Logs.AddNew(Events.CustomsEntryStatus, cusExitReport.CER_Status, ZDateTime.Now.ToOffset());
			}

			foreach (var item in dataProvider.GoodsItems)
			{
				var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
				consignmentItem.CCI_LineNumber = (short)item.DeclarationGoodsItemNumber;
				consignmentItem.CCI_UniqueConsignmentReference = item.ReferenceNumberUCR;
				consignmentItem.CCI_ReferenceNumber = item.RegistrationNumberExternal;
				consignmentItem.CCI_GrossMass = item.GrossMass ?? ZDecimal.Zero;
				consignmentItem.CCI_NetMass = item.NetMass;

				foreach (var package in item.Packages)
				{
					var pivot = consignmentItem.CusExitConsignmentPackagePivots.AddNew();

					var consignmentPackage = pivot.Package;
					consignmentPackage.CXP_Sequence = (short)package.SequenceNumber;
					consignmentPackage.CXP_PackageType = package.TypeOfPackages;
					consignmentPackage.CXP_Quantity = (int)package.NumberOfPackages;
					consignmentPackage.CXP_MarksAndNumbers = package.ShippingMarks;
				}
			}

			foreach (var equipment in dataProvider.TransportEquipment)
			{
				var container = cusExitReport.Header.CusExitContainers.AddNew();
				container.CXN_ContainerNumber = equipment.ContainerIdentificationNumber;
				container.CXN_IsEquipment = (dataProvider.ContainerIndicator ?? 0) == 0;

				foreach (var seal in equipment.SealIdentifier)
				{
					var newSeal = container.AllSealNumbers.AddNew();
					newSeal.BK_SealNumber = seal;
				}

				foreach (var itemNumber in equipment.DeclarationGoodsItemNumber)
				{
					var consignmentItem = consignment.CusExitConsignmentItems.FirstOrDefault(x => x.CCI_LineNumber == itemNumber);
					if (consignmentItem != null)
					{
						foreach (var pivot in consignmentItem.CusExitConsignmentPackagePivots.Cast<CusExitConsignmentPivot>().ToArray())
						{
							if (pivot.CNP_CXN_Container.IsEmpty)
							{
								pivot.CNP_CXN_Container = container.PK;
							}
						}
					}
				}
			}

			var body = CreateEmailBody(dataProvider, cusExitReport);
			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory, cusExitReport.Header, Res.GetString("490E6FD4-BC20-4DD4-81C1-CEE7224E2ECA", "Exit Control  Message"), body, isFailure: false, message.Branch, cusExitReport.Header, dataProvider.ReferencedMessageIdentifier);

			message.SetLogbookRegistrationNumber(message.DataProvider.MovementReferenceNumber);
		}

		string CreateEmailBody(IEXTDAT provider, CusExitReport report)
		{
			var emailBody = new StringBuilder();

			emailBody.Append(Res.GetString("EBC30316-4FA4-4ECB-8443-451BE81A5D0D",
				"Your Exit Control Presentation for Job {0} is confirmed. The data of the export declaration have been provided. For details please follow the Link to the Job.",
				report.Header.CXH_JobReference));

			emailBody.Append((NoResString)"<br/><br/>");

			var emailTable = new HtmlTableCreator();
			emailTable.WriteRow(Res.GetString("EABF5485-A13A-481A-A03C-3AB2A46AC8B5", "MRN"), provider.MovementReferenceNumber);
			emailTable.WriteRow(Res.GetString("2493ACD5-0AFE-4744-9D3D-0525FEC02A61", "LRN"), provider.LocalReferenceNumber);
			emailTable.WriteRow(Res.GetString("F5CFCBCF-6EF5-46D2-BA48-FFAD04C08A54", "Status Text"), Res.GetString("1FB90DB4-032D-4B8F-961D-F67A6A4DC854", "Presentation is confirmed. The data of the export declaration have been provided."));
			emailBody.Append(emailTable.ToHtml());

			return emailBody.ToString();
		}
	}
}
