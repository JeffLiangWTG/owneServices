using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Schema;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Tools.Telephony;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.CustomerService.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor
{
	public class ServiceRequestDataAdapter : ValueObjectDataAdapter<SupportIncident, Xsd.CustomerServiceRequest>
	{
		#region ValueObjectDataAdapter Overrides

		public override string RootCollectionElementName
		{
			get { return null; }
		}

		public override string RootElementName
		{
			get { return "CustomerServiceRequest"; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return null; }
		}

		public override XmlSchema Schema
		{
			get { return null; }
		}

		#endregion

		#region Import

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void ImportFromValueObjectCore(SupportIncident bizObj, Xsd.CustomerServiceRequest value, IValueObjectImportContext context)
		{
			LicenceHeader clientLicence = LicenceHeader.LoadFromLicenceCode(context.Factory, value.LicenceCode);

			LicenceDatabase database = bizObj.Factory.LoadTop1<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, value.DatabaseNumber));
			if (clientLicence != null)
			{
				if (database != null)
				{
					if (clientLicence.LA_LD != database.PK)
					{
						// Mismatched DB. LD_DatabaseNumber wins
						clientLicence = null;
					}
				}
				else
				{
					database = clientLicence.Database;
				}
			}

			if (database != null)
			{
				bizObj.IM_LD = database.PK;
			}

			//Set client company
			ClientCompany clientCompany = null;
			if (value.Product == ProductTypes.Codes.Enterprise ||
				value.Product == ProductTypes.Codes.CargoWiseOne ||
				value.Product == ProductTypes.Codes.ProductivityWise ||
				value.Product == ProductTypes.Codes.GLOW ||
				value.Product.IsEmpty)
			{
				if (database != null)
				{
					clientCompany = LoadOrCreateClientCompany(bizObj, value, database, ZGuid.Empty);
				}

				if (clientCompany == null && clientLicence != null)
				{
					clientCompany = clientLicence.ClientCompany;
					if (clientCompany == null)
					{
						clientCompany = LoadOrCreateClientCompany(bizObj, value, clientLicence.Database, clientLicence.Company.Header.PK);
					}
					else if (!clientCompany.LCC_DeactivateTimeUtc.IsEmpty)
					{
						//Client company with matched org pk is inactive, do not match on org pk again
						clientCompany = LoadOrCreateClientCompany(bizObj, value, clientLicence.Database, ZGuid.Empty);
					}
				}

				if (clientCompany != null)
				{
					bizObj.IM_LCC = clientCompany.PK;
				}
			}

			if (clientCompany == null && clientLicence == null && database == null)
			{
				bizObj.AddInternalSystemLogMessage(string.Format(CultureInfo.CurrentCulture, "This incident was reported by an unknown installation with licence code {0} / database number {1}", value.LicenceCode, value.DatabaseNumber));
			}

			//Set client org
			OrgHeader org = null;
			if (clientLicence != null)
			{
				org = clientLicence.Company.Header;
			}
			else if (clientCompany != null && clientCompany.Org != null)
			{
				org = clientCompany.Org;
			}
			else if (database != null)
			{
				org = database.LicEnterprise.Header;
			}

			if (org != null)
			{
				if (bizObj.Client == null)
				{
					bizObj.SetClientOnly(org);
				}

				var approvingContact = ImportContact(value, context, org, database);
				var csvContact = FindMatchingActiveContact(org, database.LicEnterprise, value, approvingContact);

				if (csvContact != null)
				{
					if (!csvContact.OC_IsActive)
					{
						csvContact.OC_IsActive = true;
					}
					bizObj.IM_OC_Contact = csvContact.PK;
				}
			}

			// Set values on Incident
			string incidentReportingLog = string.Format(CultureInfo.CurrentCulture, "This incident was reported by {0} ({1}) and approved by {2} ({3})", value.ReportingStaffMemberName, value.ReportingStaffEmail, value.ApprovingUser, value.ApprovingUserEmail);
			bizObj.AddPublicSystemLogMessage(incidentReportingLog);
			context.SetPropertyInfoValue(bizObj.IM_ClientIncidentReferenceInfo, value.ClientReferenceNumber);
			context.SetPropertyInfoValue(bizObj.IM_DescriptionInfo, value.IncidentSummary);
			context.SetPropertyInfoValue(bizObj.DetailNoteTextInfo, value.IncidentDetails);
			context.SetPropertyInfoValue(bizObj.IM_ProductInfo, value.Product.IsEmpty ? ProductTypes.Codes.Enterprise : value.Product.ToString());
			context.SetPropertyInfoValue(bizObj.IM_ModuleInfo, value.Module);

			var language = value.Language.IsEmpty ? Core.SharedConstants.Languages.English : value.Language.ToString();
			if (UpdateLanguageCodeToIsoCodesHelper.LanguageCodeMapping.ContainsKey(language))
			{
				language = UpdateLanguageCodeToIsoCodesHelper.LanguageCodeMapping[language];
			}
			context.SetPropertyInfoValue(bizObj.IM_LanguageInfo, language);

			if (IncidentApprovalLookups.GetModuleListType(value.Criticality) == ModuleListType.MenuSection)
			{
				context.SetPropertyInfoValue(bizObj.IM_SourceModuleIdInfo, value.ActiveModuleIdSpecified ? value.ActiveModuleId : value.Module);
			}
			context.SetPropertyInfoValue(bizObj.IM_PriorityInfo, value.Criticality);

			foreach (Xsd.CustomerServiceRequestAttachment attachment in value.Attachments)
			{
				attachment.FileName = MakeFilenameSafe.MakeSafe(attachment.FileName);
				if (attachment.Data != null && attachment.Data.Length > 0)
				{
					bizObj.DocManagerInfo.AddFileOrDocument(attachment.Data, attachment.FileName, attachment.DocType.IsEmpty ? "COR" : attachment.DocType, description: attachment.Desc);
				}
				// Since 20 Feb 2013, Work Item WI00040647, this code does not drop the filename from image files
			}

			((IDocManagerSupport)bizObj).DocManagerInfo.MasterFactory.Save();
		}

		ClientCompany LoadOrCreateClientCompany(SupportIncident bizObj, Xsd.CustomerServiceRequest value, LicenceDatabase database, ZGuid orgPK)
		{
			ClientCompany clientCompany = null;

			if (!value.CompanyCode.IsEmpty && database != null)
			{
				clientCompany = ClientCompany.FindOrCreate(bizObj.Factory, value.CompanyCode, database.PK, orgPK, value.CompanyName, value.CompanyCountry);
			}

			return clientCompany;
		}

		Xsd.OrgContact ImportContact(Xsd.CustomerServiceRequest value, IValueObjectImportContext context, OrgHeader org, LicenceDatabase database)
		{
			var approvingContact = ProcessStaffInfo(value);
			if (approvingContact != null)
			{
				var helper = new SupportRequestContactValueObjectHelper("Customer Service Request");
				helper.ImportFromValueObject(org, approvingContact, context, true);
			}
			return approvingContact;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		Xsd.OrgContact ProcessStaffInfo(Xsd.CustomerServiceRequest value)
		{
			Xsd.OrgContact approvingContact = null;

			var viewDeniedString = "** VIEW DENIED";
			var pattern = "[^0-9() +-]";

			foreach (Xsd.OrgContact staffItem in value.Staff)
			{
				if (staffItem.HomePhone.Contains(viewDeniedString, StringComparison.OrdinalIgnoreCase) || Regex.IsMatch(staffItem.HomePhone, pattern))
				{
					staffItem.HomePhone = ZString.Empty;
				}

				if (staffItem.Mobile.ToUpper().Contains(viewDeniedString, StringComparison.OrdinalIgnoreCase) || Regex.IsMatch(staffItem.Mobile, pattern))
				{
					staffItem.Mobile = ZString.Empty;
				}

				if (staffItem.Phone.ToUpper().Contains(viewDeniedString, StringComparison.OrdinalIgnoreCase) || Regex.IsMatch(staffItem.Phone, pattern))
				{
					staffItem.Phone = ZString.Empty;
				}

				if (staffItem.OtherPhone.ToUpper().Contains(viewDeniedString, StringComparison.OrdinalIgnoreCase) || Regex.IsMatch(staffItem.OtherPhone, pattern))
				{
					staffItem.OtherPhone = ZString.Empty;
				}

				if (staffItem.Fax.ToUpper().Contains(viewDeniedString, StringComparison.OrdinalIgnoreCase) || Regex.IsMatch(staffItem.Fax, pattern))
				{
					staffItem.Fax = ZString.Empty;
				}

				if (staffItem.EmailAddress.ToUpper().Contains(viewDeniedString, StringComparison.OrdinalIgnoreCase))
				{
					staffItem.EmailAddress = ZString.Empty;
				}

				staffItem.Phone = HasCountryCode(staffItem.Phone) ? staffItem.Phone : ZString.Empty;
				staffItem.Mobile = HasCountryCode(staffItem.Mobile) ? staffItem.Mobile : ZString.Empty;
				staffItem.HomePhone = HasCountryCode(staffItem.HomePhone) ? staffItem.HomePhone : ZString.Empty;
				staffItem.Pager = HasCountryCode(staffItem.Pager) ? staffItem.Pager : ZString.Empty;
				staffItem.OtherPhone = HasCountryCode(staffItem.OtherPhone) ? staffItem.OtherPhone : ZString.Empty;
				staffItem.Fax = HasCountryCode(staffItem.Fax) ? staffItem.Fax : ZString.Empty;
				staffItem.PhoneExtension = string.IsNullOrEmpty(staffItem.Phone) ? ZString.Empty : staffItem.PhoneExtension;
				if (staffItem.EmailAddress.IsEmpty)
				{
					staffItem.WebAccessEnable = false;
				}

				if (approvingContact == null)
				{
					if (value.ApprovingUserEmail.IsEmpty)
					{
						if (staffItem.Name.EqualsIgnoringCase(value.ApprovingUser) || staffItem.EmailAddress.EqualsIgnoringCase(value.ApprovingUser))
						{
							approvingContact = staffItem;
						}
					}
					else
					{
						if (staffItem.Name.EqualsIgnoringCase(value.ApprovingUser) && staffItem.EmailAddress.EqualsIgnoringCase(value.ApprovingUserEmail))
						{
							approvingContact = staffItem;
						}
					}
				}
			}
			return approvingContact;
		}

		bool HasCountryCode(ZString phoneNumber)
		{
			bool hasCountryCode = false;
			PhoneInfo info = TelephoneNumberDialing.FindPhoneInfoRow(phoneNumber);
			TelephoneNumberDialing dialler = new TelephoneNumberDialing("", "", "");
			ZString phone = dialler.ConvertToDialingDigits(phoneNumber).Replace("+", "");

			if (info != null)
			{
				if (info.LocalNumberLength > 0 && info.MinAreaCodeLength > 0 && info.MaxAreaCodeLength > 0)
				{
					int minPhoneNumberLength = info.LocalNumberLength + info.CountryDialingCode.Length + info.MinAreaCodeLength;
					int maxPhoneNumberLength = info.LocalNumberLength + info.CountryDialingCode.Length + info.MaxAreaCodeLength;
					hasCountryCode = (phone.Length >= minPhoneNumberLength && phone.Length <= maxPhoneNumberLength);
				}
				else
				{
					hasCountryCode = phoneNumber.StartsWith("+", StringComparison.Ordinal);
				}
			}
			return hasCountryCode;
		}

		OrgContact FindMatchingActiveContact(OrgHeader org, LicenceEnterprise licenceEnterprise, Xsd.CustomerServiceRequest serviceRequest, Xsd.OrgContact approvingContact)
		{
			ZString contactNameToMatch = serviceRequest.ApprovingUser;
			ZString emailToMatch = approvingContact != null ? approvingContact.EmailAddress : ZString.Empty;

			var result = FindFirstActiveContact(org, contactNameToMatch, emailToMatch);

			if (result == null && !emailToMatch.IsEmpty)
			{
				result = FindFirstExactMatchContact(org, contactNameToMatch, emailToMatch);
			}

			if (result == null)
			{
				result = FindFirstActiveContact(licenceEnterprise.Header, contactNameToMatch, emailToMatch);
			}

			if (result == null)
			{
				foreach (LicenceCompany licCompany in licenceEnterprise.Companies)
				{
					result = FindFirstActiveContact(licCompany.Header, contactNameToMatch, emailToMatch);
					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}

		OrgContact FindFirstActiveContact(OrgHeader orgHeader, ZString contactNameToMatch, ZString emailToMatch)
		{
			OrgContact result;

			if (emailToMatch.IsEmpty)
			{
				var nameMatchingQuery = new ZQuery(OrgContactSchema.OC_ContactName, contactNameToMatch);
				result = orgHeader.Contacts.Find(nameMatchingQuery).Cast<OrgContact>().FirstOrDefault(c => c.OC_IsActive);
			}
			else
			{
				var nameAndEmailMatchingQuery = new ZQuery(OrgContactSchema.OC_ContactName, contactNameToMatch);
				nameAndEmailMatchingQuery.AddToFilter(OrgContactSchema.OC_Email, emailToMatch);
				result = orgHeader.Contacts.Find(nameAndEmailMatchingQuery).Cast<OrgContact>().FirstOrDefault(c => c.OC_IsActive);

				if (result == null)
				{
					var emailMatchingQuery = new ZQuery(OrgContactSchema.OC_Email, emailToMatch);
					result = orgHeader.Contacts.Find(emailMatchingQuery).Cast<OrgContact>().FirstOrDefault(c => c.OC_IsActive);
				}

				if (result == null)
				{
					var nameMatchingQuery = new ZQuery(OrgContactSchema.OC_ContactName, contactNameToMatch);
					result = orgHeader.Contacts.Find(nameMatchingQuery).Cast<OrgContact>().FirstOrDefault(c => c.OC_IsActive);
				}
			}

			if (result == null)
			{
				var emailAsUserNameMatchingQuery = new ZQuery(OrgContactSchema.OC_Email, contactNameToMatch);
				result = orgHeader.Contacts.Find(emailAsUserNameMatchingQuery).Cast<OrgContact>().FirstOrDefault(c => c.OC_IsActive);
			}

			return result;
		}

		OrgContact FindFirstExactMatchContact(OrgHeader orgHeader, ZString contactNameToMatch, ZString emailToMatch)
		{
			var nameAndEmailMatchingQuery = new ZQuery(OrgContactSchema.OC_ContactName, contactNameToMatch);
			nameAndEmailMatchingQuery.AddToFilter(OrgContactSchema.OC_Email, emailToMatch);
			return orgHeader.Contacts.Find(nameAndEmailMatchingQuery).Cast<OrgContact>().FirstOrDefault();
		}

		#endregion

		#region Not Implemented

		protected override SupportIncident FindBusinessObject(Xsd.CustomerServiceRequest value, IValueObjectImportContext context)
		{
			throw new NotSupportedException("Not required function");
		}

		protected override void ExportToValueObjectCore(SupportIncident bizObj, Xsd.CustomerServiceRequest constructedValueObject, IValueObjectExportContext context)
		{
			throw new NotSupportedException("Not required function");
		}

		#endregion
	}
}

