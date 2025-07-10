using System.Linq;
using CargoWise.Customs.BR.MessageDefinitions.Export.Incoming;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(NFEImportObjectItemCollection))]
	class NFEImportObjectItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NFEImportObjectItemCollection>
	{
		protected override NFEImportObjectItemCollection GetCollectionToTest()
		{
			var nfeImport = new NFEImportObject(Factory);
			return new NFEImportObjectItemCollection(nfeImport);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new NFEImportObjectItem(Factory);
		}

		public void TestProcessStreamAndAddNewItem()
		{
			var nfeImport = new NFEImportObject(Factory);
			var nfeProc = XmlObjectSerializer.Deserialize<nfeProc>(nfeXmlText);
			NFEImportObjectItemCollection nfeImportObjectItemCollection = new NFEImportObjectItemCollection(nfeImport);
			nfeImportObjectItemCollection.AddNewItems(nfeProc.NFe.infNFe.det);

			AssertEquals("One NFEImportObject should be added", 2, nfeImportObjectItemCollection.Count);

			var result = nfeImportObjectItemCollection.First() as NFEImportObjectItem;

			AssertEquals("NF-e Key Item", "1", result.NfeItemNumber);
			AssertEquals("NF-e Product Code", "FM3555", result.ProductCode);
			AssertEquals("NF-e Goods Description", "PRODUCT DESCRIPTION", result.GoodsDescription);
			AssertEquals("NF-e Tariff Code", "87089100", result.TariffCode);
			AssertEquals("NF-e Quantity UQ", "PC", result.InvoiceQuantityUQ);
			AssertEquals("NF-e Quantity ", 4m, result.InvoiceQuantity);
			AssertEquals("NF-e Total nfe Value", 56957.47m, result.TotalNfeValue);
			AssertEquals("NF-e Total Value", 49687.15m, result.TotalValue);
			AssertEquals("NF-e Customs Quantity UQ", "UN", result.CustomsQuantityUQ);
			AssertEquals("NF-e Customs Quantity ", 4m, result.CustomsQuantity);
			AssertEquals("NF-e Complementary Description", "COMPLEMENTARY", result.ComplementartDescription);
			AssertEquals("NF-e Frete Value", 2852.10m, result.FreteValue);
			AssertEquals("NF-e Seg Value", 216.45m, result.SegValue);
			AssertEquals("NF-e Outro Value", 4204.53m, result.OutroValue);
			AssertEquals("NF-e Desc Value", 2.76m, result.DescValue);
		}

		public void TestProductCodeMaxLength()
		{
			var nfeImport = new NFEImportObject(Factory);
			var nfeXmlTextLength = nfeXmlText.Replace("FM3555", "123456789012345678901234567890123456");
			var nfeProc = XmlObjectSerializer.Deserialize<nfeProc>(nfeXmlTextLength);
			NFEImportObjectItemCollection nfeImportObjectItemCollection = new NFEImportObjectItemCollection(nfeImport);
			nfeImportObjectItemCollection.AddNewItems(nfeProc.NFe.infNFe.det);

			var result = nfeImportObjectItemCollection.First() as NFEImportObjectItem;
			AssertEquals("NF-e Product Code", ZString.Empty, result.ProductCode);
		}

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
				<dhEmi>2020-08-26T06:19:00</dhEmi>
				<dhSaiEnt>2020-08-26T06:19:00</dhSaiEnt>
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
					<xProd>PRODUCT DESCRIPTION</xProd>
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
					<vFrete>2852.10</vFrete>
					<indTot>1</indTot>
					<xPed>FC 04.20</xPed>
					<vFrete>2852.10</vFrete>
					<vSeg>216.45</vSeg>
					<vOutro>4204.53</vOutro>
					<vDesc>2.76</vDesc>
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
				<infAdProd>COMPLEMENTARY</infAdProd>
			</det>
			<det nItem='2'>
				<prod>
					<cProd>102989-1</cProd>
					<cEAN>SEM GTIN</cEAN>
					<xProd>51514923 FECHAMENTO VERTICAL ALUM 51514923 DES Z_44102941 C:2235MM</xProd>
					<NCM>84313110</NCM>
					<CFOP>7949</CFOP>
					<uCom>PC</uCom>
					<qCom>1800.0000</qCom>
					<vUnCom>106.6000000000</vUnCom>
					<vProd>191880.00</vProd>
					<cEANTrib>SEM GTIN</cEANTrib>
					<uTrib>KG</uTrib>
					<qTrib>4140.0000</qTrib>
					<vUnTrib>46.3478260870</vUnTrib>
					<vFrete>2852.10</vFrete>
					<vSeg>216.45</vSeg>
					<vOutro>4204.53</vOutro>
					<vDesc>2.76</vDesc>
					<indTot>1</indTot>
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
				<infAdProd>Quantidade tributavel: 4140,0000 Unidade de medida tributavel: KG Valor unitario tributavel: R$ 46,3478260870</infAdProd>
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
	}
}
