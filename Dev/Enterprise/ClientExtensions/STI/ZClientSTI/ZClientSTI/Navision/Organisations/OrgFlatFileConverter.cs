using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.STI.Navision
{
	public class OrgFlatFileConverter : FlatFileConverter
	{
		public OrgFlatFileConverter(INotifications notifications, BusinessObjectFactory factory)
			: base(notifications, factory)
		{
		}

		protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
		{
			FlatFileDataRowCollection dataRows = new FlatFileDataRowCollection();
			OrgFlatFileDataRow row = new OrgFlatFileDataRow();
			Xsd.Organisation org = (Xsd.Organisation)valueObject;

			row.OrgCode = org.EDICode;
			row.OrgName = org.OrganisationDetails.Name;
			row.SearchName = org.OrganisationDetails.Name;
			row.HomePage = !org.OrganisationDetails.WebAddress.IsEmpty ? org.OrganisationDetails.WebAddress : org.OrganisationDetails.OrgWebURLs.IsSpecified ? org.OrganisationDetails.OrgWebURLs[0].URL : ZString.Empty;

			Xsd.OrgAddress address = org.OrganisationDetails.Addresses.GetMainOrFirstAddress();
			if (address != null)
			{
				row.Address = address.AddressLine1;
				row.Address2 = address.AddressLine2;
				row.City = address.CityOrSuburb;
				row.PhoneNumber = address.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Business);
				row.CountryCode = address.Location.Value.Left(2);
				row.FaxNumber = address.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Fax);
				row.PostCode = address.PostCode;
				row.County = address.StateOrProvince;
				row.Email = address.Email;
			}

			OrgHeader bizOrg = (OrgHeader)Factory.LoadFromUniqueKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, org.EDICode);
			SetFieldsFromBusinessObject(row, bizOrg);
			dataRows.Add(row);

			return dataRows;
		}

		#region Implementation

		void SetFieldsFromBusinessObject(OrgFlatFileDataRow row, OrgHeader org)
		{
			if (org != null)
			{
				row.CreditLimit = org.MiscServ.OM_ARCreditLimit;
				row.ShipmentMethodCode = org.MiscServ.OM_EXDefaultIncoTerm;
				row.GSTBusinessPostingGroup = org.CompanyData.IsARTaxApplicable ? "GST" : "No GST";
				row.CustomerPostingGroup = GetDebtorGroupCode(org.MiscServ.OM_OJ_ARDebtorGroup);

				OrgHeader settlementGroup = org.ARSettlementGroup ?? org;
				if (settlementGroup != null)
				{
					row.BillToCustomerNumber = GetSettlementGroupCode(settlementGroup.PK);
				}

				var arTerms = org.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty);
				row.PaymentTermsCode = arTerms.Term + arTerms.Days;
				row.SalesPersonCode = GetResponsiblePerson(org.StaffAssignments);
				row.Blocked = org.OH_IsActive ? "" : "All";
				row.ABN = org.PrimaryRegistrationNumber.Number;
				if (org.Branch != null)
				{
					row.GlobalDimension1Code = org.Branch.GB_Code;
				}
			}
		}

		ZString GetDebtorGroupCode(ZGuid debtorGroupGuid)
		{
			ZString result = ZString.Empty;
			OrgDebtorGroup debtorGroup = (OrgDebtorGroup)Factory.Load(typeof(OrgDebtorGroup), debtorGroupGuid);
			if (debtorGroup != null)
			{
				result = debtorGroup.OJ_Code;
			}
			return result;
		}

		ZString GetResponsiblePerson(OrgStaffAssignmentsCollection staffAssignments)
		{
			ZString result = ZString.Empty;
			foreach (OrgStaffAssignments staffAssignment in staffAssignments)
			{
				if (staffAssignment.O8_Role == StaffAssignmentRoles.Codes.SalesRep)
				{
					result = staffAssignment.PersonResponsible.GS_FullName;
					break;
				}
			}
			return result;
		}

		ZString GetSettlementGroupCode(ZGuid settlementGroupPK)
		{
			ZString result = ZString.Empty;
			OrgHeader settlementGroup = (OrgHeader)Factory.Load(typeof(OrgHeader), settlementGroupPK);
			if (settlementGroup != null)
			{
				result = settlementGroup.OH_Code;
			}
			return result;
		}

		#endregion
	}
}
