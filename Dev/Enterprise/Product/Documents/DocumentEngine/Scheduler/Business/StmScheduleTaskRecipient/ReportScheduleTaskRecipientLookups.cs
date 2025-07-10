using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class ReportScheduleTaskRecipientLookups : StmScheduleTaskRecipientLookups
	{
		public ReportScheduleTaskRecipientLookups(ReportScheduleTaskRecipient parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList Printers
		{
			get
			{
				if (printers == null)
				{
					StmPrintQueueCollection collection = StmPrintQueueCollection.GetPrintersVisibleToCurrentUser(Factory, true);
					printers = collection.GetOnlinePrinterNames();
				}
				return printers;
			}
		}
		CodeDescriptionPairList printers;

		public CodeDescriptionPairList PrintersWithParent
		{
			get
			{
				if (printersWithParent == null)
				{
					var collection = StmPrintQueueCollection.GetPrintersVisibleToCurrentUser(Factory, true);
					printersWithParent = collection.GetOnlinePrinterNames();

					if (!Parent.S6_SQ.IsEmpty && !printersWithParent.Cast<ICodeDescription>().Any(p => Equals(Parent.S6_SQ, p.PK)))
					{
						var printer = Factory.LoadTop1<StmPrintQueue>(new ZQuery(StmPrintQueueSchema.PK, Parent.S6_SQ));
						if (printer != null)
						{
							printersWithParent.AddPair(printer.PK, printer.SQ_DisplayName, printer.SQ_ServerName);
						}
					}
				}
				return printersWithParent;
			}
		}
		CodeDescriptionPairList printersWithParent;

		protected override CodeDescriptionPairList GetAttachmentTypesCore()
		{
			var attachmentTypes = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			var task = Factory.Load<ReportScheduleTask>(Parent.S6_S5);

			attachmentTypes.AddPair(AttachmentTypeList.Codes.Xls, AttachmentTypeList.Descriptions.Xls);
			attachmentTypes.AddPair(AttachmentTypeList.Codes.Xlsx, AttachmentTypeList.Descriptions.Xlsx);
			attachmentTypes.AddPair(AttachmentTypeList.Codes.Csv, AttachmentTypeList.Descriptions.Csv);
			attachmentTypes.AddPair(AttachmentTypeList.Codes.Pdf, AttachmentTypeList.Descriptions.Pdf);
			attachmentTypes.AddPair(AttachmentTypeList.Codes.Pdfa, AttachmentTypeList.Descriptions.Pdfa);
			attachmentTypes.AddPair(AttachmentTypeList.Codes.Tif, AttachmentTypeList.Descriptions.Tif);
			attachmentTypes.AddPair(AttachmentTypeList.Codes.Html, AttachmentTypeList.Descriptions.Html);
			attachmentTypes.AddPair(AttachmentTypeList.Codes.Htmf, AttachmentTypeList.Descriptions.Htmf);
			attachmentTypes.AddPair(AttachmentTypeList.Codes.Txt_Comm, AttachmentTypeList.Descriptions.Txt_Comm);
			attachmentTypes.AddPair(AttachmentTypeList.Codes.Txt_Pipe, AttachmentTypeList.Descriptions.Txt_Pipe);
			attachmentTypes.AddPair(AttachmentTypeList.Codes.Txt_Semi, AttachmentTypeList.Descriptions.Txt_Semi);

			if (task != null)
			{
				if (!task.DisableCSVExport && task.ReportHasColumnHeaders)
				{
					attachmentTypes.AddPair(AttachmentTypeList.Codes.CsvWithHeadings, AttachmentTypeList.Descriptions.CsvWithHeadings);
					attachmentTypes.AddPair(AttachmentTypeList.Codes.Xml, AttachmentTypeList.Descriptions.Xml);
				}

				foreach (var typeCode in task.ExcludedAttachmentTypes)
				{
					attachmentTypes.RemoveCode(typeCode);
				}
			}

			return attachmentTypes;
		}

		protected override CodeDescriptionPairList GetNewNotifyModesList()
		{
			var list = base.GetNewNotifyModesList();
			list.AddPair(Enterprise.Core.Constants.ContactNotifyModes.Ftp, ResString.GetMultilingualString("2f6a67bb-e5f7-49f4-9af5-fdbff4049c14", "Upload to FTP"));
			list.Sort();
			return list;
		}

		protected override CodeDescriptionPairList GetNewDeliveryRecipientTypesList()
		{
			var list = base.GetNewDeliveryRecipientTypesList();
			list.AddPair(ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc, ResString.GetMultilingualString("1976f429-bfce-4ebf-ba6e-3db03a7c1216", "eDoc"));
			return list;
		}
	}
}
