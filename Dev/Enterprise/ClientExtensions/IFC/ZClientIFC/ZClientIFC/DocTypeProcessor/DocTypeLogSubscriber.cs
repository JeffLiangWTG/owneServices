using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.IFC
{
	[Serializable]
	class DocTypeLogSubscriber : LogSubscriber
	{
		#region Overrides

		public override string[] EventTypes
		{
			get { return new string[] { Events.AddedARecordToTheSystem.Code, Events.EditedARecord.Code }; }
		}

		public override string[] TableNames
		{
			get { return new[] { RefDocTypeSchema.Constants.TableName }; }
		}

		public override string Name
		{
			get { return "DocumentTypeUpdate"; }
		}

		public override string FriendlyName
		{
			get { return "Document Type Update"; }
		}

		public override bool IsClientSpecificSubscriber => true;

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			BusinessObjectFactory factory = queuedLogs[0].Factory;
			FlatFileDataRowCollection rows = new FlatFileDataRowCollection();
			ZQuery filter = new ZQuery();
			filter.OrderBy = RefDocTypeSchema.RT_DocType.Name;
			RefDocType[] refDocTypes = (RefDocType[])factory.Load(typeof(RefDocType), filter);

			rows.Add(new FlatFileDataRow(new string[] { "Reference Type","Document Type", "Description", "Active",
														  "System", "Published", "Publish Updatable", "Save Versions",
															"Log System Created Docs To EDocs" }));

			foreach (RefDocType refDocType in refDocTypes)
			{
				rows.Add(GetFlatFileDataRow(refDocType));
			}

			WriteToFile(rows);
		}

		#endregion

		#region Implementation

		void WriteToFile(FlatFileDataRowCollection rows)
		{
			string fullPath = Path.Combine(ExportDirectory, FileName);
			SetFileAttributes(fullPath);
			using (StreamWriter writer = new StreamWriter(fullPath, false))
			{
				CsvFlatFileFormat format = new CsvFlatFileFormat(false);
				foreach (FlatFileDataRow row in rows)
				{
					writer.WriteLine(format.ConvertToLine(row));
				}
				writer.Flush();
			}
		}

		void SetFileAttributes(string fullPath)
		{
			if (File.Exists(fullPath))
			{
				File.SetAttributes(fullPath, FileAttributes.Normal);
			}
		}

		FlatFileDataRow GetFlatFileDataRow(RefDocType refDocType)
		{
			FlatFileDataRow row = new FlatFileDataRow(9);

			row[0] = refDocType.RT_ReferenceType;
			row[1] = refDocType.RT_DocType;
			row[2] = refDocType.RT_DescMultilingual.GetUnresolvedString();
			row[3] = refDocType.RT_IsActive.ToString();
			row[4] = refDocType.RT_IsSystem.ToString();
			row[5] = refDocType.RT_IsPublished.ToString();
			row[6] = refDocType.RT_IsPublishUpdatable.ToString();
			row[7] = refDocType.RT_SaveVersions.ToString();
			row[8] = refDocType.RT_LogSystemCreatedDocsToEDocs.ToString();

			return row;
		}

		ZString ExportDirectory
		{
			get { return IFCDataRegistry.Instance.FSCExportDirectory; }
		}

		ZString FileName
		{
			get { return "DocumentTypeExport." + nameof(FileExtensionType.Csv).ToLower(); }
		}

		#endregion
	}
}
