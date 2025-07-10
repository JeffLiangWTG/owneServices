using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentScanning;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.Records
{
	public class ArchiveStorageMain : StorageMain
	{
		public ArchiveStorageMain(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public void ArchiveTo(ArchiveVolume volume)
		{
			var setting = new XmlWriterSettings();
			setting.Encoding = Encoding.Unicode;
			setting.Indent = true;

			using var metaFileStream = new MemoryStream((eDocs.Count * 164) + 90);
			using var writer = XmlWriter.Create(metaFileStream, setting);
			writer.WriteStartDocument(true);
			writer.WriteStartElement("StorageDocs");

			foreach (var doc in eDocs.Cast<StorageDocsBase>())
			{
				volume.Add(DocFileName(doc), doc.SaveToStream, doc);

				writer.WriteStartElement("StorageDoc");
				writer.WriteAttributeString(AutoStorageDocs.Schema.PK, doc.PK.ToString());
				writer.WriteAttributeString(AutoStorageDocs.Schema.SC_DocType, doc.SC_DocType);
				writer.WriteAttributeString(AutoStorageDocs.Schema.SC_FileName, doc.SC_FileName);
				writer.WriteAttributeString(AutoStorageDocs.Schema.SC_DataType, doc.SC_DataType);
				writer.WriteAttributeString(AutoStorageDocs.Schema.SC_Date, doc.SC_Date.ToString(DateFormatString));
				writer.WriteAttributeString(AutoStorageDocs.Schema.SC_Desc, doc.SC_DescMultilingual.GetUnresolvedString());
				writer.WriteEndElement();
			}

			writer.WriteEndElement();
			writer.WriteEndDocument();
			writer.Flush();

			volume.Add(MetaFileName, metaFileStream);

			writer.Close();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "dateformat constant")]
		const string DateFormatString = "yyyy-MM-dd HH:mm:ss";

		public void RestoreFrom(ArchiveVolume volume)
		{
			if (!File.Exists(volume.VolumeZipPath))
			{
				throw new ArgumentException("Volume: '" + volume.VolumePath + "' must already exist.");
			}

			using var tempDir = new TempDirectory();
			volume.Extract(MetaFileName, tempDir.DirectoryName);

			using (Documents.SuspendListChanged())
			using (Files.SuspendListChanged())
			using (var reader = new XmlTextReader(Path.Combine(tempDir.DirectoryName, MetaFileName)))
			{
				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element && reader.Name == "StorageDoc")
					{
						var dataType = reader.GetAttribute(StorageDocsBase.Schema.SC_DataType).Length == 0 ? "TIF" : reader.GetAttribute(StorageDocsBase.Schema.SC_DataType);
						var docFileName = DocFileName(reader.GetAttribute(StorageDocsBase.Schema.SC_FileName), dataType);
						if (volume.HasEntry(docFileName))
						{
							volume.Extract(docFileName, tempDir.DirectoryName);
						}
						else
						{
							docFileName = DocBlobFileName(reader.GetAttribute(StorageDocsBase.Schema.PK));
							volume.Extract(docFileName, tempDir.DirectoryName);
						}

						var image = DocumentUtilities.GetFileAsBytes(Path.Combine(tempDir.DirectoryName, docFileName));

						var filenameValue = reader.GetAttribute(StorageDocsBase.Schema.SC_FileName);
						var dataTypeValue = reader.GetAttribute(StorageDocsBase.Schema.SC_DataType);
						var dateValue = reader.GetAttribute(StorageDocsBase.Schema.SC_Date);
						var descValue = reader.GetAttribute(StorageDocsBase.Schema.SC_Desc);
						var docTypeValue = reader.GetAttribute(StorageDocsBase.Schema.SC_DocType);

						var newDoc = SerializableEDocsTools.IsImage(dataTypeValue) ? (StorageDocsBase)Documents.AddNew() : Files.AddNew();
						newDoc.SC_FileName = filenameValue;
						newDoc.SC_DataType = dataTypeValue;
						newDoc.SC_Desc = descValue;
						newDoc.SC_DocType = docTypeValue;
						newDoc.SC_ImageData = image;
						newDoc.SC_Date = DateTime.ParseExact(dateValue, DateFormatString, null);
					}
				}
			}
		}

		public string MetaFileName
			=> PK.ToString() + ".meta";

		public string DocBlobFileName(StorageDocsBase doc)
			=> DocBlobFileName(doc.PK.ToString());

		public string DocBlobFileName(string docPKString)
			=> docPKString + ".blb";

		string DocFileName(StorageDocsBase doc)
		{
			var dataType = doc.SC_DataType.IsEmpty ? "TIF" : doc.SC_DataType.ToString();
			return DocFileName(doc.SC_FileName.ToString(), dataType);
		}

		string DocFileName(string docPKString, string extension)
			=> docPKString + "." + extension.Trim().ToLower();

		#region MainReference

		public ZString MainReference
		{
			get
			{
				if (mainReference == null)
				{
					LoadReferences();
				}

				return mainReference;
			}
		}

		string mainReference;

		#endregion

		#region AdditionalReferences

		public ZString AdditionalReferences
		{
			get
			{
				if (additionalReferences == null)
				{
					LoadReferences();
				}

				return additionalReferences;
			}
		}

		string additionalReferences;

		#endregion

		void LoadReferences()
		{
			var references = new ActiveBusinessObjectCollection<StorageReference>(this);
			references.ApplySort(StorageReference.Schema.SR_Sequence, ListSortDirection.Ascending);

			var additionalReferenceBuilder = new StringBuilder(references.Count * 40);
			foreach (var reference in references)
			{
				var combinedRef = reference.SR_TYPE + ": " + reference.SR_Reference;
				if (reference.SR_Sequence == 0)
				{
					mainReference = combinedRef;
				}
				else
				{
					if (additionalReferenceBuilder.Length > 0)
					{
						_ = additionalReferenceBuilder.Append(" ");
					}

					_ = additionalReferenceBuilder.Append(combinedRef);
				}
			}

			additionalReferences = additionalReferenceBuilder.ToString();
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
			=> new AdditionalReferencesFetch(this);

		class AdditionalReferencesFetch : EnterpriseBusinessObjectFetchStrategy
		{
			public AdditionalReferencesFetch(EnterpriseBusinessObject bizO)
				: base(bizO)
			{ }

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				foreach (var column in columns)
				{
					if (column.ColumnName == "AdditionalReferences")
					{
						Factory.AddFetchHint(StorageReferenceSchema.SR_SM, BusinessObject.PK);
					}
				}

				base.FetchForViewCore(columns);
			}
		}

		protected override ZString HumanReadableNameCore => MainReference;
	}
}
