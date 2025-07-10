using System;
using System.IO;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;

#region SuppressResourceStringsCheckRegion

namespace Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT19581_2004
{
	public class ChinaStandard2004DataInterfaceExporter : ChinaStandardExporter
	{
		public ChinaStandard2004DataInterfaceExporter(ChinaStandard2004DataInterfaceWrapper bizObj, NotificationBuffer notification)
			: base(bizObj, notification)
		{ this.BizObj = bizObj; }

		public ChinaStandard2004DataInterfaceExporter(ChinaStandard2004DataInterfaceWrapper bizObj, NotificationBuffer notification, bool isAutomaticExport)
			: base(bizObj, notification, isAutomaticExport)
		{ }

		protected override bool ExportDataCore(IValueObjectDataAdapter dataAdapter)
		{
			bool isExportSomething;
			try
			{
				FileName = dataAdapter.RootElementName;
				XmlDocumentWriter = new XmlTextWriter(new StreamWriter(Document)) { Formatting = Formatting.Indented };
				XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);
				using (Stream tempStream = new MemoryStream())
				{
					XmlTextWriter writer = new XmlTextWriter(new StreamWriter(tempStream)) { Formatting = Formatting.Indented };

					BusinessObject bizObjToSerialize = new BizObjThatDoesntSaveForCN2004(Factory)
					{
						BranchPK = BizObj.Branch,
						BranchCode = BizObj.BranchCode,
						Period = BizObj.Period,
						FromDate = FromDate,
						ToDate = ToDate,
						ExportFiles = BizObj.ProcessFiles,
						ExportFilesType = BizObj.ExportTXTOrXML
					};

					serializer.WriteToXml(writer, dataAdapter, bizObjToSerialize, new ValueObjectExportContext(Notification));

					tempStream.Position = 0;
					StreamReader reader = new StreamReader(tempStream);
					string data = reader.ReadToEnd();

					var xmlDeclarationUTF8 = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
					data = data.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", xmlDeclarationUTF8);
					if (BizObj.ExportTXTOrXML == BizObjThatDoesntSaveForCN2004.FilesType.TXT)
					{
						data = data.Replace(xmlDeclarationUTF8, xmlDeclarationUTF8 + interchangeNodeOpenTag);
					}

					data = data.Replace("d2p1:", string.Empty);
					data = data.Replace("xmlns:d2p1=\"http://schemas.accounting.org.cn/2004/datainterface/gssm\"", string.Empty);
					data = data.Replace("d3p1:", string.Empty);
					data = data.Replace("xmlns:d3p1=\"http://schemas.accounting.org.cn/2004/datainterface/gssm\"", string.Empty);

					data = data.Replace("xmlns=\"http://schemas.accounting.org.cn/2004/datainterface/gssm\"",
										"xmlns:gssm=\"http://schemas.accounting.org.cn/2004/datainterface/gssm\"\r\n xmlns:user=\"http://schemas.accounting.org.cn/2004/datainterface/user\"\r\n xmlns=\"http://schemas.accounting.org.cn/2004/datainterface/gssm\"\r\n xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\r\n xsi:schemaLocation=\"http://schemas.accounting.org.cn/2004/datainterface/gssmgssm.xsd\" gssm:locID=\"t000\"");
					if (BizObj.ExportTXTOrXML == BizObjThatDoesntSaveForCN2004.FilesType.TXT)
					{
						data += interchangeNodeCloseTag;
					}

					XmlDocumentWriter.WriteRaw(data);
					XmlDocumentWriter.Flush();
					Document.Close();
				}
				isExportSomething = true;
			}
			catch (Exception e) when (e is XmlException || e is ArgumentException || e is NullReferenceException || e is InvalidOperationException)
			{
				isExportSomething = false;
				Notification.Notify(new InfoNotification(Res.GetString("aadd18fa-54a0-4b62-9259-98c8d82e95c3", "There are no data to export.")));
			}

			AfterDataExport(isExportSomething);
			return isExportSomething;
		}

		const string eHubClientRecipientID = "ChinaAccounting";
		const string interchangeNodeOpenTag = @"<ns0:Interchange xmlns:ns0=""http://schemas.accounting.org.cn/2004/datainterface/gssm"">";
		const string interchangeNodeCloseTag = @"</ns0:Interchange>";

		protected override void AfterDataExport(bool isExportSomething)
		{
			if (!isExportSomething || BizObj.ExportTXTOrXML == BizObjThatDoesntSaveForCN2004.FilesType.XML)
			{
				base.AfterDataExport(isExportSomething);
			}
			else
			{
				var context = new DeliveryContext(BizObj.Factory)
				{
					ParentInfo = EntityInfo.New(BizObj),
					ApplicationCode = ApplicationCodeList.Codes.ChinaInterfaceMapping,
					MessageTypeCode = EDIMessageTypeList.Codes.XMS,
					MessageSubTypeCode = EDIMessageSubTypeList.Codes.ChinaInterface,
					Notifications = Notification,
				};

				using (var sourceStream = (SubStreamableStream)File.OpenRead(TempFile))
				{
					var delivery = new EHubDelivery();
					var mode = new NonPersistentEDICommunicationMode();
					mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
					mode.EK_Filename = TXTFileName(BizObj.ProcessFiles[0]) + ".xml";
					mode.EK_Destination = eHubClientRecipientID;
					mode.EK_ServerAddressSubject = BizObj.DeliveryTo;
					delivery.Deliver(context, mode, new DeliveryStreamWrapperUXML(sourceStream, context.ParentInfo));
				}

				Notification.Notify(new InfoNotification(Res.GetString("C311AA6C-3398-4410-B2FA-DA1BA1AEF607",
													   "Data exported successfully, Please check your email '{0}'.", BizObj.DeliveryTo)));
			}
		}

		protected override ZString FileName
		{
			get
			{
				return BizObj.ExportTXTOrXML == BizObjThatDoesntSaveForCN2004.FilesType.XML ?
					Path.Combine(BizObj.ExportDirectory, "GBT19581Standard" + base.FileName + ZDateTime.Now.ToString("yyyyMMddHHmmss") + ".xml") :
					Path.Combine(BizObj.ExportDirectory, TXTFileName(BizObj.ProcessFiles[0]) + ".xml");
			}
			set { base.FileName = value; }
		}

		ZString TXTFileName(ZString fileType)
		{
			if (fileType == "AccountBook")
			{
				return "DZZB";
			}

			if (fileType == "ChartOfAccounts")
			{
				return "KJKM";
			}

			if (fileType == "AccountingVouchers")
			{
				return "JZPZ";
			}

			if (fileType == "TrialBalance")
			{
				return "KMYE";
			}

			if (fileType == "Department")
			{
				return "BMXX";
			}

			if (fileType == "Staff")
			{
				return "YGXX";
			}

			if (fileType == "Client")
			{
				return "WLDW";
			}

			if (fileType == "BalanceSheet")
			{
				return "Q_ZCFZ";
			}

			if (fileType == "ProfitAndLoss")
			{
				return "Q_LR";
			}

			if (fileType == "VATDetailed")
			{
				return "Q_ZZS";
			}

			if (fileType == "AssetProvision")
			{
				return "Q_JZZB";
			}

			if (fileType == "PNLAppropriation")
			{
				return "Q_LRFP";
			}

			if (fileType == "EquityMovement")
			{
				return "Q_GDQYBD";
			}

			if (fileType == "CashFlowStatement")
			{
				return "Q_XJLL";
			}

			return "Error";
		}

		protected new ChinaStandard2004DataInterfaceWrapper BizObj;
	}
}

#endregion
