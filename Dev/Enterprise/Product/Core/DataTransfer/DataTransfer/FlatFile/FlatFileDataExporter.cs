using System;
using System.IO;
using System.Runtime.InteropServices;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DataTransfer.Business
{
	/// <summary>
	/// Converts business object collection to IValueObjects to export to a file.
	/// </summary>
	public abstract class FlatFileDataExporter : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected FlatFileDataExporter(BusinessObjectFactory factory)
		{
			fFactory = factory;
			fIsExportOK = true;
		}

		protected FlatFileDataExporter(BusinessObjectFactory factory, ExportInstructions instructions, ZString exportedFileToAppendTo)
		{
			this.Instructions = instructions;
			this.ExportedFile = exportedFileToAppendTo;
			fFactory = factory;
		}

		public abstract ZString EnglishDescription { get; }

		public virtual ZString LocalizedDescription
		{
			get { return EnglishDescription; }
		}

		#region Event - AddProgress

		public event EventHandler<ProgressEventArgs> ProgressChanged;

		protected void OnProgressChanged(int currentCount, int totalCount)
		{
			if (ProgressChanged != null)
			{
				ProgressChanged(this, new ProgressEventArgs(currentCount, totalCount));
			}
		}

		#endregion

		#region Event - PromptForFilename

		public event FilenameEventHandler PromptForFilename;

		void OnPromptForFilename(FilenameEventArgs ea)
		{
			if (PromptForFilename != null)
			{
				PromptForFilename(this, ea);
			}
		}

		#endregion

		#region Event - PromptForEmailDetails

		public event EmailExportInstructionsEventHandler PromptForEmailDetails;

		void OnPromptForEmailDetails(EmailExportInstructionsEventArgs ea)
		{
			if (PromptForEmailDetails != null)
			{
				PromptForEmailDetails(this, ea);
			}
		}

		#endregion

		#region Convert to IValueObjects

		/// <summary>
		/// Convert business object to an IValue object. Does this for a whole collection and returns an array of IValueObjects
		/// </summary>
		protected virtual IValueObject ConvertToIValueObject(BusinessObject bizObj, IValueObjectDataAdapter adapter, int iteration, int count, INotifications notifications)
		{
			BusinessObject loadedBizO = LoadBusinessObjectForConversion(DataAdapter.BusinessObjectType, bizObj);
			IValueObject result = adapter.ExportToValueObject(loadedBizO, new ValueObjectExportContext(notifications));
			OnProgressChanged(iteration, count);
			return result;
		}

		protected virtual BusinessObject LoadBusinessObjectForConversion(Type businessObjectType, BusinessObject bizObj)
		{
			return Factory.Load(businessObjectType, bizObj.PK);
		}

		/// <summary>
		/// Instance of the data adapter that represents an individual top-level business object row from your export. 
		/// </summary>
		protected abstract IValueObjectDataAdapter DataAdapter { get; }

		/// <summary>
		/// Return the FlatFileConverter 
		/// </summary>
		protected abstract IFlatFileConverter CreateConverter(INotifications notifications);

		protected IFlatFileConverter Converter;

		#endregion

		#region File Extension

		/// <summary>
		/// The format of the flat file e.g. CSV, Tab delimited, etc.
		/// </summary>
		protected abstract IFlatFileFormat FlatFileFormat { get; }

		public FileExtensionType FileExtensionType
		{
			get { return FlatFileFormat.FileExtensionForExport; }
		}

		#endregion

		#region Export

		public bool IsExportOK
		{
			get { return fIsExportOK; }
		}

		protected void SetIsExportOK(bool value)
		{
			fIsExportOK = value;
		}

		bool fIsExportOK;

		public void Export(BusinessObjectReader objectReader, INotifications notifications)
		{
			ExportData(objectReader, notifications);
			BeforeFactorySave(objectReader);
			SaveFactory();
		}

		protected virtual void BeforeFactorySave(BusinessObjectReader objectReader)
		{
		}

		protected virtual void ExportData(BusinessObjectReader objectReader, INotifications notifications)
		{
			TemporaryFile = new FileInfo(ExportToFile(objectReader, notifications));
			if (!File.Exists(ExportedFile))
			{
				if (IsValidToDeliver(notifications, Converter))
				{
					ExportedFile = DeliverFile(TemporaryFile.FullName, notifications);
				}
				else if (TemporaryFile.Exists)
				{
					TemporaryFile.Delete();
				}
			}
		}

		protected virtual bool IsValidToDeliver(INotifications notifications, IFlatFileConverter converter)
		{
			return true;
		}

		public ZString ExportToFile(BusinessObjectReader objectReader, INotifications notifications)
		{
			string tempFilename = (File.Exists(ExportedFile)) ? (string)ExportedFile : Temp.GetTempFileName();
			return ExportToFile(objectReader, notifications, tempFilename);
		}

		public ZString ExportToFile(BusinessObjectReader objectReader, INotifications notifications, string fileName)
		{
			var success = false;
			try
			{
				using (TextWriter writer = new StreamWriter(fileName, AppendToFile))
				{
					int i = 0;
					Converter = CreateConverter(notifications);
					foreach (BusinessObject bizObj in objectReader)
					{
						ExportToFileCore(bizObj, i, objectReader, writer, notifications);
						i++;
					}
					writer.Flush();
					writer.Close();
					success = true;
				}
			}
			finally
			{
				if (!success)
				{
					TryDeleteFile(fileName, notifications);
				}
			}

			return fileName;
		}

		protected virtual void ExportToFileCore(BusinessObject bizObj, int i, BusinessObjectReader objectReader, TextWriter writer, INotifications notifications)
		{
			IValueObject valueObjectToExport = ConvertToIValueObject(bizObj, DataAdapter, i, objectReader.ApproximateCount, notifications);
			Converter.ExportFlatFile(valueObjectToExport, FlatFileFormat, writer);
		}

		protected void TryDeleteFile(string fileName, INotifications notify)
		{
			try
			{
				File.Delete(fileName);
			}
			catch (IOException ex)
			{
				if (Marshal.GetHRForException(ex) == TempFile.FileIsInUseByAnotherProcess)
				{
					notify.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("fc305d82-5309-4ea1-bf80-2f133cddf4b1", "The file {0} is locked by another process.", fileName)));
				}
			}
		}

		protected virtual bool AppendToFile
		{
			get { return false; }
		}

		protected virtual ZString DeliverFile(ZString tempFile, INotifications notifications)
		{
			ExportInstructions instructions = GetExportInstructions();
			ExportMethod method = GetExportMethod(instructions, notifications);

			if (method != null)
			{
				if (!method.CanDeliver)
				{
					PromptForUserInput(method, instructions);
				}
				method.Deliver(tempFile);
			}

			return instructions.OutputFile;
		}

		protected void SaveFactory()
		{
			var success = false;
			try
			{
				Factory.Save();
				success = true;
			}
			finally
			{
				if (!success && File.Exists(ExportedFile))
				{
					File.Delete(ExportedFile);
				}
			}
		}

		protected ExportMethod GetExportMethod(ExportInstructions instructions, INotifications notifications)
		{
			ExportMethod result = null;

			switch (instructions.MethodOfExport)
			{
				case ExportType.Email:
					result = new EmailExport(instructions, notifications);
					break;

				case ExportType.File:
					result = new FileExport(instructions, notifications);
					break;

				case ExportType.Ftp:
					result = new FtpExport(instructions, notifications);
					break;
			}
			return result;
		}

		void PromptForUserInput(ExportMethod method, ExportInstructions instructions)
		{
			switch (method.ExportType)
			{
				case ExportType.File:
					FilenameEventArgs fileEventArgs = new FilenameEventArgs();
					OnPromptForFilename(fileEventArgs);
					ZString unmappedFilename = fileEventArgs.UnmappedFilename;

					if (!unmappedFilename.IsEmpty)
					{
						instructions.BasePath = Path.GetDirectoryName(unmappedFilename) + Path.DirectorySeparatorChar;
						instructions.SpecifiedFilename = Path.GetFileNameWithoutExtension(unmappedFilename);
					}
					break;

				case ExportType.Email:
					EmailExportInstructionsEventArgs emailEventArgs = new EmailExportInstructionsEventArgs(instructions.EmailProperties);
					OnPromptForEmailDetails(emailEventArgs);
					break;
			}
		}

		#region Export Instructions

		protected virtual void PopulateExportInstructions(ExportInstructions instructions, IFlatFileConverter converter)
		{
		}

		protected virtual ExportInstructions GetExportInstructions()
		{
			if (Instructions == null || RenewInstructions)
			{
				Instructions = new ExportInstructions();
				Instructions.FileExtension = FileExtensionType;
				PopulateExportInstructions(Instructions, Converter);
			}

			return Instructions;
		}
		protected ExportInstructions Instructions;
		protected bool RenewInstructions;

		#endregion

		#endregion

		#region Implementation

		#region Factory

		BusinessObjectFactory fFactory;
		protected new BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = base.Factory ?? new BusinessObjectFactory();
				}
				return fFactory;
			}
		}

		#endregion

		protected ZString ExportedFile;

		#endregion

		#region Expose properties for base TestCase
#if DEBUG

		public ZString ExportedFileForTesting
		{
			get { return ExportedFile; }
		}

		internal IValueObjectDataAdapter DataAdapterForTest
		{
			get { return DataAdapter; }
		}

		internal IFlatFileConverter CreateConverterForTest(INotifications notifications)
		{
			return CreateConverter(notifications);
		}

		internal IFlatFileFormat FlatFileFormatForTest
		{
			get { return FlatFileFormat; }
		}

		internal ExportInstructions GetExportInstructionsTest
		{
			get { return GetExportInstructions(); }
		}
#endif

		protected FileInfo TemporaryFile;

		#endregion
	}
}
