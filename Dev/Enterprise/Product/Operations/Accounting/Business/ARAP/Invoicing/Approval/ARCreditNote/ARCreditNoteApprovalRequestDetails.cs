using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	[XmlRoot("PostingRequest")] //for compatibility with existing XMLs
	public class ARCreditNoteApprovalRequestDetails : ApprovalRequestDetailsWithCharges<ARCreditNoteApprovalRequestChargeDetails>
	{
		public new abstract class Schema : ApprovalRequestDetailsWithCharges<ARCreditNoteApprovalRequestChargeDetails>.Schema
		{
			public const string ApprovingOption = "ApprovingOption";
			public const string InvoiceDate = "InvoiceDate";
			public const string PostDate = "PostDate";
			public const string MaxAuthorisationLevelRequired = "MaxAuthorisationLevelRequired";
		}

		[Obsolete("For serializer only")]
		protected ARCreditNoteApprovalRequestDetails()
			: base(new BusinessObjectFactory())
		{
		}

		public ARCreditNoteApprovalRequestDetails(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZString ApprovalType { get; set; }

		#region ApprovingOption

		[ResourceStringData("ARCreditNoteApprovalRequestDetails|ApprovingOption", Caption = "Approving Option")]
		public ZString ApprovingOption
		{
			get { return approvingOption; }
			set
			{
				SetNonPersistentPropertyValue(ApprovingOptionInfo, ref approvingOption, value);
			}
		}
		ZString approvingOption;

		ZPropertyInfo ApprovingOptionInfo
		{
			get { return GetZPropertyInfo(Schema.ApprovingOption); }
		}

		#endregion

		#region ApprovingOption

		[ResourceStringData("ARCreditNoteApprovalRequestDetails|MaxAuthorisationLevelRequired", Caption = "Max Level Required")]
		public ZInt MaxAuthorisationLevelRequired
		{
			get { return maxAuthorisationLevelRequired; }
			set
			{
				SetNonPersistentPropertyValue(MaxAuthorisationLevelRequiredInfo, ref maxAuthorisationLevelRequired, value);
			}
		}
		ZInt maxAuthorisationLevelRequired;

		ZPropertyInfo MaxAuthorisationLevelRequiredInfo
		{
			get { return GetZPropertyInfo(Schema.MaxAuthorisationLevelRequired); }
		}

		#endregion

		#region InvoiceDate

		[ResourceStringData("ARCreditNoteApprovalRequestDetails|InvoiceDate", Caption = "Invoice Date")]
		public ZDateTime InvoiceDate
		{
			get { return invoiceDate; }
			set
			{
				SetNonPersistentPropertyValue(InvoiceDateInfo, ref invoiceDate, value);
			}
		}
		ZDateTime invoiceDate;

		ZPropertyInfo InvoiceDateInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceDate); }
		}

		#endregion

		#region PostDate

		[ResourceStringData("ARCreditNoteApprovalRequestDetails|PostDate", Caption = "Post Date")]
		public ZDateTime PostDate
		{
			get { return postDate; }
			set
			{
				SetNonPersistentPropertyValue(PostDateInfo, ref postDate, value);
			}
		}
		ZDateTime postDate;

		ZPropertyInfo PostDateInfo
		{
			get { return GetZPropertyInfo(Schema.PostDate); }
		}

		#endregion

		internal bool IsEligibleToAutoPostAmendingARCreditNote { get; private set; }

		public void UpdatePostDateToToday()
		{
			PostDate = ZDateTime.Now;
		}

		#region Overrides

		protected override NonPersistentBusinessObjectCollection<ARCreditNoteApprovalRequestChargeDetails> CreateChargesCollection()
		{
			var chargeCollection = new ARCreditNoteApprovalRequestChargeDetailsCollection(Factory);
			chargeCollection.ApprovalType = ApprovalType;
			return chargeCollection;
		}

		protected override bool IsAllowedtoViewTransactionOutsideLoginPermission(ARCreditNoteApprovalRequestChargeDetails chargeApprovalRequest)
		{
			return true;
		}

		protected override void CopyInstanceSpecificFieldsFrom(ApprovalRequestDetailsWithCharges<ARCreditNoteApprovalRequestChargeDetails> postingRequestToCopy)
		{
			base.CopyInstanceSpecificFieldsFrom(postingRequestToCopy);

			var postingRequestToCopy_Cast = postingRequestToCopy as ARCreditNoteApprovalRequestDetails;
			if (postingRequestToCopy_Cast != null)
			{
				ApprovingOption = postingRequestToCopy_Cast.ApprovingOption;
				InvoiceDate = postingRequestToCopy_Cast.InvoiceDate;
				PostDate = postingRequestToCopy_Cast.PostDate;
				MaxAuthorisationLevelRequired = postingRequestToCopy_Cast.MaxAuthorisationLevelRequired;
				IsEligibleToAutoPostAmendingARCreditNote = postingRequestToCopy_Cast.IsEligibleToAutoPostAmendingARCreditNote;
			}
		}

		protected override void ReadXmlForInstanceSpecificFields(XmlReader reader)
		{
			base.ReadXmlForInstanceSpecificFields(reader);

			if (reader.Name == Schema.ApprovingOption)
			{
				ApprovingOption = reader.ReadElementString(Schema.ApprovingOption);
			}
			if (reader.Name == Schema.InvoiceDate)
			{
				InvoiceDate = new ZDateTime(reader.ReadElementString(Schema.InvoiceDate));
				PostDate = new ZDateTime(reader.ReadElementString(Schema.PostDate));
				IsEligibleToAutoPostAmendingARCreditNote = true;
			}
			if (reader.Name == Schema.MaxAuthorisationLevelRequired)
			{
				MaxAuthorisationLevelRequired = new ZInt(reader.ReadElementString(Schema.MaxAuthorisationLevelRequired));
			}
		}

		protected override void WriteXmlForInstanceSpecificFields(XmlWriter writer)
		{
			base.WriteXmlForInstanceSpecificFields(writer);

			writer.WriteElementString(Schema.ApprovingOption, ApprovingOption);
			writer.WriteElementString(Schema.InvoiceDate, InvoiceDate.ToString());
			writer.WriteElementString(Schema.PostDate, PostDate.ToString());
			writer.WriteElementString(Schema.MaxAuthorisationLevelRequired, MaxAuthorisationLevelRequired.ToString());
		}

		#endregion
	}
}
