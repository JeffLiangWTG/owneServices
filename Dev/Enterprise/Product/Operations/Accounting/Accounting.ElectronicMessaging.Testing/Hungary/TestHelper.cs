using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary.Testing
{
	public static class TestHelper
	{
		public static GlbBranch CreateHungaryBranchWithAddressAndTaxNumber(this TestObjectCreator creator)
		{
			var company = creator.CreateCompanyAndBranch("HUCSA");
			var branch = company.FirstActiveBranch;
			branch.GB_RL_NKHomePort = "HUCSA";
			new EInvoicingTestHelper(creator).AddCustomsCodeForCountryIfMissing(branch.OrgProxy, Core.Constants.CountryCodes.Hungary, "VAT", "12345678");
			creator.CreateAddress(branch.OrgProxy,
				OrgAddressType.Office,
				true,
				"Tropicana Las Vegas Casino",
				"Vigadó u. 2",
				"Budapest",
				"BU",
				"HU",
				"1051",
				"+36 1 266 2081",
				"noreply@lasvegascasino.hu");
			return branch;
		}

		public static GlbCompanyExternalPasswordHUI CreateHungaryEInvoicingCredentials(this TestObjectCreator creator, GlbCompany company)
		{
			var credential = creator.Factory.New<GlbCompanyExternalPasswordHUI>();
			credential.GP_GC = company.PK;
			credential.Login = "h9nupbpmgi8yhet";
			credential.PasswordHash = "3581511529344BF49B6B93A44BDEDBE8AED53942D04F45D6BA0702E2E5F7A928667FB0C11CDEBA39A45D010FBD89C1F951CE9FF1B6789412780C70613CDAB56B";
			credential.SignatureKey = "fb-8530-0ead7916e5432DA4ZSW1UKWX";
			credential.ReplacementKey = "8a042DA4ZSW17HY6";
			credential.GP_PasswordStatus = "OK";
			return credential;
		}

		public static string GetEmbeddedResourceTestCaseAsUtf8String(string resourceName)
		{
			var fileName = "Enterprise.Accounting.ElectronicMessaging.Testing.Hungary.XmlBuilders.TestCases." + resourceName;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
			{
				if (stream == null)
				{
					throw new Exception($"Cannot find resource '{resourceName}'. Full name: {fileName}");
				}

				using (var reader = new StreamReader(stream, MessageEncoding.UTF8WithoutBOM))
				{
					return reader.ReadToEnd().Replace("\t", "  ");
				}
			}
		}

		#region Implementation

		public static (TransactionInfo transactionInfo, HungaryTransactionExtraInfo extraInfo) GetTransactionInfo(
			string invoiceNumber = "AR001000",
			TransactionType transactionType = TransactionType.INV,
			string invoiceCategory = "FIN",
			string currencyCode = "HUF",
			decimal exchangeRate = 1.0m,
			string companyVatNumber = "12036024",
			string companyGbrNumber = "81237290",
			string companyAddressLine2 = "Second address line",
			string debtorVatNumber = "43729838",
			string debtorGbrNumber = "76803943",
			string debtorAddressLine2 = "Line number two",
			ZDate? dueDate = null,
			List<PostingJournal> lineItems = null)
		{
			var address1 = new UniversalDataBuss.DataObjects.Universal.OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CompanyName = "Tropicana Las Vegas Casino",
				Address1 = "Vigadó u. 2",
				Address2 = companyAddressLine2,
				City = "Budapest",
				State = "BU",
				Postcode = "1051",
				Country = new UniversalDataBuss.DataObjects.Universal.Country() { Name = "Hungary", Code = "HU" },
			};
			address1.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>()
			{
				new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
				{
					CountryOfIssue = new UniversalDataBuss.DataObjects.Universal.Country() { Code = "HU" },
					Type = new UniversalDataBuss.DataObjects.Universal.RegistrationNumberType() { Code = "VAT" },
					Value = companyVatNumber,
				},
				new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
				{
					CountryOfIssue = new UniversalDataBuss.DataObjects.Universal.Country() { Code = "HU" },
					Type = new UniversalDataBuss.DataObjects.Universal.RegistrationNumberType() { Code = "GBR" },
					Value = companyGbrNumber,
				}
			});
			var address2 = new UniversalDataBuss.DataObjects.Universal.OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CompanyName = "Középkori Romkert (Medieval Ruin Garden)",
				Address1 = "Koronázó tér 1",
				Address2 = debtorAddressLine2,
				City = "Székesfehérvár",
				Postcode = "8000",
				Country = new UniversalDataBuss.DataObjects.Universal.Country() { Name = "Hungary", Code = "HU" },
			};
			address2.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>()
			{
				new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
				{
					CountryOfIssue = new UniversalDataBuss.DataObjects.Universal.Country() { Code = "HU" },
					Type = new UniversalDataBuss.DataObjects.Universal.RegistrationNumberType() { Code = "VAT" },
					Value = debtorVatNumber
				},
				new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
				{
					CountryOfIssue = new UniversalDataBuss.DataObjects.Universal.Country() { Code = "HU" },
					Type = new UniversalDataBuss.DataObjects.Universal.RegistrationNumberType() { Code = "GBR" },
					Value = debtorGbrNumber
				},
			});
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = transactionType,
				Number = invoiceNumber,
				TransactionDate = ZDateTime.Today,
				Branch = new Branch() { Code = "BHU", Name = "BHU Name" },
				Category = invoiceCategory,
				LocalCurrency = new UniversalDataBuss.DataObjects.Universal.Currency() { Code = "HUF" },
				OSCurrency = new UniversalDataBuss.DataObjects.Universal.Currency() { Code = currencyCode },
				ExchangeRate = exchangeRate,
				BranchAddress = address1,
				OrganizationAddress = address2,
				DueDate = dueDate ?? ZDateTime.Today,
			};
			transaction.SetPostingJournalCollection(() => lineItems ?? TestHelper.DefaultLineItems());
			transaction.LocalTotal = transaction.PostingJournalCollection.Sum(l => l.LocalTotalAmount ?? 0);
			transaction.OSTotal = transaction.PostingJournalCollection.Sum(l => l.OSTotalAmount ?? 0);
			return (transaction, new HungaryTransactionExtraInfo());
		}

		public static List<PostingJournal> DefaultLineItems()
			=> new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 1,
					Description = "Description of line 1",
					OSAmount = 100m,
					OSGSTVATAmount = 27m,
					OSTotalAmount = 127m,
					LocalAmount = 100m,
					LocalGSTVATAmount = 27m,
					LocalTotalAmount = 127m,
					VATTaxID = new TaxID() { TaxCode = "VAT", TaxRate = 27, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
					ChargeExchangeRate = 1.0m,
				},
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 2,
					Description = "The second description",
					OSAmount = 200m,
					OSGSTVATAmount = 0m,
					OSTotalAmount = 200m,
					LocalAmount = 200m,
					LocalGSTVATAmount = 0m,
					LocalTotalAmount = 200m,
					VATTaxID = new TaxID() { TaxCode = "FREEVAT", TaxRate = 0, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
					TaxMessageID = new TaxMessageID() { Description = "Some reason why this was excluded from paying tax that exceeds 50 characters", TaxMessageCode = "NPS", TaxGroupCode = GetVATOutOfScopeGroupCode() },
					ChargeExchangeRate = 1.0m,
				},
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 3,
					Description = @"

Description with carriage return
we expect the carriage return to be replaced by a whitespace

even if there are multiple lines


ok

",
					OSAmount = 300m,
					OSGSTVATAmount = 0m,
					OSTotalAmount = 300m,
					LocalAmount = 300m,
					LocalGSTVATAmount = 0m,
					LocalTotalAmount = 300m,
					VATTaxID = new TaxID() { TaxCode = "NOTREPORT", TaxRate = 0, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "NOT" } },
					TaxMessageID = new TaxMessageID() { Description = "Not reportable", TaxMessageCode = "NOT", TaxGroupCode = GetVATExemptGroupCode()  },
					ChargeExchangeRate = 1.0m,
				},
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 4,
					Description = "Fourth line item that exceeds 512 characters... Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque in neque magna. Vestibulum quis fermentum est. Sed malesuada nec nibh eu faucibus. Donec gravida aliquet ex ac sagittis. Praesent suscipit orci et felis maximus fermentum. Nam vel consequat nisi, sed mollis massa. Etiam sed egestas ipsum. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Proin lobortis est sit amet orci convallis, eu dignissim mauris feugiat. Proin ultricies aliquet gravida. Nam luctus tortor sit amet tortor pulvinar aliquam.",
					OSAmount = 400m,
					OSGSTVATAmount = 108m,
					OSTotalAmount = 508m,
					LocalAmount = 400m,
					LocalGSTVATAmount = 108m,
					LocalTotalAmount = 508m,
					VATTaxID = new TaxID() { TaxCode = "VATREV", TaxRate = 27, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RVS" } },
					ChargeExchangeRate = 1.0m,
				},
			};

		#endregion

		static UniversalDataBuss.DataObjects.Universal.TaxGroupCodeType GetVATExemptGroupCode()
		{
			var taxGroup = GetHungaryTaxGroups().First(c => c.Code == HungaryComplianceInfo.TaxMessageGroupCodes.H01);
			return GetTaxGroupCodeType(taxGroup);
		}

		static UniversalDataBuss.DataObjects.Universal.TaxGroupCodeType GetVATOutOfScopeGroupCode()
		{
			var taxGroup = GetHungaryTaxGroups().First(c => c.Code == HungaryComplianceInfo.TaxMessageGroupCodes.H21);
			return GetTaxGroupCodeType(taxGroup);
		}

		static UniversalDataBuss.DataObjects.Universal.TaxGroupCodeType GetTaxGroupCodeType(CodeDescriptionBoolRelatedItem taxGroup)
		{
			return new UniversalDataBuss.DataObjects.Universal.TaxGroupCodeType()
			{
				Code = taxGroup.Code,
				Description = taxGroup.Description,
				GovernmentCode = taxGroup.RelatedItemCode
			};
		}

		static IEnumerable<CodeDescriptionBoolRelatedItem> GetHungaryTaxGroups() => CountryComplianceFactory.GetITaxMessageGroupProvider(Core.Constants.CountryCodes.Hungary).GetTaxMessageGroup().OfType<CodeDescriptionBoolRelatedItem>();
	}
}
