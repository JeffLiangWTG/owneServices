using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.Business
{
	/// <summary>
	/// Writes an Excel index file 
	/// </summary>
	public class ExcelIndexFile : IndexFile
	{
		public ExcelIndexFile(string filenameToWrite)
			: base(filenameToWrite)
		{
		}

		public override void AddDirectory(string directoryName)
		{
		}

		public override void AddFile(StorageDocsBase document, string basePath, string relativePath)
		{
			FileCount++;
			SetCellAsHyperlink(FileCount, 0, basePath + relativePath, document.SC_DescMultilingual);
			SetCell(FileCount, 1, document.SC_Date.ToShortDateString());
			SetCell(FileCount, 2, document.SC_DocType);
			SetCell(FileCount, 3, document.SC_DescMultilingual);
			SetCell(FileCount, 4, document.SC_IsSystemGenerated.ToString());

			ICDArchive owner = GetOwner(document) as ICDArchive;
			if (owner != null)
			{
				int columnCount = DocumentHeaderColumnsCount;
				foreach (CodeDescriptionPair pair in owner.CDArchiveInfo.Properties)
				{
					SetCell(FileCount, columnCount, pair.Description);
					columnCount++;
				}
			}
		}

#if DEBUG
		public
#else
		internal
#endif
		BusinessObject GetOwner(StorageDocsBase document)
		{
			StorageMain parentMain = document.ParentMain;
			switch (parentMain.OwnerAssemblyData.DocManagerCode)
			{
				case Core.Constants.DocManagerCodes.Shipment:
					return (BusinessObject)parentMain.Factory.Load<Enterprise.Integration.Forwarding.IForwardingShipment>(parentMain.SM_ParentFK);
				default:
					return parentMain.DocumentOwner;
			}
		}

		void SetCell(int row, int column, string value)
		{
			Interface.WorkSheets[0][row, column] = value;
		}

		void SetCellAsHyperlink(int row, int column, string hyperlink, string description)
		{
			Interface.WorkSheets[0].SetCellFormula(row, column, @"=HYPERLINK(""" + hyperlink + @""",""" + hyperlink + @""")", 0);
		}

		public override void CloseDirectory()
		{
		}

		public override void Start()
		{
			ExcelTemplate template = new ExcelTemplateReadFromExcelTemplatesSolution(ExcelTemplateReadFromExcelTemplatesSolution.TemplateNames.DocManagerCopyDocs);
			using (Stream templateStream = template.GetAsTemplateStream())
			{
				Interface.LoadExcelFile(templateStream);
			}
			AddHeaders();
		}

		const int DocumentHeaderColumnsCount = 5;
		protected void AddHeaders()
		{
			CDArchiveInfoHeader headerInformation = new CDArchiveInfoHeader();

			ZString[] headers = new ZString[DocumentHeaderColumnsCount];
			headers[0] = (NoResString)"Link";
			headers[1] = (NoResString)"Last Edited";
			headers[2] = (NoResString)"Doc Type";
			headers[3] = (NoResString)"Description";
			headers[4] = (NoResString)"System Generated?";

			int i = 0;
			foreach (ZString header in headers)
			{
				SetCell(0, i, header);
				i++;
			}

			foreach (CodeDescriptionPair pair in headerInformation.Properties)
			{
				SetCell(0, i, pair.Code);
				i++;
			}
		}
		public override void Finish()
		{
			Interface.SaveToFile(OutputFile);
		}

		#region Excel Interface

		protected ExcelInterface Interface
		{
			get
			{
				if (fInterface == null)
				{
					fInterface = new ExcelInterface();
				}
				return fInterface;
			}
		}

		ExcelInterface fInterface;

		#endregion

		#region IDisposable Members

		public override void Dispose()
		{
			if (!Disposed)
			{
				Interface.Dispose();
				Disposed = true;
			}
		}

		bool Disposed;

		#endregion

		#region CDArchiveInfoHeader

		protected class CDArchiveInfoHeader : CDArchiveInfo
		{
			public CDArchiveInfoHeader()
				: base(null)
			{
			}

			public override CargoWise.Types.ZString ConsigneeCode
			{
				get { return ZString.Empty; }
			}

			public override ZString ConsignorCode
			{
				get { return ZString.Empty; }
			}

			public override ZString[] ContainerNumbersList
			{
				get { return System.Array.Empty<ZString>(); }
			}

			public override ZString Destination
			{
				get { return ZString.Empty; }
			}

			public override ZString[] EntryNumbersList
			{
				get { return System.Array.Empty<ZString>(); }
			}

			public override ZDateTime ETA
			{
				get { return ZDateTime.Now; }
			}

			public override ZDateTime ETD
			{
				get { return ZDateTime.Now; }
			}

			public override ZString[] InvoiceNumbersList
			{
				get { return System.Array.Empty<ZString>(); }
			}

			public override ZString HouseBill
			{
				get { return ZString.Empty; }
			}

			public override ZString JobNumber
			{
				get { return ZString.Empty; }
			}

			public override ZString MasterBill
			{
				get { return ZString.Empty; }
			}

			public override ZString[] OrderNumbersList
			{
				get { return System.Array.Empty<ZString>(); }
			}

			public override ZString Origin
			{
				get { return ZString.Empty; }
			}

			public override ZString Vessel
			{
				get { return ZString.Empty; }
			}

			public override ZString VoyageFlight
			{
				get { return ZString.Empty; }
			}
		}

		#endregion

		int FileCount;
	}
}
