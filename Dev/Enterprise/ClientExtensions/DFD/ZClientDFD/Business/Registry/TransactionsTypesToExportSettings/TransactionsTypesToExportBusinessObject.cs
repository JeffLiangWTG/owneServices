using System.Xml;
using System.Xml.Serialization;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.DFD.Registry
{
	[XmlSerializerAssembly("ZClientDFD.XmlSerializers")]
	public class TransactionsTypesToExportBusinessObject : RegistryBusinessObjectTemplate
	{
		public TransactionsTypesToExportBusinessObject()
			: base()
		{
		}

		public TransactionsTypesToExportBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Schema

		public static class Schema
		{
			public const string ARInvoice = "ARInvoice";
			public const string ARCreditNote = "ARCreditNote";
			public const string ARAdjustmentNote = "ARAdjustmentNote";
			public const string ARJobRelated = "ARJobRelated";
			public const string ARNonJobRelated = "ARNonJobRelated";
		}

		#endregion

		#region AR Invoice

		public ZBool ARInvoice
		{
			get { return fARInvoice; }
			set
			{
				fARInvoice = value;
				ARInvoiceInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ARInvoiceInfo
		{
			get { return GetZPropertyInfo(Schema.ARInvoice); }
		}

		ZBool fARInvoice;

		#endregion

		#region AR Credit Note

		public ZBool ARCreditNote
		{
			get { return fARCreditNote; }
			set
			{
				fARCreditNote = value;
				ARCreditNoteInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ARCreditNoteInfo
		{
			get { return GetZPropertyInfo(Schema.ARCreditNote); }
		}

		ZBool fARCreditNote;

		#endregion

		#region AR Adjustment Note

		public ZBool ARAdjustmentNote
		{
			get { return fARAdjustmentNote; }
			set
			{
				fARAdjustmentNote = value;
				ARAdjustmentNoteInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ARAdjustmentNoteInfo
		{
			get { return GetZPropertyInfo(Schema.ARAdjustmentNote); }
		}

		ZBool fARAdjustmentNote;

		#endregion

		#region AR Job Related

		public ZBool ARJobRelated
		{
			get { return fARJobRelated; }
			set
			{
				fARJobRelated = value;
				ARJobRelatedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ARJobRelatedInfo
		{
			get { return GetZPropertyInfo(Schema.ARJobRelated); }
		}

		ZBool fARJobRelated;

		#endregion

		#region AR Non Job Related

		public ZBool ARNonJobRelated
		{
			get { return fARNonJobRelated; }
			set
			{
				fARNonJobRelated = value;
				ARNonJobRelatedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ARNonJobRelatedInfo
		{
			get { return GetZPropertyInfo(Schema.ARNonJobRelated); }
		}

		ZBool fARNonJobRelated;

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TransactionsTypesToExportBusinessObject();
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ARInvoice, ARInvoice.ToString());
			writer.WriteElementString(Schema.ARCreditNote, ARCreditNote.ToString());
			writer.WriteElementString(Schema.ARAdjustmentNote, ARAdjustmentNote.ToString());
			writer.WriteElementString(Schema.ARJobRelated, ARJobRelated.ToString());
			writer.WriteElementString(Schema.ARNonJobRelated, ARNonJobRelated.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			fARInvoice = new ZBool(reader.ReadElementStringAsZBool(Schema.ARInvoice));
			fARCreditNote = new ZBool(reader.ReadElementStringAsZBool(Schema.ARCreditNote));
			fARAdjustmentNote = new ZBool(reader.ReadElementStringAsZBool(Schema.ARAdjustmentNote));
			fARJobRelated = new ZBool(reader.ReadElementStringAsZBool(Schema.ARJobRelated));
			fARNonJobRelated = new ZBool(reader.ReadElementStringAsZBool(Schema.ARNonJobRelated));
		}

		#endregion

		#endregion
	}
}
