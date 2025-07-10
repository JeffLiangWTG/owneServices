using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Business
{
	/// <summary>
	/// Data Importer specifically for Flat File imports
	/// </summary>
	/// 
	public abstract class FlatFileDataImporter : DataImporter
	{
		public FlatFileDataImporter()
		{
		}

		public FlatFileDataImporter(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{
		}

		public FlatFileDataImporter(BusinessObject businessObject)
			: base(businessObject.Factory)
		{
			BusinessEntity = businessObject;
		}

		#region ImportLockWithHash

		protected SqlApplicationLock ImportLockWithHash;

		#endregion

		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			var result = false;

			var buffer = new NotificationBuffer(notifications);
			additionalTransactionActions = Array.Empty<ITransactionParticipant>();
			ImportLockWithHash = null;

			var checkResult = IsValidDataBeforeImport(dataReader, notifications);
			if (checkResult.Result
				&& CanContinueWithAppLockIfApplicable(notifications, checkResult.Hash))
			{
				var xsd = CreateXsd();
				CreateConverter(buffer).ImportFlatFile(xsd, FlatFileFormat, checkResult.ReaderOut);

				if (!buffer.HasErrors)
				{
					if (ExtractToDataAdapter(xsd, buffer))
					{
						if (!HasFatalErrors(buffer))
						{
							PostProcess();
							var importHistoryFactory = CreateImportHistoryRecordIfApplicable(attachmentFileName, checkResult.Hash);
							if (importHistoryFactory != null)
							{
								additionalTransactionActions = new ITransactionParticipant[] { importHistoryFactory };
							}
							result = true;
						}
					}
				}
			}
			return result;
		}

		bool CanContinueWithAppLockIfApplicable(INotifications notifications, byte[] hash)
		{
			var result = true;

			if (hash != null)
			{
				var key = Environment.Env.CurrentCompany.Code
					+ ImportTypeForDuplicatesPrevention
					+ Convert.ToBase64String(hash);

				if (!((IDbConnected)FactoryProvider.Current).Connection.TryGetLock(key, out ImportLockWithHash))
				{
					notifications.AddError(Res.GetString("fcbbcc68-61a7-412f-acc3-4a0196131364", "An import of the same file is in progress."));
					ImportLockWithHash = null;
					result = false;
				}
			}

			return result;
		}

		protected override void OnImportDataEnd(NotificationBuffer buffer, bool sucessfullyImported)
		{
			if (ImportLockWithHash != null)
			{
				ImportLockWithHash.Dispose();
				ImportLockWithHash = null;
			}

			if (SupportsHistoryForDuplicatesPrevention
				&& DaysToKeepHistoryFor > 0)
			{
				using (var cmd = ((IDbConnected)FactoryProvider.Current).Connection.Command("DELETE dbo.StmDataImportHistory WHERE DIH_GC = @CompanyPK AND DIH_ImportType = @ImportType AND DIH_ImportDateTime < @CutOffDate"))
				{
					cmd.AddParameterBasedOnDbColumn("@CompanyPK", Environment.Env.CurrentCompanyPK, StmDataImportHistorySchema.DIH_GC);
					cmd.AddParameterBasedOnDbColumn("@ImportType", ImportTypeForDuplicatesPrevention, StmDataImportHistorySchema.DIH_ImportType);
					cmd.AddParameterBasedOnDbColumn("@CutOffDate", ZDateTimeOffset.Today.AddDays(-DaysToKeepHistoryFor).ToDateTimeOffset(), StmDataImportHistorySchema.DIH_ImportDateTime);

					cmd.ExecuteNonQuery();
				}
			}
		}

		#region Duplicates Prevention

		protected bool SupportsHistoryForDuplicatesPrevention => !string.IsNullOrEmpty(ImportTypeForDuplicatesPrevention);

		protected virtual string ImportTypeForDuplicatesPrevention => string.Empty;

		protected virtual int DaysToKeepHistoryFor => 0;

		#endregion

		protected virtual (bool Result, TextReader ReaderOut, byte[] Hash) IsValidDataBeforeImport(TextReader reader, INotifications notifications)
		{
			var result = true;
			var readerOut = reader;
			byte[] hash = null;

			if (SupportsHistoryForDuplicatesPrevention)
			{
				var streamReader = reader as StreamReader;
				var encoding = streamReader?.CurrentEncoding ?? Encoding.UTF8;
				var stream = streamReader?.BaseStream;
				string content;

				if (stream != null)
				{
					var readerForHash = new StreamReader(stream, encoding);
					content = readerForHash.ReadToEnd();
					stream.Position = 0;
				}
				else
				{
					content = reader.ReadToEnd();
					readerOut = new StringReader(content);
				}

				using var hashProvider = SHA256.Create();
				hash = hashProvider.ComputeHash(encoding.GetBytes(content));

				var importHistoryFactory = new BusinessObjectFactory();
				var query = new ZDBOnlyQuery(typeof(StmDataImportHistory));
				if (ShouldCheckImportHistoryInCurrentCompany)
				{
					query.AddToFilter(StmDataImportHistorySchema.DIH_GC, Environment.Env.CurrentCompanyPK);
				}
				query.AddToFilter(StmDataImportHistorySchema.DIH_ImportType, ImportTypeForDuplicatesPrevention);
				query.AddToFilter(StmDataImportHistorySchema.DIH_DataHash, new ZBlob(hash));
				query.OrderBy = StmDataImportHistorySchema.Constants.DIH_ImportDateTime + OrderByClause.Descending;
				var previousImport = importHistoryFactory.LoadTop1<StmDataImportHistory>(query);

				if (previousImport != null)
				{
					var companyInfo = ShouldCheckImportHistoryInCurrentCompany ? string.Empty : " " + Res.GetString("2B187A6C-2E73-4E0F-A1B5-6D0CAF02EE85", "in company {0}", previousImport.Company.GC_Code);

					var buffer = new NotificationBuffer(notifications);
					buffer.AddError(Res.GetString("60919985-8a3e-444f-bb90-c1dbca7af7d0", "A flat file with the same Hash was imported by {0}{1} at {2} with file name {3}.",
							previousImport.ImportStaff.GS_Code,
							companyInfo,
							previousImport.DIH_ImportDateTime,
							previousImport.DIH_SourceDescription));
					result = false;
				}
			}

			return (result, readerOut, hash);
		}

		protected virtual bool ShouldCheckImportHistoryInCurrentCompany => true;

		BusinessObjectFactory CreateImportHistoryRecordIfApplicable(string attachmentFileName, byte[] hash)
		{
			if (!SupportsHistoryForDuplicatesPrevention
				|| hash is null)
			{
				return null;
			}

			var importHistoryFactory = new BusinessObjectFactory();
			var historyEntry = importHistoryFactory.New<StmDataImportHistory>();
			historyEntry.DIH_GC = Environment.Env.CurrentCompanyPK;
			historyEntry.DIH_ImportType = ImportTypeForDuplicatesPrevention;
			historyEntry.DIH_ImportDateTime = ZDateTimeOffset.Now;
			historyEntry.DIH_GS_NKImportStaff = GlbStaff.CurrentUser.GS_Code;
			historyEntry.DIH_DataHash = new ZBlob(hash);
			historyEntry.DIH_SourceDescription = new ZString(attachmentFileName).Right(historyEntry.DIH_SourceDescriptionInfo.MaxLength);   // To preserve the file name in case we are getting a very long attachmentFileName
			return importHistoryFactory;
		}

		protected virtual void PostProcess()
		{
		}

		protected readonly BusinessObject BusinessEntity;

		#region Abstracts

		public FileExtensionType FileExtensionType
		{
			get { return FlatFileFormat.FileExtensionForImport; }
		}

		/// <summary>
		/// Return the FlatFileConverter 
		/// </summary>
		protected abstract IFlatFileConverter CreateConverter(INotifications notifications);

		/// <summary>
		/// The format of the flat file e.g. CSV, Tab delimited, etc.
		/// </summary>
		protected virtual IFlatFileFormat FlatFileFormat
		{
			get { return new AutoDetectFlatFileFormat(); }
		}

		/// <summary>
		/// Maps data in the XSD to appropriate data adapter(s) so it can be imported into Business Objects.
		/// </summary>
		protected abstract bool ExtractToDataAdapter(IValueObject xsd, INotifications notifications);

		/// <summary>
		/// Creates an instance of the XSD to import data into. Can be singular or collection.
		/// </summary>
		protected abstract IValueObject CreateXsd();

		#endregion

		#region Expose properties for base TestCase
#if DEBUG

		internal IFlatFileConverter CreateConverterForTest(INotifications notifications)
		{
			return CreateConverter(notifications);
		}

		internal IFlatFileFormat FlatFileFormatForTest
		{
			get { return FlatFileFormat; }
		}

		internal bool ExtractToDataAdapterForTest(IValueObject xsd, IValueObjectImportContext context)
		{
			return ExtractToDataAdapter(xsd, context);
		}

		internal IValueObject CreateXsdForTest()
		{
			return CreateXsd();
		}

#endif
		#endregion
	}
}
