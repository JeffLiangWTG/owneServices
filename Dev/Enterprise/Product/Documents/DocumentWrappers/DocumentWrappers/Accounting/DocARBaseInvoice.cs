using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort.InvoiceDocLineRollUpper;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort.RollUpper;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.DocRollUpSort;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public abstract partial class DocARBaseInvoice : DocTransactionHeader, IDocHeader
	{
		protected DocARBaseInvoice(TransactionHeader wrappedInvoice, BusinessObjectFactory factoryToWrap)
			: base(wrappedInvoice, factoryToWrap)
		{
		}

		#region Logo

		public virtual Image InvoiceLogo
		{
			get { return InvoiceLogoCore; }
		}

		protected virtual Image InvoiceLogoCore
		{
			get
			{
				if (DocumentBrandingImage != null)
				{
					return DocumentBrandingImage;
				}
				else
				{
					if (Branch != null)
					{
						Guid departmentPK = Department != null ? ((GlbDepartment)Department.WrappedObject).PK.ToGuid() : Guid.Empty;
						return Branch.GetARInvoiceLogo(departmentPK);
					}
				}

				return base.CompanyLogo;
			}
		}

		#endregion

		#region Wrapper Fields

		#region DocWrapper Fields

		public virtual DocBankAccount ReceiptBankAccount
		{
			get { return DocBankAccount.New(TransactionHeader.ReceiptBankAccount, Factory); }
		}

		public override DocJobHeader JobHeader
		{
			get
			{
				DocJobHeader result = null;
				if (!TransactionHeader.AH_JH.IsEmpty)
				{
					result = DocJobHeader.New(InvoicingJob, Factory);
				}

				return result;
			}
		}

		public DocOrganisation AccountOrg
		{
			get { return DocOrganisation.New(InvoicingOrgHeader, Factory); }
		}

		public DocAddress AccountOrgAddress
		{
			get
			{
				DocAddress result = null;

				if (AccountOrg != null)
				{
					var addressPk = TransactionHeader.DisplayInvoiceAddressOverride;
					result = addressPk != ZGuid.Empty ? DocAddress.New(Factory.Load<OrgAddress>(addressPk), Factory) : null;
				}

				return result;
			}
		}

		#endregion

		public ZString CurrentCompanyCity
		{
			get
			{
				return TransactionHeader.Branch.GB_City;
			}
		}

		#region Tax Messages with Asterisks

		public ZString LocalLanguageTaxMessages
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsLocalInvoice())
				{
					List<string> messagesFormatted = new List<string>();
					BuildTaxMessagesToAsterisksMapping();
					foreach (Tuple<string, string, string, int> element in TaxMessagesToAsterisksMapping.Values)
					{
						messagesFormatted.Add(element.Item1 + " " + (element.Item3 == ZString.Empty ? element.Item2 : element.Item3));
					}
					result = String.Join(System.Environment.NewLine, messagesFormatted.ToArray());
				}
				return result;
			}
		}

		public ZString EnglishLanguageTaxMessages
		{
			get
			{
				ZString result = ZString.Empty;
				if (!IsLocalInvoice())
				{
					List<string> messagesFormatted = new List<string>();
					BuildTaxMessagesToAsterisksMapping();
					foreach (Tuple<string, string, string, int> element in TaxMessagesToAsterisksMapping.Values)
					{
						messagesFormatted.Add(element.Item1 + " " + element.Item2);
					}
					result = String.Join(System.Environment.NewLine, messagesFormatted.ToArray());
				}

				return result;
			}
		}

		internal Dictionary<ZGuid, Tuple<string, string, string, int>> TaxMessagesToAsterisksMapping
		{
			get
			{
				if (taxMessagesToAsterisksMapping == null)
				{
					BuildTaxMessagesToAsterisksMapping();
				}
				return taxMessagesToAsterisksMapping;
			}
		}
		Dictionary<ZGuid, Tuple<string, string, string, int>> taxMessagesToAsterisksMapping;

		protected void BuildTaxMessagesToAsterisksMapping()
		{
			// the mappins structure is InvTaxMsgPK => <#asterisks, English Msg, Local Msg, number to replace asterisks>
			taxMessagesToAsterisksMapping = new Dictionary<ZGuid, Tuple<string, string, string, int>>();

			int i = 1;
			foreach (DocARInvoiceLine docLine in GetLinesToFormat())
			{
				if (!docLine.Line.IsDeleted)
				{
					ZGuid key = docLine.Line.AL_A9_VATClass;
					if (!taxMessagesToAsterisksMapping.ContainsKey(key))
					{
						AccInvMsg message = Factory.Load<AccInvMsg>(key); // can improve by doing 1 load for all keys
						if (message != null && message.A9_IsShownOnDocuments)
						{
							string asterisks = ZString.Empty.PadLeft(i, '*');
							taxMessagesToAsterisksMapping.Add(key, new Tuple<string, string, string, int>(asterisks, message.A9_EnglishMsgMultilingual, message.A9_LocalMsg, i));
							i++;
						}
					}
				}
			}
		}

		#endregion

		#region Tax Messages without Asterisks

		public ZString LocalLanguageTaxMessagesWithNumbers
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsLocalInvoice())
				{
					List<string> messagesFormatted = new List<string>();
					BuildTaxMessagesToAsterisksMapping();
					foreach (Tuple<string, string, string, int> element in taxMessagesToAsterisksMapping.Values)
					{
						messagesFormatted.Add(element.Item4.ToString() + ". " + (element.Item3 == ZString.Empty ? element.Item2 : element.Item3));
					}
					result = String.Join(System.Environment.NewLine, messagesFormatted.ToArray());
				}
				return result;
			}
		}

		public ZString EnglishLanguageTaxMessagesWithNumbers
		{
			get
			{
				ZString result = ZString.Empty;
				if (!IsLocalInvoice())
				{
					List<string> messagesFormatted = new List<string>();
					BuildTaxMessagesToAsterisksMapping();
					foreach (Tuple<string, string, string, int> element in taxMessagesToAsterisksMapping.Values)
					{
						messagesFormatted.Add(element.Item4.ToString() + ". " + element.Item2);
					}
					result = String.Join(System.Environment.NewLine, messagesFormatted.ToArray());
				}

				return result;
			}
		}

		#endregion

		protected virtual DocARInvoiceLineCollection GetLinesToFormat()
		{
			return new DocARInvoiceLineCollection(Factory);
		}

		#region Wrapper Around Standard BizO

		public OrgHeader InvoicingOrgHeader
		{
			get { return TransactionHeader.Header; }
		}

		protected AccBankAccount InvoicingBankAccount
		{
			get { return TransactionHeader.BankAccount; }
		}

		#endregion

		#endregion

		#region Public Base Properties

		#region ZString

		public ZString RecipientGovtTaxID
		{
			get
			{
				ZString result = ZString.Empty;

				if (CurrentCompany.Country != null && CurrentCompany.Country.Code == Core.Constants.CountryCodes.VietNam)
				{
					if (InvoicingOrgHeader != null)
					{
						OrgCusCode vatCode = InvoicingOrgHeader.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(
							OrgCusCode.CodeTypes.VATCode, GlbCompany.CurrentCompany.Country);

						result = (vatCode != null) ? vatCode.OK_CustomsRegNo : ZString.Empty;
					}
				}

				return result;
			}
		}

		bool HasRecipientTaxIDNumber
		{
			get
			{
				var result = false;
				if (GlbCompany.CurrentCompany.Country != null && TransactionHeader.Header != null)
				{
					result = GlbCompany.CurrentCompany.Country.Code == TransactionHeader.Header.CountryOfTaxRegistration.Code ||
							(GlbCompany.CurrentCompany.Country.IsPartOfEuropeanUnion && TransactionHeader.Header.CountryOfTaxRegistration.IsPartOfEuropeanUnion) ||
							(GlbCompany.CurrentCompany.Country.Code == Constants.CountryCodes.India);
				}
				return result;
			}
		}

		OrgCusCode DIMCustomCode
		{
			get
			{
				OrgCusCode result = null;
				if (TransactionHeader.Header != null)
				{
					result = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(CostaRicaOrgCusCodeInfo.OrgCusCodes.DIMEXDocumentIdentificationNumber, RefCountry.LoadFromCountryCode(Factory, CurrentCompany.Country.Code));
				}

				return result;
			}
		}

		OrgCusCode NITCustomCode
		{
			get
			{
				OrgCusCode result = null;
				if (TransactionHeader.Header != null)
				{
					result = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(CostaRicaOrgCusCodeInfo.OrgCusCodes.NITIdentificationNumber, RefCountry.LoadFromCountryCode(Factory, CurrentCompany.Country.Code));
				}

				return result;
			}
		}

		bool HasOnlyFallBackTaxIDNumber
		{
			get
			{
				var result = false;
				var orgCountryCode = TransactionHeader.Header.CountryCode;

				var defaultTaxCodeForOrgCountry = Country.GetConsumptionTaxRegistrationOrgCusCode(orgCountryCode);
				if (string.IsNullOrEmpty(defaultTaxCodeForOrgCountry)
					|| TransactionHeader.Header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(defaultTaxCodeForOrgCountry, orgCountryCode) == null)
				{
					var taxCodesForOrgCountry = Country.GetConsumptionTaxRegistrationCodesForOrgCountry(orgCountryCode);

					var matchingCountryCustomsCode = TransactionHeader.Header.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(orgCountryCode, taxCodesForOrgCountry.Select(x => new ZString(x)).ToArray());
					result = !matchingCountryCustomsCode.IsEmpty;
				}

				return result;
			}
		}

		public virtual ZString RecipientTaxIDNumberInRecipientCountry
		{
			get
			{
				if (!recipientTaxIDNumberInRecipientCountry.HasValue)
				{
					if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Malaysia &&
						HasRecipientTaxIDNumber &&
						RecipientTaxIDNumber != ZString.Empty)
					{
						recipientTaxIDNumberInRecipientCountry = ZString.Empty;
					}
					else
					{
						var header = TransactionHeader.Header;
						recipientTaxIDNumberInRecipientCountry = header?.GetCodeForTaxRegistrationInOrgCountry(TransactionHeader.InvoiceAddressOverride) ?? ZString.Empty;

						if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Portugal &&
							RecipientTaxIDNumber == PTUnknownTaxIDPlaceholder)
						{
							recipientTaxIDNumberInRecipientCountry = ZString.Empty;
						}
					}
				}

				return recipientTaxIDNumberInRecipientCountry.Value;
			}
		}

		ZString? recipientTaxIDNumberInRecipientCountry;

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public virtual ZString RecipientTaxIDNumber
		{
			get
			{
				var recipientTaxIDNumberFromJobDocAddressForPT = GetRecipientTaxIDNumberFromJobDocAddressForPT();
				if (!recipientTaxIDNumberFromJobDocAddressForPT.IsEmpty)
				{
					return recipientTaxIDNumberFromJobDocAddressForPT;
				}

				ZString result = ZString.Empty;

				if (HasRecipientTaxIDNumber)
				{
					bool displayTaxIDForCurrentCompany = AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.GetValueWithoutFallback(TransactionHeader.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

					if (TransactionHeader.Company.Country.Code == Core.Constants.CountryCodes.Netherlands && displayTaxIDForCurrentCompany)
					{
						if (TransactionHeader.Header != null && !TransactionHeader.Header.RawTaxRegistrationNumber.IsEmpty)
						{
							result = TransactionHeader.Header.TaxRegistrationNumber;
						}
					}
					else if (IsTaxed)
					{
						if ((TransactionHeader.Company.Country.IsPartOfEuropeanUnion
							|| TransactionHeader.Company.Country.Code == Core.Constants.CountryCodes.Norway
							|| TransactionHeader.Company.Country.Code == Core.Constants.CountryCodes.UnitedKingdom)
							&& displayTaxIDForCurrentCompany)
						{
							if (TransactionHeader.Header != null)
							{
								if (TransactionHeader.Company.Country.Code == Core.Constants.CountryCodes.Spain && HasREGLineForSpain)
								{
									result = TransactionHeader.Header.CustomsCodes.Cast<OrgCusCode>().Where(x => x.OK_CodeType == OrgCusCode.SpainCodeTypes.IGC).Select(x => x.OK_CustomsRegNo).FirstOrDefault();
								}
								else if (!TransactionHeader.Header.RawTaxRegistrationNumber.IsEmpty)
								{
									result = TransactionHeader.Header.TaxRegistrationNumber;
								}
							}

							if (AccountingCountrySpecificValidationHelper.IsEmptyPortugalIVA(result) &&
								TransactionHeader.Company.Country.Code == Constants.CountryCodes.Portugal)
							{
								if (TransactionHeader.Header.Country != null)
								{
									if (TransactionHeader.Header.Country.IsPartOfEuropeanUnion &&
									TransactionHeader.Header.CountryCode != Constants.CountryCodes.Portugal)
									{
										result = RecipientTaxIDNumberInRecipientCountry.IsEmpty
											? new ZString(PTUnknownTaxIDPlaceholder)
											: (HasOnlyFallBackTaxIDNumber)
												? ZString.Empty
												: RecipientTaxIDNumberInRecipientCountry;
									}
									else if (!TransactionHeader.Header.Country.IsPartOfEuropeanUnion)
									{
										result = RecipientTaxIDNumberInRecipientCountry.IsEmpty ? new ZString(PTUnknownTaxIDPlaceholder) : ZString.Empty;
									}
									else
									{
										result = new ZString(PTUnknownTaxIDPlaceholder);
									}
								}
								else
								{
									result = new ZString(PTUnknownTaxIDPlaceholder);
								}
							}
						}
						else if (displayTaxIDForCurrentCompany)
						{
							switch (TransactionHeader.Company.Country.Code)
							{
								case Core.Constants.CountryCodes.Taiwan:
								case Core.Constants.CountryCodes.SouthAfrica:
									if (Organisation != null)
									{
										result = Organisation.LocalVATCode;
									}
									break;
								case Constants.CountryCodes.India:
									if (TransactionHeader.Header != null && TransactionHeader.Header.CountryCode == Constants.CountryCodes.India)
									{
										var gstCode = TransactionHeader.Header.RawTaxRegistrationNumber;
										if (!string.IsNullOrWhiteSpace(gstCode))
										{
											result = gstCode;
										}
										else
										{
											result = TransactionHeader.Header.UINCodeForIndia;
										}
									}
									break;
								case Constants.CountryCodes.Malaysia:
									var taxRegistrationType = GetTaxRegistrationTypeForMalaysia();
									result = GetOrgTaxRegistrationNumberByType(TransactionHeader?.Header, taxRegistrationType);
									break;
								case Constants.CountryCodes.NewZealand:
									if (ShouldPrintGSTCodeForNewZealand)
									{
										result = GetCustomsRegNo(TransactionHeader.Header, OrgCusCode.CodeTypes.GSTCode, Constants.CountryCodes.NewZealand);
									}
									break;
								case Constants.CountryCodes.LaoPeoplesDemocraticRepublic:
									if (TransactionHeader.Header != null)
									{
										result = GetCustomsRegNo(TransactionHeader.Header, OrgCusCode.CodeTypes.VATCode, Constants.CountryCodes.LaoPeoplesDemocraticRepublic);
									}
									break;
								case Constants.CountryCodes.Chad:
									result = GetCustomsRegNo(TransactionHeader.Header, OrgCusCode.ChadCodeTypes.NIF, Constants.CountryCodes.Chad);
									break;
								case Constants.CountryCodes.Bangladesh:
									result = GetCustomsRegNo(TransactionHeader.Header, Country.GetConsumptionTaxDescription(Constants.CountryCodes.Bangladesh), Constants.CountryCodes.Bangladesh);
									break;
								case Constants.CountryCodes.PapuaNewGuinea:
									result = GetCustomsRegNo(TransactionHeader.Header, Country.GetConsumptionTaxDescription(Constants.CountryCodes.PapuaNewGuinea), Constants.CountryCodes.PapuaNewGuinea);
									break;
								default:
									if (TransactionHeader.Header != null)
									{
										result = TransactionHeader.Header.RawTaxRegistrationNumber;
									}
									break;
							}
						}
					}
				}
				return result;
			}
		}

		const string PTUnknownTaxIDPlaceholder = "XXXXXXXXX";

		ZString GetRecipientTaxIDNumberFromJobDocAddressForPT()
		{
			if (AccountingUtils.ShouldPortugalStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting(TransactionHeader.AH_Ledger, TransactionHeader.AH_TransactionType))
			{
				if (TransactionHeader is InvoicingBase invoicingBase)
				{
					var docAddress = invoicingBase.DocAddresses.FindByDocAddressType(DocAddressType.DebtorAddress);
					return docAddress?.E2_GovRegNum ?? ZString.Empty;
				}
			}

			return ZString.Empty;
		}

		protected ZString GetCustomsRegNo(OrgHeader header, string codeType, string countryCode)
		{
			return header?.CustomsCodes.Cast<OrgCusCode>().Where(x => x.OK_CodeType == codeType && x.OK_RN_NKCodeCountry == countryCode).Select(x => x.OK_CustomsRegNo).FirstOrDefault() ?? ZString.Empty;
		}

		bool ShouldPrintGSTCodeForNewZealand
		{
			get
			{
				return TransactionHeader.Header != null
								&& TransactionHeader.AH_Ledger == LedgerTypes.AccountsPayable
								&& TransactionHeader is InvoicingBase invoicingBase
								&& invoicingBase.IsSelfBillingInvoice;
			}
		}

		protected string GetTaxRegistrationTypeForMalaysia()
		{
			string result = null;

			var list = (TransactionHeader as InvoicingBase)?.Lines.Cast<InvoicingLineBase>();
			if (list != null)
			{
				if (list.Any(x => x.TaxRate != null && x.TaxRate.AT_ExtraTaxRateType == ZString.Empty))
				{
					result = OrgCusCode.CodeTypes.GSTCode;
				}
				else if (!list.Any(x => x.TaxRate != null && x.TaxRate.AT_ExtraTaxRateType != "SER"))
				{
					result = MalaysiaOrgCusCodeInfo.OrgCusCodes.SER;
				}
			}

			return result;
		}

		protected ZString GetOrgTaxRegistrationNumberByType(OrgHeader org, string registrationType, string countryCode = "")
		{
			var result = ZString.Empty;

			if (org == null || string.IsNullOrEmpty(registrationType))
			{
				return result;
			}

			var orgCountryCode = string.IsNullOrEmpty(countryCode) ? org.CountryCode : (ZString)countryCode;

			foreach (OrgCusCode customCode in org.CustomsCodes)
			{
				if (customCode.CodeCountry != null && customCode.OK_CodeType == registrationType &&
					!orgCountryCode.IsEmpty && orgCountryCode == customCode.OK_RN_NKCodeCountry)
				{
					result = customCode.OK_CustomsRegNo;
					break;
				}
			}

			return result;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public virtual ZString RecipientTaxIDHeading
		{
			get
			{
				ZString result = ZString.Empty;
				var currentCompany = GlbCompany.CurrentCompany;

				if (currentCompany.Country != null)
				{
					bool displayTaxIDForCurrentCompany = AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.GetValueWithoutFallback(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
					string displayTaxIDHeadingForCurrentCompany = AccountingConfigurationRegistry.Instance.DisplayRecipientTaxIDHeading.GetValueWithoutFallback(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

					if (currentCompany.Country.Code == Core.Constants.CountryCodes.Netherlands && displayTaxIDForCurrentCompany)
					{
						if (TransactionHeader.Header != null && !TransactionHeader.Header.RawTaxRegistrationNumber.IsEmpty)
						{
							result = string.IsNullOrEmpty(displayTaxIDHeadingForCurrentCompany) ? Res.GetString("7b6e2bed-5816-4494-82cb-f36eccabe25a", "Client VAT #:") : displayTaxIDHeadingForCurrentCompany;
						}
					}
					else if (IsTaxed)
					{
						bool isResultFixed = false;

						if (displayTaxIDForCurrentCompany)
						{
							var recipientTaxIDHeading = ObjectFactory.Get<ICountryComplianceFactoryIntegration>()?.GetICountryComplianceInfo(CurrentCompany.Country.Code)?.GetRecipientTaxIDHeading();
							if (recipientTaxIDHeading != null)
							{
								result = string.IsNullOrEmpty(displayTaxIDHeadingForCurrentCompany) ? recipientTaxIDHeading : displayTaxIDHeadingForCurrentCompany;
							}
							#region This code design is obsolete please add new codes to through classes created by CountryComplianceFactory
							else
							{
								if (currentCompany.Country.Code == Core.Constants.CountryCodes.SouthAfrica ||
									currentCompany.Country.Code == Core.Constants.CountryCodes.LaoPeoplesDemocraticRepublic ||
									currentCompany.Country.Code == Core.Constants.CountryCodes.Morocco)
								{
									result = string.IsNullOrEmpty(displayTaxIDHeadingForCurrentCompany) ? Res.GetString("7b6e2bed-5816-4494-82cb-f36eccabe25a", "Client VAT #:") : displayTaxIDHeadingForCurrentCompany;
								}
								else if (currentCompany.Country.Code == Core.Constants.CountryCodes.Taiwan && Organisation.LocalVATCode != ZString.Empty)
								{
									result = string.IsNullOrEmpty(displayTaxIDHeadingForCurrentCompany) ? Res.GetString("60e9bb8a-0b5e-4d2a-ae92-d328216782c4", "Client Tax #:") : displayTaxIDHeadingForCurrentCompany;
								}
								else if (currentCompany.Country.Code == Core.Constants.CountryCodes.Germany)
								{
									result = string.IsNullOrEmpty(displayTaxIDHeadingForCurrentCompany) ? Res.GetString("cfcc0b59-d484-4916-8e55-d8e25ebc7e2b", "Client VAT ID No:") : displayTaxIDHeadingForCurrentCompany;
								}
								else if (currentCompany.Country.Code == Core.Constants.CountryCodes.Spain && HasREGLineForSpain && IsIGICRecordedAgainstOrg(TransactionHeader.Header))
								{
									result = string.IsNullOrEmpty(displayTaxIDHeadingForCurrentCompany) ? Res.GetString("7dbdfe61-8420-4da7-af28-d5f6d1487444", "Client NIF #:") : displayTaxIDHeadingForCurrentCompany;
								}
								else if (currentCompany.Country.Code == Core.Constants.CountryCodes.Australia)
								{
									result = string.IsNullOrEmpty(displayTaxIDHeadingForCurrentCompany) ? Res.GetString("6dd930e5-862f-4d4d-aea7-0f106fb0796a", "Client ABN #") : displayTaxIDHeadingForCurrentCompany;
								}
								else if (currentCompany.Country.Code == Core.Constants.CountryCodes.Chad || currentCompany.Country.Code == Core.Constants.CountryCodes.Djibouti)
								{
									result = string.IsNullOrEmpty(displayTaxIDHeadingForCurrentCompany) ? Res.GetString("7dbdfe61-8420-4da7-af28-d5f6d1487444", "Client NIF #:") : displayTaxIDHeadingForCurrentCompany;
								}
								else if (currentCompany.Country.Code == Constants.CountryCodes.India)
								{
									if (!string.IsNullOrWhiteSpace(displayTaxIDHeadingForCurrentCompany))
									{
										result = displayTaxIDHeadingForCurrentCompany;
									}
									else if (!string.IsNullOrWhiteSpace(TransactionHeader.Header?.RawTaxRegistrationNumber))
									{
										result = Res.GetString("51718acc-b768-4d9a-9168-9c65fb43ed57", "Client GSTIN #:");
									}
									else if (!string.IsNullOrWhiteSpace(TransactionHeader.Header?.UINCodeForIndia))
									{
										result = Res.GetString("10f7911a-26fa-4087-8af6-f2536bcd09aa", "Client UIN #:");
									}
								}
								else if (currentCompany.Country.Code == Core.Constants.CountryCodes.Malaysia)
								{
									var taxRegistrationType = GetTaxRegistrationTypeForMalaysia();
									result = GetRecipientTaxIDHeadingForMalaysia(taxRegistrationType);
									isResultFixed = true;
								}
								else if (currentCompany.Country.Code == Constants.CountryCodes.NewZealand)
								{
									if (ShouldPrintGSTCodeForNewZealand)
									{
										result = Res.GetString("B3543228-CF8A-4b16-90BF-080B0A995617", "GST #:");
									}
								}
							}
							#endregion
						}

						bool displayTaxIDForCalculatedCompany = AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.GetValueWithoutFallback(TransactionHeader.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
						if (result.IsEmpty && !isResultFixed && displayTaxIDForCalculatedCompany)
						{
							string displayTaxIDHeadingForCalculatedCompany = AccountingConfigurationRegistry.Instance.DisplayRecipientTaxIDHeading.GetValueWithoutFallback(TransactionHeader.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

							if (!string.IsNullOrEmpty(displayTaxIDHeadingForCalculatedCompany))
							{
								result = displayTaxIDHeadingForCalculatedCompany;
							}
							else
							{
								string value;

								if (TransactionHeader.Company.Country.IsPartOfEuropeanUnion || TransactionHeader.Company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Norway)
								{
									result = Res.GetString("5c4666fa-d867-45ff-8993-78eba17b3713", "Client {0} #:", GetTranslatedTaxCodeFromCountryCode(TransactionHeader.Company.GC_RN_NKCountryCode));
								}
								else if (RecipientTaxIDHeadingForCalculatedCompanyDictionary.TryGetValue(TransactionHeader.Company.GC_RN_NKCountryCode.ToString(), out value))
								{
									result = value;
								}
								else
								{
									result = Res.GetString("5c4666fa-d867-45ff-8993-78eba17b3713", "Client {0} #:", GetTranslatedTaxCodeFromCountryCode(TransactionHeader.Company.GC_RN_NKCountryCode));
								}
							}
						}
					}
				}
				return result;
			}
		}

		ZString GetRecipientTaxIDHeadingForMalaysia(string taxRegistrationType)
		{
			var result = ZString.Empty;

			if (taxRegistrationType == OrgCusCode.CodeTypes.GSTCode)
			{
				result = Res.GetString("84888CFC-C80A-4A43-8A42-88F11E032197", "Client GST #:");
			}
			else if (taxRegistrationType == MalaysiaOrgCusCodeInfo.OrgCusCodes.SER)
			{
				result = Res.GetString("4F5AD29E-4B71-42CA-A22D-D39C439E30B3", "Client SST #:");
			}

			return result;
		}

		public Dictionary<string, string> RecipientTaxIDHeadingForCalculatedCompanyDictionary
		{
			get
			{
				if (recipientTaxIDHeadingForCalculatedCompanyDictionary == null)
				{
					recipientTaxIDHeadingForCalculatedCompanyDictionary = new Dictionary<string, string>();
					AddRecipientTaxIDHeadingToDictionary(recipientTaxIDHeadingForCalculatedCompanyDictionary);
				}
				return recipientTaxIDHeadingForCalculatedCompanyDictionary;
			}
		}

		Dictionary<string, string> recipientTaxIDHeadingForCalculatedCompanyDictionary;

		void AddRecipientTaxIDHeadingToDictionary(Dictionary<string, string> recipientTaxIDHeadingDictionary)
		{
			if (recipientTaxIDHeadingDictionary != null)
			{
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Pakistan, PakistanComplianceInfo.RecipientTaxIDHeading);
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.PapuaNewGuinea, PapuaNewGuineaComplianceInfo.RecipientTaxIDHeading);
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Bangladesh, BangladeshComplianceInfo.RecipientTaxIDHeading);
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Turkey, Res.GetString("adcbcdc5-aa18-4477-b604-30b09dcaced6", "CLIENT TIN:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.SriLanka, Res.GetString("7b6e2bed-5816-4494-82cb-f36eccabe25a", "Client VAT #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Ethiopia, Res.GetString("7b6e2bed-5816-4494-82cb-f36eccabe25a", "Client VAT #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Uganda, Res.GetString("7b6e2bed-5816-4494-82cb-f36eccabe25a", "Client VAT #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Azerbaijan, Res.GetString("7b6e2bed-5816-4494-82cb-f36eccabe25a", "Client VAT #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Kenya, Res.GetString("7b6e2bed-5816-4494-82cb-f36eccabe25a", "Client VAT #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Mauritius, Res.GetString("7b6e2bed-5816-4494-82cb-f36eccabe25a", "Client VAT #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Mongolia, Res.GetString("7b6e2bed-5816-4494-82cb-f36eccabe25a", "Client VAT #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Botswana, Res.GetString("7b6e2bed-5816-4494-82cb-f36eccabe25a", "Client VAT #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Kazakhstan, Res.GetString("7b6e2bed-5816-4494-82cb-f36eccabe25a", "Client VAT #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Tanzania, Res.GetString("7b6e2bed-5816-4494-82cb-f36eccabe25a", "Client VAT #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Zimbabwe, Res.GetString("7b6e2bed-5816-4494-82cb-f36eccabe25a", "Client VAT #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.KoreaSouth, Res.GetString("7b6e2bed-5816-4494-82cb-f36eccabe25a", "Client VAT #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Philippines, Res.GetString("0c53ffb7-bda9-485d-a882-65366ff7c948", "TIN:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Maldives, Res.GetString("0c53ffb7-bda9-485d-a882-65366ff7c948", "TIN:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Tonga, Res.GetString("0c53ffb7-bda9-485d-a882-65366ff7c948", "TIN:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Peru, Res.GetString("c9e2b4e5-64b3-4a17-8055-af9685a5dc3a", "R.U.C."));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.China, Res.GetString("92acdb07-6da8-4219-80b5-924c16fc1ec2", "VAT"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Indonesia, Res.GetString("20b32cb2-29ed-46ae-b954-13158ee29e86", "NPWP"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Guatemala, Res.GetString("f7b7215d-8722-4f53-9020-71d380342b8e", "Client NIT #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Colombia, Res.GetString("f7b7215d-8722-4f53-9020-71d380342b8e", "Client NIT #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Bolivia, Res.GetString("f7b7215d-8722-4f53-9020-71d380342b8e", "Client NIT #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Venezuela, Res.GetString("d560085e-f2a2-4d59-84a0-11ea1bdc4bbc", "Client RIF #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Ecuador, Res.GetString("980a6b50-6b1e-4c77-bef6-24680ab7cd6d", "Client RUC #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Paraguay, Res.GetString("980a6b50-6b1e-4c77-bef6-24680ab7cd6d", "Client RUC #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Nicaragua, Res.GetString("980a6b50-6b1e-4c77-bef6-24680ab7cd6d", "Client RUC #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Uruguay, Res.GetString("d4a151ea-eeb2-4e95-a420-c48357224daf", "Client RUT #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.ElSalvador, Res.GetString("c9157763-20b1-42af-a5c1-77f8a041e2e6", "Client NRC #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.PuertoRico, Res.GetString("3c0c0e37-edfc-4a77-a7a7-622fa62454fb", "Client NRC #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.FrenchPolynesia, Res.GetString("8c71b286-14e4-4925-bf58-8c01963fed51", "Client TAHITI #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Honduras, Res.GetString("a4926715-4e8c-4c20-b9f6-ccb0726295df", "Client RTN #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Mali, Res.GetString("fd3fec79-7124-420a-8558-eb999adfae77", "Client NIF #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.EquatorialGuinea, Res.GetString("fd3fec79-7124-420a-8558-eb999adfae77", "Client NIF #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Algeria, Res.GetString("fd3fec79-7124-420a-8558-eb999adfae77", "Client NIF #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Jordan, Res.GetString("8cdb458e-e3b4-42f6-906a-307ae3d650f8", "Client GST #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Thailand, Res.GetString("cf4bde96-d55a-4499-9cd3-dfe3605c2957", "Client VAT #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.VietNam, Res.GetString("2848bc40-1d19-487c-a9aa-a634116bb8ae", "Client MST #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Senegal, Res.GetString("a2d55c86-8d30-4df7-ac2d-e84a716c1feb", "Client NINEA #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.CoteDivoire, Res.GetString("05aa2285-4fe9-4119-b0fb-f3015b3cc47b", "Client CC #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Cameroon, Res.GetString("9ccaafac-1e7b-41b0-8ab8-cbd66f2e9b12", "Client NIU #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Mozambique, Res.GetString("c768d37f-4386-4850-84a7-0668ad4fd020", "Client NUIT #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.DominicanRepublic, Res.GetString("6856aa01-b1dd-481b-b643-59c08b98bd91", "Client RNC #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Yemen, Res.GetString("5839cf2a-c204-4b86-bbc1-6726313e778a", "Client GST #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Malawi, Res.GetString("c982147b-99d5-4e47-a5f0-2963485565ea", "Client TPIN #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Niger, Res.GetString("a8fb4167-6d5e-456a-98be-5d7a4b666457", "Client NIF #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.CostaRica, Res.GetString("4229bdc2-85c0-4a5a-824c-058d4e5c5fbc", "CÉD. JURÍDICA #"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Ghana, Res.GetString("b7827673-2d9d-4342-bf74-9c23b2e78588", "Client TIN #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Belarus, Res.GetString("41f4f2d7-d520-4ef3-8d14-6899def52044", "Client TIN #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.SierraLeone, Res.GetString("bbcf78d2-989c-407c-92c4-53eabc185b89", "Client TIN #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Cambodia, Res.GetString("b5c4fe36-4fde-49ec-94db-f463d90da9a8", "Client VATTIN #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Madagascar, Res.GetString("c2fd9eaf-1d16-4602-9287-8b1adb4107e6", "Client NIF #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Kiribati, Res.GetString("ccdddd2c-c5d0-4b9f-94ad-214b46ecbb1a", "Client TIN #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Lithuania, Res.GetString("fa127a3f-87d3-4d4d-b00e-cb1a54c5e3ef", "Client ĮM.KODA #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Nepal, Res.GetString("8084DBAC-C931-492E-ADF8-2DE7D026BACC", "Client VAT #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Togo, Res.GetString("C269916A-5285-45E2-A0F9-9645548F4E85", "Client NIF #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Rwanda, Res.GetString("DB5580D6-164E-4C6B-9281-58C78D2926E5", "Client TIN #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.Kosovo, Res.GetString("1179FD50-B652-4404-B3B7-1348FBD7D89B", "CLIENT TVSH #:"));
				recipientTaxIDHeadingDictionary.Add(Core.Constants.CountryCodes.NewCaledonia, Res.GetString("eaed938d-bf87-4ce0-9a0d-16804469ca40", "CLIENT TGC #:"));
			}
		}

		protected internal static ZString GetTranslatedTaxCodeFromCountryCode(ZString countryCode)
		{
			ZString result = string.Empty;
			if (GlbCompany.CurrentCompany.Country.UseEUVatDescription)
			{
				result = Res.GetString("VATTranslation|EU", "VAT");
			}
			else if (GlbCompany.CurrentCompany.Country.RN_Code == Constants.CountryCodes.Canada)
			{
				result = Res.GetString("VATTranslation|CA", "GST");
			}
			else
			{
				result = Country.GetDomesticNameofTaxCode(countryCode);
			}
			return result;
		}

		protected ZString GetQCTExtraTaxCodeFromCountryCode()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CurrentCompany.Country.Code) as ICountryComplianceInfo;
			if (complianceInfo != null && complianceInfo.HasExtraTaxInfo().HasValue && complianceInfo.HasExtraTaxInfo().Value)
			{
				return GetExtraTaxCodeFromCountryCode(complianceInfo, AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase);
			}
			else
			{
				return GetExtraTaxCodeFromCountryCode(null, string.Empty);
			}
		}

		protected internal static ZString GetExtraTaxCodeFromCountryCode(string extraTaxTypeCode)
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(GlbCompany.CurrentCompany.Country.Code) as ICountryComplianceInfo;
			return GetExtraTaxCodeFromCountryCode(complianceInfo, extraTaxTypeCode);
		}

		protected internal static ZString GetExtraTaxCodeFromCountryCode(ICountryComplianceInfo complianceInfo, string extraTaxTypeCode)
		{
			if (complianceInfo != null && complianceInfo.HasExtraTaxInfo().HasValue && complianceInfo.HasExtraTaxInfo().Value)
			{
				return complianceInfo.GetExtraTaxDescription(extraTaxTypeCode);
			}
			else if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Ghana)
			{
				return GhanaComplianceInfo.ExtraTax;
			}
			else
			{
				return Res.GetString("1bc5d42d-e3b2-4adf-a561-5f8a2f5bbf9f", "QST");
			}
		}

		public virtual ZString RecipientLocalBusinessReg2Number
		{
			get
			{
				ZString result = ZString.Empty;
				OrgCusCode orgCusCode = null;

				if (CurrentCompany != null && CurrentCompany.Country != null && TransactionHeader.Header != null)
				{
					var recipientLocalBusinessReg2NumberCodeType = ObjectFactory.Get<ICountryComplianceFactoryIntegration>()?.GetICountryComplianceInfo(CurrentCompany.Country.Code)?.GetRecipientLocalBusinessReg2NumberCodeType();
					if (recipientLocalBusinessReg2NumberCodeType != null)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(recipientLocalBusinessReg2NumberCodeType, RefCountry.LoadFromCountryCode(Factory, CurrentCompany.Country.Code));
						return orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}

					#region This code design is obsolete please add new codes to through classes created by CountryComplianceFactory
					if (CurrentCompany.Country.Code == Constants.CountryCodes.KoreaSouth)
					{
						OrgCusCode kBCCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(KoreaSouthComplianceInfo.CodeTypes.KBC, RefCountry.LoadFromCountryCode(Factory, CurrentCompany.Country.Code));
						result = kBCCode != null ? kBCCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Constants.CountryCodes.Italy)
					{
						OrgCusCode iTCCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(ItalyOrgCusCodeInfo.OrgCusCodes.CUU, RefCountry.LoadFromCountryCode(Factory, CurrentCompany.Country.Code));
						result = iTCCode != null ? iTCCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Constants.CountryCodes.India && TransactionHeader.Header.CountryCode == Constants.CountryCodes.India)
					{
						OrgCusCode gIDCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(IndiaOrgCusCodeInfo.OrgCusCodes.GID, RefCountry.LoadFromCountryCode(Factory, CurrentCompany.Country.Code));
						result = gIDCode != null ? gIDCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Constants.CountryCodes.CostaRica)
					{
						if (DIMCustomCode != null)
						{
							result = DIMCustomCode.OK_CustomsRegNo;
						}
						else if (NITCustomCode != null)
						{
							result = NITCustomCode.OK_CustomsRegNo;
						}
					}
					else if (CurrentCompany.Country.Code == Constants.CountryCodes.BosniaAndHerzegovina)
					{
						var jmbCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.BosniaAndHerzegovinaCodeTypes.JMB, RefCountry.LoadFromCountryCode(Factory, CurrentCompany.Country.Code));
						result = jmbCode != null ? jmbCode.OK_CustomsRegNo : ZString.Empty;
					}
					#endregion
				}
				return result;
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public virtual ZString RecipientLocalBusinessRegNumber
		{
			get
			{
				ZString result = ZString.Empty;
				OrgCusCode orgCusCode = null;

				if (CurrentCompany != null && CurrentCompany.Country != null && TransactionHeader.Header != null)
				{
					var recipientLocalBusinessRegNumberCodeType = ObjectFactory.Get<ICountryComplianceFactoryIntegration>()?.GetICountryComplianceInfo(CurrentCompany.Country.Code)?.GetRecipientLocalBusinessRegNumberCodeType();
					if (recipientLocalBusinessRegNumberCodeType != null)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(recipientLocalBusinessRegNumberCodeType, RefCountry.LoadFromCountryCode(Factory, CurrentCompany.Country.Code));
						return orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}

					#region This code design is obsolete please add new codes to through classes created by CountryComplianceFactory
					if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Italy)
					{
						if (!TransactionHeader.Header.LocalBusinessRegNo.IsEmpty)
						{
							result = TransactionHeader.Header.LocalBusinessRegNo;
						}
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Greece)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.GreeceCodeTypes.DOY, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Greece));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Morocco)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.MoroccoCodeTypes.ICE, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Morocco));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Spain)
					{
						if (!HasREGLineForSpain)
						{
							OrgCusCode nIFCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.SpainCodeTypes.NIF, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Spain));
							if (nIFCode == null || nIFCode.OK_CustomsRegNo.IsEmpty)
							{
								OrgCusCode dNICode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.SpainCodeTypes.DNI, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Spain));
								result = dNICode != null ? dNICode.OK_CustomsRegNo : ZString.Empty;
							}
						}
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Denmark)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.DenmarkCodeTypes.EANLocationNumber, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Denmark));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.SriLanka)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.SriLankaCodeTypes.SVATBusinessRegistrationNumber, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.SriLanka));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Ethiopia)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.EthiopiaCodeTypes.TaxIdentificationNumber, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Ethiopia));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Uganda)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(UgandaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Uganda));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.France)
					{
						var sIRETCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.FranceCodeTypes.Siret, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France))
							?? TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.FranceCodeTypes.Siren, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France));
						result = sIRETCode != null ? sIRETCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Turkey)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(TurkeyOrgCusCodeInfo.OrgCusCodes.VDM, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Turkey));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Azerbaijan)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.AzerbaijanCodeTypes.TIN, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Azerbaijan));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Kenya)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.KenyaCodeTypes.PIN, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Kenya));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Mauritius)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.CodeTypes.BusinessRegistrationNumber, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Mauritius));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Tanzania)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.TanzaniaCodeTypes.TIN, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Tanzania));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.KoreaSouth)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(KoreaSouthComplianceInfo.CodeTypes.KBT, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.KoreaSouth));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Thailand)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.ThailandCodeTypes.BID, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Thailand));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Peru)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.PeruCodeTypes.DNI, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Peru));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Romania)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.RomaniaCodeTypes.CIF, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Romania));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.CostaRica)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(CostaRicaOrgCusCodeInfo.OrgCusCodes.IndividualIdentificationNumber, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.CostaRica));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.India)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(IndiaOrgCusCodeInfo.OrgCusCodes.PAN, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.India));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Madagascar)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.MadagascarCodeTypes.NIS, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Madagascar));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Lithuania)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.LithuaniaCodeTypes.IMK, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Lithuania));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Croatia)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.CroatiaCodeTypes.OIB, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Croatia));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Kosovo)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.KosovoCodeTypes.NFK, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Kosovo));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.BosniaAndHerzegovina)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.BosniaAndHerzegovinaCodeTypes.IDB, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.BosniaAndHerzegovina));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Angola)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(OrgCusCode.AngolaCodeTypes.NumeroDeIdentificacioFiscal, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Angola));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.ElSalvador)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(ElSalvadorOrgCusCodeInfo.OrgCusCodes.NIT, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.ElSalvador));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Constants.CountryCodes.Gabon)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(GabonOrgCusCodeInfo.OrgCusCodes.NIF, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Gabon));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Constants.CountryCodes.Guyana)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(GuyanaOrgCusCodeInfo.OrgCusCodes.TIN, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Guyana));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Constants.CountryCodes.Mauritania)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(MauritaniaOrgCusCodeInfo.OrgCusCodes.NIF, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Mauritania));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Constants.CountryCodes.Pakistan)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(PakistanOrgCusCodeInfo.OrgCusCodes.NTN, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Pakistan));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					else if (CurrentCompany.Country.Code == Constants.CountryCodes.Cyprus)
					{
						orgCusCode = TransactionHeader.Header.CustomsCodes.GetOrgCusCode(CyprusOrgCusCodeInfo.OrgCusCodes.TIC, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Cyprus));
						result = orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
					}
					#endregion
				}
				return result;
			}
		}

		public virtual ZString RecipientLocalBusinessReg2Heading
		{
			get
			{
				ZString result = ZString.Empty;

				if (!RecipientLocalBusinessReg2Number.IsEmpty && CurrentCompany != null && CurrentCompany.Country != null)
				{
					var recipientLocalBusinessReg2Heading = ObjectFactory.Get<ICountryComplianceFactoryIntegration>()?.GetICountryComplianceInfo(CurrentCompany.Country.Code)?.GetRecipientLocalBusinessReg2Heading();
					if (recipientLocalBusinessReg2Heading != null)
					{
						return recipientLocalBusinessReg2Heading;
					}

					#region This code design is obsolete please add new codes to through classes created by CountryComplianceFactory
					if (CurrentCompany.Country.Code == Constants.CountryCodes.KoreaSouth)
					{
						result = (NoResString)"Category"; // Korea Category
					}
					else if (CurrentCompany.Country.Code == Constants.CountryCodes.Italy)
					{
						result = (NoResString)"COD. UNIV. UFFICIO"; // Italy Category
					}
					else if (CurrentCompany.Country.Code == Constants.CountryCodes.India)
					{
						result = (NoResString)"Client GID"; // India GID
					}
					else if (CurrentCompany.Country.Code == Constants.CountryCodes.CostaRica)
					{
						if (DIMCustomCode != null)
						{
							result = (NoResString)"DIMEX #"; // Costa Rica DIM
						}
						else if (NITCustomCode != null)
						{
							result = (NoResString)"NITE #"; // Costa Rica NIT
						}
					}
					else if (CurrentCompany.Country.Code == Constants.CountryCodes.BosniaAndHerzegovina)
					{
						result = (NoResString)"MATICNOM BROJ"; // Italy Category
					}
					#endregion
				}

				return result;
			}
		}

		public virtual ZString RecipientLocalBusinessRegHeading
		{
			get
			{
				ZString result = ZString.Empty;

				if (CurrentCompany != null && CurrentCompany.Country != null && !RecipientLocalBusinessRegNumber.IsEmpty)
				{
					var recipientLocalBusinessRegHeading = ObjectFactory.Get<ICountryComplianceFactoryIntegration>()?.GetICountryComplianceInfo(CurrentCompany.Country.Code)?.GetRecipientLocalBusinessRegHeading();
					if (recipientLocalBusinessRegHeading != null)
					{
						return recipientLocalBusinessRegHeading;
					}

					#region This code design is obsolete please add new codes to through classes created by CountryComplianceFactory
					if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Italy)
					{
						result = (NoResString)"Codice Fiscale:"; // Italian words
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Morocco)
					{
						result = "ICE"; // Morocco ICE
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Greece)
					{
						result = (NoResString)"Client DOY:"; // Greece DOY
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Spain)
					{
						result = (NoResString)"Client DNI:"; // Spain DNI
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Denmark)
					{
						result = (NoResString)"EAN NUMBER"; // Denmark EAN
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.SriLanka)
					{
						result = (NoResString)"Client SVAT #"; // SriLanka SVAT
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Ethiopia
						|| CurrentCompany.Country.Code == Core.Constants.CountryCodes.Uganda)
					{
						result = "TIN:"; // TIN
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.France)
					{
						result = "SIRET/SIREN:"; // France SVAT
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Turkey)
					{
						result = Res.GetString("5b55c71f-8dbf-49a7-97df-da5805e1b2e9", "CLIENT TAX OFFICE:");
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Azerbaijan
						|| CurrentCompany.Country.Code == Core.Constants.CountryCodes.Tanzania)
					{
						result = (NoResString)"CLIENT TIN #"; // Azerbaijan TIN
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Kenya)
					{
						result = (NoResString)"CLIENT PIN #"; // Kenya PIN
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Mauritius)
					{
						result = (NoResString)"CLIENT BRN #"; // Mauritius BRN
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.KoreaSouth)
					{
						result = (NoResString)"Type"; // Korea Type
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Thailand)
					{
						result = (NoResString)"Client Branch"; // Thailand BID
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Peru)
					{
						result = (NoResString)"DNI #:"; // Peru DNI
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Romania)
					{
						result = (NoResString)"CLIENT CIF"; // Romania CIF
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.CostaRica)
					{
						result = (NoResString)"CÉD. FÍSICA #"; // CostaRica CID
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.India)
					{
						result = (NoResString)"CLIENT PAN #"; // India PAN
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Madagascar)
					{
						result = (NoResString)"N° Statistique:"; // Madagascar CID
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Lithuania)
					{
						result = (NoResString)"CLIENT ĮM.KODA #"; // Lithuania IM
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Croatia)
					{
						result = (NoResString)"CLIENT OIB #:"; // Croatia IM
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Kosovo)
					{
						result = (NoResString)"CLIENT NFK #:"; // Kosovo IM
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.BosniaAndHerzegovina)
					{
						result = (NoResString)"CLIENT ID #"; // BosniaAndHerzegovina IM
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.Angola)
					{
						result = (NoResString)"CLIENT NIF #"; // Angola IM
					}
					else if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.ElSalvador)
					{
						result = (NoResString)"CLIENT NIT #"; // El Salvador
					}
					else if (CurrentCompany.Country.Code == Constants.CountryCodes.Gabon)
					{
						result = GabonComplianceInfo.RecipientLocalBusinessRegHeading;
					}
					else if (CurrentCompany.Country.Code == Constants.CountryCodes.Guyana)
					{
						result = GuyanaComplianceInfo.RecipientLocalBusinessRegHeading;
					}
					else if (CurrentCompany.Country.Code == Constants.CountryCodes.Mauritania)
					{
						result = MauritaniaComplianceInfo.RecipientLocalBusinessRegHeading;
					}
					else if (CurrentCompany.Country.Code == Constants.CountryCodes.Pakistan)
					{
						result = PakistanComplianceInfo.RecipientLocalBusinessRegHeading;
					}
					else if (CurrentCompany.Country.Code == Constants.CountryCodes.Cyprus)
					{
						result = CyprusComplianceInfo.RecipientLocalBusinessRegHeading;
					}
					#endregion
				}

				return result;
			}
		}

		#region Recipient State and Country

		public ZString RecipientLocalState
		{
			get
			{
				if (TransactionHeader.Header?.MainAddress != null)
				{
					if (CurrentCompany.Country.Code == TransactionHeader.Header.MainAddress.OA_RN_NKCountryCode)
					{
						var query = new ZQuery(RefCountryStatesSchema.RW_Code, TransactionHeader.Header.MainAddress.OA_State);
						query.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, TransactionHeader.Header.MainAddress.OA_RN_NKCountryCode);

						var state = Factory.LoadTop1<RefCountryStates>(query);
						return state?.RW_DescriptionMultilingual ?? ZString.Empty;
					}
				}

				return ZString.Empty;
			}
		}

		public ZString RecipientLocalStateHeading
		{
			get
			{
				if (!RecipientLocalState.IsEmpty)
				{
					if (CurrentCompany.Country.Code == Constants.CountryCodes.India)
					{
						return (NoResString)"State of Supply"; // India RecipientLocalStateHeading
					}
					else
					{
						return (NoResString)"State"; // Other countries RecipientLocalStateHeading
					}
				}

				return ZString.Empty;
			}
		}

		public ZString RecipientCountryName
		{
			get
			{
				if (TransactionHeader.Header?.MainAddress != null)
				{
					return Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, TransactionHeader.Header.MainAddress.OA_RN_NKCountryCode)?.RN_DescMultilingual ?? ZString.Empty;
				}

				return ZString.Empty;
			}
		}

		public ZString RecipientCountryNameHeading
		{
			get
			{
				return CurrentCompany.Country.Code == Constants.CountryCodes.India ? (NoResString)"Country of Supply" : (NoResString)"Country";
			}
		}

		#endregion

		#region Transaction Header Branch Reg

		public ZString BranchTaxIDNumber
		{
			get
			{
				switch (CurrentCompany.Country.Code)
				{
					case Constants.CountryCodes.India:
					case Constants.CountryCodes.UnitedArabEmirates:
					case Constants.CountryCodes.Bahrain:
					case Constants.CountryCodes.Kuwait:
					case Constants.CountryCodes.Oman:
					case Constants.CountryCodes.Qatar:
					case Constants.CountryCodes.Sudan:
					case Constants.CountryCodes.SaudiArabia:
					case Constants.CountryCodes.Djibouti:
						return (TransactionHeader.Branch.OrgProxy ?? TransactionHeader.Company.OrgProxy)?.RawTaxRegistrationNumber ?? ZString.Empty;
					case Constants.CountryCodes.Ecuador:
						return GetTaxIdForEcuador();
					default:
						return ZString.Empty;
				}
			}
		}

		string GetTaxIdForEcuador()
		{
			var orgProxy = (TransactionHeader.Branch.OrgProxy ?? TransactionHeader.Company.OrgProxy);
			if (orgProxy != null)
			{
				var cusCode = orgProxy.CustomsCodes.GetOrgCusCode(OrgCusCode.EcuadorCodeTypes.SRI, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Ecuador));
				return cusCode?.OK_CustomsRegNo ?? ZString.Empty;
			}

			return ZString.Empty;
		}

		public ZString BranchTaxIDHeading
		{
			get
			{
				switch (CurrentCompany.Country.Code)
				{
					case Constants.CountryCodes.India:
						return "GSTIN"; // BranchTaxIDHeading for India
					case Constants.CountryCodes.UnitedArabEmirates:
					case Constants.CountryCodes.Bahrain:
					case Constants.CountryCodes.Kuwait:
					case Constants.CountryCodes.Oman:
					case Constants.CountryCodes.Qatar:
					case Constants.CountryCodes.Sudan:
					case Constants.CountryCodes.SaudiArabia:
						return (NoResString)"VAT #"; // BranchTaxIDHeading for GCC 6 Countries
					case Constants.CountryCodes.Djibouti:
						return (NoResString)"NIF #"; // BranchTaxIDHeading for Djibouti
					case Constants.CountryCodes.Ecuador:
						return (NoResString)"AUT. SRI"; // BranchTaxIDHeading for Ecuador
					default:
						return ZString.Empty;
				}
			}
		}

		public ZString BranchBusRegNumber
		{
			get
			{
				OrgHeader orgProxy;
				switch (CurrentCompany.Country.Code)
				{
					case Constants.CountryCodes.India:
						orgProxy = TransactionHeader.Branch.OrgProxy ?? TransactionHeader.Company.OrgProxy;
						return orgProxy.CustomsCodes.GetOrgCusCode(IndiaOrgCusCodeInfo.OrgCusCodes.PAN, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.India))?.OK_CustomsRegNo ?? ZString.Empty;
					case Constants.CountryCodes.Ecuador:
						orgProxy = TransactionHeader.Branch.OrgProxy ?? TransactionHeader.Company.OrgProxy;
						return orgProxy.CustomsCodes.GetOrgCusCode(OrgCusCode.EcuadorCodeTypes.SRF, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Ecuador))?.OK_CustomsRegNo ?? ZString.Empty;
					default:
						return string.Empty;
				}
			}
		}

		public ZString BranchBusRegHeading
		{
			get
			{
				switch (CurrentCompany.Country.Code)
				{
					case Constants.CountryCodes.India:
						return "PAN"; // BranchBusRegHeading
					case Constants.CountryCodes.Ecuador:
						return (NoResString)"FECHA AUT. SRI"; // BranchBusRegHeading
					default:
						return string.Empty;
				}
			}
		}

		internal ZString BranchFullName
		{
			get
			{
				var orgProxy = TransactionHeader.Branch.OrgProxy ?? TransactionHeader.Company.OrgProxy;
				return orgProxy.OH_FullName;
			}
		}

		#endregion

		public ZString StoredInvoiceRecepientNameandAddress
		{
			get
			{
				var result = new ZStringBuilder();

				if (AccountingUtils.ShouldPortugalStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting(TransactionHeader.AH_Ledger, TransactionHeader.AH_TransactionType))
				{
					if (TransactionHeader is InvoicingBase invoicingBase)
					{
						var jobDocAddress = invoicingBase.DocAddresses.FindByDocAddressType(DocAddressType.DebtorAddress);
						var invalidGovNum = new ZString[] { ZString.Empty, "PT999999990", "999999990", "XXXXXXXXX" };
						if (jobDocAddress != null && !invalidGovNum.Contains(jobDocAddress.E2_GovRegNum))
						{
							var docDocAddress = DocDocAddress.New(jobDocAddress, Factory);
							if (docDocAddress != null)
							{
								var lines = docDocAddress.PostalAddress.Split('\n');
								result.AppendIfNotEmpty(lines[0].Trim());
								result.AppendIfNotEmpty(docDocAddress.ContactName);

								for (int i = 1; i < lines.Length; i++)
								{
									result.AppendIfNotEmpty(lines[i].Trim());
								}
							}
						}
					}
				}

				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString MailToAddressWithCountry
		{
			get
			{
				var mailToAddressWithCountryFromJobDocAddressForPT = GetMailToAddressWithCountryFromJobDocAddressForPT();
				if (!mailToAddressWithCountryFromJobDocAddressForPT.IsEmpty)
				{
					return mailToAddressWithCountryFromJobDocAddressForPT;
				}

				ZString result = ZString.Empty;
				if (Branch != null && Branch.MailToAddress != null)
				{
					DocAddress docAddress = Branch.MailToAddress;
					if (!BrandName.IsEmpty)
					{
						result = BrandName + "\n" + docAddress.PostalAddressExcludeName;
					}
					else
					{
						result = docAddress.PostalAddress;
					}
				}
				return result;
			}
		}

		ZString GetMailToAddressWithCountryFromJobDocAddressForPT()
		{
			var result = ZString.Empty;

			if (AccountingUtils.ShouldPortugalStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting(TransactionHeader.AH_Ledger, TransactionHeader.AH_TransactionType))
			{
				if (TransactionHeader is InvoicingBase invoicingBase)
				{
					var jobDocAddress = invoicingBase.DocAddresses.FindByDocAddressType(DocAddressType.BranchOrCompanyProxyARAdress);
					if (jobDocAddress != null)
					{
						var docDocAddress = DocDocAddress.New(jobDocAddress, Factory);
						if (docDocAddress != null)
						{
							if (!BrandName.IsEmpty)
							{
								result = BrandName + "\n" + docDocAddress.PostalAddressExcludeName;
							}
							else
							{
								result = docDocAddress.PostalAddress;
							}
						}
					}
				}
			}

			return result;
		}

		public ZString StampDutyARDocumentMessage
		{
			get
			{
				var result = ZString.Empty;
				var registryValue = AccountingConfigurationRegistry.Instance.StampDutyARDocumentMessage.GetValueWithoutFallback(TransactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty);
				if (!registryValue.IsEmpty &&
					(
						TransactionHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StampDutyLiabilityCode)).Any(x => !x.SL_IsCancelled) ||
						(TransactionHeader is InvoicingBase && ((InvoicingBase)TransactionHeader).Lines.Cast<InvoicingLineBase>().Any(x => x.IsStampDutyChargeLine()))
					))
				{
					result = registryValue;
				}
				return result;
			}
		}

		protected bool DoesTransactionHaveREGLineAndOrgRecordedIGIC(OrgHeader org)
		{
			return HasREGLineForSpain && IsIGICRecordedAgainstOrg(org);
		}

		protected virtual ZBool HasREGLineForSpain
		{
			get
			{
				return InvoiceLine.Cast<DocARInvoiceLine>().Any(x => x.TaxRate != null && x.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.Spain && x.TaxRate.ExtraType == AccTaxRate.ExtraTypes.RegionalTax);
			}
		}

		protected ZBool IsIGICRecordedAgainstOrg(OrgHeader org)
		{
			return org != null && org.CustomsCodes.Cast<OrgCusCode>().Any(x => x.OK_CodeType == OrgCusCode.SpainCodeTypes.IGC);
		}

		protected override ZString DefaultBrandName
		{
			get
			{
				if (AccountingConfigurationRegistry.Instance.PrintBranchAddressInFooter.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty))
				{
					return Branch.MailToAddress.CompanyName;
				}
				else
				{
					return Branch.MailToAddress.CompanyNameOverride.IsEmpty ? base.DefaultBrandName : Branch.MailToAddress.CompanyNameOverride;
				}
			}
		}

		public ZString AccountCode => IsRegistrationNumberAvailable()
			? AccountOrg?.Code ?? ZString.Empty
			: TransactionHeader.PortugalAccountCodeForMissingRegistrationNumber;

		public DocAddress RecipientTaxIDPremisesAddress
		{
			get
			{
				DocAddress result = null;

				if (!RecipientTaxIDNumber.IsEmpty
					&& TransactionHeader?.Header?.PrimaryRegistrationNumber?.CusCode?.PremisesAddress != null)
				{
					var addressPk = TransactionHeader.Header.PrimaryRegistrationNumber.CusCode.PremisesAddress.PK;
					result = DocAddress.New(Factory.Load<OrgAddress>(addressPk), Factory);
				}

				return result;
			}
		}

		public DocAddress RecipientTaxIDPremisesAddressInRecipientCountry
		{
			get
			{
				DocAddress result = null;
				var header = TransactionHeader?.Header;
				if (header != null && !RecipientTaxIDNumberInRecipientCountry.IsEmpty)
				{
					var (countryCode, registrationNumber) = header.GetCountryCodeAndTaxRegistrationWithoutPrefix(TransactionHeader.InvoiceAddressOverride);
					var orgCusCode = header.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => countryCode == c.OK_RN_NKCodeCountry && registrationNumber == c.OK_CustomsRegNo);
					var addressPk = orgCusCode?.PremisesAddress?.PK;
					if (addressPk.HasValue)
					{
						result = DocAddress.New(Factory.Load<OrgAddress>(addressPk.Value), Factory);
					}
				}

				return result;
			}
		}

		public bool ShowRecipientNameAndAddress => IsRegistrationNumberAvailable();

		bool IsRegistrationNumberAvailable() => CurrentCompany.Country.Code != Core.Constants.CountryCodes.Portugal
			|| TransactionHeader.GetTransactionHeaderReferenceToValidateMissingRegistrationNumber(AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IVA) == null;

		public ZString AccountName
		{
			get
			{
				ZString result = ZString.Empty;
				if (AccountOrgAddress != null)
				{
					result = AccountOrgAddress.Organisation.Name;
				}

				if (result.IsEmpty)
				{
					result = AccountOrg != null ? AccountOrg.Name : ZString.Empty;
				}

				return result;
			}
		}

		public ZString AccountFullName
		{
			get
			{
				return InvoicingOrgHeader != null ? InvoicingOrgHeader.OH_FullName : ZString.Empty;
			}
		}

		public ZString OrganisationARAgreedPaymentMethod
		{
			get
			{
				return TransactionARPaymentMethod;
			}
		}

		public ZString OrganisationAPAgreedPaymentMethod
		{
			get
			{
				return string.IsNullOrWhiteSpace(TransactionHeader.AH_AgreedPaymentMethodOverride)
					? string.Empty
					: Env.Registry.PayablesCreditAgreedPaymentMethodsList.GetDescriptionFromCode(TransactionHeader.AH_AgreedPaymentMethodOverride);
			}
		}

		public ZString ElectronicPaymentBillerCode
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.ElectronicPaymentBillerCode.Value;
			}
		}

		public ZString ElectronicPaymentTerms
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.ElectronicPaymentTerms.Value;
			}
		}

		#region Header Fixed Place of Supply

		public ZString FixedPlaceOfSupplyLabel
		{
			get
			{
				var result = ZString.Empty;
				if (TransactionHeader.AH_PlaceOfSupplyType.IsEmpty)
				{
					result = RecipientLocalStateHeading;
				}
				else if (TransactionHeader.AH_PlaceOfSupplyType == PlaceOfSupplyTypes.State.Code)
				{
					result = Res.GetString("45A4F9C2-8DD1-4D95-A3DF-EC2104F77C07", "State of Supply");
				}
				else if (TransactionHeader.AH_PlaceOfSupplyType == PlaceOfSupplyTypes.PredefinedRule.Code)
				{
					result = Res.GetString("66E2D17E-0EC8-4F73-955B-264CB2135124", "Place of Supply");
				}
				return result;
			}
		}

		public ZString FixedPlaceOfSupply
		{
			get
			{
				var result = ZString.Empty;
				if (TransactionHeader.AH_PlaceOfSupplyType.IsEmpty)
				{
					result = RecipientLocalState;
				}
				else if (TransactionHeader.AH_PlaceOfSupplyType == PlaceOfSupplyTypes.State.Code)
				{
					result = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(TransactionHeader.Company).GetDescriptionFromCode(TransactionHeader.AH_PlaceOfSupply) ?? ZString.Empty;
				}
				else if (TransactionHeader.AH_PlaceOfSupplyType == PlaceOfSupplyTypes.PredefinedRule.Code)
				{
					if (TransactionHeader.AH_PlaceOfSupply == PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry)
					{
						result = Res.GetString("0F6FD5E8-F188-420C-9F57-7DBA1A0B9780", "Foreign Country/Region");
					}
					else if (TransactionHeader.AH_PlaceOfSupply == PlaceOfSupplyListProvider.Codes.OtherTerritories)
					{
						result = Res.GetString("c42498e4-25cf-4c10-b7e1-bf31ade287cf", "Other Territories");
					}
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region ZBool

		public ZBool ShowInvoiceTotalsbyTaxRate
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.DisplayInvoiceTotalsbyTaxRate.Value;
			}
		}

		public ZBool ShowElectronicPaymentsDetails
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.EnableElectronicPayments.Value;
			}
		}

		public virtual ZBool IsTaxed
		{
			get { return false; }
		}

		public ZBool ShowBankDetails
		{
			get { return !IsCreditNote; }
		}

		public virtual ZBool ShowFooter
		{
			get { return ShowFooterCore; }
		}

		protected virtual ZBool ShowFooterCore
		{
			get { return ZBool.True; }
		}

		#endregion

		#endregion

		public DocARInvoiceLineCollection InvoiceLine
		{
			get
			{
				return fInvoiceLine ?? (fInvoiceLine = GetInvoiceLines());
			}
		}
		DocARInvoiceLineCollection fInvoiceLine;

		protected abstract DocARInvoiceLineCollection GetInvoiceLines();

		#region InvoiceLineByCharge

		public DocARInvoiceLineCollection InvoiceLineByCharge
		{
			get
			{
				if (fInvoiceLineByCharge == null)
				{
					fInvoiceLineByCharge = GetInvoiceLineByChargeCore();
				}
				return fInvoiceLineByCharge;
			}
		}

		DocARInvoiceLineCollection fInvoiceLineByCharge;

		protected virtual DocARInvoiceLineCollection GetInvoiceLineByChargeCore()
		{
			DocARInvoiceLineCollection invoiceLineByCharge = null;
			if (this is DocARInvoice)
			{
				this.InvoiceLine.ResetMultiplierTo1();
			}
			var grouper = new RollUpGrouperByChargeAndTax(DocLineRollUpper, this, Factory, this.InvoiceLine);
			invoiceLineByCharge = grouper.RollUp();
			invoiceLineByCharge.Sort("LineDescription", ListSortDirection.Ascending);
			return invoiceLineByCharge;
		}

		#endregion

		#region InvoiceLineByJob

		public DocARInvoiceLineCollection InvoiceLineByJob
		{
			get
			{
				if (fInvoiceLineByJob == null)
				{
					fInvoiceLineByJob = GetInvoiceLineByJobCore();
				}
				return fInvoiceLineByJob;
			}
		}

		DocARInvoiceLineCollection fInvoiceLineByJob;

		protected virtual DocARInvoiceLineCollection GetInvoiceLineByJobCore()
		{
			DocARInvoiceLineCollection invoiceLineByJob;

			if (this is DocARInvoice)
			{
				this.InvoiceLine.ResetMultiplierTo1();
			}

			if (AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule == AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code)
			{
				var grouper = new RollUpGrouperByJob(DocLineRollUpper, this, Factory, this.InvoiceLine);
				invoiceLineByJob = grouper.RollUp();
			}
			else
			{
				var grouper = new RollUpGrouperByJobAndTaxRate(DocLineRollUpper, this, Factory, this.InvoiceLine);
				invoiceLineByJob = grouper.RollUp();
			}

			invoiceLineByJob.Sort("JobNumber", ListSortDirection.Ascending);

			return invoiceLineByJob;
		}

		#endregion

		#region InvoiceLineByNON

		public DocARInvoiceLineCollection InvoiceLineByNON
		{
			get
			{
				if (invoiceLineByNON == null)
				{
					invoiceLineByNON = GetInvoiceLineByNONCore();
				}
				return invoiceLineByNON;
			}
		}

		DocARInvoiceLineCollection invoiceLineByNON;

		protected virtual DocARInvoiceLineCollection GetInvoiceLineByNONCore()
		{
			return new DocARInvoiceLineCollection(Factory);
		}

		#endregion

		#region Utility Method

		protected ZString StripConsolidatedInvoiceRefOfEndChars()
		{
			ZString result = ConsolidatedInvoiceRef;
			ZString[] splitConsolidatedInvoiceRef = result.Split('/');

			if (splitConsolidatedInvoiceRef.Length > 1)
			{
				result = splitConsolidatedInvoiceRef[0];
			}

			return result.Trim();
		}

		#region Is* Boolean Method

		public ZBool IsPeriodicInvoice
		{
			get { return InvoiceTypeCalculationProvider.IsDeferredInvoiceType(TransactionHeader.AH_TransactionCategory); }
		}

		protected bool IsCreditNote
		{
			get { return TransactionType == ZArchitecture.Core.TransactionTypes.CreditNote; }
		}

		protected bool IsISFJob()
		{
			return (JobHeader != null && JobHeader.ParentTableCode == CusISFHeaderSchema.Constants.Prefix);
		}

		protected bool IsDeclarationJob()
		{
			return (JobHeader != null && JobHeader.ParentTableCode == JobDeclarationSchema.Constants.Prefix);
		}

		protected bool IsCusInBondHeaderJob()
		{
			return (JobHeader != null && JobHeader.ParentTableCode == CusInBondHeaderSchema.Constants.Prefix);
		}

		protected bool IsWorkItemJob()
		{
			return (JobHeader != null && JobHeader.ParentTableCode == WorkItemSchema.Constants.Prefix);
		}

		protected bool IsShipmentJob()
		{
			return (JobHeader != null && JobHeader.ParentTableCode == JobShipmentSchema.Constants.Prefix);
		}

		protected bool IsConsolJob()
		{
			return (!TransactionHeader.AH_ConsolidatedInvoiceRef.IsEmpty && TransactionHeader.AH_JH.IsEmpty);
		}

		protected bool IsLoadListJob()
		{
			return (JobHeader != null && JobHeader.ParentTableCode == JobConsolSchema.Constants.Prefix);
		}

		protected bool IsContainerRegistrationJob()
		{
			return (JobHeader != null && JobHeader.ParentTableCode == JobContainerSchema.Constants.Prefix);
		}

		protected bool IsCartageJob()
		{
			return (JobHeader != null && JobHeader.ParentTableCode == JobCartageSchema.Constants.Prefix);
		}

		protected bool IsWarehousePeriodicJob()
		{
			return (JobHeader != null && JobHeader.ParentTableCode == JobStorageSchema.Constants.Prefix);
		}

		protected bool IsWarehouseOperationsJob()
		{
			return (JobHeader != null && JobHeader.ParentTableCode == WhsDocketSchema.Constants.Prefix);
		}

		protected bool IsWarehouseStocktakeJob()
		{
			return (JobHeader != null && JobHeader.ParentTableCode == WhsStocktakeSchema.Constants.Prefix);
		}

		#endregion

		#endregion

		#region Periodic Invoicing & Batch Invoice Shared Methods

		public string GetChargeDescription(InvoicingLineBase line)
		{
			return UseLocalLanguage(line) ? line.ChargeCode.AC_LocalLanguageDescription : line.ChargeCode.AC_DescMultilingual;
		}

		bool UseLocalLanguage(InvoicingLineBase line)
		{
			return AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.Value &&
				!line.ChargeCode.AC_LocalLanguageDescription.IsEmpty &&
				(AccountOrg.Country.Code == GlbCompany.CurrentCompany.GC_RN_NKCountryCode || AccountingConfigurationRegistry.Instance.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.Value);
		}

		public ZString GetLineDescription(ZString moduleCode, InvoicingLineBase line)
		{
			ZString result = ZString.Empty;
			DocARInvoiceLine invoiceLine = DocARInvoiceLine.New(line, Factory);

			if (invoiceLine != null)
			{
				switch (moduleCode)
				{
					case InvoiceTypeModuleList.Codes.CFS:
						result = invoiceLine.ClientReference;
						break;

					case InvoiceTypeModuleList.Codes.TCN:
						result = invoiceLine.OtherReference;
						break;

					case InvoiceTypeModuleList.Codes.TPT:
						result = invoiceLine.OtherReference;
						break;

					case InvoiceTypeModuleList.Codes.FWD:
						if (invoiceLine.Shipment != null)
						{
							if (invoiceLine.Shipment.IsAgencyShipping)
							{
								if (invoiceLine.Shipment.IsAgencyShipmentBooking)
								{
									if (!string.IsNullOrWhiteSpace(invoiceLine.Shipment.BookingReference))
									{
										result = Res.GetString("5f478e65-5485-4482-8643-23e85c711e5c", "Booking Ref: {0}", invoiceLine.Shipment.BookingReference);
									}
								}
								else
								{
									if (!string.IsNullOrWhiteSpace(invoiceLine.Shipment.MasterBillNum))
									{
										result = Res.GetString("c4016dfe-2bbc-4c58-bc3a-80755f707b2a", "Bill of Lading: {0}", invoiceLine.Shipment.MasterBillNum);
									}
								}
							}
							else
							{
								result = invoiceLine.Shipment.HouseBill;
							}
						}
						break;

					case InvoiceTypeModuleList.Codes.CUS:
						if (invoiceLine.Customs != null)
						{
							result = invoiceLine.Customs.HouseBill;
						}

						break;

					case InvoiceTypeModuleList.Codes.MSC:
						if (invoiceLine.Customs != null && invoiceLine.Customs.HouseBill != "")
						{
							result = Res.GetString("cea2c858-ee38-4f2b-8ba5-11d0038044f1", "House: {0}", invoiceLine.Customs.HouseBill) + " ";
						}

						if (invoiceLine.OtherReference != "")
						{
							result += Res.GetString("2f9bb010-16d9-47bc-ac04-74e5feb1f028", "Other: {0}", invoiceLine.OtherReference);
						}
						break;
				}
			}
			return result;
		}

		public ZString GetInvoiceModule(JobHeader job)
		{
			ZString result = ZString.Empty;
			if (job != null)
			{
				if (job.JH_ParentTableCode == JobShipmentSchema.Constants.Prefix)
				{
					ForwardingShipment shipment = Factory.Load<ForwardingShipment>(job.JH_ParentID);
					if (shipment != null && shipment.JS_IsCFSRegistered && !shipment.JS_IsForwardRegistered)
					{
						result = InvoiceTypeModuleList.Codes.CFS;
					}
					else
					{
						result = InvoiceTypeModuleList.Codes.FWD;
					}
				}
				else if (job.JH_ParentTableCode == JobDeclarationSchema.Constants.Prefix)
				{
					result = InvoiceTypeModuleList.Codes.CUS;
				}
				else if (job.JH_ParentTableCode == DtbBookingSchema.Constants.Prefix)
				{
					result = InvoiceTypeModuleList.Codes.TCN;
				}
				else if (job.JH_ParentTableCode == JobCartageSchema.Constants.Prefix)
				{
					result = InvoiceTypeModuleList.Codes.TPT;
				}
				else if (job.JH_ParentTableCode == JobConsolSchema.Constants.Prefix)
				{
					result = InvoiceTypeModuleList.Codes.CFS;
				}
				else if (job.JH_ParentTableCode == CusISFHeaderSchema.Constants.Prefix)
				{
					result = InvoiceTypeModuleList.Codes.ISF;
				}
			}
			else
			{
				result = InvoiceTypeModuleList.Codes.MSC;
			}
			return result;
		}

		#endregion

		protected bool IsLocalInvoice()
		{
			if (AccountOrg != null && AccountOrg.Country != null)
			{
				return AccountOrg.Country.Code == GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}

			return false;
		}

		protected override Image GetInvoiceLogoCore()
		{
			return InvoiceLogo;
		}

		protected override ZString GetAccountCodeCore()
		{
			return AccountCode;
		}

		protected override ZString GetAccountNameCore()
		{
			return AccountName;
		}

		protected override ZString GetAccountFullNameCore()
		{
			return AccountFullName;
		}

		protected override ZString GetOrganisationARAgreedPaymentMethodCore()
		{
			return OrganisationARAgreedPaymentMethod;
		}

		protected override ZString GetOrganisationAPAgreedPaymentMethodCore()
		{
			return OrganisationAPAgreedPaymentMethod;
		}

		protected override ZBool GetIsTaxedCore()
		{
			return IsTaxed;
		}

		protected const string InvoiceTypePeriodicInvoice = "PeriodicInvoice";
		protected const string InvoiceTypeAgencyShipment = "AgencyShipment";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected const string InvoiceTypeShipment = "Shipment";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public const string InvoiceTypeConsol = "Consol";
		protected const string InvoiceTypeLoadList = "LoadList";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected const string InvoiceTypeCustoms = "Customs";
		protected const string InvoiceTypeReconDeclaration = "ReconDeclaration";
		protected const string InvoiceTypeImporterSecurityFiling = "ImporterSecurityFiling";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected const string InvoiceTypeTransport = "Transport";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected const string InvoiceTypeMisc = "Miscellaneous";
		protected const string InvoiceTypeNctsHeader = "NctsHeader";

		protected const string InvoiceTypeContainerRegistration = "ContainerRegistration";
		protected const string InvoiceTypeShipmentReceivals = "ShipmentReceivalsAndGatePass";
		protected const string InvoiceTypeWarehousePeriodic = "WarehousePeriodic";
		protected const string InvoiceTypeWarehouseWhsInwards = "WarehouseWhsInwards";
		protected const string InvoiceTypeWarehouseWhsOrder = "WarehouseWhsOrder";
		protected const string InvoiceTypeWarehouseWhsStocktake = "WarehouseWhsStocktake";
		protected const string InvoiceTypeWarehouseAdHocServiceJob = "WarehouseAdHocServiceJob";

		protected virtual BaseInvoiceDocLineRollUpper DocLineRollUpper => new BaseInvoiceDocLineRollUpper(this);
	}
}
