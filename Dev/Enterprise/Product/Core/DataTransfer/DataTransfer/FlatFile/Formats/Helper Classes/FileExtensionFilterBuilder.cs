using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Business
{
	public enum FileExtensionType
	{
		Csv = 8,
		Txt = 4,
		All = 2,
		ClientSpecific = 1,
		None = 0
	}

	public class FileExtensionFilterBuilder
	{
		public FileExtensionFilterBuilder()
		{
		}

		internal FileExtensionFilterBuilder(bool useUpperCaseFileExtension)
		{
			this.UseUpperCaseFileExtension = useUpperCaseFileExtension;
		}

		public static readonly Overridable<ZString> ClientSpecificFileExtension = new Overridable<ZString>(ZString.Empty);

		public static readonly Overridable<ZString> ClientSpecificFileExtensionDescription = new Overridable<ZString>(ZString.Empty);

		public readonly bool UseUpperCaseFileExtension;

		public ZString GetFileExtension(FileExtensionType fileExtension)
		{
			ZString result = "";

			if (fileExtension != FileExtensionType.None)
			{
				if (fileExtension == FileExtensionType.ClientSpecific && ClientSpecificFileExtension.Value.Trim() == ZString.Empty)
				{
					ErrorReporter.ReportOnce("FileExtensionFilterBuilder.GetFileExtension", "The client specific file extension has not been set. Override GetClientSpecificFileExtension() in your FlatFileFormat class.");
				}

				result = (fileExtension == FileExtensionType.ClientSpecific) ? ClientSpecificFileExtension.Value.ToString() : fileExtension.ToString();
				result = (UseUpperCaseFileExtension) ? result.ToUpper() : result.ToLower();
			}

			return result;
		}

		/// <summary>
		/// It does not add 'None' to the list, anybody bothered?
		/// </summary>
		public void Add(FileExtensionType extensionType)
		{
			if (!FileExtensions.Contains(extensionType))
			{
				foreach (FileExtensionType extension in Enum.GetValues(typeof(FileExtensionType)))
				{
					if ((extensionType & extension) == extension && extension != FileExtensionType.None)
					{
						FileExtensions.Add(extension);
					}
				}
			}
		}

		public void Remove(FileExtensionType extensionType)
		{
			FileExtensions.Remove(extensionType);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded file filter")]
		public string FilterClause
		{
			get
			{
				string clause = "";

				foreach (FileExtensionType extension in FileExtensions)
				{
					switch (extension)
					{
						case FileExtensionType.ClientSpecific:
							if (ClientSpecificFileExtensionDescription.Value.IsEmpty)
							{
								clause += ClientSpecificFileExtension.Value + " Files (*." + ClientSpecificFileExtension.Value + ")|*." + ClientSpecificFileExtension.Value + "|";
							}
							else
							{
								clause += ClientSpecificFileExtensionDescription.Value + "|*." + ClientSpecificFileExtension.Value;
							}
							break;

						case FileExtensionType.Csv:
							clause += "CSV Files (*.csv)|*.csv|";
							break;

						case FileExtensionType.All:
							clause += "All Files (*.*)|*.*|";
							break;

						case FileExtensionType.Txt:
							clause += "Text Files (*.txt)|*.txt|";
							break;
					}
				}

				return clause.TrimEnd('|');
			}
		}

		protected readonly ArrayList FileExtensions = new ArrayList();
	}
}
