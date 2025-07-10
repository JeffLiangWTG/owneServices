using System.IO;
using System.Text;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.AUS.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.AUS.Products
{
	public class ExportProducts
	{
		public ExportProducts(ZGuid importerPK, ZGuid supplierPK, bool includeManuallyAddedParts, bool closeRecords, ZString emailAddress)
		{
			this.ImporterPK = importerPK;
			this.SupplierPK = supplierPK;
			this.IncludeManuallyAddedParts = includeManuallyAddedParts;
			this.CloseRecords = closeRecords;
			this.EmailAddress = emailAddress;
			ClosedTime = ZDateTime.UtcNow;
		}

		#region Factory

		public BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}

				return fFactory;
			}
		}

		BusinessObjectFactory fFactory;

		#endregion

		#region Interface Mediator

		public ClientAUSProductInterfaceMediator InterfaceMediator
		{
			get
			{
				if (fInterfaceMediator == null)
				{
					fInterfaceMediator = new ClientAUSProductInterfaceMediator(ImporterPK, SupplierPK);
				}

				return fInterfaceMediator;
			}
		}

		ClientAUSProductInterfaceMediator fInterfaceMediator;

		#endregion

		#region Importer

		public OrgHeader Importer
		{
			get
			{
				if (fImporter == null)
				{
					fImporter = (OrgHeader)Factory.Load(typeof(OrgHeader), ImporterPK);
				}

				return fImporter;
			}
		}

		OrgHeader fImporter;

		#endregion

		#region Supplier

		public OrgHeader Supplier
		{
			get
			{
				if (fSupplier == null)
				{
					fSupplier = (OrgHeader)Factory.Load(typeof(OrgHeader), SupplierPK);
				}

				return fSupplier;
			}
		}

		OrgHeader fSupplier;

		#endregion

		#region Registry Data

		public ClientAUSProductImportRegistry RegistryData
		{
			get
			{
				if (fRegistryData == null)
				{
					ZQuery registryFilter = new ZQuery(ClientAUSProductImportRegistrySchema.T6_OH_Importer, ImporterPK);
					registryFilter.AddToFilter(ClientAUSProductImportRegistrySchema.T6_OH_Supplier, SupplierPK);
					fRegistryData = (ClientAUSProductImportRegistry)Factory.LoadTop1(typeof(ClientAUSProductImportRegistry), registryFilter);
				}

				return fRegistryData;
			}
		}

		ClientAUSProductImportRegistry fRegistryData;

		#endregion

		#region Cancel

		public void CancelExport()
		{
			CancelProcess = true;
		}

		#endregion

		public void Export(ProcessedEventHandler handler)
		{
			try
			{
				using (var transactionManager = InterfaceMediator.BeginTransactionWithManager())
				{
					UpdateProductInterfaceTableWithEnterpriseChanges(InterfaceMediator, handler);

					if (!CancelProcess)
					{
						CreateExportFiles(InterfaceMediator, handler);
					}

					if (CloseRecords && !CancelProcess)
					{
						CloseRecordsAndCreateBackupFile(InterfaceMediator, handler);
					}

					if (!CancelProcess)
					{
						transactionManager.CommitTransaction();
					}
				}
			}
			catch (IOException)
			{
				Globals.Message.ShowError("The file " + FileAccessError + " cannot be accessed. (It is currently open by another process)." + System.Environment.NewLine + System.Environment.NewLine + "Data Export has been cancelled");
			}
		}

		#region Implementation

		readonly ZGuid ImporterPK;
		readonly ZGuid SupplierPK;
		readonly bool IncludeManuallyAddedParts;
		readonly bool CloseRecords;
		readonly ZString EmailAddress;
		readonly ZDateTime ClosedTime;
		bool CancelProcess;
		string FileAccessError;

		#region Update Interface Table

		void UpdateProductInterfaceTableWithEnterpriseChanges(ClientAUSProductInterfaceMediator mediator, ProcessedEventHandler handler)
		{
			///
			/// Update the interface table with any enterprise products for this Importer/Supplier that have been added manually,
			/// then scan the interface table and delete any records where the parts have been deleted from enterprise,
			/// plus update all other records with current details stored in enterprise
			///

			InitialiseProgressCounters();
			if (IncludeManuallyAddedParts)
			{
				OrgSupplierPartCollection partsList = ImporterSupplierPartsList;
				partsList.Load();
				AddManuallyAddedProductsToInterfaceTable(partsList, mediator, handler);
			}

			ClientAUSProductInterfaceMediator.ProductInterfaceDetails[] productsInterfaceList = null;
			if (!CancelProcess)
			{
				InitialiseProgressCounters();
				productsInterfaceList = mediator.GetAllProductKeysForThisImporterAndSupplier();
				foreach (ClientAUSProductInterfaceMediator.ProductInterfaceDetails interfaceKeys in productsInterfaceList)
				{
					if (CancelProcess)
					{
						break;
					}
					else
					{
						if (PartHasBeenDeleted(interfaceKeys.PartPK))
						{
							mediator.DeletePartFromProductInterface(interfaceKeys.InterfacePK);
						}
						else
						{
							UpdateProductsInInterfaceTable(mediator, interfaceKeys);
						}

						HandlerProcessing(handler, productsInterfaceList.Length, "Updating Parts in Interface Table with " + BrandingFactory.Instance.ProductName + " Values");
					}
				}
			}

			if (!CancelProcess)
			{
				InitialiseProgressCounters();
				foreach (ClientAUSProductInterfaceMediator.ProductInterfaceDetails interfaceKeys in productsInterfaceList)
				{
					if (CancelProcess)
					{
						break;
					}
					else
					{
						if (mediator.InterfaceRecordExists(interfaceKeys.InterfacePK))
						{
							SetChangedRecordStatusIfRequired(mediator, interfaceKeys.InterfacePK);
						}

						HandlerProcessing(handler, productsInterfaceList.Length, "Updating Status of Parts in Interface Table");
					}
				}
			}
		}

		void AddManuallyAddedProductsToInterfaceTable(OrgSupplierPartCollection partsList, ClientAUSProductInterfaceMediator mediator, ProcessedEventHandler handler)
		{
			InitialiseProgressCounters();
			ZDateTime highWaterMark = mediator.GetHighWaterMarkForDataExportForThisImporterAndSupplier();

			foreach (AUOrgSupplierPart part in partsList)
			{
				if (CancelProcess)
				{
					break;
				}
				else
				{
					if (PartHasBeenManuallyAdded(part))
					{
						if (PartAddedAfterHighWaterMark(part, highWaterMark))
						{
							AddPartToProductInterface(part, mediator);
						}
					}

					HandlerProcessing(handler, partsList.Count, "Checking for Manually Added Parts to add to Interface Table");
				}
			}
		}

		bool PartHasBeenDeleted(ZGuid productPK)
		{
			ZQuery doesPartExistFilter = new ZQuery(OrgSupplierPartSchema.PK, productPK);
			return !Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(OrgSupplierPart)), doesPartExistFilter);
		}

		void UpdateProductsInInterfaceTable(ClientAUSProductInterfaceMediator mediator, ClientAUSProductInterfaceMediator.ProductInterfaceDetails interfaceKeys)
		{
			AUOrgSupplierPart enterpriseProduct = (AUOrgSupplierPart)Factory.Load(typeof(AUOrgSupplierPart), interfaceKeys.PartPK);
			ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData dataToUpdate = new ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData()
			{
				NewProductCode = enterpriseProduct.OP_PartNum,
				NewProductDesc = enterpriseProduct.OP_Desc,
				NewImportClassification = GetImportClassification(enterpriseProduct.PivotsForBinding?.GetMatch(Customs.Business.ClassificationTypeList.Codes.HTI, mediator.ImporterPK, mediator.SupplierPK, true)?.CI_CC ?? ZGuid.Empty)
			};
			mediator.UpdateRowWithCurrentDetails(dataToUpdate, interfaceKeys.InterfacePK);
		}

		void SetChangedRecordStatusIfRequired(ClientAUSProductInterfaceMediator mediator, ZGuid interfacePK)
		{
			if (mediator.CheckChangedDetails(interfacePK))
			{
				mediator.UpdateRowStatus(interfacePK, 'E', "Imported - Changed");
			}
		}

		string GetImportClassification(ZGuid classPK)
		{
			string classLookupCode = "";
			Classification importClassification = (Classification)Factory.Load(typeof(Classification), classPK);
			if (importClassification != null)
			{
				classLookupCode = importClassification.CC_LookupCode;
			}

			return classLookupCode;
		}

		bool PartHasBeenManuallyAdded(OrgSupplierPart part)
		{
			bool partHasBeenManuallyCreated = true;

			ZQuery dataImportFilter = new ZQuery(StmALogSchema.SL_Table, "OrgSupplierPart");
			dataImportFilter.AddToFilter(JoinCondition.And, StmALogSchema.SL_Parent, SQLComparisonOperator.Equal, part.PK);
			dataImportFilter.AddToFilter(JoinCondition.And, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, "DIM");
			StmALog[] dataImportLogs = (StmALog[])Factory.Load(typeof(StmALog), dataImportFilter);

			if (dataImportLogs.Length > 0)
			{
				partHasBeenManuallyCreated = false;
			}

			return partHasBeenManuallyCreated;
		}

		bool PartAddedAfterHighWaterMark(OrgSupplierPart part, ZDateTime highWaterMark)
		{
			bool partAddedAfterHighWaterMark = false;

			ZQuery partAddedDateFilter = new ZQuery(StmALogSchema.SL_Table, "OrgSupplierPart");
			partAddedDateFilter.AddToFilter(JoinCondition.And, StmALogSchema.SL_Parent, SQLComparisonOperator.Equal, part.PK);
			partAddedDateFilter.AddToFilter(JoinCondition.And, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, "ADD");
			partAddedDateFilter.AddToFilter(JoinCondition.And, StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThan, highWaterMark);
			StmALog[] partAddedLog = (StmALog[])Factory.Load(typeof(StmALog), partAddedDateFilter);

			if (partAddedLog.Length > 0)
			{
				partAddedAfterHighWaterMark = true;
			}

			return partAddedAfterHighWaterMark;
		}

		OrgSupplierPartCollection ImporterSupplierPartsList
		{
			get
			{
				ZQuery importerSupplierParts = GetImportSupplierFilter(Importer, Supplier);
				return new OrgSupplierPartCollection(Factory, importerSupplierParts);
			}
		}

		public static ZDBOnlyQuery GetImportSupplierFilter(OrgHeader importer, OrgHeader supplier)
		{
			ZDBOnlyQuery queryFilter = new ZDBOnlyQuery(typeof(OrgSupplierPart));

			if (importer != null)
			{
				ZDBOnlySubQuery subQueryOwner = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
				subQueryOwner.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_OH, importer.PK);
				ZQuery relationshipQuery = new ZQuery(OrgPartRelationSchema.OU_Relationship, OrgPartRelation.RelationshipTypes.Owner);
				relationshipQuery.AddToFilter(JoinCondition.Or, OrgPartRelationSchema.OU_Relationship, OrgPartRelation.RelationshipTypes.Both);
				subQueryOwner.AddToFilter(relationshipQuery, JoinCondition.And);
				queryFilter.AddSubQuery(subQueryOwner, JoinCondition.And);
			}

			if (supplier != null)
			{
				ZDBOnlySubQuery subQuerySupplier = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
				subQuerySupplier.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_OH, SQLComparisonOperator.Equal, supplier.PK);
				ZQuery relationshipQuery = new ZQuery(OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.Equal, OrgPartRelation.RelationshipTypes.Supplier);
				relationshipQuery.AddToFilter(JoinCondition.Or, OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.Equal, OrgPartRelation.RelationshipTypes.Both);
				subQuerySupplier.AddToFilter(relationshipQuery, JoinCondition.And);
				queryFilter.AddSubQuery(subQuerySupplier, JoinCondition.And);
			}

			return queryFilter;
		}

		void AddPartToProductInterface(AUOrgSupplierPart part, ClientAUSProductInterfaceMediator mediator)
		{
			if (!mediator.InterfaceRecordExistsForPart(part.PK))
			{
				ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData dataToLoad = GetDataFromEnterpisePart(part, mediator.ImporterPK, mediator.SupplierPK);
				mediator.InsertRow(dataToLoad);
			}
		}

		ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData GetDataFromEnterpisePart(AUOrgSupplierPart part, ZGuid importerPK, ZGuid supplierPK)
		{
			ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData dataToLoad = new ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData()
			{
				OriginalProductCode = part.OP_PartNum,
				OriginalProductDesc = part.OP_Desc,
				OriginalImportClassification = GetImportClassification(part.PivotsForBinding?.GetMatch(Customs.Business.ClassificationTypeList.Codes.HTI, importerPK, supplierPK, true)?.CI_CC ?? ZGuid.Empty),
				ProductPK = part.PK,
				ModifiedDateTime = part.Logs.CreatedDateUtc.Date,
				Indicator = 'M',
				Status = "Manually Added"
			};
			return dataToLoad;
		}

		#endregion

		#region Export Data Files

		void CreateExportFiles(ClientAUSProductInterfaceMediator mediator, ProcessedEventHandler handler)
		{
			if (RegistryData != null)
			{
				string directory = ClientSharedComponents.SharedUtil.GetFinalPath(RegistryData.DirectoryToStoreExportFiles);
				string allProductsFileName = RegistryData.AllProductsFileName;
				string clientInvoicingFileName = RegistryData.ClientInvoicingFileName;
				string productUpdateFileName = RegistryData.ProductUpdateFileName;

				if (!CancelProcess)
				{
					CreateAllProductsFile(mediator, handler, directory, allProductsFileName);
				}

				if (!CancelProcess)
				{
					CreateClientInvoicingFile(mediator, handler, directory, clientInvoicingFileName);
				}

				if (!CancelProcess)
				{
					CreateClientDataUpdateFile(mediator, handler, directory, productUpdateFileName);
				}
			}
		}

		#region All Products File

		void CreateAllProductsFile(ClientAUSProductInterfaceMediator mediator, ProcessedEventHandler handler, string exportDirectory, string fileName)
		{
			InitialiseProgressCounters();
			ExportFileType fileType = ExportFileType.AllProducts;

			ClientAUSProductInterfaceMediator.ProductInterfaceData[] productsList = mediator.GetAllProductsForThisImporterAndSupplier(IncludeManuallyAddedParts);
			foreach (ClientAUSProductInterfaceMediator.ProductInterfaceData productData in productsList)
			{
				if (CancelProcess)
				{
					break;
				}
				else
				{
					OutputExportFile(fileType, productData.ProductCode, productData.ProductDesc, productData.ImportClassification, productData.Status);
					HandlerProcessing(handler, productsList.Length, "Creating All Products Export File");
				}
			}

			SaveExportFile(exportDirectory, fileName, fileType);
		}

		#endregion

		#region Client Invoicing File

		void CreateClientInvoicingFile(ClientAUSProductInterfaceMediator mediator, ProcessedEventHandler handler, string exportDirectory, string fileName)
		{
			InitialiseProgressCounters();
			ExportFileType fileType = ExportFileType.ClientInvoicing;

			ClientAUSProductInterfaceMediator.ProductInterfaceData[] productsList = mediator.GetClientInvoicingForThisImporterAndSupplier();
			foreach (ClientAUSProductInterfaceMediator.ProductInterfaceData productData in productsList)
			{
				if (CancelProcess)
				{
					break;
				}
				else
				{
					OutputExportFile(fileType, productData.ProductCode, productData.ProductDesc, productData.ImportClassification, productData.Status);
					HandlerProcessing(handler, productsList.Length, "Creating Client Invoicing Export File");
				}
			}

			SaveExportFile(exportDirectory, fileName, fileType);
		}

		#endregion

		#region Client Update File

		void CreateClientDataUpdateFile(ClientAUSProductInterfaceMediator mediator, ProcessedEventHandler handler, string exportDirectory, string fileName)
		{
			InitialiseProgressCounters();
			ExportFileType fileType = ExportFileType.ClientUpdate;

			ClientAUSProductInterfaceMediator.ProductInterfaceData[] productsList = mediator.GetClientUpdateDataForThisImporterAndSupplier(IncludeManuallyAddedParts);
			foreach (ClientAUSProductInterfaceMediator.ProductInterfaceData productData in productsList)
			{
				if (CancelProcess)
				{
					break;
				}
				else
				{
					OutputExportFile(fileType, productData.ProductCode, productData.ProductDesc, productData.ImportClassification, productData.Status);
					HandlerProcessing(handler, productsList.Length, "Creating Client Update Export File");
				}
			}

			SaveExportFile(exportDirectory, fileName, fileType);
		}

		#endregion

		#region Export File Creation

		void OutputExportFile(ExportFileType fileType, string productNo, string productDesc, string impClass, string status)
		{
			string productLine = "\"" + productNo + "\",\"" + productDesc + "\",\"" + impClass + "\",\"" + status + "\"";

			if (fileType == ExportFileType.AllProducts)
			{
				if (AllProductsFile != null)
				{
					AllProductsFile.Append(productLine);
					AllProductsFile.Append(System.Environment.NewLine);
				}
			}
			else if (fileType == ExportFileType.ClientInvoicing)
			{
				if (ClientInvoicingFile != null)
				{
					ClientInvoicingFile.Append(productLine);
					ClientInvoicingFile.Append(System.Environment.NewLine);
				}
			}
			else if (fileType == ExportFileType.ClientUpdate)
			{
				if (ClientDataUpdateFile != null)
				{
					ClientDataUpdateFile.Append(productLine);
					ClientDataUpdateFile.Append(System.Environment.NewLine);
				}
			}
		}

		void SaveExportFile(string directory, string fileName, ExportFileType fileType)
		{
			if (!Globals.IsTest && !CancelProcess)
			{
				if (string.IsNullOrEmpty(fileName))
				{
					fileName = fileType.ToString();
				}

				if (fileType == ExportFileType.AllProducts)
				{
					CreateExportFile(AllProductsFile, directory, fileName);
				}
				else if (fileType == ExportFileType.ClientInvoicing)
				{
					CreateExportFile(ClientInvoicingFile, directory, fileName);
				}
				else if (fileType == ExportFileType.ClientUpdate)
				{
					CreateAndEmailExportFile(ClientDataUpdateFile, directory, fileName, true);
				}
			}
		}

		void CreateExportFile(StringBuilder fileData, string dataDirectory, string fileName)
		{
			CreateAndEmailExportFile(fileData, dataDirectory, fileName, false);
		}

		void CreateAndEmailExportFile(StringBuilder fileData, string dataDirectory, string fileName, bool emailFile)
		{
			DirectoryInfo directory = new DirectoryInfo(dataDirectory);
			if (!directory.Exists)
			{
				directory.Create();
			}

			if (!fileName.EndsWith(".csv"))
			{
				fileName = fileName + ".csv";
			}

			string pathAndFileName = Path.Combine(directory.FullName, fileName);
			FileInfo outputFile = new FileInfo(pathAndFileName);

			try
			{
				if (outputFile.Exists)
				{
					outputFile.Delete();
				}

				using (StreamWriter sw = new StreamWriter(pathAndFileName, true))
				{
					sw.WriteLine(fileData);
					sw.Flush();
				}

				if (emailFile)
				{
					EmailUpdateFileToRegistryValueAddress(fileData);
				}
			}
			catch (IOException)
			{
				FileAccessError = pathAndFileName;
				throw;
			}
		}

		#region Email Processing

		void EmailUpdateFileToRegistryValueAddress(StringBuilder fileData)
		{
			if (EmailAddress != "" && !CancelProcess)
			{
				EmailDef email = new EmailDef();
				email.AddRecipientForSystemCommunication(EmailAddress);
				email.Subject = "Austin Customs - Product Update File";
				email.Body = "Please find attached a product update file from Austin Customs." + System.Environment.NewLine + System.Environment.NewLine +
					"The format of the .csv file is: Product Number, Product Description, Import Classification, Status" + System.Environment.NewLine + System.Environment.NewLine +
					"Regards," + System.Environment.NewLine + "Austin International Trade Services P/L.";

				string attachmentLabel = "ClientDataUpdate.csv";
				byte[] attachmentData = Encoding.ASCII.GetBytes(fileData.ToString());

				email.Attachments.Add(new AttachmentDef(attachmentLabel, attachmentData));

				try
				{
					Env.OutgoingMailManager.CreateAndSave(email);
				}
				catch (EmailNotCompleteException)
				{
					ErrorReporter.ReportOnce("Forward To Email", "The Austin Customs Product Update File cannot be forwarded as the email details are not complete - probably the registry table.");
				}
				catch (EmailSendFailedException ex)
				{
					ErrorReporter.ReportOnce("Trying to forward the Product Update file for Austin Customs", ex);
				}
			}
		}

		#endregion

		#endregion

		enum ExportFileType { AllProducts, ClientInvoicing, ClientUpdate }
		readonly StringBuilder AllProductsFile = new StringBuilder();
		readonly StringBuilder ClientInvoicingFile = new StringBuilder();
		readonly StringBuilder ClientDataUpdateFile = new StringBuilder();

		#endregion

		#region Backup Processing

		void CloseRecordsAndCreateBackupFile(ClientAUSProductInterfaceMediator mediator, ProcessedEventHandler handler)
		{
			///
			/// Export a copy of all records from the ClientAUSProductInterface table that do not already have an indicator of "C"
			/// and then flag these records with the indicator "C" and set the closed date time value.
			///
			InitialiseProgressCounters();

			ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData[] productsInterfaceList = mediator.GetAllProductsForBackUpForThisImporterAndSupplier();
			foreach (ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData interfaceData in productsInterfaceList)
			{
				OutputToBackupFile(interfaceData);
				mediator.UpdateProductAsClosed(interfaceData.PK, ClosedTime);
				HandlerProcessing(handler, productsInterfaceList.Length, "Creating Back-up File");
			}

			if (RegistryData != null)
			{
				string directory = ClientSharedComponents.SharedUtil.GetFinalPath(RegistryData.DirectoryToStoreExportFiles);
				string backUpFileName = RegistryData.BackUpFileName;
				SaveBackupFile(directory, backUpFileName);
			}
		}

		#region BackUp Creation

		#region BackupDataFile

		StringBuilder BackupDataFile
		{
			get
			{
				if (fBackupDataFile == null)
				{
					fBackupDataFile = new StringBuilder();
					string headerLine = @"Product Interface Key, Importer Key, Supplier Key, Orig Product Code, Orig Product Description, Orig Import Classification, " + BrandingFactory.Instance.ProductName + " Product Key, Import Date Time, New Product Code, New Product Description, New Import Classification, Closed Date Time, Modified Date Time, Indicator, Status";
					fBackupDataFile.Append(headerLine);
					fBackupDataFile.Append(System.Environment.NewLine);
				}

				return fBackupDataFile;
			}
		}

		StringBuilder fBackupDataFile;

		#endregion

		void OutputToBackupFile(ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData interfaceData)
		{
			if (BackupDataFile != null)
			{
				string productLine = "\"" + interfaceData.PK + "\"," +
					"\"" + interfaceData.ImporterPK + "\"," +
					"\"" + interfaceData.SupplierPK + "\"," +
					"\"" + interfaceData.OriginalProductCode + "\"," +
					"\"" + interfaceData.OriginalProductDesc + "\"," +
					"\"" + interfaceData.OriginalImportClassification + "\"," +
					"\"" + interfaceData.ProductPK + "\"," +
					"\"" + interfaceData.ImportDateTime + "\"," +
					"\"" + interfaceData.NewProductCode + "\"," +
					"\"" + interfaceData.NewProductDesc + "\"," +
					"\"" + interfaceData.NewImportClassification + "\"," +
					"\"" + interfaceData.ClosedDateTime + "\"," +
					"\"" + interfaceData.ModifiedDateTime + "\"," +
					"\"" + interfaceData.Indicator + "\"," +
					"\"" + interfaceData.Status + "\"";

				BackupDataFile.Append(productLine);
				BackupDataFile.Append(System.Environment.NewLine);
			}
		}

		void SaveBackupFile(string directory, string fileName)
		{
			if (!Globals.IsTest)
			{
				CreateBackupFile(BackupDataFile, directory, fileName);
			}
		}

		void CreateBackupFile(StringBuilder fileData, string dataDirectory, string fileName)
		{
			DirectoryInfo directory = new DirectoryInfo(dataDirectory);
			if (!directory.Exists)
			{
				directory.Create();
			}

			if (!fileName.EndsWith(".csv"))
			{
				fileName = fileName + ".csv";
			}

			string pathAndFileName = Path.Combine(directory.FullName, fileName);

			FileInfo backUpFile = new FileInfo(pathAndFileName);
			if (backUpFile.Exists)
			{
				backUpFile.Delete();
			}

			using (StreamWriter sw = new StreamWriter(pathAndFileName, true))
			{
				sw.WriteLine(fileData);
				sw.Flush();
			}
		}

		#endregion

		#endregion

		#region ProgressWindow

		void HandlerProcessing(ProcessedEventHandler handler, int collectionSize, string progressText)
		{
			ProgressCounter++;
			float percentage = (ProgressCounter / collectionSize) * 100;
			if (percentage - PreviousPercentage > 1)
			{
				handler(this, new ProcessedEventArgs((int)percentage, 0, 0, progressText));
				PreviousPercentage = percentage;
			}
		}

		void InitialiseProgressCounters()
		{
			PreviousPercentage = 0;
			ProgressCounter = 0;
		}

		float PreviousPercentage;
		float ProgressCounter;

		#endregion

		#endregion
	}
}
