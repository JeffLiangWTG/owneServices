using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Xml;

using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.Business
{
	public abstract class LinkedeNettEDIMessage : eNettEDIMessage
	{
		public LinkedeNettEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected abstract Type DataAdapterType { get; }

		new readonly public static TypeDecider TypeDecider = new eNettEDIMessageTypeDecider();

		TxnHeader fHeader;
		public TxnHeader Header
		{
			get
			{
				if (fHeader == null && !EM_MessageText.IsEmpty)
				{
					using (StringReader sr = new StringReader(EM_MessageText))
					using (XmlReader xmlReader = XmlReader.Create(sr))
					{
						try
						{
							XmlInterchange interchange;
							return fHeader = (TxnHeader)XmlInterchange.DeserializeInterchangeAndPayload(xmlReader, Serializer, out interchange, true);
						}
						catch (XmlException) { }
						catch (InvalidOperationException) { }
						catch (ArgumentException) { }
						catch (NullReferenceException) { }
						return null;
					}
				}
				return fHeader;
			}
		}

		XmlValueObjectSerializer fSerializer;
		XmlValueObjectSerializer Serializer
		{
			get
			{
				if (fSerializer == null)
				{
					object dataAdapter = Activator.CreateInstance(DataAdapterType);
					PropertyInfo valueObjectTypeProperty = DataAdapterType.GetProperty("ValueObjectType");
					PropertyInfo rootElementNameProperty = DataAdapterType.GetProperty("RootElementName");
					fSerializer = new XmlValueObjectSerializer((Type)valueObjectTypeProperty.GetValue(dataAdapter, null));
				}
				return fSerializer;
			}
		}

		#region Overridden Properties

		public override ZString EM_Ledger
		{
			get { return Header == null ? String.Empty : Header.Ledger.ToString(); }
		}

		public override ZString EM_TransactionType
		{
			get { return Header == null ? String.Empty : Header.TxnType.ToString(); }
		}

		public override ZString EM_TransactionNumber
		{
			get { return Header == null ? String.Empty : Header.TxnNumber.ToString(); }
		}

		public override ZString EM_JobInvoiceNo
		{
			get { return Header == null ? String.Empty : Header.JobInvoiceNo.ToString(); }
		}

		public override ZString EM_ChequeOrReference
		{
			get { return Header == null ? String.Empty : Header.ChequeOrReference.ToString(); }
		}

		public override ZString EM_InvoiceDate
		{
			get { return Header == null ? String.Empty : Header.InvoiceDate.ToString(); }
		}

		public override ZString EM_PostDate
		{
			get { return Header == null ? String.Empty : Header.PostDate.ToString(); }
		}

		public override ZString EM_LocalInvoiceAmtInclTax
		{
			get { return Header == null ? String.Empty : Header.LocalInvoiceAmtInclTax.Value.ToString(); }
		}

		public override ZString EM_OSInvoiceAmtInclTax
		{
			get { return Header == null ? String.Empty : Header.OsInvoiceAmtInclTax.Value.ToString(); }
		}

		public override ZString EM_Currency
		{
			get { return Header == null ? String.Empty : Header.OsInvoiceAmtInclTax.CurrencyCode.ToString(); }
		}

		public override ZString EM_OrganisationCode
		{
			get { return Header == null ? String.Empty : Header.DebtorOrCreditor.EDICode.ToString(); }
		}

		public override ZString EM_OrganisationName
		{
			get { return Header == null ? String.Empty : Header.DebtorOrCreditor.OrganisationDetails.Name.ToString(); }
		}

		#endregion
	}
}