using System;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	class ArgentinaEInvoiceXmlWriter : TransactionBatchToXmlWriter
	{
		readonly IArgentinaEInvoicingDependencyFactory ArgentinaEInvocingDependencies;

		public ArgentinaEInvoiceXmlWriter()
		{
			localEInvoiceXmlBuilder_constructorInitializedOnly = new LocalEInvoiceXmlBuilder();
			exportEInvoiceXmlBuilder_constructorInitializedOnly = new ExportEInvoiceXmlBuilder();
			ArgentinaEInvocingDependencies = ObjectFactory.Get<IEInvoicingDependencyFactory>().GetArgentinaEInvoicingDependencyFactory();
		}

		#region Obsolete, we must use ArgentinaEInvocingDependencies

		IEInvoiceXmlBuilder LocalEInvoiceXmlBuilder => localEInvoiceXmlBuilder_constructorInitializedOnly;
		IEInvoiceXmlBuilder localEInvoiceXmlBuilder_constructorInitializedOnly;
		IEInvoiceXmlBuilder ExportEInvoiceXmlBuilder => exportEInvoiceXmlBuilder_constructorInitializedOnly;
		IEInvoiceXmlBuilder exportEInvoiceXmlBuilder_constructorInitializedOnly;

#if DEBUG
		public void SubstituteLocalEInvoiceXmlBuilder_ForTestOnly(IEInvoiceXmlBuilder replacement) => localEInvoiceXmlBuilder_constructorInitializedOnly = replacement;
		public IEInvoiceXmlBuilder LocalEInvoiceXmlBuilder_ExposedForTestOnly => LocalEInvoiceXmlBuilder;
		public void SubstituteExportEInvoiceXmlBuilder_ForTestOnly(IEInvoiceXmlBuilder replacement) => exportEInvoiceXmlBuilder_constructorInitializedOnly = replacement;
		public IEInvoiceXmlBuilder ExportEInvoiceXmlBuilder_ExposedForTestOnly => ExportEInvoiceXmlBuilder;
#endif
		#endregion

		protected override XmlWriterSettings Settings()
		{
			var settings = base.Settings();
			settings.NewLineChars = " ";

			return settings;
		}

		protected override void WriteDocumentBody(XmlWriter writer, TransactionInfo transactionInfo, ZString messageType, AccEInvoicingBatch accBatch, INotifications notifications, INotifications warnings)
		{
			XStreamingElement eInvoiceInfo;
			switch (messageType)
			{
				case ArgentinaEInvoiceAPICommandList.Codes.GenerateExportInvoiceRequest:
					eInvoiceInfo = ExportEInvoiceXmlBuilder.BuildXml(transactionInfo, accBatch);
					break;
				case ArgentinaEInvoiceAPICommandList.Codes.GenerateLocalInvoiceRequest:
					eInvoiceInfo = LocalEInvoiceXmlBuilder.BuildXml(transactionInfo, accBatch);
					break;
				case ArgentinaEInvoiceAPICommandList.Codes.GenerateDetailItemsInvoiceRequest:
					eInvoiceInfo = ArgentinaEInvocingDependencies.GetItemDetailEInvoiceXmlBuilder().BuildXml(transactionInfo, accBatch);
					break;
				default:
					throw new ArgumentException("Invalid Message Type.");
			}

			eInvoiceInfo.WriteTo(writer);
		}
	}
}
