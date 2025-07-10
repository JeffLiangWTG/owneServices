using System;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Environment;
using FlexCel.XlsAdapter;

namespace Enterprise.DocumentEngine.Service
{
	public class DocumentPreviewService : IDocumentPreviewService
	{
		public DocumentCommand GetDocumentCommand(Guid documentCommandPk, string tablePrefix, Guid businessObjectPk)
		{
			var factory = new BusinessObjectFactory() { NameForDebugging = "DocumentPreview WebService" };
			var documentCommand = factory.Load<DocumentCommand>(documentCommandPk);
			if (documentCommand == null)
			{
				return null;
			}

			documentCommand.Parent = factory.Load(tablePrefix, businessObjectPk) as IDocumentSupportable
				?? throw new ArgumentException("Invalid document command parent.", nameof(documentCommand.Parent));

			return documentCommand;
		}

		public void WriteDocumentPreview(DocumentCommand documentCommand, Stream outputStream)
		{
			if (documentCommand == null)
			{
				throw new ArgumentNullException(nameof(documentCommand));
			}

			if (documentCommand.Parent == null)
			{
				throw new ArgumentNullException(nameof(documentCommand.Parent));
			}

			if (outputStream == null)
			{
				throw new ArgumentNullException(nameof(outputStream));
			}

			try
			{
				WriteDocumentPreviewCore(documentCommand, outputStream);
			}
			catch (Exception ex) when (ex.Find<FlexCelXlsAdapterException>() != null)
			{
				throw new DocumentPreviewException(ex.Message, ex);
			}
		}

		void WriteDocumentPreviewCore(DocumentCommand documentCommand, Stream outputStream)
		{
			switch (documentCommand.SU_MenuType)
			{
				case Core.Constants.StmMenuItemTypes.Forms:
					WriteFormBuilderDocumentPreview(documentCommand, outputStream);
					break;

				default:
					WriteDocEngineDocumentPreview(documentCommand, outputStream);
					break;
			}
		}

		void WriteDocEngineDocumentPreview(DocumentCommand documentCommand, Stream outputStream)
		{
			using (var set = GetDocumentPrintSet(documentCommand))
			using (var mergedExcel = MergeDocumentPacks(set))
			{
				if (mergedExcel.Length > 0)
				{
					var deliveryInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
					DocumentConverter.WriteOutputFromExcel(
						mergedExcel,
						outputStream,
						OutputFormatType.PDFAcrobat5,
						deliveryInfo.Watermark,
						ColourDepth.BlackAndWhite,
						documentCommand.IsLocalDocument,
						documentCommand.SU_FlexCelLineSpacing);
				}
			}
		}

		void WriteFormBuilderDocumentPreview(DocumentCommand documentCommand, Stream outputStream)
		{
			var pdfWriter = ObjectFactory.Get<IDocumentPDFWriter>();
			pdfWriter.WriteToStream((BusinessObject)documentCommand.Parent, documentCommand, outputStream);
		}

		DocumentPrintSet GetDocumentPrintSet(DocumentCommand documentCommand)
		{
			if (documentCommand.ParentDocumentSupporter is ISupportCustomizedDocumentPrintSet scdps && scdps.ShouldCustomizedDocumentPrintSet(documentCommand))
			{
				return (DocumentPrintSet)scdps.GetCustomizedDocumentPrintSet(documentCommand);
			}
			else
			{
				return new DocumentPrintSet(documentCommand, new RuntimeOptions.UserControlProviderList(), null);
			}
		}

		Stream MergeDocumentPacks(DocumentPrintSet set)
		{
			using (var method = new MergedDeliveryMethod())
			{
				foreach (var pack in set.GetDocumentPacks())
				{
					using (pack)
					{
						foreach (IDeliverable deliverable in pack.OfType<Report>().ToList())
						{
							using (deliverable)
							{
								var deliveryInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document)
								{
									ShowDraftWatermark = pack.DeliveryInstructions.IsDraft,
								};

								method.AddFile(deliveryInfo);
								deliverable.Save(null, null, deliveryInfo.FileContents);
							}
						}
					}
				}

				return method.MergeFilesIntoOneXLS();
			}
		}

		class MergedDeliveryMethod : DeliveryMethod, IDisposable
		{
			public void Dispose()
			{
				foreach (DeliveryInfo info in DeliveryInfos)
				{
					info.FileContents?.Dispose();
				}
			}
		}
	}
}
