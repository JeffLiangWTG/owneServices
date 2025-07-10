using System;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Registry;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class DCGResponseMessageProcessor : ImportDeltaDResponseMessageProcessor
	{
		public DCGResponseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			if (message is FREDIMessage frEDIMessage)
			{
				var dataProvider = frEDIMessage.MessageDataObject as IDCGResponseDataProvider;
				if (dataProvider != null)
				{
					var shouldCreateNewOneIfNotLoaded = !dataProvider.HasErrors && !dataProvider.HasAnomalies;
					var statementHeader = CusStatementHeader.LoadOrCreateDCGStatementHeader(message.Factory, message.Company.PK, dataProvider, shouldCreateNewOneIfNotLoaded);
					if (statementHeader != null)
					{
						message.EM_LinkedObject = statementHeader;
						statementHeader.EntryNumber = dataProvider.EntryNum;
						CreateStatementEntriesAndUpdateReportedEntryStatus(message.Factory, statementHeader, dataProvider, frEDIMessage.GetCountryCodeSafe());
						CreateStatementLineCharges(message.Factory, statementHeader, dataProvider);
						statementHeader.B2_Status = GetStatementHeaderStatus(dataProvider);

						if (!dataProvider.HasErrors && !dataProvider.HasAnomalies)
						{
							SendEntryDocs(statementHeader.Factory, statementHeader);
						}
					}

					ProcessorHelper.SendEmail(message.Factory, statementHeader?.B2_SystemCreateUser ?? ZString.Empty, dataProvider.HasErrors || dataProvider.HasAnomalies, GetEmailBody(dataProvider, statementHeader), GetEmailsubject(dataProvider), GetEmailGroupRegistryItem());
					message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
				}
			}
		}

		void SendEntryDocs(BusinessObjectFactory factory, CusStatementHeader header)
		{
			try
			{
				var docManagerSupport = header as IDocManagerSupport;
				var documentFactory = docManagerSupport.DocManagerInfo.MasterFactory;
				var storageMain = documentFactory.RetrieveExistingOrCreateStorageMain(header, Core.Constants.DocManagerCodes.CusStatementHeader);
				var template = ExcelTemplateRetriever.GetTemplate((NoResString)"Global Supplementary Statement", DataContext.Statement, factory);
				var docDataProvider = BODocDataProvider.Get(DocumentWrapperHelper.GetFRSpecificDocumentWrapper(DataContext.Statement, header));
				using (var report = new Report(null, template, docDataProvider, template.TemplateName, null, DocumentDirection.ANY, false))
				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					var binaryData = DocumentConverter.ConvertFromExcel(outputStream.ToArray(), OutputFormatType.PDF, ZArchitecture.Environment.ColourDepth.BlackAndWhite);
					storageMain.AddFileOrDocument(binaryData, $"Global Supplementary Statement - {header.EntryNumber}.pdf", Core.Constants.RefDocTypes.MiscellaneousDocument);
					documentFactory.Save();
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				Logger.Log("The Global Supplementary Statement document has failed to be generated due to the following error : " + e.Message);
			}
		}

		#region E-mail

		ZString GetEmailsubject(IDCGResponseDataProvider dataProvider)
		{
			var responseReference = dataProvider.DCGReference;
			var responseEntryNumber = dataProvider.EntryNum;
			var readableResponseReference = responseReference != ZString.Empty ? (ZString)Res.GetString("449BB296-7659-420D-A10B-5B1A67C3D1AC", " Reference: {0}", responseReference) : ZString.Empty;
			var readableEntryNumber = responseEntryNumber != ZString.Empty ? (ZString)Res.GetString("E2D3E186-D026-4E90-AB53-C0E8EFC338D3", " Entry Number: {0}", responseEntryNumber) : ZString.Empty;

			return Res.GetString("CC330E8D-EDEE-4224-9F39-074231E6C6FA", "New DCG response received.{0}{1}", readableResponseReference, readableEntryNumber);
		}

		Integration.IRegistryItem GetEmailGroupRegistryItem() => FRCustomsDataRegistry.Instance.DCGResponseNotificationGroup;

		ZString GetEmailBody(IDCGResponseDataProvider dataProvider, CusStatementHeader statementHeader)
		{
			var commentOnStatement = ZString.Empty;

			if (statementHeader == null)
			{
				commentOnStatement = Res.GetString("012B4908-1FF4-4CCB-82A5-F842B69D6105", "Liquidation statement does not exist.");
			}
			else
			{
				commentOnStatement = Res.GetString("39E9507A-CA1F-4D1D-AED1-017EEF69B4CC", "Liquidation statement status changed to {0}.", dataProvider.MessageStatusDescription);
			}

			return Res.GetString("44164EC5-254A-4268-9835-3A3765312E4C", "A DCG response has been received. {0}{1}{2}{3}", commentOnStatement, dataProvider.GetReadableTaxList(), dataProvider.GetReadableAnomalyList(), dataProvider.GetReadableErrorList());
		}

		#endregion

		public void CreateStatementLineCharges(BusinessObjectFactory factory, CusStatementHeader statementHeader, IDCGResponseDataProvider dataProvider)
		{
			if (dataProvider.HasTaxes)
			{
				var chargesDetail = statementHeader.ChargesDetail;
				foreach (var tax in dataProvider.Taxes)
				{
					var charge = chargesDetail.Charges.AddNew();
					charge.B4_ChargeType = tax.Codtax;
					charge.B4_ChargeAmount = tax.Montanttax;
					charge.B4_ChargeGroup = tax.Codtaxeeu;
					charge.B4_MethodOfPayment = tax.Statutliquidation;
				}
			}
		}

		void CreateStatementEntriesAndUpdateReportedEntryStatus(BusinessObjectFactory factory, CusStatementHeader statementHeader, IDCGResponseDataProvider dataProvider, ZString country)
		{
			if (dataProvider.HasEntries)
			{
				foreach (var refDsidse in dataProvider.Entries)
				{
					var newStatementEntry = statementHeader.Entries.AddNew();
					newStatementEntry.B3_EntryType = statementHeader.B2_BranchDesignation;
					newStatementEntry.B3_EntryNum = refDsidse.Refdec;
					newStatementEntry.B3_BrokerReference = refDsidse.Refdos;

					UpdateReportedEntryStatus(factory, refDsidse.Refdec, country);
				}
			}
		}

		void UpdateReportedEntryStatus(BusinessObjectFactory factory, ZString refdec, ZString country)
		{
			var reportedEntry = EntryActionHelper.GetEntryHeaderFromEntryNumber(factory, refdec, country);
			if (reportedEntry != null)
			{
				reportedEntry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES140;
			}
		}

		ZString GetStatementHeaderStatus(IDCGResponseDataProvider dataProvider) => dataProvider.HasErrors || dataProvider.HasAnomalies ? StatementStatusList.Codes.Incomplete : StatementStatusList.Codes.Complete;

		protected override ZString MessageType => MessageTypeList.Codes.DCG;
	}
}
