using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
using BRIncoTermList = Enterprise.Customs.BR.Business.BRIncoTermList;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(NFEImportForm))]
	public class NFEImportFormTest : ZFormBasherTest
	{
		public void TestNFEImportObjectParentAfterProcessFileWithWarning()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
			invoiceHeader.JZ_IncoTerm = ZString.Empty;
			Factory.Save();

			using (var tempFile = TempFile.New())
			{
				File.WriteAllText(tempFile.Filename, nfeXmlText);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
				using (var form = new NFEImportFormForTest(new NFEImportObjectParent(declaration)))
				{
					form.Show();

					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					form.PerformClickBrowseButton();

					AssertEquals("One NFEImportObject added", 1, form.Master.NFEImportObjectCollection.Count);
					AssertNull("No error", UnitTestUserNotification.Instance.LastMessage.Text);

					form.Master.NFEImportObjectCollection[0].InvoiceHeaderPK = invoiceHeader.PK;

					form.PerformClickImportButton();

					AssertEquals("Log", "Inv. Header:  - NF-e 180666 - Error - Incoterm and/or Currency were not entered.", form.Logs.First());
				}
			}
		}

		public void TestNFEImportObjectParentAfterProcessFile_Success()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
			Factory.Save();

			using (var tempFile = TempFile.New())
			{
				File.WriteAllText(tempFile.Filename, nfeXmlText);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
				using (var form = new NFEImportFormForTest(new NFEImportObjectParent(declaration)))
				{
					form.Show();

					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					form.PerformClickBrowseButton();

					AssertEquals("One NFEImportObject added", 1, form.Master.NFEImportObjectCollection.Count);
					AssertNull("No error", UnitTestUserNotification.Instance.LastMessage.Text);

					var item = form.Master.NFEImportObjectCollection[0];

					AssertEquals("NfeKey", "35200857012650000174550010001806661378259720", item.NfeKey);
					AssertEquals("NfeSerie", "1", item.NfeSerie);
					AssertEquals("NfeNumber", "180666", item.NfeNumber);
					AssertEquals("NfeDate", new ZDateTime(2021, 5, 2), item.NfeDate);
					AssertEquals("Currency", ZString.Empty, item.CurrencyCode);
					item.CurrencyCode = Core.Constants.CurrencyCodes.Brazil;
					AssertEquals("Currency", Core.Constants.CurrencyCodes.Brazil, item.CurrencyCode);
					AssertEquals("ExchangeRate", new ZDecimal(1), item.ExchangeRate);
					AssertEquals("Exchange Rate Date", new ZDateTime(2021, 4, 30), item.ExchangeRateDate);
					AssertEquals("Exchange Rate Buy", new ZDecimal(1), item.ExchangeRateBuy);
					AssertEquals("Exchange Rate Sell", new ZDecimal(1), item.ExchangeRateSell);
					item.InvoiceHeaderPK = ZGuid.Empty;

					form.PerformClickImportButton();
					var importedInvoiceHeader = (JobComInvoiceHeader)declaration.Invoices.First(invoice => invoice.JZ_InvoiceNumber == "180666");
					AssertEquals("JZ_InvoiceNumber should be", "180666", importedInvoiceHeader.JZ_InvoiceNumber);
					AssertEquals("JZ_Weight should be", 280m, importedInvoiceHeader.JZ_Weight);
					AssertEquals("Log", "Inv. Header: 180666 - NF-e 180666 - New invoice created, lines successfully imported into Inv. Header.", form.Logs.First());
					AssertContains("Loaded: ", form.Logs.Last());
				}
			}
		}

		public void TestNFEImportObjectParentAfterProcessFile_Failure()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
			Factory.Save();

			using (var tempFile = TempFile.New())
			{
				File.WriteAllText(tempFile.Filename, @"<?xml version='1.0' encoding='UTF-8'?><nfeProc />");

				using (var form = new NFEImportFormForTest(new NFEImportObjectParent(declaration)))
				{
					form.Show();

					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					form.PerformClickBrowseButton();

					AssertEquals("No NFEImportObject added", 0, form.Master.NFEImportObjectCollection.Count);
					AssertEquals($"Unable to read {Path.GetFileName(tempFile.Filename)}", form.Logs.First());
				}
			}
		}

		public void TestCreateChargeOfInvoiceLinesBasedOnNfeImport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.FOB;
			Factory.Save();

			using (var tempFile = TempFile.New())
			{
				File.WriteAllText(tempFile.Filename, nfeXmlTextWithProductsAndFrete);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
				using (var form = new NFEImportFormForTest(new NFEImportObjectParent(declaration)))
				{
					form.Show();

					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					form.PerformClickBrowseButton();

					AssertEquals("One NFEImportObject added", 1, form.Master.NFEImportObjectCollection.Count);
					AssertNull("No error", UnitTestUserNotification.Instance.LastMessage.Text);

					form.Master.NFEImportObjectCollection[0].InvoiceHeaderPK = invoiceHeader.PK;

					form.PerformClickImportButton();
					var importedInvoiceHeader = (JobComInvoiceHeader)declaration.Invoices.First();

					AssertEquals("Log", "Inv. Header:  - NF-e 3344 lines successfully imported into existing Inv. Header.", form.Logs.First());

					var invoiceLine1 = declaration.InvoiceLines[0];
					var charge1 = invoiceLine1.Charges[0];
					AssertEquals("J7_ChargeType should be", Common.CustomsChargeTypeList.Codes.OverseasFreight, charge1.J7_ChargeType);
					AssertEquals("J7_Amount should be", new ZDecimal(391.67), charge1.J7_Amount);
					AssertEquals("J7_RX_NKCurrency should be", importedInvoiceHeader.JZ_RX_NKInvoice_Currency, charge1.J7_RX_NKCurrency);
					AssertEquals("J7_IsDutiable should be", ZBool.False, charge1.J7_IsDutiable);
					AssertEquals("J7_IsGSTApplicable should be", ZBool.True, charge1.J7_IsGSTApplicable);
					AssertEquals("J7_IsIncludedInITOT should be", ZBool.False, charge1.J7_IsIncludedInITOT);
					AssertEquals("J7_IsNotIncludedInInvoice should be", ZBool.True, charge1.J7_IsNotIncludedInInvoice);

					var invoiceLine2 = declaration.InvoiceLines[1];
					var charge2 = invoiceLine2.Charges[0];
					AssertEquals("J7_ChargeType should be", Common.CustomsChargeTypeList.Codes.OverseasFreight, charge2.J7_ChargeType);
					AssertEquals("J7_Amount should be", new ZDecimal(11652.94), charge2.J7_Amount);
					AssertEquals("J7_RX_NKCurrency should be", importedInvoiceHeader.JZ_RX_NKInvoice_Currency, charge2.J7_RX_NKCurrency);
					AssertEquals("J7_IsDutiable should be", ZBool.False, charge2.J7_IsDutiable);
					AssertEquals("J7_IsGSTApplicable should be", ZBool.True, charge2.J7_IsGSTApplicable);
					AssertEquals("J7_IsIncludedInITOT should be", ZBool.False, charge2.J7_IsIncludedInITOT);
					AssertEquals("J7_IsNotIncludedInInvoice should be", ZBool.True, charge2.J7_IsNotIncludedInInvoice);

					var invoiceLine3 = declaration.InvoiceLines[2];
					var charge3 = invoiceLine3.Charges[0];
					AssertEquals("J7_ChargeType should be", Common.CustomsChargeTypeList.Codes.OverseasFreight, charge3.J7_ChargeType);
					AssertEquals("J7_Amount should be", new ZDecimal(146.87), charge3.J7_Amount);
					AssertEquals("J7_RX_NKCurrency should be", importedInvoiceHeader.JZ_RX_NKInvoice_Currency, charge3.J7_RX_NKCurrency);
					AssertEquals("J7_IsDutiable should be", ZBool.False, charge3.J7_IsDutiable);
					AssertEquals("J7_IsGSTApplicable should be", ZBool.True, charge3.J7_IsGSTApplicable);
					AssertEquals("J7_IsIncludedInITOT should be", ZBool.False, charge3.J7_IsIncludedInITOT);
					AssertEquals("J7_IsNotIncludedInInvoice should be", ZBool.True, charge3.J7_IsNotIncludedInInvoice);
				}
			}
		}

		public void TestRemoveColumn()
		{
			var invoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			var declaration = new FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData as JobDeclaration;

			using (var form = new NFEImportFormForTest(new NFEImportObjectParent(declaration)))
			{
				form.Show();

				AssertNull("NFE grid should not contain column EntryInstructionPK", form.NFEGrid.Columns[NFEImportObject.Schema.EntryInstructionPK]);
			}

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.FOB;

			using (var form = new NFEImportFormForTest(new NFEImportObjectParent(declaration)))
			{
				form.Show();

				AssertNotNull("NFE grid should not contain column EntryInstructionPK", form.NFEGrid.Columns[NFEImportObject.Schema.EntryInstructionPK]);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new NFEImportFormForTest(new NFEImportObjectParent(declaration));
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "FileNameTextBox";
		}

		#endregion

		#region NFE Xml

		const string nfeXmlText = @"<?xml version='1.0' encoding='UTF-8'?>
<nfeProc versao = '4.00' xmlns = 'http://www.portalfiscal.inf.br/nfe'>
	<NFe xmlns = 'http://www.portalfiscal.inf.br/nfe'>
		<infNFe versao = '4.00' Id = 'NFe35200857012650000174550010001806661378259720'>
			<ide>
				<cUF>35</cUF>
				<cNF>37825972</cNF>
				<natOp>Venda de producao</natOp>
				<mod>55</mod>
				<serie>1</serie>
				<nNF>180666</nNF>
				<dhEmi>2021-05-02T06:19:00-03:00</dhEmi>
				<dhSaiEnt>2021-05-02T06:19:00-03:00</dhSaiEnt>
				<tpNF>1</tpNF>
				<idDest>3</idDest>
				<cMunFG>3550308</cMunFG>
				<tpImp>1</tpImp>
				<tpEmis>1</tpEmis>
				<cDV>0</cDV>
				<tpAmb>1</tpAmb>
				<finNFe>1</finNFe>
				<indFinal>1</indFinal>
				<indPres>0</indPres>
				<procEmi>0</procEmi>
				<verProc>4.00</verProc>
			</ide>
			<emit>
				<CNPJ>57012650000174</CNPJ>
				<xNome>PINGUIM IND.E COM. DE RADIADORES EIRELI</xNome>
				<xFant>PINGUIM</xFant>
				<enderEmit>
					<xLgr>RUA MADALENA DE MADUREIRA</xLgr>
					<nro>151</nro>
					<xBairro>LIMAO</xBairro>
					<cMun>3550308</cMun>
					<xMun>Sao Paulo</xMun>
					<UF>SP</UF>
					<CEP>02551040</CEP>
					<cPais>1058</cPais>
					<xPais>BRASIL</xPais>
					<fone>1138566440</fone>
				</enderEmit>
				<IE>111759209113</IE>
				<CRT>3</CRT>
			</emit>
			<dest>
				<idEstrangeiro/>
				<xNome>FELIX RUIZ PADILLA NIT. 3222655014</xNome>
				<enderDest>
					<xLgr>AV SANTA CRUZ</xLgr>
					<nro>2410</nro>
					<xBairro>VILLA VICTORIA</xBairro>
					<cMun>9999999</cMun>
					<xMun>EXTERIOR</xMun>
					<UF>EX</UF>
					<CEP>17201970</CEP>
					<cPais>0973</cPais>
					<xPais>BOLIVIA, ESTADO PLURINACIONAL DA</xPais>
					<fone>1333462299</fone>
				</enderDest>
				<indIEDest>9</indIEDest>
				<email>FALECONOSCO.BR @DHL.COM</email>
			</dest>
			<det nItem = '1'>
				<prod>
					<cProd>FM3555</cProd>
					<cEAN/>
					<xProd>COLMEIA ESPECIAL 890 X 910 X 7TBC / 8APP ESTANHADA</xProd>
					<NCM>87089100</NCM>
					<CFOP>7101</CFOP>
					<uCom>PC</uCom>
					<qCom>4.0000</qCom>
					<vUnCom>12421.7880000000</vUnCom>
					<vProd>49687.15</vProd>
					<cEANTrib/>
					<uTrib>UN</uTrib>
					<qTrib>4.0000</qTrib>
					<vUnTrib>12421.7880000000</vUnTrib>
					<indTot>1</indTot>
					<xPed>FC 04.20</xPed>
					<nItemPed>1</nItemPed>
				</prod>
				<imposto>
					<ICMS>
						<ICMS40>
							<orig>0</orig>
							<CST>41</CST>
						</ICMS40>
					</ICMS>
					<IPI>
						<cEnq>999</cEnq>
						<IPINT>
							<CST>53</CST>
						</IPINT>
					</IPI>
					<PIS>
						<PISNT>
							<CST>09</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>09</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
			</det>
			<total>
				<ICMSTot>
					<vBC>0.00</vBC>
					<vICMS>0.00</vICMS>
					<vICMSDeson>0.00</vICMSDeson>
					<vFCP>0.00</vFCP>
					<vBCST>0.00</vBCST>
					<vST>0.00</vST>
					<vFCPST>0.00</vFCPST>
					<vFCPSTRet>0.00</vFCPSTRet>
					<vProd>49687.15</vProd>
					<vFrete>0.00</vFrete>
					<vSeg>0.00</vSeg>
					<vDesc>0.00</vDesc>
					<vII>0.00</vII>
					<vIPI>0.00</vIPI>
					<vIPIDevol>0.00</vIPIDevol>
					<vPIS>0.00</vPIS>
					<vCOFINS>0.00</vCOFINS>
					<vOutro>0.00</vOutro>
					<vNF>49687.15</vNF>
				</ICMSTot>
			</total>
			<transp>
				<modFrete>1</modFrete>
				<transporta>
					<CNPJ>58890252000113</CNPJ>
					<xNome>DHL EXPRESS(BRASIL) LTDA</xNome>
					<IE>109746831115</IE>
					<xEnder>AVENIDA MANUEL BANDEIRA,291-VILA LEOPOLDINA</xEnder>
					<xMun>Guarulhos</xMun>
					<UF>SP</UF>
				</transporta>
				<vol>
					<qVol>4</qVol>
					<esp>CAIXAS</esp>
					<nVol>4,0000</nVol>
					<pesoL>240.000</pesoL>
					<pesoB>280.000</pesoB>
				</vol>
			</transp>
			<cobr>
				<fat>
					<nFat>180666</nFat>
					<vOrig>49687.15</vOrig>
					<vLiq>49687.15</vLiq>
				</fat>
				<dup>
					<nDup>001</nDup>
					<dVenc>2020-08-26</dVenc>
					<vDup>49687.15</vDup>
				</dup>
			</cobr>
			<pag>
				<detPag>
					<tPag>15</tPag>
					<vPag>49687.15</vPag>
				</detPag>
			</pag>
			<infAdic>
				<infCpl>(Valor aproximado dos impostos: )(Pedido: 151640)(Vendedor: 8-PINGUIM)(Vendedor: 909-MARIO VIOTTI)(Cobranca: O MESMO(13) 3346 2299)(Entrega: O MESMO)(Pedido Cliente: FC 04.20)(Cidade: VILLA VICTORIA- BOL)</infCpl>
			</infAdic>
			<exporta>
				<UFSaidaPais>SP</UFSaidaPais>
				<xLocExporta>SAO PAULO</xLocExporta>
			</exporta>
		</infNFe>
		<Signature xmlns = 'http://www.w3.org/2000/09/xmldsig#'>
			<SignedInfo>
				<CanonicalizationMethod Algorithm = 'http://www.w3.org/TR/2001/REC-xml-c14n-20010315'/>
				<SignatureMethod Algorithm = 'http://www.w3.org/2000/09/xmldsig#rsa-sha1'/>
				<Reference URI = '#NFe35200857012650000174550010001806661378259720'>
					<Transforms>
						<Transform Algorithm = 'http://www.w3.org/2000/09/xmldsig#enveloped-signature'/>
						<Transform Algorithm = 'http://www.w3.org/TR/2001/REC-xml-c14n-20010315'/>
					</Transforms>
					<DigestMethod Algorithm = 'http://www.w3.org/2000/09/xmldsig#sha1'/>
					<DigestValue>OdEvCQSz9b / a30LqK8yIgCc + mgo =</DigestValue>
				</Reference>
			</SignedInfo>
			<SignatureValue>avYzy + HnzQXUAfKwqNv / YIjnZOB + sBHJCuhXvWcJDfveqxE0ikPtgaXaEEN4it4L67 + Uzqvzwm5QBfZ7FXMm1MkkENqbZ7SobyZnbYZp2Yzy3C92LEliB2dVWclM1H2bf + KLWa / AOCur5yXpg54Q0kXomAE6ZV7VdK5GtZwiL5I =</SignatureValue>
			<KeyInfo>
				<X509Data>
				<X509Certificate>MIICOTCCAaKgAwIBAgIQUXn + h7UWga1DGzpPjVYJZDANBgkqhkiG9w0BAQUFADBbMVkwVwYDVQQDHlAAdwB3AHcALgBmAHMAaQBzAHQALgBjAG8AbQAuAGIAcgAgACgAUwBFAE0AIABWAEEATABJAEQAQQBEAEUAIABKAFUAUgDNAEQASQBDAEEAKTAeFw0yMDA4MTkwOTI3MDZaFw0yMzA4MTkwOTI3MDZaMFsxWTBXBgNVBAMeUAB3AHcAdwAuAGYAcwBpAHMAdAAuAGMAbwBtAC4AYgByACAAKABTAEUATQAgAFYAQQBMAEkARABBAEQARQAgAEoAVQBSAM0ARABJAEMAQQApMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDVuzPKD5jupb87mp0A11qpowBTokuORSdopo6YbwvK9eyM0wi3P0qu1UuuVLSQTnHiKMdWEoEWvBmvMYKXpfvQWgAoyJb + IfI21edIrs / UjRxzaBsq8Ui / J9EAcACt60nAFmWMS3U / ZqIqxmv5VDyb + DE / DU9ZOlQ + KnWcE3o7vQIDAQABMA0GCSqGSIb3DQEBBQUAA4GBABV4U5v2ZnFKblaCRGWLZp / jaupbYpzHyYLSl7ur / 7eLYH8vpK9JPKcnLekNvbrT224FuBfo1tRHV3vAH + lAWQKwsLNJByRHH4 / 5LmYVzIZCWLwWRUapANeouRYSGY7Ye9RuCK1T2I1RmrIBaG7uNFT + d9BXA / +bKcJ5hjYShOs8</X509Certificate>
				</X509Data>
			</KeyInfo>
		</Signature>
	</NFe>
	<protNFe versao = '4.00'>
		<infProt>
			<tpAmb>1</tpAmb>
			<verAplic>4.00</verAplic>
			<chNFe>35200857012650000174550010001806661378259720</chNFe>
			<dhRecbto>2020-08-26T16:21:55-03:00</dhRecbto>
			<nProt>135200743266554</nProt>
			<digVal>AOmRVlRpJQunBxGWoV9ZC3A1+Zo=</digVal>
			<cStat>100</cStat>
			<xMotivo>Autorizado o uso da NF-e</xMotivo>
		</infProt>
	</protNFe>
</nfeProc>
";

		const string nfeXmlTextWithProductsAndFrete = @"<?xml version='1.0' encoding='utf-8'?>
<nfeProc xmlns='http://www.portalfiscal.inf.br/nfe' versao='4.00'>
	<NFe xmlns='http://www.portalfiscal.inf.br/nfe'>
		<infNFe versao='4.00' Id='NFe31200743790666000101550020000033441878442667'>
			<ide>
				<cUF>31</cUF>
				<cNF>87844266</cNF>
				<natOp>Venda de produção do estabelecimento</natOp>
				<mod>55</mod>
				<serie>2</serie>
				<nNF>3344</nNF>
				<dhEmi>2021-05-21T15:42:46-03:00</dhEmi>
				<dhSaiEnt>2020-07-16T18:42:08-03:00</dhSaiEnt>
				<tpNF>1</tpNF>
				<idDest>3</idDest>
				<cMunFG>3118601</cMunFG>
				<tpImp>1</tpImp>
				<tpEmis>1</tpEmis>
				<cDV>7</cDV>
				<tpAmb>1</tpAmb>
				<finNFe>1</finNFe>
				<indFinal>0</indFinal>
				<indPres>9</indPres>
				<procEmi>0</procEmi>
				<verProc>1,00</verProc>
			</ide>
			<emit>
				<CNPJ>43790666000101</CNPJ>
				<xNome>MAGOTTEAUX BRASIL LTDA</xNome>
				<xFant>MAGOTTEAUX BRASIL LTDA</xFant>
				<enderEmit>
					<xLgr>Avenida General David Sarnoff</xLgr>
					<nro>1221</nro>
					<xBairro>Cidade Industrial</xBairro>
					<cMun>3118601</cMun>
					<xMun>Contagem</xMun>
					<UF>MG</UF>
					<CEP>32210110</CEP>
					<xPais>Brasil</xPais>
					<fone>3121918901</fone>
				</enderEmit>
				<IE>1861530820092</IE>
				<IM>52605019</IM>
				<CNAE>2451200</CNAE>
				<CRT>3</CRT>
			</emit>
			<dest>
				<idEstrangeiro>78803130-0</idEstrangeiro>
				<xNome>MAGOTTEAUX ANDINO S.A.</xNome>
				<enderDest>
					<xLgr>Panamericana Norte KM 37</xLgr>
					<nro>S/N</nro>
					<xBairro>EXTERIOR</xBairro>
					<cMun>9999999</cMun>
					<xMun>EXTERIOR</xMun>
					<UF>EX</UF>
					<CEP>00000000</CEP>
					<cPais>1589</cPais>
					<xPais>Chile</xPais>
				</enderDest>
				<indIEDest>9</indIEDest>
			</dest>
			<entrega>
				<CNPJ/>
				<xNome>PLANTA LA CALERA</xNome>
				<xLgr>IGNACIO CARRERA PINTO</xLgr>
				<nro>32</nro>
				<xBairro>EXTERIOR</xBairro>
				<cMun>9999999</cMun>
				<xMun>EXTERIOR</xMun>
				<UF>EX</UF>
				<CEP>00000000</CEP>
				<cPais>1589</cPais>
				<xPais>Chile</xPais>
			</entrega>
			<det nItem='1'>
				<prod>
					<cProd>000000000005000041</cProd>
					<cEAN>SEM GTIN</cEAN>
					<xProd>ESFERAS MOEDORAS EM AÇO FUNDIDO 40MM HAR</xProd>
					<NCM>73259100</NCM>
					<cBenef>PR800002</cBenef>
					<CFOP>7101</CFOP>
					<uCom>KG</uCom>
					<qCom>800.0000</qCom>
					<vUnCom>4.4955500000</vUnCom>
					<vProd>3596.44</vProd>
					<cEANTrib>SEM GTIN</cEANTrib>
					<uTrib>KG</uTrib>
					<qTrib>800.0000</qTrib>
					<vUnTrib>4.4955500000</vUnTrib>
					<vFrete>391.67</vFrete>
					<indTot>1</indTot>
					<xPed>20004928</xPed>
				</prod>
				<imposto>
					<ICMS>
						<ICMS40>
							<orig>0</orig>
							<CST>41</CST>
						</ICMS40>
					</ICMS>
					<IPI>
						<cEnq>002</cEnq>
						<IPINT>
							<CST>54</CST>
						</IPINT>
					</IPI>
					<PIS>
						<PISNT>
							<CST>08</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>08</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
				<infAdProd>Pedido 20004928 Item</infAdProd>
			</det>
			<det nItem='2'>
				<prod>
					<cProd>000000000005000040</cProd>
					<cEAN>SEM GTIN</cEAN>
					<xProd>ESFERAS MOEDORAS EM AÇO FUNDIDO 30MM HAR</xProd>
					<NCM>73259100</NCM>
					<cBenef>PR800002</cBenef>
					<CFOP>7101</CFOP>
					<uCom>KG</uCom>
					<qCom>23800.0000</qCom>
					<vUnCom>4.4955176471</vUnCom>
					<vProd>106993.32</vProd>
					<cEANTrib>SEM GTIN</cEANTrib>
					<uTrib>KG</uTrib>
					<qTrib>23800.0000</qTrib>
					<vUnTrib>4.4955176471</vUnTrib>
					<vFrete>11652.94</vFrete>
					<indTot>1</indTot>
					<xPed>20004928</xPed>
				</prod>
				<imposto>
					<ICMS>
						<ICMS40>
							<orig>0</orig>
							<CST>41</CST>
						</ICMS40>
					</ICMS>
					<IPI>
						<cEnq>002</cEnq>
						<IPINT>
							<CST>54</CST>
						</IPINT>
					</IPI>
					<PIS>
						<PISNT>
							<CST>08</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>08</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
				<infAdProd>Pedido 20004928 Item</infAdProd>
			</det>
			<det nItem='3'>
				<prod>
					<cProd>000000000005000035</cProd>
					<cEAN>SEM GTIN</cEAN>
					<xProd>ESFERAS MOEDORAS EM AÇO FUNDIDO 20MM HAR</xProd>
					<NCM>73259100</NCM>
					<cBenef>PR800002</cBenef>
					<CFOP>7101</CFOP>
					<uCom>KG</uCom>
					<qCom>300.0000</qCom>
					<vUnCom>4.4956000000</vUnCom>
					<vProd>1348.68</vProd>
					<cEANTrib>SEM GTIN</cEANTrib>
					<uTrib>KG</uTrib>
					<qTrib>300.0000</qTrib>
					<vUnTrib>4.4956000000</vUnTrib>
					<vFrete>146.87</vFrete>
					<indTot>1</indTot>
					<xPed>20004928</xPed>
				</prod>
				<imposto>
					<ICMS>
						<ICMS40>
							<orig>0</orig>
							<CST>41</CST>
						</ICMS40>
					</ICMS>
					<IPI>
						<cEnq>002</cEnq>
						<IPINT>
							<CST>54</CST>
						</IPINT>
					</IPI>
					<PIS>
						<PISNT>
							<CST>08</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>08</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
				<infAdProd>Pedido 20004928 Item</infAdProd>
			</det>
			<total>
				<ICMSTot>
					<vBC>0.00</vBC>
					<vICMS>0.00</vICMS>
					<vICMSDeson>0.00</vICMSDeson>
					<vFCP>0.00</vFCP>
					<vBCST>0.00</vBCST>
					<vST>0.00</vST>
					<vFCPST>0.00</vFCPST>
					<vFCPSTRet>0.00</vFCPSTRet>
					<vProd>111938.44</vProd>
					<vFrete>12191.48</vFrete>
					<vSeg>0.00</vSeg>
					<vDesc>0.00</vDesc>
					<vII>0.00</vII>
					<vIPI>0.00</vIPI>
					<vIPIDevol>0.00</vIPIDevol>
					<vPIS>0.00</vPIS>
					<vCOFINS>0.00</vCOFINS>
					<vOutro>0.00</vOutro>
					<vNF>124129.92</vNF>
				</ICMSTot>
			</total>
			<transp>
				<modFrete>0</modFrete>
				<vol>
					<qVol>16</qVol>
					<esp>Pallets</esp>
					<nVol>00016</nVol>
					<pesoL>24900.000</pesoL>
					<pesoB>25828.000</pesoB>
				</vol>
			</transp>
			<cobr>
				<fat>
					<nFat>0000334401</nFat>
					<vOrig>124129.91</vOrig>
					<vDesc>0.00</vDesc>
					<vLiq>124129.91</vLiq>
				</fat>
				<dup>
					<nDup>001</nDup>
					<dVenc>2020-08-15</dVenc>
					<vDup>124129.91</vDup>
				</dup>
			</cobr>
			<pag>
				<detPag>
					<tPag>99</tPag>
					<vPag>124129.92</vPag>
				</detPag>
			</pag>
			<infAdic>
				<infAdFisco>NAO INCIDENCIA DE ICMS CONFORME ARTIGO 5 INCISO III DA PARTE GERAL DO. DECRETO 43.080 2006..</infAdFisco>
				<infCpl>PORTO DE EMBARQUE RIO DE JANEIRO LOCAL DE ENTREGA DESEMBARACO TTC.LOGISTICA LTDA. TERMINAL DE EMBARQUE LIBRA TERMINAIS TAXA DE.CONVERSAO USD 5 3485 DE 15 07 2020..ESFERAS MOEDORAS EM ACO FUNDIDO 40MM HARDALLOY B PARA MOINHO DE CIMENTO.ESFERAS MOEDORAS EM ACO FUNDIDO 30MM HARDALLOY B PARA MOINHO DE CIMENTO.ESFERAS MOEDORAS EM ACO FUNDIDO 20MM HARDALLOY B PARA MOINHO DE CIMENTO.DADOS BANCARIOS BANCO SANTANDER BRASIL S A 033 AGENCIA 3097. CONTA CORRENTE 13001384 03.</infCpl>
			</infAdic>
			<exporta>
				<UFSaidaPais>RJ</UFSaidaPais>
				<xLocExporta>Rio de Janeiro</xLocExporta>
			</exporta>
		</infNFe>
	</NFe>
</nfeProc>
";

		#endregion

		class NFEImportFormForTest : NFEImportForm
		{
			public NFEImportFormForTest()
			{
			}

			public NFEImportFormForTest(NFEImportObjectParent nFEImportObjectParent)
				: base(nFEImportObjectParent)
			{
			}

			public void PerformClickImportButton() => ImportButton.PerformClick();
			public void PerformClickBrowseButton() => BrowseButton.PerformClick();

			public IEnumerable<string> Logs => LogDetailsListBox.Items.Cast<string>();
		}
	}
}
