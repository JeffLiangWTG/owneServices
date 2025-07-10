using System.IO;
using System.Xml;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer
{
	public class XmlAccountingTransactionExporter : AccountingTransactionsDataExporter, ISupportHighWaterMark
	{
		public XmlAccountingTransactionExporter(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override void ExportObjectsToEndPoint(BusinessObject bizObj, IValueObjectDataAdapter dataAdapter, ZString status)
		{
			XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);

			using (Stream tempStream = new MemoryStream())
			{
				XmlTextWriter writer = new XmlTextWriter(new StreamWriter(tempStream));
				writer.Formatting = Formatting.Indented;

				BusinessObject bizObjToSerialize = LoadCorrectTypeOfBusinessObject(dataAdapter, bizObj, status);
				IncreaseStandardTransactionsProcessedCount(bizObjToSerialize);
				serializer.WriteToXml(writer, dataAdapter, bizObjToSerialize, new ValueObjectExportContext(Notify));
				ResetStreamToZero(tempStream);

				StreamReader reader = new StreamReader(tempStream);
				string data = reader.ReadToEnd();
				data = data.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n", string.Empty);
				data = data.Replace("xmlns=\"http://www.edi.com.au/EnterpriseService/\"", string.Empty);
				data = data.Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", string.Empty);
				data = data.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", string.Empty);
				XmlDocumentWriter.WriteRaw(data);
				XmlDocumentWriter.Flush();
			}
		}

		protected override void BeforeDocumentBuild()
		{
			EnsureDocumentStreamPositionForPayload();

#if DEBUG
			if (Globals.IsTest)
			{
				XmlDocumentWriter.WriteRaw("<FinancialTransactions xmlns=\"http://www.edi.com.au/EnterpriseService/\">");
			}
			else
			{
				XmlDocumentWriter.WriteRaw("<FinancialTransactions>");
			}
#else
			XmlDocumentWriter.WriteRaw("<FinancialTransactions>");
#endif
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected virtual void EnsureDocumentStreamPositionForPayload()
		{
			Xsd.XmlInterchange interchange = CreateInterchange();
			XmlValueObjectSerializer interchangeSerializer = new XmlValueObjectSerializer(typeof(Xsd.XmlInterchange));
			interchangeSerializer.Serialize(XmlDocumentWriter, interchange);
			XmlDocumentWriter.Flush();

			ResetStreamToZero(Document);
			StreamReader reader = new StreamReader(Document);
			string interchangeString = reader.ReadToEnd();
			int index = interchangeString.LastIndexOf(InterchangeInfoCloseTagRaw);

			ToWriteWhenDone = interchangeString.Substring(index + InterchangeInfoCloseTagRaw.Length);

			byte[] toWriteWhenDoneAsBytes = reader.CurrentEncoding.GetBytes(ToWriteWhenDone);

			Document.Position -= toWriteWhenDoneAsBytes.Length;

			XmlDocumentWriter.WriteRaw("<Payload>");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected override void AfterDocumentBuild()
		{
			base.AfterDocumentBuild();
			XmlDocumentWriter.WriteRaw("</FinancialTransactions>");
			WriteEndOfDocument();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected virtual void WriteEndOfDocument()
		{
			XmlDocumentWriter.WriteRaw("</Payload>" + ToWriteWhenDone);
		}

		protected override void InitialiseDocumentWriter(Stream exportFile)
		{
			Document = exportFile;
			InitialiseXmlDocumentWriter();
		}

		protected void InitialiseXmlDocumentWriter()
		{
			XmlDocumentWriter = new XmlTextWriter(new StreamWriter(Document));
			XmlDocumentWriter.Formatting = Formatting.Indented;
		}

		protected override void DeInitialiseDocumentWriter()
		{
			XmlDocumentWriter.Flush();
			base.DeInitialiseDocumentWriter();
		}

		void ResetStreamToZero(Stream streamToReset)
		{
			streamToReset.Position = 0;
		}

		Xsd.XmlInterchange CreateInterchange()
		{
			Xsd.XmlInterchange result = new Xsd.XmlInterchange();
			result.InterchangeInfo = new Xsd.InterchangeInfo();
			result.Version = "1";

			var currentCompany = Env.CurrentCompany;
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			OrgHeader organisation = Factory.Load<OrgHeader>(currentCompany.OrganisationPK);
			result.InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(organisation, new ValueObjectExportContext(Notify));
			result.InterchangeInfo.Source = new Xsd.InterchangeInfoSource();
			result.InterchangeInfo.Source.CompanyCode = currentCompany.Code;
			result.InterchangeInfo.Source.EnterpriseCode = registrationKey.EnterpriseCode;
			result.InterchangeInfo.Source.OriginServer = registrationKey.ServerCode;
			result.InterchangeInfo.Source.LoginName = GlbStaff.CurrentUser.GS_LoginName;
			result.InterchangeInfo.Date = ZDateTime.Now.ToDateTime();
			Xsd.InterchangeInfoReferenceKey referenceKey = result.InterchangeInfo.ReferenceKeys.AddNew();
			referenceKey.ReferenceKeyName = Xsd.ReferenceType.BatchNumber;
			referenceKey.ReferenceKeyNameSpecified = true;
			referenceKey.Value = FilterProvider.CurrentBatchNo.ToString();

			result.PayloadSpecified = false;

			return result;
		}

		#region ISupportHighWaterMark

		public virtual bool IsHighWaterMarkEnabled
		{
			get { return true; }
		}

		public virtual DateTimeRegistryItem HighWaterMarkRegistry
		{
			get { return SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark; }
		}

		#endregion

		string ToWriteWhenDone = string.Empty;
		const string InterchangeInfoCloseTagRaw = "</InterchangeInfo>"; // Hard-coded constant
		protected XmlTextWriter XmlDocumentWriter;
	}
}
