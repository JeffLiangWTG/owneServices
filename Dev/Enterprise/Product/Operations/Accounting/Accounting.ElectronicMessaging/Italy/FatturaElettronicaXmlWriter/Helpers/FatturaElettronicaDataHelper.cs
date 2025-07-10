using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using OrganizationAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using XmlElementCreator = Enterprise.Accounting.Business.EInvoicing.FatturaElettronicaXmlElementCreator;

namespace Enterprise.Accounting.ElectronicMessaging.Italy
{
	public static class FatturaElettronicaDataHelper
	{
		internal static RegistrationNumber GetItalyRegistrationNumber(IEnumerable<RegistrationNumber> regNumbers, ZString registrationCode, TransactionInfo transaction = null)
		{
			var numbers = regNumbers?.Where(x =>
			x.Type.Code.HasValue && x.Type.Code.Value == registrationCode &&
			x.CountryOfIssue.Code.HasValue && x.CountryOfIssue.Code.Value == Core.Constants.CountryCodes.Italy);

			if (numbers != null)
			{
				var count = numbers.Count();
				if (count == 1)
				{
					return numbers.First();
				}
				else if (count > 1)
				{
					return GetItalyRegistrationNumberCore(numbers, registrationCode, transaction);
				}
			}

			return null;
		}

		static RegistrationNumber GetItalyRegistrationNumberCore(IEnumerable<RegistrationNumber> numbers, ZString registrationCode, TransactionInfo transaction)
		{
			var query = new ZDBOnlyQuery(typeof(OrgCusCode));
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, registrationCode);
			query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Italy);
			query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, numbers.Select(x => x.Value));

			var addressSubQuery = new ZDBOnlyQuery(typeof(OrgAddress));
			var addressCode = transaction?.OrganizationAddress?.AddressShortCode;
			if (addressCode.HasValue)
			{
				var sql = FormattableString.Invariant($@"
OK_OA_PremisesAddress IN 
	(
		SELECT OA_PK FROM dbo.OrgAddress WHERE OA_Code = @OA_Code
	)
	OR
	OK_OA_PremisesAddress is NULL
");
				var parameters = new ZSqlParameterCollection(ZSqlParameter.New("@OA_Code", addressCode, OrgAddressSchema.OA_Code));
				addressSubQuery.AddFilterAndZSQLParameterCollection(sql, parameters);
			}
			else
			{
				addressSubQuery.AddToFilter(JoinCondition.And, OrgCusCodeSchema.OK_OA_PremisesAddress, null);
			}

			query.AddToFilter(addressSubQuery, JoinCondition.And);

			var factory = new BusinessObjectFactory();
			var cusCodes = factory.Load<OrgCusCode>(query);

			RegistrationNumber result = null;
			if (cusCodes != null && cusCodes.Length > 0)
			{
				result = numbers.First(x => x.Value.Value == cusCodes.OrderByDescending(y => y.OK_OA_PremisesAddress).First().OK_CustomsRegNo);
			}
			else
			{
				result = numbers.First();
			}
			return result;
		}

		internal static ZString GetTransmissionFormat(ZString orgCategory, ZString orgCountry)
		{
			return (orgCategory == OrgConstants.Category.Government && orgCountry == Core.Constants.CountryCodes.Italy) ? "FPA12" : "FPR12";
		}

		internal static bool IsOrganizationCategoryNAT(this OrganizationAddress transactionAddress, BusinessObjectFactory factory)
		{
			return transactionAddress.GetOrgHeader(factory).GetCategory() == OrgConstants.Category.NaturalPersonIndividual;
		}

		internal static OrgHeader GetOrgHeader(this OrganizationAddress transactionAddress, BusinessObjectFactory factory)
		{
			OrgHeader result = null;
			var orgCode = transactionAddress?.OrganizationCode?.SourceValue;
			if (orgCode.HasValue)
			{
				result = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, orgCode.Value));
			}
			return result;
		}

		internal static ZString GetCategory(this OrgHeader orgHeader) => orgHeader?.OH_Category ?? ZString.Empty;

		internal static bool IsRegNumberDifferentThanCodiceFiscale(TransactionInfo transaction, OrgHeader orgHeader)
		{
			var idCodice = ZString.Empty;

			if (orgHeader != null)
			{
				idCodice = orgHeader.GetCountryCodeAndTaxRegistrationWithoutPrefix(Core.Constants.CountryCodes.Italy).registrationNumber;
				var codRegistrationNumber = GetItalyRegistrationNumber(transaction.OrganizationAddress?.RegistrationNumberCollection, ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale);
				var codiceFiscale = codRegistrationNumber?.Value ?? ZString.Empty;

				return (!idCodice.IsEmpty && !codiceFiscale.IsEmpty && !idCodice.Equals(codiceFiscale));
			}

			return false;
		}

		public static ZString getIssuerRegistrationNumber(TransactionInfo transaction)
		{
			RegistrationNumber idTrasmitter;
			var regNumbers = transaction.BranchAddress?.RegistrationNumberCollection;
			var numOfIVA = FatturaElettronicaDataHelper.GetItalyRegistrationNumber(regNumbers, OrgCusCode.CodeTypes.IVA);
			if (numOfIVA != null)
			{
				idTrasmitter = numOfIVA;
			}
			else
			{
				var numOfCOD = FatturaElettronicaDataHelper.GetItalyRegistrationNumber(regNumbers, ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale);
				idTrasmitter = numOfCOD;
			}

			var tempIdTrasmitter = idTrasmitter == null ? ZString.Empty : ((ZString)idTrasmitter.Value);
			if (tempIdTrasmitter.Length == 11)
			{
				var fileNameSuffix = IsReceivable(transaction) ? FileNameSuffixForAR
									: IsPayable(transaction) ? FileNameSuffixForAP : string.Empty;

				return Core.Constants.CountryCodes.Italy + tempIdTrasmitter + fileNameSuffix;
			}
			else
			{
				return Core.Constants.CountryCodes.Italy + tempIdTrasmitter;
			}
		}

		#region DatiAnagrafici Based on Login Company

		public static XStreamingElement BuildXmlForDatiAnagraficiBasedOnLoginCompany(TransactionInfo transaction)
		{
			var companyName = transaction.BranchAddress?.CompanyName;
			var regNumbers = transaction.BranchAddress?.RegistrationNumberCollection;
			XElement idFiscaleType = null,
					 codiceFiscale = null;

			var numOfIVA = FatturaElettronicaDataHelper.GetItalyRegistrationNumber(regNumbers, OrgCusCode.CodeTypes.IVA);
			idFiscaleType = XmlElementCreator.CreateIdFiscaleType(Core.Constants.CountryCodes.Italy, numOfIVA?.Value);
			var numOfCOD = FatturaElettronicaDataHelper.GetItalyRegistrationNumber(regNumbers, ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale);
			codiceFiscale = numOfCOD == null ? null : XmlElementCreator.CreateCodiceFiscale(numOfCOD?.Value);

			var anagrafica = XmlElementCreator.CreateAnagraficaType(companyName, false, null, null);

			var datiAnagrafici = new XStreamingElement("DatiAnagrafici", idFiscaleType);
			if (codiceFiscale != null && numOfCOD != null && numOfIVA != null && !numOfCOD.Value.Equals(numOfIVA.Value))
			{
				datiAnagrafici.Add(codiceFiscale);
			}

			datiAnagrafici.Add(anagrafica);

			return datiAnagrafici;
		}

		public static void BuildXmlForRegimeFiscale(XStreamingElement datiAnagrafici)
		{
			var regimeFiscale = new XElement("RegimeFiscale", AccountingConfigurationRegistry.Instance.ItalyTaxRegimeID.Value);
			datiAnagrafici.Add(regimeFiscale);
		}

		public static XElement BuildXmlForDatiSedeBasedOnLoginCompany(TransactionInfo transaction)
		{
			var address = new ZStringBuilder().
				AppendIfNotEmpty(transaction.BranchAddress?.Address1).AppendIfNotEmpty(transaction.BranchAddress?.Address2).
				ToStringWithDelimiterBetweenAppends(" ");
			var postCode = FatturaElettronicaXmlValueFormatter.GetItalyPostCode(transaction.BranchAddress?.Postcode);
			var city = transaction.BranchAddress?.City;
			var state = transaction.BranchAddress?.State;
			var country = transaction.BranchAddress?.Country?.Code;

			return XmlElementCreator.CreateIndirizzoType(address, null, postCode, city, state, country);
		}

		#endregion

		#region DatiAnagrafici Based on Billing Org

		public static XStreamingElement BuildXmlForDatiAnagraficiBasedOnBillingOrg(TransactionInfo transaction, ZString recipientOrgCategory, OrgHeader orgHeader)
		{
			var arOrgCountryCode = transaction?.OrganizationAddress?.Country?.Code ?? ZString.Empty;
			var isRecipientANaturalIndividualPerson = recipientOrgCategory == OrgConstants.Category.NaturalPersonIndividual;
			var isRecipientInItaly = arOrgCountryCode == Core.Constants.CountryCodes.Italy;
			var shouldCreateCodiceFiscale = isRecipientInItaly && isRecipientANaturalIndividualPerson;
			var companyName = transaction.OrganizationAddress?.CompanyName ?? ZString.Empty;
			var isRegNumberDifferentThanCodiceFiscale = FatturaElettronicaDataHelper.IsRegNumberDifferentThanCodiceFiscale(transaction, orgHeader);

			var idFiscaleIVAElement = !shouldCreateCodiceFiscale ? BuildXmlForIdFiscaleIVA(transaction, orgHeader) : null;
			var codiceFiscaleElement = isRecipientInItaly && (isRecipientANaturalIndividualPerson || isRegNumberDifferentThanCodiceFiscale) ? BuildXmlForCodiceFiscale(transaction) : null;

			return new XStreamingElement("DatiAnagrafici", idFiscaleIVAElement, codiceFiscaleElement,
				XmlElementCreator.CreateAnagraficaType(companyName, isRecipientANaturalIndividualPerson, null, null));
		}

		public static XElement BuildXmlForIdFiscaleIVA(TransactionInfo transaction, OrgHeader orgHeader)
		{
			var idPaese = ZString.Empty;
			var idCodice = ZString.Empty;

			if (orgHeader != null)
			{
				(idPaese, idCodice) = orgHeader.GetCountryCodeAndTaxRegistrationWithoutPrefix(transaction.OrganizationAddress?.Country?.Code);

				if (idCodice.IsEmpty)
				{
					var companyName = transaction.OrganizationAddress?.CompanyName ?? ZString.Empty;
					if (!companyName.IsEmpty)
					{
						var companyNameSpacesRemoved = companyName.Replace(" ", "");
						idCodice = companyNameSpacesRemoved.Length > 28 ? companyNameSpacesRemoved.Substring(0, 28) : companyNameSpacesRemoved;
					}
				}
			}

			return XmlElementCreator.CreateIdFiscaleType(idPaese, idCodice);
		}

		public static XElement BuildXmlForCodiceFiscale(TransactionInfo transaction)
		{
			var codRegistrationNumber = FatturaElettronicaDataHelper.GetItalyRegistrationNumber(transaction.OrganizationAddress?.RegistrationNumberCollection, ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale);
			var codiceFiscale = codRegistrationNumber?.Value ?? ZString.Empty;
			return XmlElementCreator.CreateCodiceFiscale(codiceFiscale);
		}

		#endregion

		#region Sede Based on Billing Org

		public static XElement BuildXmlForSedeBasedOnBillingOrg(TransactionInfo transaction)
		{
			var address1 = transaction.OrganizationAddress?.Address1 ?? ZString.Empty;
			var address2 = transaction.OrganizationAddress?.Address2 ?? ZString.Empty;
			var combinedAddress = address1 + " " + address2;
			var postCode = FatturaElettronicaXmlValueFormatter.GetItalyPostCode(transaction.OrganizationAddress?.Postcode);
			var city = transaction.OrganizationAddress?.City ?? ZString.Empty;
			var state = transaction.OrganizationAddress?.State;
			var country = transaction.OrganizationAddress?.Country?.Code ?? ZString.Empty;

			return XmlElementCreator.CreateIndirizzoType(combinedAddress.Trim(), null, postCode, city, state, country);
		}

		#endregion

		public static bool IsReceivable(TransactionInfo transaction)
		{
			return transaction.Ledger.HasValue && transaction.Ledger.Value == LedgerTypes.AccountsReceivable;
		}

		public static bool IsPayable(TransactionInfo transaction)
		{
			return transaction.Ledger.HasValue && transaction.Ledger.Value == LedgerTypes.AccountsPayable;
		}

		public static bool IsPayableInvOrPositiveAdj(TransactionInfo transaction)
		{
			var type = transaction.TransactionType;
			return IsPayable(transaction) && type.HasValue
				&& (type.Value == TransactionType.INV
				|| (type.Value == TransactionType.ADJ && transaction.OSTotal > 0));
		}

		public static bool IsPayableForGoods(TransactionInfo transaction)
		{
			return transaction.PostingJournalCollection?.Any(l => l.ChargeCode?.Class?.Code == (ZString?)GoodServiceTypes.Codes.GDS) ?? false;
		}

		public static bool IsReceivableForInternal(TransactionInfo transaction, BusinessObjectFactory factory)
		{
			var numOfIVA = GetItalyRegistrationNumber(transaction.BranchAddress?.RegistrationNumberCollection, OrgCusCode.CodeTypes.IVA);
			var idFiscaleCreditor = XmlElementCreator.CreateIdFiscaleType(Constants.CountryCodes.Italy, numOfIVA?.Value);

			var org = transaction?.OrganizationAddress;
			var orgHeader = org?.GetOrgHeader(factory);
			var idFiscaleDebtor = new XElement("IdFiscaleIVA");
			if (org?.Country?.Code?.ToString() != Constants.CountryCodes.Italy ||
				orgHeader?.GetCategory().ToString() != OrgConstants.Category.NaturalPersonIndividual)
			{
				idFiscaleDebtor = BuildXmlForIdFiscaleIVA(transaction, orgHeader);
			}

			return XNode.DeepEquals(idFiscaleDebtor, idFiscaleCreditor);
		}

		#region Exemption Document

		public static List<(ZString, ZDateTime)> ExtractExporterExemptionDocumentNumberAndDate(TransactionInfo transaction)
		{
			var documentTracking = transaction.OrganizationDocumentTrackingCollection?.FindAll(
				x => x.DocumentType.Code.Equals(Constants.RefDocTypes.VATExporterExemption) &&
				x.DocumentUsage.Code.Equals(JobRequiredDocument.DocUsage.Debtor) &&
				x.Category.Code.Equals(Constants.ReferenceTypes.ClientSupplierRelationship));

			var result = new List<(ZString, ZDateTime)>();
			if (documentTracking != null)
			{
				foreach (var document in documentTracking)
				{
					var attributes = document.DocumentTrackingAttributeCollection;
					var govAuthReference = attributes.FirstOrDefault(
						x => x.Type.Equals(JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference))?.Value;
					var buyerIssueDate = attributes.FirstOrDefault(
						x => x.Type.Equals(JobRequiredDocAttribTypeList.Codes.BuyerIssueDate))?.Value;

					ZDateTime dateParsed;
					var exemptionDocDate = buyerIssueDate.HasValue && ZDateTime.TryParseISO8601Date(buyerIssueDate.Value, out dateParsed) ? dateParsed :
						document.ReceivedDate ?? ZDateTime.Empty;

					result.Add(govAuthReference.HasValue ? (govAuthReference.Value, exemptionDocDate) : (ZString.Empty, ZDateTime.Empty));
				}
			}

			return result;
		}

		#endregion

		#region Zero Sequence Posting Journal Fix

		internal static void RemoveZeroSequenceIfAny(IEnumerable<PostingJournal> postingJournalsCollection)
		{
			var containsAnyZeroSequence = postingJournalsCollection.Any(l => l.Sequence == 0);
			if (containsAnyZeroSequence)
			{
				foreach (var line in postingJournalsCollection)
				{
					line.Sequence++;
				}
			}
		}

		#endregion

		internal static ARJournal[] GetInstalmentJournals(TransactionInfo transaction)
		{
			var factory = new BusinessObjectFactory();
			var trans = GetTransaction(transaction, factory);

			var query = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, Constants.TransactionCategory.Codes.InstalmentJournal);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, trans?.PK);

			return factory.Load<ARJournal>(query);
		}

		internal static TransactionHeader GetTransaction(TransactionInfo transaction, BusinessObjectFactory factory)
		{
			var query = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, transaction.Ledger);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transaction.TransactionType);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, transaction.Category);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transaction.Number);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			query.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, transaction.TransactionDate);
			query.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, transaction.PostDate);

			return factory.LoadTop1<TransactionHeader>(query);
		}

		internal static bool ShouldUseNewSchema => AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.Value.CompareTo(ZDate.Today.ToDateTime()) <= 0;

		public const string IssuerRegistrationNumber = "IssuerRegistrationNumber";
		const string FileNameSuffixForAR = "CWEAR";
		const string FileNameSuffixForAP = "CWEAP";
	}
}
