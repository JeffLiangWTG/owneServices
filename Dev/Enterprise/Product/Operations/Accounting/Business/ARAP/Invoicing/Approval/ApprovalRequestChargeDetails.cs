using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ApprovalRequestChargeDetails : NonPersistentBusinessObject, IObsoleteValidation, IXmlSerializable
	{
		#region Schema

		public abstract class Schema
		{
			public const string JobNumber = "JobNumber";
			public const string ChargeCode = "ChargeCode";
			public const string Branch = "Branch";
			public const string Department = "Department";
			public const string PlaceOfSupply = "PlaceOfSupply";
			public const string PlaceOfSupplyType = "PlaceOfSupplyType";
			public const string Description = "Description";
			public const string AccInvMsgPK = "AccInvMsgPK";
			public const string TaxDate = "TaxDate";
		}

		#endregion

		public ApprovalRequestChargeDetails(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void CopyFrom(ApprovalRequestChargeDetails chargeDetailsToCopy)
		{
			JobNumber = chargeDetailsToCopy.JobNumber;
			ChargeCode = chargeDetailsToCopy.ChargeCode;
			Branch = chargeDetailsToCopy.Branch;
			Department = chargeDetailsToCopy.Department;
			PlaceOfSupply = chargeDetailsToCopy.PlaceOfSupply;
			PlaceOfSupplyType = chargeDetailsToCopy.PlaceOfSupplyType;

			if (!chargeDetailsToCopy.Description.IsEmpty)
			{
				Description = chargeDetailsToCopy.Description;
			}
			if (!chargeDetailsToCopy.TaxDate.IsEmpty)
			{
				TaxDate = chargeDetailsToCopy.TaxDate;
			}
			AccInvMsgPK = chargeDetailsToCopy.AccInvMsgPK;
			CopyInstanceSpecificFieldsFrom(chargeDetailsToCopy);
		}

		protected virtual void CopyInstanceSpecificFieldsFrom(ApprovalRequestChargeDetails chargeDetailsToCopy)
		{
		}

		public static bool operator ==(ApprovalRequestChargeDetails a, ApprovalRequestChargeDetails b)
		{
			if (((object)a) == null && ((object)b) == null)
			{
				return true;
			}

			if (((object)a) == null || ((object)b) == null)
			{
				return false;
			}

			//Description, AccInvMsgPK and TaxDate properties are not checked in the IsEqual otherwise the AutoPosting process is failing with the error: "Can't post this request because source details have been modified since then"
			//In LevelAuthorizationWithApprovalRequest.PerformTransactionLevelAuthorization() it checks that requests are equals but the auto posting modify automatically some fields from the transaction or line. So I decided to not include any new fields.

			return
				a.JobNumber == b.JobNumber &&
				a.ChargeCode == b.ChargeCode &&
				a.Branch == b.Branch &&
				a.Department == b.Department &&
				a.PlaceOfSupply == b.PlaceOfSupply &&
				a.placeOfSupplyType == b.placeOfSupplyType &&
				a.AreInstanceSpecificFieldsEqual(b);
		}

		protected virtual bool AreInstanceSpecificFieldsEqual(ApprovalRequestChargeDetails b)
		{
			return true;
		}

		public static bool operator !=(ApprovalRequestChargeDetails a, ApprovalRequestChargeDetails b)
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

		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		#region JobNumber

		[ResourceStringData("ARCreditNoteApprovalChargeDetails|JobNumber", Caption = "Job Number")]
		public ZString JobNumber
		{
			get { return JobNumber_cached; }
			set
			{
				SetNonPersistentPropertyValue(JobNumberInfo, ref JobNumber_cached, value);
			}
		}
		ZString JobNumber_cached;

		ZPropertyInfo JobNumberInfo
		{
			get { return GetZPropertyInfo(Schema.JobNumber); }
		}

		#endregion

		#region ChargeCode

		[ResourceStringData("ARCreditNoteApprovalChargeDetails|ChargeCode", Caption = "Charge Code")]
		public ZString ChargeCode
		{
			get { return ChargeCode_cached; }
			set
			{
				SetNonPersistentPropertyValue(ChargeCodeInfo, ref ChargeCode_cached, value);
			}
		}
		ZString ChargeCode_cached;

		ZPropertyInfo ChargeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeCode); }
		}

		#endregion

		#region Branch

		[ResourceStringData("ARCreditNoteApprovalChargeDetails|Branch", Caption = "Branch")]
		public ZString Branch
		{
			get { return Branch_cached; }
			set
			{
				SetNonPersistentPropertyValue(BranchInfo, ref Branch_cached, value);
			}
		}
		ZString Branch_cached;

		ZPropertyInfo BranchInfo
		{
			get { return GetZPropertyInfo(Schema.Branch); }
		}

		#endregion

		#region Department

		[ResourceStringData("ARCreditNoteApprovalChargeDetails|Department", Caption = "Department")]
		public ZString Department
		{
			get { return Department_cached; }
			set
			{
				SetNonPersistentPropertyValue(DepartmentInfo, ref Department_cached, value);
			}
		}
		ZString Department_cached;

		ZPropertyInfo DepartmentInfo
		{
			get { return GetZPropertyInfo(Schema.Department); }
		}

		#endregion

		#region PlaceOfSupply

		[ResourceStringData("ARCreditNoteApprovalChargeDetails|FPOS", Caption = "FPOS")]
		public ZString PlaceOfSupply
		{
			get { return placeOfSupply; }
			set { SetNonPersistentPropertyValue(PlaceOfSupplyInfo, ref placeOfSupply, value); }
		}
		ZString placeOfSupply;

		public ZPropertyInfo PlaceOfSupplyInfo => GetZPropertyInfo(Schema.PlaceOfSupply);

		#endregion

		#region PlaceOfSupplyType

		public ZString PlaceOfSupplyType
		{
			get { return placeOfSupplyType; }
			set { SetNonPersistentPropertyValue(PlaceOfSupplyTypeInfo, ref placeOfSupplyType, value); }
		}
		ZString placeOfSupplyType;

		public ZPropertyInfo PlaceOfSupplyTypeInfo => GetZPropertyInfo(Schema.PlaceOfSupplyType);

		#endregion

		#region Description

		[ResourceStringData("ApprovalRequestChargeDetails|Description", Caption = "Description", ShortCaption = "Desc.")]
		public ZString Description
		{
			get { return description; }
			set { SetNonPersistentPropertyValue(DescriptionInfo, ref description, value); }
		}
		ZString description;

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(Schema.Description);

		#endregion

		#region AccInvMsgPK

		[ResourceStringData("ApprovalRequestChargeDetails|AccInvMsgPK", Caption = "Tax Message")]
		public ZGuid AccInvMsgPK
		{
			get { return accInvMsgPK; }
			set { SetNonPersistentPropertyValue(AccInvMsgPKeInfo, ref accInvMsgPK, value); }
		}
		ZGuid accInvMsgPK;

		public ZPropertyInfo AccInvMsgPKeInfo => GetZPropertyInfo(Schema.AccInvMsgPK);

		#endregion

		#region TaxDate

		[ResourceStringData("ApprovalRequestChargeDetails|TaxDate", Caption = "Tax Date")]
		public ZDate TaxDate
		{
			get { return taxDate; }
			set { SetNonPersistentPropertyValue(TaxDateInfo, ref taxDate, value); }
		}
		ZDate taxDate;

		public ZPropertyInfo TaxDateInfo => GetZPropertyInfo(Schema.TaxDate);

		#endregion

		#region IXmlSerializable Members

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			var xElement = XNode.ReadFrom(reader) as XElement;
			xElement.TryToSetValueFromXElelment<ZString>(Schema.JobNumber, x => JobNumber = x);
			xElement.TryToSetValueFromXElelment<ZString>(Schema.ChargeCode, x => ChargeCode = x);
			xElement.TryToSetValueFromXElelment<ZString>(Schema.Branch, x => Branch = x);
			xElement.TryToSetValueFromXElelment<ZString>(Schema.Department, x => Department = x);
			xElement.TryToSetValueFromXElelment<ZString>(Schema.PlaceOfSupply, x => PlaceOfSupply = x);
			xElement.TryToSetValueFromXElelment<ZString>(Schema.PlaceOfSupplyType, x => PlaceOfSupplyType = x);
			xElement.TryToSetValueFromXElelment<ZString>(Schema.Description, x => Description = x);
			xElement.TryToSetValueFromXElelment<ZGuid>(Schema.AccInvMsgPK, x => AccInvMsgPK = x);
			xElement.TryToSetValueFromXElelment<ZDateTime>(Schema.TaxDate, x => TaxDate = x.Date);
			ReadXmlForInstanceSpecificFields(xElement);
		}

		protected virtual void ReadXmlForInstanceSpecificFields(XElement element)
		{
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteElementString(Schema.JobNumber, JobNumber);
			writer.WriteElementString(Schema.ChargeCode, ChargeCode);
			writer.WriteElementString(Schema.Branch, Branch);
			writer.WriteElementString(Schema.Department, Department);
			if (!PlaceOfSupply.IsEmpty)
			{
				writer.WriteElementString(Schema.PlaceOfSupply, PlaceOfSupply);
				writer.WriteElementString(Schema.PlaceOfSupplyType, PlaceOfSupplyType);
			}
			writer.WriteElementString(Schema.Description, Description);
			writer.WriteElementString(Schema.AccInvMsgPK, AccInvMsgPK.ToString());
			writer.WriteElementString(Schema.TaxDate, TaxDate.ToString());
			WriteXmlForInstanceSpecificFields(writer);
		}

		protected virtual void WriteXmlForInstanceSpecificFields(XmlWriter writer)
		{
		}

		#endregion
	}
}
