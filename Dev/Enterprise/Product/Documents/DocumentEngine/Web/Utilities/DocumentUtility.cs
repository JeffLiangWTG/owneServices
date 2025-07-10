using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(DataContentTypes))]

namespace Enterprise.DocumentEngine.Web
{
	public class DocumentUtility
	{
		public DocumentUtility(BusinessObjectFactory factory, bool requiresWebUser)
		{
			this.factory = factory;
			this.requiresWebUser = requiresWebUser;
		}

		public DocumentUtility(BusinessObjectFactory factory)
			: this(factory, true)
		{
		}

		readonly BusinessObjectFactory factory;
		readonly bool requiresWebUser;

		public byte[] GetDocument(DocumentPack pack, OrgContact contact, string contentType)
		{
			return GetDocumentInternal(pack, contact, contentType);
		}

		public byte[] GetDocument(IDocumentSupportable bizO, ZString printCommandName, OrgContact contact, string contentType)
		{
			using (var docPack = GetDocumentPack(bizO, printCommandName, contact))
			{
				return GetDocumentInternal(docPack, contact, contentType);
			}
		}

		public void RenderAndAddDocumentPackToDeliveryMethod(DocumentPack pack, OrgContact contact, WebDeliveryMethod method)
		{
			if (pack != null && pack.Count > 0 && (!requiresWebUser || contact != null))
			{
				DocDeliveryContact deliveryContact = contact != null ? GetDeliveryContact(pack, contact) : null;

				for (int i = 0; i < pack.Count; i++)
				{
					var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
					method.AddFile(info);

					pack[i].Save(deliveryContact, deliveryContact, info.FileContents);
					pack[i].Dispose();
				}
			}
		}

		#region Implementation

		byte[] GetDocumentInternal(DocumentPack pack, OrgContact contact, string contentType)
		{
			byte[] result = System.Array.Empty<byte>();

			if (pack != null && pack.Count > 0 && (!requiresWebUser || contact != null))
			{
				DocDeliveryContact deliveryContact = contact != null ? GetDeliveryContact(pack, contact) : null;

				byte[] excelData = System.Array.Empty<byte>();
				bool isLocalDocument = false;
				decimal lineSpacing = 1m;

				if (pack.StmMenuCommand != null)
				{
					isLocalDocument = pack.StmMenuCommand.IsLocalDocument;
					lineSpacing = pack.StmMenuCommand.SU_FlexCelLineSpacing;
				}

				if (pack.Count > 1)
				{
					using (var method = new WebDeliveryMethod())
					{
						foreach (IDeliverable deliverable in pack.OfType<Report>().ToList())
						{
							var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
							method.AddFile(info);
							deliverable.Save(deliveryContact, deliveryContact, info.FileContents);
							if (info.FileContents.Length == 0)
							{
								return System.Array.Empty<byte>();
							}
						}

						using (MemoryStream stream = method.MergeFilesIntoSingleExcelFile())
						{
							excelData = stream.ToArray();
						}
					}
				}
				else
				{
					using (var stream = new MemoryStream())
					{
						pack[0].Save(deliveryContact, deliveryContact, stream);
						excelData = stream.ToArray();
						if (stream.Length == 0)
						{
							return System.Array.Empty<byte>();
						}
					}
				}

				var deliveryInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
				deliveryInfo.ShowDraftWatermark = pack.DeliveryInstructions.IsDraft;

				switch (contentType)
				{
					case DataContentTypes.Excel:
						result = excelData;
						break;

					case DataContentTypes.Pdf:
						result = DocumentConverter.ConvertFromExcel(excelData, string.Empty, OutputFormatType.PDF, deliveryInfo.Watermark, ColourDepth.BlackAndWhite, isLocalDocument, lineSpacing);
						break;
				}
			}

			return result;
		}

		DocDeliveryContact GetDeliveryContact(DocumentPack pack, OrgContact contact)
		{
			var docDelivery = new DocAutoDelivery();

			// Document delivery can occur on multiple threads simultaneously
			// since DocumentRequestHandler only gets a reader lock on the session state.
			// Need to synchronize access to the contact and its Factory.
			lock (contact)
			{
				DocDeliveryContact deliveryContact = docDelivery.GetDeliveryDetailsForContact(contact, pack.StmMenuCommand);
				return deliveryContact;
			}
		}

#if DEBUG
		internal
#endif
 DocumentPack GetDocumentPack(IDocumentSupportable bizO, ZString documentCommandName, OrgContact contact)
		{
			DocumentPack pack = null;
			DocumentCommand command = DocumentCommand.GetDocumentCommand(factory, bizO, documentCommandName);
			if (command != null)
			{
				pack = new DocumentPack(command, bizO, new RuntimeOptions.UserControlProviderList(), null);
				if (contact != null)
				{
					pack.Organisation = contact.Header;
				}
			}
			return pack;
		}

		#endregion
	}
}
