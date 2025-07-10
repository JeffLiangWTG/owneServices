using System.IO;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.FlexCelIntegration;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentVisualizer.Business
{
	[CodeAlive("work in progress")]
	public sealed class DocumentPDFWriter : IDocumentPDFWriter
	{
		public bool WriteToStream(BusinessObject bizObj, IStmMenuItem menuItem, Stream outputStream)
		{
			if (bizObj == null
				|| menuItem == null
				|| outputStream == null)
			{
				return false;
			}

			var builder = new DocumentInfoBuilder(bizObj, menuItem);
			var infos = builder.CreateDocumentInfos();

			if (infos == null
				|| infos.Count == 0)
			{
				return false;
			}

			foreach (var info in infos)
			{
				if (info?.Document is IDocument document)
				{
					WriteToStream(document, outputStream);
				}
			}

			return true;
		}

		void WriteToStream(IDocument document, Stream outputStream)
		{
			using (var xlsStream = new MemoryStream())
			{
				var xls = document.ToXlsFile();
				xls.Save(xlsStream);

				DocumentConverter.WriteOutputFromExcel(
					xlsStream,
					outputStream,
					OutputFormatType.PDFAcrobat5);
			}
		}
	}
}
