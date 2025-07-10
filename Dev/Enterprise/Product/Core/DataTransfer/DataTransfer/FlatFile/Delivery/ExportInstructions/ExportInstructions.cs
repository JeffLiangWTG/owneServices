using System.IO;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Business
{
	public class ExportInstructions
	{
		#region OutputFile

		/// <summary>
		/// Actual file with export contents saved to disk; ready to attach to email export if required
		/// </summary>
		public ZString OutputFile
		{
			get { return fOutputFile; }
		}

		ZString fOutputFile;

		protected internal void SetOutputFile(ZString filename)
		{
			fOutputFile = filename;
		}

		#endregion

		#region MethodOfExport

		public ExportType MethodOfExport
		{
			get { return fMethodOfExport; }
			set { fMethodOfExport = value; }
		}

		ExportType fMethodOfExport = ExportType.File;

		#endregion

		#region Filename methods

		#region BasePath

		/// <summary>
		/// For a File export to filesystem, you can specify the fully qualified base path where the file should be saved.
		/// </summary>
		public ZString BasePath
		{
			get { return fBasePath; }
			set { fBasePath = value; }
		}

		ZString fBasePath = Temp.TempPath;

		#endregion

		#region Specified Filename

		/// <summary>
		/// Name only of the file to export to (without extension). Required for file exports
		/// </summary>
		public ZString SpecifiedFilename
		{
			get { return fSpecifiedFilename; }
			set { fSpecifiedFilename = value; }
		}

		ZString fSpecifiedFilename;

		#endregion

		#region File Extension 

		public FileExtensionType FileExtension
		{
			get { return fFileExtension; }
			set { fFileExtension = value; }
		}

		FileExtensionType fFileExtension = FileExtensionType.Txt;

		#endregion

		/// <summary>
		/// Fully qualified path, name, and extension that the file should be saved to
		/// </summary>
		/// 

		public ZString SpecifiedFilePathWithExtension
		{
			get
			{
				ZString returnValue = ZString.Empty;

				if (!SpecifiedFilename.IsEmpty && !BasePath.IsEmpty)
				{
					returnValue = ConstructFilePathWithExtension();
				}

				return returnValue;
			}
		}

		protected virtual ZString ConstructFilePathWithExtension()
		{
			ZString baseFilename = Path.Combine(BasePath, Path.GetFileNameWithoutExtension(SpecifiedFilename));
			FileExtensionFilterBuilder fileExtensionFilterBuilder = new FileExtensionFilterBuilder(UseUpperCaseFileExtension);
			return baseFilename + '.' + fileExtensionFilterBuilder.GetFileExtension(FileExtension);
		}

		#endregion

		#region EmailInstructions

		public EmailExportInstructions EmailProperties
		{
			get
			{
				if (fEmailProperties == null)
				{
					fEmailProperties = new EmailExportInstructions();
				}
				return fEmailProperties;
			}
			set { fEmailProperties = value; }
		}

		EmailExportInstructions fEmailProperties;

		#endregion

		#region FtpInstructions

		public FtpExportInstructions FtpProperties
		{
			get
			{
				if (fFtpProperties == null)
				{
					fFtpProperties = new FtpExportInstructions();
				}
				return fFtpProperties;
			}
			set
			{
				fFtpProperties = value;
			}
		}

		FtpExportInstructions fFtpProperties;

		#endregion

		#region UseUpperCaseFileExtension

		public bool UseUpperCaseFileExtension
		{
			get { return fUseUpperCaseFileExtension; }
			set { fUseUpperCaseFileExtension = value; }
		}

		bool fUseUpperCaseFileExtension;

		#endregion
	}
}
