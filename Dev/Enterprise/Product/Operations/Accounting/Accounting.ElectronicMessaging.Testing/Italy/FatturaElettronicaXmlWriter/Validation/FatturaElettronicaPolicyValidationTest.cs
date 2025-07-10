using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Italy.Testing
{
	public class FatturaElettronicaPolicyValidationTest : TestCaseWithFactory
	{
		public void TestMixedValidation()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var orgProxyCode = CreateNatOrg();
			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.CompanyName = "Test Company";
			transaction.BranchAddress.Country = new Country() { Code = CountryCodes.Italy };
			transaction.BranchAddress.OrganizationCode = orgProxyCode;

			var xml = CreateFullTestXML(cap: "07100ERROR");
			var (errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertContainsExactElementsInAnyOrder(new string[] {
				@"The 'CAP' element is invalid - The value '07100ERROR' is invalid according to its datatype 'http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2:CAPType' - The Pattern constraint failed.",
				@"FatturaElettronicaHeader/CedentePrestatore/DatiAnagrafici/Anagrafica: <Nome> and <Cognome> Organization Name must contain First Name and Last Name when Account is an Individual Person (NAT)." },
				errorNotifications.GetErrors().Select(x => x.Message));

			transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			xml = CreateFullTestXML();
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertEquals("Valid XML", 0, errorNotifications.GetErrors().Count());
		}

		public void TestFormatoTrasmissioneValidation()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var xml = CreateFullTestXML(versione: FPR12Versione);
			var (errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);

			AssertContainsExactElementsInAnyOrder(new string[] {
				@"FatturaElettronicaHeader/DatiTrasmissione/FormatoTrasmissione: <FormatoTrasmissione> must be equal to VERSIONE attribute in XML Main Root <FatturaElettronica>. Please raise an e-Request." },
				errorNotifications.GetErrors().Select(x => x.Message));
		}

		public void TestCodiceDestinatarioValidation()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var xml = CreateFullTestXML(codiceDestinatario: "0000000", pecDestinatario: "<PECDestinatario>TEST@123.com</PECDestinatario>", versione: FPR12Versione, formatoTrasmissione: FPR12Versione);
			var (errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertEquals("Valid: PECDestinatario is filled when CodiceDestinatario is equal to '0000000'.", 0, errorNotifications.GetErrors().Count());

			xml = CreateFullTestXML(codiceDestinatario: "0000000", pecDestinatario: null, versione: FPR12Versione, formatoTrasmissione: FPR12Versione);
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			Assert("Expect no errors", !errorNotifications.GetErrors().Any());

			xml = CreateFullTestXML(codiceDestinatario: "1000000", pecDestinatario: "<PECDestinatario>TEST@123.com</PECDestinatario>", versione: FPR12Versione, formatoTrasmissione: FPR12Versione);
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertContainsExactElementsInAnyOrder(new string[] {
				@"FatturaElettronicaHeader/DatiTrasmissione: <PECDestinario> PEC Email Address must be omitted when Account has an Italy CUU Registration Code." },
				errorNotifications.GetErrors().Select(x => x.Message));

			xml = CreateFullTestXML(codiceDestinatario: "123456", pecDestinatario: null, versione: FPR12Versione, formatoTrasmissione: FPR12Versione);
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertContainsExactElementsInAnyOrder(new string[] {
				@"FatturaElettronicaHeader/DatiTrasmissione: <CodiceDestinatario> CUU Registration Code must be 7 characters long when Account is a non-GOV organization." },
				errorNotifications.GetErrors().Select(x => x.Message));

			xml = CreateFullTestXML(codiceDestinatario: "1234567", pecDestinatario: null, versione: FPA12Versione, formatoTrasmissione: FPA12Versione);
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertContainsExactElementsInAnyOrder(new string[] {
				@"FatturaElettronicaHeader/DatiTrasmissione: <CodiceDestinatario> CUU Registration Code must be 6 characters long when Account is a GOV Organization." },
				errorNotifications.GetErrors().Select(x => x.Message));

			xml = CreateFullTestXML(codiceDestinatario: "0000000", pecDestinatario: null, versione: FPA12Versione, formatoTrasmissione: FPA12Versione);
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertContainsExactElementsInAnyOrder(new string[] {
				@"FatturaElettronicaHeader/DatiTrasmissione: <CodiceDestinatario> CUU Registration Code must be 6 characters long when Account is a GOV Organization." },
				errorNotifications.GetErrors().Select(x => x.Message));

			xml = CreateFullTestXML(codiceDestinatario: "123456", pecDestinatario: null, versione: FPA12Versione, formatoTrasmissione: FPA12Versione);
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertEquals("Valid: CodiceDestinatario has length of 6 when FormatoTrasmissione is FPA12.", 0, errorNotifications.GetErrors().Count());

			xml = CreateFullTestXML(codiceDestinatario: "1234567", pecDestinatario: null, versione: FPR12Versione, formatoTrasmissione: FPR12Versione);
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertEquals("Valid: CodiceDestinatario has length of 7 when FormatoTrasmissione is FPR12.", 0, errorNotifications.GetErrors().Count());
		}

		public void TestCompanyNameValidation()
		{
			const string individuleName = @"<Nome>Hello</Nome><Cognome>World</Cognome>";
			var orgProxyCode = CreateNatOrg();

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.CompanyName = "Test Company 1";
			transaction.BranchAddress.Country = new Country() { Code = CountryCodes.Italy };
			transaction.BranchAddress.OrganizationCode = orgProxyCode;

			var xml = CreateFullTestXML(supplierCompanyName: individuleName);
			var (errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertEquals("Valid supplier individule business name.", 0, errorNotifications.GetErrors().Count());

			xml = CreateFullTestXML();
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertContainsExactElementsInAnyOrder(new string[] {
				@"FatturaElettronicaHeader/CedentePrestatore/DatiAnagrafici/Anagrafica: <Nome> and <Cognome> Organization Name must contain First Name and Last Name when Account is an Individual Person (NAT)." },
				errorNotifications.GetErrors().Select(x => x.Message));

			transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.CompanyName = "Test Company 2";
			transaction.OrganizationAddress.Country = new Country() { Code = CountryCodes.Italy };
			transaction.OrganizationAddress.OrganizationCode = orgProxyCode;

			xml = CreateFullTestXML(recipientCompanyName: individuleName);
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertEquals("Valid recipient individule business name.", 0, errorNotifications.GetErrors().Count());

			xml = CreateFullTestXML();
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertContainsExactElementsInAnyOrder(new string[] {
				@"FatturaElettronicaHeader/CessionarioCommittente/DatiAnagrafici/Anagrafica: <Nome> and <Cognome> Organization Name must contain First Name and Last Name when Account is an Individual Person (NAT)." },
				errorNotifications.GetErrors().Select(x => x.Message));

			transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			xml = CreateFullTestXML(supplierCompanyName: individuleName, recipientCompanyName: individuleName);
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertContainsExactElementsInAnyOrder(new string[] {
				@"FatturaElettronicaHeader/CedentePrestatore/DatiAnagrafici/Anagrafica: <Denominazione> Organization Name must contain Business Name. Please check that Organization Category and Organization Name is correct.",
				@"FatturaElettronicaHeader/CessionarioCommittente/DatiAnagrafici/Anagrafica: <Denominazione> Organization Name must contain Business Name. Please check that Organization Category and Organization Name is correct." },
				errorNotifications.GetErrors().Select(x => x.Message));

			xml = CreateFullTestXML();
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertEquals("Valid company names.", 0, errorNotifications.GetErrors().Count());
		}

		public void TestPurchaseOrderValidation()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var xml = CreateFullTestXML(versione: FPR12Versione, formatoTrasmissione: FPR12Versione, codiceDestinatario: "1234567", datiOrdineAcquisto: "<DatiOrdineAcquisto><IdDocumento>66685</IdDocumento></DatiOrdineAcquisto>");
			var (errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertEquals("Valid: When FormatoTrasmissione is not FPA12, CodiceCUP / CIG are optional. IdDocumento is the only mandatory field in the <DatiOrdineAcquisto> XML node (accordingto XSD).", 0, errorNotifications.GetErrors().Count());

			xml = CreateFullTestXML(datiOrdineAcquisto: "<DatiOrdineAcquisto><IdDocumento>66685</IdDocumento></DatiOrdineAcquisto>");
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertContainsExactElementsInAnyOrder(new string[] {
				@"FatturaElettronicaBody/DatiGenerali/DatiOrdineAcquisto[1]: <IdDocumento>, <CodiceCUP> and <CodiceCIG> Job should contain Order Reference, CIG and CUP when Account is a GOV Organization. Without these references, the invoice may be rejected. You can add CIG and CUP as additional references on the operational job record.",
				@"FatturaElettronicaBody/DatiGenerali/DatiOrdineAcquisto[2]: <IdDocumento>, <CodiceCUP> and <CodiceCIG> Job should contain Order Reference, CIG and CUP when Account is a GOV Organization. Without these references, the invoice may be rejected. You can add CIG and CUP as additional references on the operational job record." },
				warningsNotification.GetWarnings().Select(x => x.Message));

			xml = CreateFullTestXML(datiOrdineAcquisto: "<DatiOrdineAcquisto><IdDocumento>66685</IdDocumento><CodiceCUP>CUP001</CodiceCUP></DatiOrdineAcquisto>");
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertContainsExactElementsInAnyOrder(new string[] {
				@"FatturaElettronicaBody/DatiGenerali/DatiOrdineAcquisto[1]: <IdDocumento>, <CodiceCUP> and <CodiceCIG> Job should contain Order Reference, CIG and CUP when Account is a GOV Organization. Without these references, the invoice may be rejected. You can add CIG and CUP as additional references on the operational job record.",
				@"FatturaElettronicaBody/DatiGenerali/DatiOrdineAcquisto[2]: <IdDocumento>, <CodiceCUP> and <CodiceCIG> Job should contain Order Reference, CIG and CUP when Account is a GOV Organization. Without these references, the invoice may be rejected. You can add CIG and CUP as additional references on the operational job record." },
				warningsNotification.GetWarnings().Select(x => x.Message));

			xml = CreateFullTestXML(datiOrdineAcquisto: "<DatiOrdineAcquisto><IdDocumento>66685</IdDocumento><CodiceCIG>CIG001</CodiceCIG></DatiOrdineAcquisto>");
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertContainsExactElementsInAnyOrder(new string[] {
				@"FatturaElettronicaBody/DatiGenerali/DatiOrdineAcquisto[1]: <IdDocumento>, <CodiceCUP> and <CodiceCIG> Job should contain Order Reference, CIG and CUP when Account is a GOV Organization. Without these references, the invoice may be rejected. You can add CIG and CUP as additional references on the operational job record.",
				@"FatturaElettronicaBody/DatiGenerali/DatiOrdineAcquisto[2]: <IdDocumento>, <CodiceCUP> and <CodiceCIG> Job should contain Order Reference, CIG and CUP when Account is a GOV Organization. Without these references, the invoice may be rejected. You can add CIG and CUP as additional references on the operational job record." },
				warningsNotification.GetWarnings().Select(x => x.Message));

			xml = CreateFullTestXML(datiOrdineAcquisto: "");
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertContainsExactElementsInAnyOrder(new string[] {
				@"FatturaElettronicaBody/DatiGenerali/DatiOrdineAcquisto <IdDocumento>, <CodiceCUP> and <CodiceCIG> Job should contain Order Reference, CIG and CUP when Account is a GOV Organization. Without these references, the invoice may be rejected. You can add CIG and CUP as additional references on the operational job record." },
				warningsNotification.GetWarnings().Select(x => x.Message));

			xml = CreateFullTestXML(datiOrdineAcquisto: "<DatiOrdineAcquisto><IdDocumento>66685</IdDocumento><CodiceCUP>123abc</CodiceCUP><CodiceCIG>456def</CodiceCIG></DatiOrdineAcquisto>");
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertEquals("Valid no errors: When FormatoTrasmissione is FPA12, IdDocumento is mandatory in <DatiOrdineAcquisto>, CodiceCUP / CIG are optional but they should be specified in the transaction.", 0, errorNotifications.GetErrors().Count());
			AssertEquals("Valid no warnings: When FormatoTrasmissione is FPA12, IdDocumento is mandatory in <DatiOrdineAcquisto>, CodiceCUP / CIG are optional but they should be specified in the transaction.", 0, warningsNotification.GetWarnings().Count());
		}

		public void TestLineValidation()
		{
			const string LineNaturaN5Group = "<Natura>N5</Natura><RiferimentoAmministrazione>TST</RiferimentoAmministrazione>";
			const string SummaryNaturaN5 = "<Natura>N5</Natura>";
			const string SummaryNaturaN6 = "<Natura>N6</Natura>";
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var xml = CreateFullTestXML(lineAliquotaIVA: "0.00", lineNatura: null);
			var (errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertContainsExactElementsInAnyOrder(new string[] {
				@"FatturaElettronicaBody/DatiBeniServizi/DettaglioLinee[1]: <Natura> Tax Message including a Tax Group Code (Natura N1-N7) must be selected when IVA is zero.",
				@"FatturaElettronicaBody/DatiBeniServizi/DettaglioLinee[2]: <Natura> Tax Message including a Tax Group Code (Natura N1-N7) must be selected when IVA is zero." },
				errorNotifications.GetErrors().Select(x => x.Message));

			xml = CreateFullTestXML(lineAliquotaIVA: "0.00", lineNatura: "<Natura>N6</Natura>");
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertContainsExactElementsInAnyOrder(new string[] {
				@"FatturaElettronicaBody/DatiBeniServizi/DettaglioLinee[1]: <RiferimentoAmministrazione> Tax Message must have a Tax Group Code (Natura N1-N7).",
				@"FatturaElettronicaBody/DatiBeniServizi/DettaglioLinee[2]: <RiferimentoAmministrazione> Tax Message must have a Tax Group Code (Natura N1-N7)." },
				errorNotifications.GetErrors().Select(x => x.Message));

			xml = CreateFullTestXML(lineNatura: "<RiferimentoAmministrazione>TST</RiferimentoAmministrazione>");
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertContainsExactElementsInAnyOrder(new string[] {
				@"FatturaElettronicaBody/DatiBeniServizi/DettaglioLinee[1]: <RiferimentoAmministrazione> Tax Message must have a Tax Group Code (Natura N1-N7).",
				@"FatturaElettronicaBody/DatiBeniServizi/DettaglioLinee[2]: <RiferimentoAmministrazione> Tax Message must have a Tax Group Code (Natura N1-N7)." },
				errorNotifications.GetErrors().Select(x => x.Message));

			xml = CreateFullTestXML(lineAliquotaIVA: "0.00", lineNatura: LineNaturaN5Group);
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertEquals("Valid: Line Natura is filled when AliquotaIVA is ZERO.", 0, errorNotifications.GetErrors().Count());

			xml = CreateFullTestXML();
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertEquals("Valid: Line Natura is empty when AliquotaIVA is non-ZERO.", 0, errorNotifications.GetErrors().Count());

			xml = CreateFullTestXML(lineNatura: LineNaturaN5Group);
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertEquals("Valid: Line Natura is filled when AliquotaIVA is non-ZERO.", 0, errorNotifications.GetErrors().Count());

			xml = CreateFullTestXML(summaryAliquotaIVA: "0.00");
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertContainsExactElementsInAnyOrder(new string[] {
				@"FatturaElettronicaBody/DatiBeniServizi/DatiRiepilogo[1]: <Natura> Tax Message including a Tax Group Code (Natura N1-N7) must be selected when IVA is zero.",
				@"FatturaElettronicaBody/DatiBeniServizi/DatiRiepilogo[2]: <Natura> Tax Message including a Tax Group Code (Natura N1-N7) must be selected when IVA is zero." },
			errorNotifications.GetErrors().Select(x => x.Message));

			xml = CreateFullTestXML(summaryAliquotaIVA: "0.00", summaryNatura: SummaryNaturaN5);
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertEquals("Valid: Summary Natura is filled when AliquotaIVA is ZERO.", 0, errorNotifications.GetErrors().Count());

			xml = CreateFullTestXML();
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertEquals("Valid: Summary Natura is empty when AliquotaIVA is non-ZERO.", 0, errorNotifications.GetErrors().Count());

			xml = CreateFullTestXML(summaryNatura: SummaryNaturaN5);
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertEquals("Valid: Summary Natura is filled when AliquotaIVA is non-ZERO.", 0, errorNotifications.GetErrors().Count());

			xml = CreateFullTestXML(summaryAliquotaIVA: "0.00", summaryNatura: SummaryNaturaN6, esigibilitaIVA: EsigibilitaIVA_S);
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertContainsExactElementsInAnyOrder(new string[] {
				@"FatturaElettronicaBody/DatiBeniServizi/DatiRiepilogo[1]: <EsigibilitaIVA> must not be 'S' (split payment) when <Natura> equals to 'N6'. Please raise an e-Request.",
				@"FatturaElettronicaBody/DatiBeniServizi/DatiRiepilogo[2]: <EsigibilitaIVA> must not be 'S' (split payment) when <Natura> equals to 'N6'. Please raise an e-Request." },
				errorNotifications.GetErrors().Select(x => x.Message));

			xml = CreateFullTestXML(summaryAliquotaIVA: "0.00", summaryNatura: SummaryNaturaN6);
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertEquals("Valid: EsigibilitaIVA is 'S' when Natura is 'N6'.", 0, errorNotifications.GetErrors().Count());
		}

		public void TestNumeroValidation()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var xml = CreateFullTestXML(numero: "XXX");
			var (errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertContainsExactElementsInAnyOrder(new string[] {
				@"FatturaElettronicaBody/DatiGenerali/DatiGeneraliDocumento/Numero: <Numero> Transaction Number must have at least one number." },
				errorNotifications.GetErrors().Select(x => x.Message));

			xml = CreateFullTestXML(numero: "X1X");
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertEquals("Valid Numero code.", 0, errorNotifications.GetErrors().Count());

			xml = CreateFullTestXML(numero: "");
			(errorNotifications, warningsNotification) = ValidateXmlWithXsdAndPolicies(transaction, xml);
			AssertCollectionContains(
				"The 'Numero' element is invalid - The value '' is invalid according to its datatype 'http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2:String20Type' - The Pattern constraint failed.",
				errorNotifications.GetErrors().Select(x => x.Message));
		}

		#region Implementation

		(NotificationContainer, NotificationContainer) ValidateXmlWithXsdAndPolicies(TransactionInfo transaction, string xml)
		{
			var errorNotifications = new NotificationContainer();
			var warningNotifications = new NotificationContainer();

			var document = new XmlDocument();
			document.LoadXml(xml);

			using (var stream = new MemoryStream())
			{
				var xmlWriterSettings = new XmlWriterSettings
				{
					ConformanceLevel = ConformanceLevel.Document,
					OmitXmlDeclaration = false,
					NamespaceHandling = NamespaceHandling.OmitDuplicates,
					Encoding = Encoding.UTF8
				};

				using (var writer = XmlWriter.Create(stream, xmlWriterSettings))
				{
					document.WriteTo(writer);
					writer.Flush();
				}

				var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
				transactionBatch.TransactionCollection.Add(transaction);

				var validatedDocument = new FatturaElettronicaXsdValidation(errorNotifications).ValidateXml(stream);
				new FatturaElettronicaPolicyValidation(errorNotifications, warningNotifications, validatedDocument, transactionBatch).ValidateXml();
			}

			return (errorNotifications, warningNotifications);
		}

		string CreateNatOrg()
		{
			var newFactory = Factory.CreateNewFactory();

			var testObjectCreator = new TestObjectCreator(newFactory);
			var orgProxy = testObjectCreator.CreateOrgHeader("ITPROXY", true, true);
			var company = testObjectCreator.CreateNewCompany("TST", orgProxy: orgProxy);
			company.GC_BusinessRegNo = "00000";
			company.GC_RN_NKCountryCode = CountryCodes.Italy;
			company.OrgProxy.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

			newFactory.Save();

			return orgProxy.OH_Code;
		}

		const string FPR12Versione = "FPR12";
		const string FPA12Versione = "FPA12";
		const string EsigibilitaIVA_S = "S";
		const string EsigibilitaIVA_I = "I";

		#region Notification Container

		public class NotificationContainer : INotifications
		{
			public void Add(INotification notification)
			{
				list.Add(notification);
			}

			public IEnumerable<INotification> GetErrors()
			{
				return list.Where(x => x.Type == NotificationType.Error);
			}

			public IEnumerable<INotification> GetWarnings()
			{
				return list.Where(x => x.Type == NotificationType.Warning);
			}

			readonly List<INotification> list = new List<INotification>();
		}

		#endregion

		#region XML Source

		string CreateFullTestXML(string versione = FPA12Versione, string cap = "07100", string formatoTrasmissione = FPA12Versione,
								 string codiceDestinatario = "AAAAAA", string pecDestinatario = null,
								 string supplierCompanyName = "<Denominazione>ALPHA SRL</Denominazione>", string recipientCompanyName = "<Denominazione>AMMINISTRAZIONE BETA</Denominazione>",
								 string numero = "123", string datiOrdineAcquisto = "<DatiOrdineAcquisto><IdDocumento>66685</IdDocumento><CodiceCUP>123abc</CodiceCUP><CodiceCIG>456def</CodiceCIG></DatiOrdineAcquisto>",
								 string lineAliquotaIVA = "22.00", string lineNatura = null, string summaryAliquotaIVA = "22.00", string summaryNatura = null, string esigibilitaIVA = EsigibilitaIVA_I)
		{
			return  string.Format(CultureInfo.InvariantCulture, XMLSourceHeaderForMultipleTest, versione, cap, supplierCompanyName, recipientCompanyName, formatoTrasmissione, codiceDestinatario, pecDestinatario) +
					string.Format(CultureInfo.InvariantCulture, XMLSourceBodyForMultipleTest, numero, datiOrdineAcquisto, lineAliquotaIVA, lineNatura, summaryAliquotaIVA, summaryNatura, esigibilitaIVA);
		}

		readonly string XMLSourceHeaderForMultipleTest = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<p:FatturaElettronica versione = ""{0}"" xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"" 
xmlns:p=""http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2"" 
xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
xsi:schemaLocation=""http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2 http://www.fatturapa.gov.it/export/fatturazione/sdi/fatturapa/v1.2/Schema_del_file_xml_FatturaPA_versione_1.2.xsd"">
  <FatturaElettronicaHeader>
    <DatiTrasmissione>
      <IdTrasmittente>
        <IdPaese>IT</IdPaese>
        <IdCodice>01234567890</IdCodice>
      </IdTrasmittente>
      <ProgressivoInvio>00001</ProgressivoInvio>
      <FormatoTrasmissione>{4}</FormatoTrasmissione>
      <CodiceDestinatario>{5}</CodiceDestinatario>
      {6}
    </DatiTrasmissione>
    <CedentePrestatore>
      <DatiAnagrafici>
        <IdFiscaleIVA>
          <IdPaese>IT</IdPaese>
          <IdCodice>01234567890</IdCodice>
        </IdFiscaleIVA>
        <Anagrafica>
          {2}
        </Anagrafica>
        <RegimeFiscale>RF19</RegimeFiscale>
      </DatiAnagrafici>
      <Sede>
        <Indirizzo>VIALE ROMA 543</Indirizzo>
        <CAP>{1}</CAP>
        <Comune>SASSARI</Comune>
        <Provincia>SS</Provincia>
        <Nazione>IT</Nazione>
      </Sede>
    </CedentePrestatore>
    <CessionarioCommittente>
      <DatiAnagrafici>
        <CodiceFiscale>09876543210</CodiceFiscale>
        <Anagrafica>
          {3}
        </Anagrafica>
      </DatiAnagrafici>
      <Sede>
        <Indirizzo>VIA TORINO 38-B</Indirizzo>
        <CAP>00145</CAP>
        <Comune>ROMA</Comune>
        <Provincia>RM</Provincia>
        <Nazione>IT</Nazione>
      </Sede>
    </CessionarioCommittente>
  </FatturaElettronicaHeader>";
		readonly string XMLSourceBodyForMultipleTest = @"  <FatturaElettronicaBody>
    <DatiGenerali>
      <DatiGeneraliDocumento>
        <TipoDocumento>TD01</TipoDocumento>
        <Divisa>EUR</Divisa>
        <Data>2017-01-18</Data>
        <Numero>{0}</Numero>
        <Causale>LA FATTURA FA RIFERIMENTO AD UNA OPERAZIONE AAAA BBBBBBBBBBBBBBBBBB CCC DDDDDDDDDDDDDDD E FFFFFFFFFFFFFFFFFFFF GGGGGGGGGG HHHHHHH II LLLLLLLLLLLLLLLLL MMM NNNNN OO PPPPPPPPPPP QQQQ RRRR SSSSSSSSSSSSSS</Causale>
        <Causale>SEGUE DESCRIZIONE CAUSALE NEL CASO IN CUI NON SIANO STATI SUFFICIENTI 200 CARATTERI AAAAAAAAAAA BBBBBBBBBBBBBBBBB</Causale>
      </DatiGeneraliDocumento>
		{1}
		{1}
    </DatiGenerali>
    <DatiBeniServizi>
      <DettaglioLinee>
        <NumeroLinea>1</NumeroLinea>
        <Descrizione>DESCRIZIONE DELLA FORNITURA 1</Descrizione>
        <Quantita>5.00</Quantita>
        <PrezzoUnitario>1.00</PrezzoUnitario>
        <PrezzoTotale>5.00</PrezzoTotale>
        <AliquotaIVA>{2}</AliquotaIVA>
        {3}
      </DettaglioLinee>
      <DettaglioLinee>
        <NumeroLinea>2</NumeroLinea>
        <Descrizione>DESCRIZIONE DELLA FORNITURA 2</Descrizione>
        <Quantita>15.00</Quantita>
        <PrezzoUnitario>1.00</PrezzoUnitario>
        <PrezzoTotale>15.00</PrezzoTotale>
        <AliquotaIVA>{2}</AliquotaIVA>
        {3}
      </DettaglioLinee>
      <DatiRiepilogo>
        <AliquotaIVA>{4}</AliquotaIVA>
        {5}
        <ImponibileImporto>5.00</ImponibileImporto>
        <Imposta>1.10</Imposta>
        <EsigibilitaIVA>{6}</EsigibilitaIVA>
      </DatiRiepilogo>
      <DatiRiepilogo>
        <AliquotaIVA>{4}</AliquotaIVA>
        {5}
        <ImponibileImporto>7.00</ImponibileImporto>
        <Imposta>2.10</Imposta>
        <EsigibilitaIVA>{6}</EsigibilitaIVA>
      </DatiRiepilogo>
    </DatiBeniServizi>
    <DatiPagamento>
      <CondizioniPagamento>TP01</CondizioniPagamento>
      <DettaglioPagamento>
        <ModalitaPagamento>MP01</ModalitaPagamento>
        <DataScadenzaPagamento>2017-02-18</DataScadenzaPagamento>
        <ImportoPagamento>6.10</ImportoPagamento>
      </DettaglioPagamento>
    </DatiPagamento>
  </FatturaElettronicaBody>
</p:FatturaElettronica>";

		#endregion

		#endregion
	}
}
