using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.TestHelpers.Xml;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Italy.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForItaly))]
	public class ElectronicMessagingProcessingServiceTaskForItalyTest : GEIElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForItaly>
	{
		protected override ElectronicMessagingProcessingServiceTaskForItaly GetCountrySpecificServiceTask()
		{
			return new ElectronicMessagingProcessingServiceTaskForItaly_ForTest();
		}

		[TestDate(2018, 5, 10)]
		public override void TestServiceTaskProcessesEInvoiceOnlyForThoseCompaniesWhereFunctionalityIsEnabled()
		{
			var company1 = Helper.CreateCompanyAndBranch("MN1", "BR1", CountryCode, true);
			var company2 = Helper.CreateCompanyAndBranch("MN2", "BR2", CountryCode, true);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 3);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, 3);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var serviceTask = GetCountrySpecificServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);

				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1, 1, 1 }, logger);
				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, Array.Empty<int>(), logger);
			}
		}

		[TestDate(2018, 5, 10)]
		public override void TestSuccessfulCreationOfEDIInterchange_OneCompany()
		{
			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "BRN", CountryCode, true);

			//Create first set of transactions and run the service task.
			Helper.CreateARAPINVCRDADJTransactions(company.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, 3);

			var serviceTask = GetCountrySpecificServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company, new int[] { 1, 1, 1 }, logger);

			//Create another set of transactions and run the service task.
			Helper.CreateARAPINVCRDADJTransactions(company.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, 3);

			serviceTask = GetCountrySpecificServiceTask();
			logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company, new int[] { 1, 1, 1, 1, 1, 1 }, logger);
		}

		[TestDate(2018, 5, 10)]
		public override void TestSuccessfulCreationOfEDIInterchange_MoreThanOneCompany()
		{
			var company1 = Helper.CreateCompanyAndBranch(CountryCode + "1", "BRN", CountryCode, true);
			var company2 = Helper.CreateCompanyAndBranch(CountryCode + "2", "BR2", CountryCode, true);

			//Create first set of transactions for 2 comapnies and run the service task.

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 3);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, 3);

			var serviceTask = GetCountrySpecificServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1, 1, 1 }, logger);
			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, new int[] { 1, 1, 1 }, logger);

			//Create another set of transactions for 2 comapnies and run the service task again.
			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 3);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, 3);

			serviceTask = GetCountrySpecificServiceTask();
			logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1, 1, 1, 1, 1, 1 }, logger);
			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, new int[] { 1, 1, 1, 1, 1, 1 }, logger);
		}

		[TestDate(2019, 09, 18, 0, 0, 0)]
		public override void TestSuccessfulCreatedEDIInterchangeBodyText()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var company1 = Helper.CreateCompanyAndBranch("MN1", "BR1", CountryCode, true);
			var branch = company1.FirstActiveBranch;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var job1 = objectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge1 = objectCreator.CreateCharge(job1, objectCreator.CC1, "charge1", objectCreator.EUR, 10m, objectCreator.Creditor1, objectCreator.EUR, 10m, objectCreator.Debtor1);
				charge1.JR_AT_SellGSTRate = objectCreator.ServiceTax.PK;
				Factory.Save();

				var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(5), objectCreator.EUR, 1m, 10m, 0M, 10m, 0m, objectCreator.Debtor1, objectCreator.CC1.PK);
				arInvoice.Lines[0].AL_AT = charge1.JR_AT_SellGSTRate;
				Factory.Save();

				AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 1);

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var serviceTask = new ElectronicMessagingProcessingServiceTaskForItaly();
					var logger = InitialiseAndRunTaskSchedule(serviceTask);

					var interchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_GB, company1.FirstActiveBranch.PK));
					AssertEquals(1, interchanges.Length);

					var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchanges[0].PK));
					AssertEquals(1, messages.Length);

					var message = messages[0];

					var expectedText = @"<GlobalElectronicInvoicing xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing"">
  <Header>
    <ElectronicInvoiceBatchRequest>
      <MessagingSystem>Italy electronic invoicing system</MessagingSystem>
      <MessageType>REQ</MessageType>
      <BatchNumber>1</BatchNumber>
      <AdditionalDataItems>
        <AdditionalDataItem>
          <Key>IssuerRegistrationNumber</Key>
          <Value>IT</Value>
        </AdditionalDataItem>
      </AdditionalDataItems>
      <IsProductionSystem>false</IsProductionSystem>
    </ElectronicInvoiceBatchRequest>
  </Header>
  <Payload><![CDATA[<?xml version=""1.0"" encoding=""utf-8""?><p:FatturaElettronica versione=""FPR12"" xmlns:p=""http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2"" xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2 http://www.fatturapa.gov.it/export/fatturazione/sdi/fatturapa/v1.2/Schema_del_file_xml_FatturaPA_versione_1.2.xsd""><FatturaElettronicaHeader><DatiTrasmissione><IdTrasmittente><IdPaese>IT</IdPaese><IdCodice>13149600150</IdCodice></IdTrasmittente><ProgressivoInvio>PLACEHOLDER</ProgressivoInvio><FormatoTrasmissione>FPR12</FormatoTrasmissione><CodiceDestinatario>0000000</CodiceDestinatario></DatiTrasmissione><CedentePrestatore><DatiAnagrafici><IdFiscaleIVA><IdPaese>IT</IdPaese><IdCodice /></IdFiscaleIVA><Anagrafica><Denominazione>Test Company Name</Denominazione></Anagrafica><RegimeFiscale>RF01</RegimeFiscale></DatiAnagrafici><Sede><Indirizzo>184 Bourke Road</Indirizzo><CAP>00000</CAP><Comune>Alexandria</Comune><Nazione>IT</Nazione></Sede></CedentePrestatore><CessionarioCommittente><DatiAnagrafici><IdFiscaleIVA><IdPaese></IdPaese><IdCodice>TestCompanyName</IdCodice></IdFiscaleIVA><Anagrafica><Denominazione>Test Company Name</Denominazione></Anagrafica></DatiAnagrafici><Sede><Indirizzo>184 Bourke Road</Indirizzo><CAP>00000</CAP><Comune>Alexandria</Comune><Nazione>IT</Nazione></Sede></CessionarioCommittente></FatturaElettronicaHeader><FatturaElettronicaBody><DatiGenerali><DatiGeneraliDocumento><TipoDocumento>TD01</TipoDocumento><Divisa>EUR</Divisa><Data>2019-09-18</Data><Numero>00001000</Numero><ImportoTotaleDocumento>10.53</ImportoTotaleDocumento><Causale>Documento emesso in valuta EUR al cambio 1.00 per totali EUR 10.53</Causale><Causale>Test Invoice</Causale></DatiGeneraliDocumento></DatiGenerali><DatiBeniServizi><DettaglioLinee><NumeroLinea>1</NumeroLinea><CodiceArticolo><CodiceTipo>ZZCC1</CodiceTipo><CodiceValore>00001000</CodiceValore></CodiceArticolo><Descrizione>Charge Code 1</Descrizione><PrezzoUnitario>10.00</PrezzoUnitario><PrezzoTotale>10.00</PrezzoTotale><AliquotaIVA>5.00</AliquotaIVA></DettaglioLinee><DatiRiepilogo><AliquotaIVA>5.00</AliquotaIVA><ImponibileImporto>10.00</ImponibileImporto><Imposta>0.53</Imposta><EsigibilitaIVA>I</EsigibilitaIVA></DatiRiepilogo></DatiBeniServizi><DatiPagamento><CondizioniPagamento>TP02</CondizioniPagamento><DettaglioPagamento><ModalitaPagamento></ModalitaPagamento><DataScadenzaPagamento>2019-09-18</DataScadenzaPagamento><ImportoPagamento>10.53</ImportoPagamento></DettaglioPagamento></DatiPagamento></FatturaElettronicaBody></p:FatturaElettronica>]]></Payload>
</GlobalElectronicInvoicing>";

					var expectecBytes = Encoding.UTF8.GetBytes(expectedText);

					XmlComparison.CompareAndAssertXml(expectedText, message.EM_MessageText);
					AssertEquals(expectecBytes, message.EM_MessageData);
				}
			}
		}

		protected override ZString CountryCode => CountryCodes.Italy;

#region Inner Class

		public class ElectronicMessagingProcessingServiceTaskForItaly_ForTest : ElectronicMessagingProcessingServiceTaskForItaly
		{
			protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeCreator(GlbCompany company)
			{
				return new MockEDIInterchangeCreatorForItalyEInvoicingBatch(company, () => new MockAccEInvoiceBatchToGEIConverter());
			}
		}

#endregion
	}
}
