using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.DataTransfer.DataExport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.DataExport
{
	public sealed class DirectDebitBatchDataExportAdapter : IDisposable
	{
		public DirectDebitBatchDataExportAdapter(BusinessObjectFactory factory, DirectDebitBatchHeader header)
		{
			Factory = factory;
			Header = header;
		}

		public void Dispose()
		{
			if (exportWizardFileMapper != null)
			{
				exportWizardFileMapper.Dispose();
			}
		}

		readonly BusinessObjectFactory Factory;
		readonly DirectDebitBatchHeader Header;

		public IEnumerable<BusinessObject> GetBusinessObjectsForExport()
		{
			return GetBusinessObjectsForExport(Header);
		}

		public IEnumerable<BusinessObject> GetBusinessObjectsForExport(DirectDebitBatchHeader header)
		{
			int numberOfRowsForNordeaFormat = 0;
			DataExportFileHeader exportFileHeader = new DataExportFileHeader(Factory, header);
			numberOfRowsForNordeaFormat++;
			yield return exportFileHeader;

			DataExportDirectDebitBatchHeader exportHeader = new DataExportDirectDebitBatchHeader(Factory, header);
			numberOfRowsForNordeaFormat++;
			yield return exportHeader;

			int sequenceNumberOffset = 0;
			int.TryParse(exportHeader.SequenceNumberOffset, out sequenceNumberOffset);
			int sequenceNumber = 1 + sequenceNumberOffset;
			foreach (TransactionHeader payment in header.Lines)
			{
				DataExportPayment exportPayment = new DataExportPaymentHeader(Factory, payment);
				exportPayment.DDRHeader = exportHeader;

				TransactionMatchLink[] allMatchLinks = null;

				if (payment is Payment)
				{
					AccTransactionMatchLink paymentMatchLink = payment.Factory.LoadTop1<AccTransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, payment.PK));
					allMatchLinks = DataTransfer.DataExport.DirectDebitBatchDataExportAdapter.GetAllMatchlinksExcludingEXX((Payment)payment, paymentMatchLink);
					exportPayment.MatchLinks = allMatchLinks;
				}
				exportPayment.SequenceNumber = sequenceNumber;
				numberOfRowsForNordeaFormat++;
				yield return exportPayment;

				if (payment is Payment)
				{
					List<TransactionHeader> transactionHeaders = new List<TransactionHeader>();

					if (allMatchLinks != null)
					{
						foreach (var matchLink in allMatchLinks)
						{
							TransactionHeader transactionHeader = payment.Factory.Load<TransactionHeader>(matchLink.AP_AH);
							transactionHeader.SetMatchedAmount(matchLink, false);
							transactionHeaders.Add(transactionHeader);
						}
					}

					transactionHeaders.Sort((x, y) => { return CompareTransactionHeader(x, y); });

					foreach (TransactionHeader transactionHeader in transactionHeaders)
					{
						DataExportPaidTransaction exportPaidTransaction = new DataExportPaidTransaction(Factory, transactionHeader, exportPayment);
						exportPaidTransaction.DDRHeader = exportHeader;
						numberOfRowsForNordeaFormat++;
						yield return exportPaidTransaction;
					}
				}
				exportPayment = new DataExportPaymentFooter(Factory, payment);
				exportPayment.DDRHeader = exportHeader;
				exportPayment.SequenceNumber = sequenceNumber++;
				if (allMatchLinks != null)
				{
					exportPayment.MatchLinks = allMatchLinks;
				}
				//numberOfRowsForNordeaFormat++;		//we deliberately ignore the counting for Nordea Payment footer here
				yield return exportPayment;
			}
			DataExportDirectDebitBatchFooter exportFooter = new DataExportDirectDebitBatchFooter(Factory, header);
			numberOfRowsForNordeaFormat++;
			yield return exportFooter;

			DataExportFileFooter exportFileFooter = new DataExportFileFooter(Factory, header);
			numberOfRowsForNordeaFormat++;
			yield return exportFileFooter;

			int rowsToAddForNordeaFormat = 10 - (numberOfRowsForNordeaFormat % 10);
			if (rowsToAddForNordeaFormat > 0 && rowsToAddForNordeaFormat < 10)
			{
				for (int i = 0; i < rowsToAddForNordeaFormat; i++)
				{
					DataExportNordeaFormatSpacingFooter exportNordeaFooter = new DataExportNordeaFormatSpacingFooter(Factory, header);
					yield return exportNordeaFooter;
				}
			}
		}

		public IExportCollectionInfo GetMultiTypeCollectionInfo(IEnumerable<BusinessObject> businessObjects)
		{
			var info = new ExportCollectionInfoImpl(Factory, businessObjects);

			AddProperties<DataExportFileHeader>(info, ResString.GetMultilingualString("d0fd2988-1bbf-40cc-9192-a4678bbbe01e", "File Header"));
			AddProperties<DataExportDirectDebitBatchHeader>(info, ResString.GetMultilingualString("7b760388-1766-48ea-a71a-9a08029c764e", "Direct Debit Batch Header"));
			AddProperties<DataExportPaymentHeader>(info, ResString.GetMultilingualString("a4a9489f-33a9-4d11-8152-185ad129b9a1", "Payment Header"));
			AddProperties<DataExportPaidTransaction>(info, ResString.GetMultilingualString("23dc75f9-32d4-413a-a1cb-32727e93125a", "Paid Transaction"));
			AddProperties<DataExportPaymentFooter>(info, ResString.GetMultilingualString("5589ec06-afdf-4286-a0fa-b7836f31d2cd", "Payment Footer"));
			AddProperties<DataExportDirectDebitBatchFooter>(info, ResString.GetMultilingualString("78755576-234d-492a-94bd-caf51e58d6fa", "Direct Debit Batch Footer"));
			AddProperties<DataExportFileFooter>(info, ResString.GetMultilingualString("304aeb2b-2023-4c23-a814-3a29e2303161", "File Footer"));
			AddProperties<DataExportNordeaFormatSpacingFooter>(info, ResString.GetMultilingualString("6f165ebf-9bca-450e-a84e-653fdc173202", "Nordea Format Spacing Footer"));
			return info;
		}

		public IExportCollectionInfo GetMultiTypeCollectionInfo(DirectDebitBatchHeader header)
		{
			//Note: ToList() is important! The List is more efficient when you need to enumerate the data multiple times because it already has all of it in memory.
			var listForExport = GetBusinessObjectsForExport(header).ToList();
			return GetMultiTypeCollectionInfo(listForExport);
		}

		ExportWizard ExportWizard
		{
			get
			{
				if (exportWizard == null)
				{
					Sort(Header);
					IExportCollectionInfo collectionInfo = GetMultiTypeCollectionInfo(BusinessObjectsForExport);
					string contextPrefix = "DEW:"; // Context Prefix does not need to be localised.
					string contextKey = "DDRBatch";
					exportWizardFileMapper = new InMemoryFileMapper();
					exportWizard = new ExportWizard(collectionInfo, new StmModuleFilterSettingsStorage(contextPrefix, contextKey), exportWizardFileMapper);
				}
				return exportWizard;
			}
		}

		ExportWizard exportWizard;
		InMemoryFileMapper exportWizardFileMapper;

		IEnumerable<BusinessObject> BusinessObjectsForExport
		{
			get
			{
				if (businessObjectsForExport == null)
				{
					businessObjectsForExport = GetBusinessObjectsForExport(Header);
				}
				return businessObjectsForExport;
			}
		}

		IEnumerable<BusinessObject> businessObjectsForExport;

		StmData DDRBatchExportSetting
		{
			get
			{
				if (ddrBatchExportSetting == null)
				{
					ZQuery query = new ZQuery(StmDataSchema.SD_Owner, Header.BankAccount.PK);
					query.AddToFilter(StmDataSchema.SD_Name, "DDRBatchExportSetting");
					ddrBatchExportSetting = Factory.LoadTop1<StmData>(query);
				}
				return ddrBatchExportSetting;
			}
		}

		StmData ddrBatchExportSetting;

		public bool IsFileNameExpressionUsed
		{
			get
			{
				if (DDRBatchExportSetting == null)
				{
					return false;
				}
				ExportWizard.Setting = DDRBatchExportSetting.SD_BinaryValue.ToAscii();
				if (!string.IsNullOrEmpty(ExportWizard.FileNameExpression))
				{
					return true;
				}
				return false;
			}
		}

		public ZString LastExportedFullFileName { get; set; }

		public bool CreateFile(string unmappedFilePath, out string errorMessage)
		{
			if (DDRBatchExportSetting == null)
			{
				errorMessage = Res.GetString("d43bf38d-d85f-4ab7-b341-d141236ab009", "You must configure an export for this bank account.");
				return false;
			}

			ExportWizard.Setting = DDRBatchExportSetting.SD_BinaryValue.ToAscii();
			ExportWizard.FileNameExpressionObject = new DataExportDirectDebitBatchHeader(Header.Factory, Header);

			// Export is done to an in-memory stream (defined in ExportWizard), then copied to the evaluated path: preventing accidental leakage before the eDoc & StmLog are saved.
			var (finalUnmappedPath, exception) = ExportWizard.EvaluateFileNameExpression();
			if (exception != null)
			{
				errorMessage = Res.GetString("6713a9a7-e3f4-44ac-83ed-6f64f6a2f3bf", "Unable to evaluate Filename Expression from Data Export Wizard. Please check your configuration in Actions > Customize Export.\r\n  {0}", exception.GetType().Name + ": " + exception.Message);
				return false;
			}
			if (finalUnmappedPath.IsEmpty)
			{
				finalUnmappedPath = unmappedFilePath;
			}
			ExportWizard.FileNameExpression = ZString.Empty;
			ExportWizard.FileName = finalUnmappedPath;
			var exportSuccessful = ExportWizard.ExportCollection(BusinessObjectsForExport, out errorMessage);

			if (exportSuccessful)
			{
#if DEBUG
				CreateFile_ErrorAction_ForTestOnly?.Invoke(finalUnmappedPath);
#endif

				Header.AddEventLogAndAttachDDRFileToEDocsAndSave(exportWizardFileMapper.MemoryStream, Path.GetFileName(finalUnmappedPath));

				using (var memoryStream = new MemoryStream(exportWizardFileMapper.MemoryStream.ToArray()))      // ExportWizard closes the stream, so must create a new one.
				using (var targetStream = ZSaveFileDialog.OpenFile(finalUnmappedPath))
				{
					memoryStream.CopyTo(targetStream);
				}

				LastExportedFullFileName = finalUnmappedPath;
			}
			return exportSuccessful;
		}

#if DEBUG
		public Action<string> CreateFile_ErrorAction_ForTestOnly;
#endif

		sealed class InMemoryFileMapper : IFileMapper, IDisposable
		{
			readonly MemoryStream fMemoryStream = new MemoryStream();
			public MemoryStream MemoryStream => fMemoryStream;

			public bool IsRemote => false;

			public void Dispose()
			{
				fMemoryStream.Dispose();
			}

			public string GetFolderPath(System.Environment.SpecialFolder folder)
				=> new FileMapper().GetFolderPath(folder);

			public Stream OpenRead(string unmappedPath) => fMemoryStream;

			public Stream OpenWrite(string unmappedPath) => fMemoryStream;
		}

		public void Sort(DirectDebitBatchHeader header)
		{
			header.Lines.Sort<TransactionHeader>((x, y) => { return CompareTransactionHeader(x, y); });
		}

		int CompareTransactionHeader(TransactionHeader x, TransactionHeader y)
		{
			int value = x.AH_Ledger.CompareTo(y.AH_Ledger);

			if (value == 0)
			{
				value = x.AH_TransactionType.CompareTo(y.AH_TransactionType);
				if (value == 0)
				{
					return x.AH_TransactionNum.CompareTo(y.AH_TransactionNum);
				}
				return value;
			}
			return value;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Literal string comparison")]
		void AddProperties<T>(ExportCollectionInfoImpl info, MultilingualString name)
		{
			ImportPropertyInfoCollection fields = new ImportPropertyInfoCollection();
			string schemaClassName = (NoResString)"Schema";
			Type schemaType = typeof(T).BaseType.GetNestedType(schemaClassName) ?? typeof(T).BaseType.BaseType.GetNestedType(schemaClassName);

			if (schemaType != null)
			{
				foreach (FieldInfo fieldInfo in schemaType.GetFields())
				{
					if (fieldInfo.FieldType.Name == "String" && !fieldInfo.Name.EndsWith("MaxLength"))
					{
						fields.Add(new ImportPropertyInfoImpl<T>(fieldInfo.Name));
					}
				}
			}
			info.Add(name, typeof(T), fields);
		}
	}
}
