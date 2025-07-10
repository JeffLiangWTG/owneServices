using System;
using System.IO;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1
{
	public class ChinaStandard2010DataInterfaceExporter : ChinaStandardExporter
	{
		public ChinaStandard2010DataInterfaceExporter(ChinaStandard2010DataInterfaceWrapper bizObj, NotificationBuffer notification)
			: base(bizObj, notification)
		{ }

		public ChinaStandard2010DataInterfaceExporter(ChinaStandard2010DataInterfaceWrapper bizObj, NotificationBuffer notification, bool isAutomaticExport)
			: base(bizObj, notification, isAutomaticExport)
		{ }

		#region ExportDataCore

		protected override bool ExportDataCore(IValueObjectDataAdapter dataAdapter)
		{
			bool isExportSomething = false;
			try
			{
				FileName = dataAdapter.RootElementName;
				XmlDocumentWriter = new XmlTextWriter(new StreamWriter(Document)) { Formatting = Formatting.Indented };
				XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);
				using (Stream tempStream = new MemoryStream())
				{
					XmlTextWriter writer = new XmlTextWriter(new StreamWriter(tempStream)) { Formatting = Formatting.Indented };

					BusinessObject bizObjToSerialize = new BusinessObjectThatDoesntSaveForCN(Factory) { BranchPK = BizObj.Branch, BranchCode = BizObj.BranchCode, Period = BizObj.Period, FromDate = FromDate, ToDate = ToDate, ChartType = BizObj.OLdChartType };

					serializer.WriteToXml(writer, dataAdapter, bizObjToSerialize, new ValueObjectExportContext(Notification));

					tempStream.Position = 0;
					StreamReader reader = new StreamReader(tempStream);
					string data = reader.ReadToEnd();
					data = data.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n", "<?xml version=\"1.0\" encoding=\"GB18030\"?>\r\n");
					data = data.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "<?xml version=\"1.0\" encoding=\"GB18030\"?>\r\n");
					data = data.Replace("d2p1:", string.Empty);
					data = data.Replace("xmlns:d2p1=\"http://sxbw.audit.gov.cn/AccountingSoftwareDataInterfaceStandard/2010/SOE/XMLSchema\"", string.Empty);
					data = data.Replace("xmlns=\"http://sxbw.audit.gov.cn/AccountingSoftwareDataInterfaceStandard/2010/SOE/XMLSchema",
										"xmlns=\"http://sxbw.audit.gov.cn/AccountingSoftwareDataInterfaceStandard/2010/SOE/XMLSchema\" xsi:schemaLocation=\"http://sxbw.audit.gov.cn/AccountingSoftwareDataInterfaceStandard/2010/SOE/XMLSchema " + base.FileName + ".xsd\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance");
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

		#endregion

		protected override ZString FileName
		{
			get
			{
				return Path.Combine(BizObj.ExportDirectory, "GBT245891ChinaStandard" + base.FileName + ZDateTime.Now.ToString("yyyyMMddHHmmss") + ".xml");// May be an identifier or GUID.
			}
			set { base.FileName = value; }
		}
	}
}
