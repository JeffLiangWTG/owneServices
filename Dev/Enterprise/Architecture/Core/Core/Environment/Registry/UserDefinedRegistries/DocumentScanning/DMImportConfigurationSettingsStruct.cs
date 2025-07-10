using System;
using System.Data;
using System.IO;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public struct DMImportConfigurationSettingsStruct
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "This is specifying a valid default directory for doc manager import folder")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant, This is specifying a valid default directory for doc manager import folder")]
		public DMImportConfigurationSettingsStruct(byte[] registryValue)
		{
			if (registryValue != null)
			{
				try
				{
					MemoryStream xmlStream = new MemoryStream(registryValue);
					DataSet data = new DataSet();
					data.ReadXml(xmlStream, XmlReadMode.Auto);
					DataTable table = data.Tables[0];

					DefaultDirectory = (string)table.Rows[0]["DefaultDirectory"];
					PDFColourOption = (string)table.Rows[0]["PDFColourOption"];
					AutoAllocate = bool.Parse((string)table.Rows[0]["AutoAllocate"]);
					DeleteSourceFilesAfterImport = bool.Parse((string)table.Rows[0]["DeleteSourceFilesAfterImport"]);
					IncludeSubdirectories = bool.Parse((string)table.Rows[0]["IncludeSubdirectories"]);
					OutputOption = (table.Columns.Contains("OutputOption")) ? (string)table.Rows[0]["OutputOption"] : "Automatic";
					UseCoverSheet = bool.Parse((string)table.Rows[0]["UseCoverSheet"]);

					return;
				}
				catch (ArgumentException)
				{
					Globals.Message.Show(Res.GetString("f041b82a-4061-4806-9612-c578bf2004ca", "Import Configuration data is corrupted. Settings will be reset to default value."));
				}
			}

			DefaultDirectory = @"C:\";
			PDFColourOption = "Black & White";
			OutputOption = "Automatic";
			AutoAllocate = true;
			UseCoverSheet = true;
			DeleteSourceFilesAfterImport = false;
			IncludeSubdirectories = true;
		}

		public string DefaultDirectory;
		public string PDFColourOption;
		public string OutputOption;
		public bool AutoAllocate;
		public bool UseCoverSheet;
		public bool DeleteSourceFilesAfterImport;
		public bool IncludeSubdirectories;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public byte[] GetValue()
		{
			DataTable table = new DataTable();

			DataColumn defaultDirectoryColumn = new DataColumn("DefaultDirectory", typeof(string));
			DataColumn pDFColourOptionColumn = new DataColumn("PDFColourOption", typeof(string));
			DataColumn autoAllocateColumn = new DataColumn("AutoAllocate", typeof(bool));
			DataColumn deleteSourceFilesAfterImportColumn = new DataColumn("DeleteSourceFilesAfterImport", typeof(bool));
			DataColumn includeSubdirectoriesColumn = new DataColumn("IncludeSubdirectories", typeof(bool));
			DataColumn outputOptionColumn = new DataColumn("OutputOption", typeof(string));
			DataColumn useCoverSheetColumn = new DataColumn("UseCoverSheet", typeof(bool));

			table.Columns.Add(defaultDirectoryColumn);
			table.Columns.Add(pDFColourOptionColumn);
			table.Columns.Add(autoAllocateColumn);
			table.Columns.Add(deleteSourceFilesAfterImportColumn);
			table.Columns.Add(includeSubdirectoriesColumn);
			table.Columns.Add(outputOptionColumn);
			table.Columns.Add(useCoverSheetColumn);

			DataRow row = table.NewRow();
			row[defaultDirectoryColumn] = DefaultDirectory;
			row[pDFColourOptionColumn] = PDFColourOption;
			row[autoAllocateColumn] = AutoAllocate;
			row[deleteSourceFilesAfterImportColumn] = DeleteSourceFilesAfterImport;
			row[includeSubdirectoriesColumn] = IncludeSubdirectories;
			row[outputOptionColumn] = OutputOption;
			row[useCoverSheetColumn] = UseCoverSheet;

			table.Rows.Add(row);
			DataSet data = new DataSet();
			data.Tables.Add(table);

			MemoryStream xmlStream = new MemoryStream();
			data.WriteXml(xmlStream, XmlWriteMode.IgnoreSchema);

			return xmlStream.ToArray();
		}
	}
}
