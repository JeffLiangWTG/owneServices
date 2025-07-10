using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public class AdditionalInfoConverter
	{
		public virtual AdditionalInfo ConvertTaxInvoiceAdditionalInfo(TransactionInfo transactionInfo, AccEInvoicingBatch batch)
		{
			Argument.NotNull(transactionInfo, nameof(transactionInfo));
			Argument.NotNull(transactionInfo.Branch, nameof(TransactionInfo.Branch), "Could not determine the transaction branch");
			Argument.NotNull(batch, nameof(batch));

			var factory = batch.Factory;
			var invoicePK = new ZGuid(transactionInfo.ComplianceSubType);
			var invoice = factory.Load<InvoicingBase>(invoicePK)
						  ?? throw new InvalidOperationException("Could not find the transaction");

			var company = invoice.Company;
			if (company == null)
			{
				var branch = factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, transactionInfo.Branch.Code));
				company = branch?.Company;
			}

			var debtor = invoice.Header
				?? factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, transactionInfo.OrganizationAddress.OrganizationCode.ToString()));

			ITransactionInfoHelper transactionInfoHelper = new TransactionInfoHelper();

			var (invoiceeIdTypeCode, invoiceeId) = InvoiceeIDProvider.GetInvoiceeID(transactionInfo, transactionInfoHelper.GetRegistrationCode);
			var invoiceePrimaryDefinedContact = GetInvoiceePrimaryDefinedContact(invoice, debtor);
			var invoiceeSecondaryDefinedContact = GetInvoiceeSecondaryDefinedContact(invoice, debtor);
			var invoiceCreateUser = transactionInfo.CreateUser != null
				? factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, transactionInfo.CreateUser.Code))
				: null;

			var invoicerAddress = GetInvoicerPartyKoreanAddress(company);
			return new AdditionalInfo()
			{
				IssueID = GetIssueID(factory, invoice.PK),
				OriginalIssueID = invoice.AH_TransactionBelongsToGroup.IsValid
					? GetIssueID(factory, invoice.AH_TransactionBelongsToGroup)
					: default,
				AmendStatusCode = invoice.AH_Calc_AmendStatusCode,
				InvoiceeAlienRegistrationNo = transactionInfoHelper.GetRegistrationCode(transactionInfo.OrganizationAddress, CountryCodes.KoreaSouth, KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForForeigner),
				InvoiceePassportNo = transactionInfoHelper.GetRegistrationCode(transactionInfo.OrganizationAddress, CountryCodes.KoreaSouth, OrgCusCode.CodeTypes.PassportID),

				InvoiceeID = invoiceeId,
				InvoiceeTypeCode = transactionInfoHelper.GetRegistrationCode(transactionInfo.OrganizationAddress, CountryCodes.KoreaSouth, KoreaSouthComplianceInfo.CodeTypes.KBT),
				InvoiceeBusinessTypeCode = GetInvoiceeBusinessTypeCode(invoiceeIdTypeCode),
				InvoiceeClassificationCode = transactionInfoHelper.GetRegistrationCode(transactionInfo.OrganizationAddress, CountryCodes.KoreaSouth, KoreaSouthComplianceInfo.CodeTypes.KBC),
				InvoiceeNameText = invoice.InvoiceAddressOverride?.CompanyName ?? default,
				InvoiceeTaxRegistrationID = transactionInfoHelper.GetRegistrationCode(transactionInfo.OrganizationAddress, CountryCodes.KoreaSouth, KoreaSouthComplianceInfo.CodeTypes.OfficeID),
				InvoiceeSpecifiedPersonNameText = GetPersonNameText(debtor, true),
				InvoiceeSpecifiedAddressLineOneText = GetSpecifiedAddressLineOneText(factory, invoice.InvoiceAddressOverride, company),
				InvoiceePrimaryDefinedContactPersonName = invoiceePrimaryDefinedContact.PersonName,
				InvoiceePrimaryDefinedContactTel = invoiceePrimaryDefinedContact.Tel,
				InvoiceePrimaryDefinedContactURICommunication = invoiceePrimaryDefinedContact.URICommunication,
				InvoiceeSecondaryDefinedContactPersonName = invoiceeSecondaryDefinedContact.PersonName,
				InvoiceeSecondaryDefinedContactTel = invoiceeSecondaryDefinedContact.Tel,
				InvoiceeSecondaryDefinedContactURICommunication = invoiceeSecondaryDefinedContact.URICommunication,

				InvoicerID = company?.OrgProxy?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, CountryCodes.KoreaSouth) ?? default,
				InvoicerTypeCode = company?.OrgProxy?.CustomsCodes.GetCustomsRegNo(KoreaSouthComplianceInfo.CodeTypes.KBT, CountryCodes.KoreaSouth) ?? default,
				InvoicerClassificationCode = company?.OrgProxy?.CustomsCodes.GetCustomsRegNo(KoreaSouthComplianceInfo.CodeTypes.KBC, CountryCodes.KoreaSouth) ?? default,
				InvoicerNameText = invoicerAddress?.CompanyName ?? company?.OrgProxy?.OH_FullName ?? default,
				InvoicerTaxRegistrationID = company?.OrgProxy?.CustomsCodes.GetCustomsRegNo(KoreaSouthComplianceInfo.CodeTypes.OfficeID, CountryCodes.KoreaSouth) ?? default,
				InvoicerSpecifiedPersonNameText = GetPersonNameText(company?.OrgProxy, false),
				InvoicerSpecifiedAddressLineOneText = invoicerAddress != null
					? GetSpecifiedAddressLineOneText(factory, invoicerAddress, company)
					: GetSpecifiedAddressLineOneText(factory, company?.OrgProxy, company),
				InvoicerDefinedContactPersonName = invoiceCreateUser?.GS_FullName ?? default,
				InvoicerDefinedContactTel = invoiceCreateUser?.GS_WorkPhone ?? default,
				InvoicerDefinedContactURICommunication = invoiceCreateUser?.GS_EmailAddress ?? default,
				Lines = GetInvoiceLines(invoice),
				FullTypeCode = KoreaSouthEInvoicingHelper.GetTaxInvoiceDocumentTypeCode(
					invoice.AH_ComplianceSubType,
					transactionInfo.PostingJournalCollection?.Select(x => x.VATTaxID?.TaxType?.Code).WhereNotNull() ?? Array.Empty<ZString>(),
					transactionInfo.IsAmendment,
					transactionInfo.LocalVATAmount
				)
			};
		}

		ZString GetIssueID(BusinessObjectFactory factory, ZGuid invoicePK)
		{
			var query = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_AH, invoicePK)
				.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Type, AccTransactionHeaderReferenceTypes.KRI);

			return factory.LoadTop1<AccTransactionHeaderReference>(query)?.AH1_Reference ?? null;
		}

		OrgAddress GetInvoicerPartyKoreanAddress(GlbCompany comapny)
		{
			var addresses = comapny?.OrgProxy?.Addresses.OfType<OrgAddress>().Where(x => x.OA_Language == Languages.Korean);

			return addresses?.FirstOrDefault(x => x.AddressCapability.GetIsMainAddress(OrgAddressType.Office))
				?? addresses?.FirstOrDefault(x => x.IsAddressOfType(OrgAddressType.Office));
		}

		ZString GetPersonNameText(OrgHeader orgHeader, bool shouldContainKRS)
		{
			var orgContacts = orgHeader?.Contacts.OfType<OrgContact>();
			var personName = orgContacts?.FirstOrDefault(x => x.Allocations.OfType<OrgContactAllocation>().Any(y => y.PC_Type == ReadyKoreaConstants.KRC))?.OC_ContactName ?? default;
			if (shouldContainKRS)
			{
				var contactNameKRS = (orgContacts?.FirstOrDefault(x => x.Allocations.OfType<OrgContactAllocation>().Any(y => y.PC_Type == ReadyKoreaConstants.KRS))?.OC_ContactName ?? default);
				if (!contactNameKRS.IsEmpty)
				{
					personName = $"{personName}, {contactNameKRS}";
				}
			}

			return personName;
		}

		ZString GetSpecifiedAddressLineOneText(BusinessObjectFactory factory, OrgAddress address, GlbCompany comapny)
		{
			return address == null || comapny == null ? default : new AddressFormatter(factory, address, comapny, false).PostalAddressWithoutCompanyName();
		}

		ZString GetSpecifiedAddressLineOneText(BusinessObjectFactory factory, OrgHeader orgHeader, GlbCompany comapny)
		{
			return orgHeader == null || comapny == null ? default : new AddressFormatter(factory, orgHeader, comapny, false).PostalAddressWithoutCompanyName();
		}

		ZString GetInvoiceeBusinessTypeCode(ZString invoiceeIdTypeCode)
		{
			switch (invoiceeIdTypeCode)
			{
				case OrgCusCode.CodeTypes.VATCode:
					return ReadyKoreaConstants.InvoiceePartyBusinessTypeCode.RegistrationNumberOfBusinessOperator;
				case KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForResident:
					return ReadyKoreaConstants.InvoiceePartyBusinessTypeCode.RegistrationNumberOfResident;
				case KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForForeigner:
					return ReadyKoreaConstants.InvoiceePartyBusinessTypeCode.Foreigner;
				default:
					return default;
			}
		}

		List<TaxInvoiceTradeLineItem> GetInvoiceLines(InvoicingBase invoice)
		{
			var isNeedReSequence = invoice.Lines.Cast<InvoicingLineBase>().Any(x => x.AL_Sequence > 99);

			var sequenceIndex = 1;
			return invoice.Lines.Cast<InvoicingLineBase>().Select(x => new TaxInvoiceTradeLineItem()
			{
				JobNumber = x.Job?.JH_JobNum ?? ZString.Empty,
				Sequence = isNeedReSequence ? sequenceIndex++ : x.AL_Sequence,
				ReverseDate = x.AL_ReverseDate,
				InvoiceAmount = x.AL_LineAmount.ToStringTrimZeros(),
				CalculatedAmount = x.AL_GSTVAT.ToStringTrimZeros(),
				DescriptionText = x.AL_Desc
			}).ToList();
		}

		(ZString PersonName, ZString Tel, ZString URICommunication) GetInvoiceePrimaryDefinedContact(InvoicingBase invoice, OrgHeader debtor)
		{
			switch (AccountingMasterFilesRegistry.Instance.ExportMultipleDebtorOrganizationContactEmail.GetFallBackValueAtAllLevels(invoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				case ExportMultipleDebtorOrganizationContactEmailCodes.DEF:
					return GetContactInfo(invoice.InvoiceContactOverride);

				case ExportMultipleDebtorOrganizationContactEmailCodes.MAR:
					if (invoice.InvoiceContactOverride != null)
					{
						return GetContactInfo(invoice.InvoiceContactOverride);
					}

					if (GetAREmailContact(debtor, true) is var officialAREmailContact && officialAREmailContact != null)
					{
						return GetContactInfo(officialAREmailContact);
					}

					return GetContactInfo(null);

				default:
					throw new NotImplementedException("ExportMultipleDebtorOrganizationContactEmailCode is not supported.");
			}
		}

		(ZString PersonName, ZString Tel, ZString URICommunication) GetInvoiceeSecondaryDefinedContact(InvoicingBase invoice, OrgHeader debtor)
		{
			switch (AccountingMasterFilesRegistry.Instance.ExportMultipleDebtorOrganizationContactEmail.GetFallBackValueAtAllLevels(invoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				case ExportMultipleDebtorOrganizationContactEmailCodes.DEF:
					return GetContactInfo(null);

				case ExportMultipleDebtorOrganizationContactEmailCodes.MAR:
					if (invoice.InvoiceContactOverride == null &&
						GetAREmailContact(debtor, true) is var officialAREmailContact && officialAREmailContact != null &&
						GetAREmailContact(debtor, false) is var unofficialAREmailContact && unofficialAREmailContact != null)
					{
						return GetContactInfo(unofficialAREmailContact);
					}

					return GetContactInfo(null);

				default:
					throw new NotImplementedException("ExportMultipleDebtorOrganizationContactEmailCode is not supported.");
			}
		}

		(ZString PersonName, ZString Tel, ZString URICommunication) GetContactInfo(OrgContact contact) => (contact?.OC_ContactName ?? default, contact?.OC_Phone ?? default, contact?.OC_Email ?? default);

		OrgContact GetAREmailContact(OrgHeader debtor, bool isOfficial)
		{
			var contacts = debtor?.ContactsActive.Cast<OrgContact>();
			return contacts.FirstOrDefault(contact => contact.Documents
				.Cast<OrgDocument>()
				.Any(doc =>
					doc.OD_DocumentGroup == ContactType.Receivables.Code &&
					doc.OD_DeliverBy == Core.Constants.ContactNotifyModes.Email &&
					doc.OD_DefaultContact == isOfficial
			));
		}
	}
}
