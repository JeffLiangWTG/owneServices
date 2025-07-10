using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class APInvoiceChargesApprovalRequestDetails : ApprovalRequestDetailsWithCharges<APInvoiceChargesApprovalRequestChargeDetails>
	{
		#region Schema

		public new abstract class Schema : ApprovalRequestDetailsWithCharges<APInvoiceChargesApprovalRequestChargeDetails>.Schema
		{
			public const string Creditor = "Creditor";
			public const string TransactionNumber = "TransactionNumber";
			public const string DetailsXmlNode = "DetailsXmlNode";
		}

		#endregion

		[Obsolete("For serializer only")]
		protected APInvoiceChargesApprovalRequestDetails()
			: base(new BusinessObjectFactory())
		{
		}

		public APInvoiceChargesApprovalRequestDetails(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Creditor

		[ResourceStringData("APInvoiceChargesApprovalRequestInvoiceDetails|Creditor", Caption = "Creditor")]
		public ZString Creditor
		{
			get { return creditor; }
			set
			{
				SetNonPersistentPropertyValue(CreditorInfo, ref creditor, value);
			}
		}
		ZString creditor;

		ZPropertyInfo CreditorInfo
		{
			get { return GetZPropertyInfo(Schema.Creditor); }
		}

		#endregion

		#region TransactionNumber

		[ResourceStringData("APInvoiceChargesApprovalRequestInvoiceDetails|TransactionNumber", Caption = "Transaction Number", MediumCaption = "Tran. Number", ShortCaption = "Tran. Num.")]
		public ZString TransactionNumber
		{
			get { return transactionNumber; }
			set
			{
				SetNonPersistentPropertyValue(TransactionNumberInfo, ref transactionNumber, value);
			}
		}
		ZString transactionNumber;

		ZPropertyInfo TransactionNumberInfo
		{
			get { return GetZPropertyInfo(Schema.TransactionNumber); }
		}

		#endregion

		#region RequisitionStatus

		public ZString RequisitionStatus => requisitionStatus;
		ZString requisitionStatus;

		public ZDateTime RequisitionDate => requisitionDate;
		ZDateTime requisitionDate;

		#endregion

		internal bool IsTransactionRelated { get; set; }

		#region Overrides

		protected override NonPersistentBusinessObjectCollection<APInvoiceChargesApprovalRequestChargeDetails> CreateChargesCollection()
		{
			return new APInvoiceChargesApprovalRequestChargeDetailsCollection(Factory);
		}

		protected override bool IsAllowedtoViewTransactionOutsideLoginPermission(APInvoiceChargesApprovalRequestChargeDetails chargeApprovalRequest)
		{
			var result = true;
			if (chargeApprovalRequest != null)
			{
				var branch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, chargeApprovalRequest.Branch);
				var department = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, chargeApprovalRequest.Department);
				var isAllowedtoViewTransaction = Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed;

				if (!isAllowedtoViewTransaction && branch != null && department != null)
				{
					if (branch != GlbBranch.CurrentBranch || department != GlbDepartment.CurrentDepartment)
					{
						result = AllowedToLogin(branch, department);
					}
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Combining Cache key, not related to GUI")]
		bool AllowedToLogin(GlbBranch branch, GlbDepartment department)
		{
			return Factory.GetCachedValue("Login BRN:" + branch.GB_Code + " DEP:" + department.GE_Code, delegate
			{
				var security = new SecurityCore(GlbStaff.CurrentUser.StaffSecurityPermissionsCollection, GlbStaff.CurrentUser, branch.PK.ToGuid(), department.PK.ToGuid(), branch.GB_GC.ToGuid(), false);
				return security.Login.IsAllowed;
			});
		}

		protected override bool IsPostingActionTheSameCore(ApprovalRequestDetails postingApprovalDetails)
		{
			var result = false;
			var postingApprovalDetails_Cast = postingApprovalDetails as APInvoiceChargesApprovalRequestDetails;
			if (postingApprovalDetails_Cast != null)
			{
				result = IsTransactionRelated ||  //only one posting action is allowed for transaction related requests
							Creditor == postingApprovalDetails_Cast.Creditor &&
							TransactionNumber == postingApprovalDetails_Cast.TransactionNumber;
			}

			return result;
		}

		protected override void CopyInstanceSpecificFieldsFrom(ApprovalRequestDetailsWithCharges<APInvoiceChargesApprovalRequestChargeDetails> postingRequestToCopy)
		{
			base.CopyInstanceSpecificFieldsFrom(postingRequestToCopy);

			var postingRequestToCopy_Cast = postingRequestToCopy as APInvoiceChargesApprovalRequestDetails;
			if (postingRequestToCopy_Cast != null)
			{
				Creditor = postingRequestToCopy_Cast.Creditor;
				TransactionNumber = postingRequestToCopy_Cast.TransactionNumber;
				requisitionStatus = postingRequestToCopy_Cast.RequisitionStatus;
				requisitionDate = postingRequestToCopy_Cast.RequisitionDate;
			}
		}

		protected override bool AreInstanceSpecificFieldsEqual(ApprovalRequestDetailsWithCharges<APInvoiceChargesApprovalRequestChargeDetails> b)
		{
			var result = base.AreInstanceSpecificFieldsEqual(b);

			if (result)
			{
				var b_Casted = b as APInvoiceChargesApprovalRequestDetails;
				if (b_Casted != null)
				{
					result =
						Creditor == b_Casted.Creditor &&
						TransactionNumber == b_Casted.TransactionNumber;
				}
			}

			return result;
		}

		#region IXmlSerializable Members

		protected override void ReadXmlForInstanceSpecificFields(XmlReader reader)
		{
			base.ReadXmlForInstanceSpecificFields(reader);

			Creditor = reader.ReadElementString(Schema.Creditor);
			TransactionNumber = reader.ReadElementString(Schema.TransactionNumber);

			if (reader.Name != Schema.DetailsXmlNode)
			{
				return;
			}

			var xElement = XElement.ReadFrom(reader) as XElement;
			SetValidValueHelper.SetStringIfValid(xElement.GetNodeValue("RequisitionStatus"), x => requisitionStatus = x); //obsolete value, for backward compatibility only.
			SetValidValueHelper.SetDateIfValid(xElement.GetNodeValue("RequisitionDate"), x => requisitionDate = x); //obsolete value, for backward compatibility only.
		}

		protected override void WriteXmlForInstanceSpecificFields(XmlWriter writer)
		{
			base.WriteXmlForInstanceSpecificFields(writer);

			writer.WriteElementString(Schema.Creditor, Creditor);
			writer.WriteElementString(Schema.TransactionNumber, TransactionNumber);
		}

		#endregion

		#endregion
	}
}
