using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public abstract class ApprovalRequestDetails : NonPersistentBusinessObject, IObsoleteValidation, IXmlSerializable
	{
		#region Schema

		public abstract class Schema
		{
			public const string MaxAmountToApprove = "MaxAmountToApprove";
			public const string Description = "Description";
			public const string InvoiceTerm = "InvoiceTerm";
			public const string InvoiceTermDays = "InvoiceTermDays";
		}

		#endregion

		public ApprovalRequestDetails(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		#region MaxAmountToApprove

		[DecimalPlaces(nameof(LocalDecimals))]
		[ResourceStringData("ApprovalRequestDetails|MaxAmountToApprove", Caption = "Maximum Amount To Approve", MediumCaption = "Max. Amount To Approve", ShortCaption = "Max. Amt. To Approve")]
		public ZDecimal MaxAmountToApprove
		{
			get { return maxAmountToApprove; }
			set
			{
				SetNonPersistentPropertyValue(MaxAmountToApproveInfo, ref maxAmountToApprove, value);
			}
		}
		ZDecimal maxAmountToApprove;

		ZPropertyInfo MaxAmountToApproveInfo
		{
			get { return GetZPropertyInfo(Schema.MaxAmountToApprove); }
		}

		#endregion

		#region Description

		[ResourceStringData("ApprovalRequestDetails|Description", Caption = "Description", ShortCaption = "Desc.")]
		public ZString Description
		{
			get { return description; }
			set
			{
				SetNonPersistentPropertyValue(DescriptionInfo, ref description, value);
			}
		}
		ZString description;

		ZPropertyInfo DescriptionInfo => GetZPropertyInfo(Schema.Description);

		#endregion

		#region InvoiceTerm

		[ResourceStringData("ApprovalRequestDetails|InvoiceTerm", Caption = "Invoice Term")]
		public ZString InvoiceTerm
		{
			get { return invoiceTerm; }
			set
			{
				SetNonPersistentPropertyValue(InvoiceTermInfo, ref invoiceTerm, value);
			}
		}
		ZString invoiceTerm;

		ZPropertyInfo InvoiceTermInfo => GetZPropertyInfo(Schema.InvoiceTerm);

		#endregion

		#region InvoiceTermDays

		[ResourceStringData("ApprovalRequestDetails|InvoiceTermDays", Caption = "Invoice Term Days")]
		public ZByte InvoiceTermDays
		{
			get { return invoiceTermDays; }
			set
			{
				SetNonPersistentPropertyValue(InvoiceTermDaysInfo, ref invoiceTermDays, value);
			}
		}
		ZByte invoiceTermDays;

		ZPropertyInfo InvoiceTermDaysInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceTermDays); }
		}

		#endregion

		public void CopyFrom(ApprovalRequestDetails approvalDetailsToCopy)
		{
			CopyFromCore(approvalDetailsToCopy);
		}

		protected virtual void CopyFromCore(ApprovalRequestDetails approvalDetailsToCopy)
		{
			MaxAmountToApprove = approvalDetailsToCopy.MaxAmountToApprove;
			if (!approvalDetailsToCopy.Description.IsEmpty)
			{
				Description = approvalDetailsToCopy.Description;
			}
			if (!approvalDetailsToCopy.InvoiceTerm.IsEmpty)
			{
				InvoiceTerm = approvalDetailsToCopy.InvoiceTerm;
				InvoiceTermDays = approvalDetailsToCopy.InvoiceTermDays;
			}
		}

		public static bool operator ==(ApprovalRequestDetails a, ApprovalRequestDetails b)
		{
			if (((object)a) == null && ((object)b) == null)
			{
				return true;
			}

			if (((object)a) == null || ((object)b) == null)
			{
				return false;
			}

			return a.IsEqual(b);
		}

		protected virtual bool IsEqual(ApprovalRequestDetails b)
		{
			//Description, Invoiceterm and InvoicetermDays properties are not checked in the IsEqual otherwise the AutoPosting process is failing with the error: "Can't post this request because source details have been modified since then"
			//In LevelAuthorizationWithApprovalRequest.PerformTransactionLevelAuthorization() it checks that requests are equals but the auto posting modify automatically some fields from the transaction or line. So I decided to not include any new fields.
			return MaxAmountToApprove == b.MaxAmountToApprove;
		}

		public static bool operator !=(ApprovalRequestDetails a, ApprovalRequestDetails b)
		{
			return !(a == b);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required to avoid CS0661")]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required to avoid CS0660")]
		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public bool IsPostingActionTheSame(ApprovalRequestDetails approvalDetails)
		{
			return IsPostingActionTheSameCore(approvalDetails);
		}
		protected abstract bool IsPostingActionTheSameCore(ApprovalRequestDetails approvalDetails);

		#region IXmlSerializable Members

		public XmlSchema GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			reader.ReadStartElement();

			ReadXmlCore(reader);

			reader.ReadEndElement();
		}

		protected virtual void ReadXmlCore(XmlReader reader)
		{
			MaxAmountToApprove = ZDecimal.Parse(reader.ReadElementString(Schema.MaxAmountToApprove));

			var desc = (reader.Name == Schema.Description) ? reader.ReadElementString(Schema.Description) : string.Empty;
			if (!string.IsNullOrEmpty(desc))
			{
				Description = desc;
			}

			var invTerm = (reader.Name == Schema.InvoiceTerm) ? reader.ReadElementString(Schema.InvoiceTerm) : string.Empty;
			if (!string.IsNullOrEmpty(invTerm))
			{
				InvoiceTerm = invTerm;
			}

			var invTermDays = (reader.Name == Schema.InvoiceTermDays) ? ZByte.ParseSafe(reader.ReadElementString(Schema.InvoiceTermDays), ZByte.Zero) : ZByte.Zero;
			if (!invTermDays.IsEmpty)
			{
				InvoiceTermDays = invTermDays;
			}
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			WriteXmlCore(writer);
		}

		protected virtual void WriteXmlCore(XmlWriter writer)
		{
			writer.WriteElementString(Schema.MaxAmountToApprove, MaxAmountToApprove.ToString());
			writer.WriteElementString(Schema.Description, Description);
			writer.WriteElementString(Schema.InvoiceTerm, InvoiceTerm);
			writer.WriteElementString(Schema.InvoiceTermDays, InvoiceTermDays.ToString());
		}
		#endregion
	}
}
