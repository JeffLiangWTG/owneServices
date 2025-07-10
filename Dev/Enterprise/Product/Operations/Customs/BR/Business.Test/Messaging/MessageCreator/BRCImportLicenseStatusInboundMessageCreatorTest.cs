using System.Linq;
using CargoWise.Customs.BR.MessageDefinitions.ImportLicense.Incoming;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCImportLicenseStatusInboundMessageCreatorTest : BRCInboundMessageCreatorAbstractTest
	{
		protected override ZString InterchangeType => MessageTypeList.Codes.LIS;

		protected override ZString TransportType => ZString.Empty;

		public void TestGenerateMessageFromInterchange()
		{
			var liMessageText1 = @"<li-completa>
        <Grupo-Dados-Basicos>
          <numero-li>22/1070364-8</numero-li>
          <Importador>
            <importador-tipo>1</importador-tipo>
            <importador-identificador>08.264.406/0001-93</importador-identificador>
            <importador-nome>GARANTE DISTRIBUIDORA E IMPORTADORA DE PRODUTOS ALIMENT</importador-nome>
            <importador-atividade-economica/>
            <importador-natureza-juridica>SOCIEDADE POR COTAS DE RESPONS. LIMITADA-EMPRESA PRIVADA</importador-natureza-juridica>
            <importador-endereco-logradouro>EST DO GANCHINHO- LD</importador-endereco-logradouro>
            <importador-endereco-numero>690</importador-endereco-numero>
            <importador-endereco-complemento/>
            <importador-endereco-bairro>UMBARA</importador-endereco-bairro>
            <importador-endereco-cidade>CURITIBA</importador-endereco-cidade>
            <importador-endereco-uf>PR</importador-endereco-uf>
            <importador-endereco-cep>81930165</importador-endereco-cep>
            <importador-telefone>41 - 38882000</importador-telefone>
            <importador-pais/>
            <importador-pais-nome/>
          </Importador>
          <Outras-Informacoes>
            <pais-procedencia-mercadoria>158</pais-procedencia-mercadoria>
            <pais-procedencia-mercadoria-nome>CHILE</pais-procedencia-mercadoria-nome>
            <urf-entrada>1017500</urf-entrada>
            <urf-entrada-nome>ALF - URUGUAIANA</urf-entrada-nome>
            <urf-despacho>0917900</urf-despacho>
            <urf-despacho-nome>ALF - CURITIBA</urf-despacho-nome>
          </Outras-Informacoes>
          <Informacoes-Complementares>
            <texto-informacoes-complementares>DIR00089/22 - INVOICE: 676 - MASTERSUL COMEX LTDA (41) 3024-0100                                                                                                                                                                                             </texto-informacoes-complementares>
          </Informacoes-Complementares>
        </Grupo-Dados-Basicos>
        <Grupo-Fornecedor>
          <fornecedor-tipo>1</fornecedor-tipo>
          <pais-aquisicao-mercadoria>158</pais-aquisicao-mercadoria>
          <pais-aquisicao-mercadoria-nome>CHILE</pais-aquisicao-mercadoria-nome>
          <pais-origem-mercadoria>158</pais-origem-mercadoria>
          <pais-origem-mercadoria-nome>CHILE</pais-origem-mercadoria-nome>
          <Fornecedor>
            <fornecedor-estrangeiro-nome>TERRAUSTRAL S.A</fornecedor-estrangeiro-nome>
            <fornecedor-estrangeiro-email/>
            <fornecedor-estrangeiro-responsavel/>
            <fornecedor-estrangeiro-logradouro>HERNANDO DE AGUIRRE</fornecedor-estrangeiro-logradouro>
            <fornecedor-estrangeiro-numero>1915</fornecedor-estrangeiro-numero>
            <fornecedor-estrangeiro-complemento/>
            <fornecedor-estrangeiro-cidade>SANTIAGO</fornecedor-estrangeiro-cidade>
            <fornecedor-estrangeiro-uf>EXTERIOR</fornecedor-estrangeiro-uf>
          </Fornecedor>
          <Fabricante>
            <fabricante-nome/>
            <fabricante-email/>
            <fabricante-responsavel/>
            <fabricante-endereco-logradoudo/>
            <fabricante-endereco-numero/>
            <fabricante-endereco-complemento/>
            <fabricante-endereco-cidade/>
            <fabricante-endereco-estado/>
          </Fabricante>
        </Grupo-Fornecedor>
        <Grupo-Mercadoria>
          <Dados-Gerais>
            <subitem-ncm>2204.21.00</subitem-ncm>
            <subitem-ncm-nome>-- Em recipientes de capacidade não superior a 2 l</subitem-ncm-nome>
            <unidade-medida-estatistica>LITRO</unidade-medida-estatistica>
            <mercadoria-naladi>22042110</mercadoria-naladi>
            <mercadoria-naladi-nome/>
            <moeda>220</moeda>
            <moeda-nome>DOLAR DOS EUA</moeda-nome>
            <incoterm>FCA</incoterm>
            <incoterm-nome>FCA - FREE CARRIER</incoterm-nome>
          </Dados-Gerais>
          <Condicao-Mercadoria>
            <condicao-mercadoria>N</condicao-mercadoria>
            <condicao-mercadoria-nome>Nenhuma</condicao-mercadoria-nome>
            <tipo-enquadramento-material-usado/>
            <tipo-enquadramento-material-usado-nome/>
            <tipo-operacao-enquadramento-material-usado/>
            <tipo-operacao-enquadramento-material-usado-nome/>
          </Condicao-Mercadoria>
          <lista-destaque-ncm/>
          <lista-processo-anuente/>
          <Informacoes-Drawback>
            <drawback-regime>3</drawback-regime>
            <drawback-numero-ato-isencao/>
            <drawback-numero-ato-suspencao/>
          </Informacoes-Drawback>
          <lista-detalhe-ncm>
            <detalhe-ncm-item-drawback>
              <numero-sequencial-produto>1</numero-sequencial-produto>
              <nome-unidade-medida-comercializada>CAIXAS</nome-unidade-medida-comercializada>
              <peso-liquido-total>5.625,00000</peso-liquido-total>
              <qtd-mercadoria-unidade-comercializada>1.250,00000</qtd-mercadoria-unidade-comercializada>
              <qtd-mercadoria-unidade-estatistica>5.625,00000</qtd-mercadoria-unidade-estatistica>
              <valor-total-local-embarque>20.000,0000000</valor-total-local-embarque>
              <valor-unitario-condicao-venda>16,0000000</valor-unitario-condicao-venda>
              <valor-total-condicao-venda>20.000,0000000</valor-total-condicao-venda>
              <descricao-produto>1250 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO ROSADO MEIO SECO, ROSE (50% SYRAH - 50% CABERNET SAUVIGNON), MARCA: QUE BELLA RESERVE, SAFRA: 2021, TEOR ALC.: 13,1, LOTE: L-22071, INDICACAO GEOGRAFICA: VALLE CENTRAL</descricao-produto>
              <marca/>
              <modelo/>
              <numero-serie/>
              <ano-fabricacao/>
              <item-ac-drawback/>
            </detalhe-ncm-item-drawback>
            <detalhe-ncm-item-drawback>
              <numero-sequencial-produto>2</numero-sequencial-produto>
              <nome-unidade-medida-comercializada>CAIXAS</nome-unidade-medida-comercializada>
              <peso-liquido-total>4.410,00000</peso-liquido-total>
              <qtd-mercadoria-unidade-comercializada>980,00000</qtd-mercadoria-unidade-comercializada>
              <qtd-mercadoria-unidade-estatistica>4.410,00000</qtd-mercadoria-unidade-estatistica>
              <valor-total-local-embarque>11.760,0000000</valor-total-local-embarque>
              <valor-unitario-condicao-venda>12,0000000</valor-unitario-condicao-venda>
              <valor-total-condicao-venda>11.760,0000000</valor-total-condicao-venda>
              <descricao-produto>980 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO TINTO MEIO SECO, CARMENERE, MARCA: SUNTANA, SAFRA: 2021, TEOR ALC.: 12,6, LOTE: L-22073, INDICACAO GEOGRAFICA: VALLE CENTRAL</descricao-produto>
              <marca/>
              <modelo/>
              <numero-serie/>
              <ano-fabricacao/>
              <item-ac-drawback/>
            </detalhe-ncm-item-drawback>
            <detalhe-ncm-item-drawback>
              <numero-sequencial-produto>3</numero-sequencial-produto>
              <nome-unidade-medida-comercializada>CAIXAS</nome-unidade-medida-comercializada>
              <peso-liquido-total>3.780,00000</peso-liquido-total>
              <qtd-mercadoria-unidade-comercializada>840,00000</qtd-mercadoria-unidade-comercializada>
              <qtd-mercadoria-unidade-estatistica>3.780,00000</qtd-mercadoria-unidade-estatistica>
              <valor-total-local-embarque>10.080,0000000</valor-total-local-embarque>
              <valor-unitario-condicao-venda>12,0000000</valor-unitario-condicao-venda>
              <valor-total-condicao-venda>10.080,0000000</valor-total-condicao-venda>
              <descricao-produto>840 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO TINTO MEIO SECO, CABERNET SAUVIGNON, MARCA: SUNTANA, SAFRA: 2021, TEOR ALC.: 12,7, LOTE: L-22074, INDICACAO GEOGRAFICA: VALLE CENTRAL</descricao-produto>
              <marca/>
              <modelo/>
              <numero-serie/>
              <ano-fabricacao/>
              <item-ac-drawback/>
            </detalhe-ncm-item-drawback>
          </lista-detalhe-ncm>
          <Totalizadores>
            <quantidade-total-medida-estatistica>13.815,00000</quantidade-total-medida-estatistica>
            <peso-liquido-total-kg>13.815,00000</peso-liquido-total-kg>
            <valor-total-local-embarque>41.840,0000000</valor-total-local-embarque>
            <valor-total-condicao-venda>41.840,0000000</valor-total-condicao-venda>
          </Totalizadores>
        </Grupo-Mercadoria>
        <Grupo-Negociacao>
          <regime-acordo-tributario>1</regime-acordo-tributario>
          <regime-acordo-tributario-nome>RECOLHIMENTO INTEGRAL</regime-acordo-tributario-nome>
          <fundamento-legal-regime/>
          <fundamento-legal-regime-nome/>
          <tipo-acordo-tarifario>2</tipo-acordo-tarifario>
          <tipo-acordo-tarifario-nome>ALADI</tipo-acordo-tarifario-nome>
          <codigo-acordo-aladi>335</codigo-acordo-aladi>
          <codigo-acordo-aladi-nome>ACORDO DE COMPLEMENTACAO ECONOMICA N. 35 - MERCOSUL/CHILE</codigo-acordo-aladi-nome>
          <cobertura-cambial>1</cobertura-cambial>
          <cobertura-cambial-nome>COM COBERTURA CAMBIAL E PAGAMENTO FINAL A PRAZO DE ATE' 180</cobertura-cambial-nome>
          <modalidade-pagamento>31</modalidade-pagamento>
          <modalidade-pagamento-nome>FINANCIAMENTO DO FORNECEDOR (SUPPLIER'S CREDIT) - OUTROS</modalidade-pagamento-nome>
          <numero-dias-limite-pagamento/>
          <codigo-orgao-financeiro-internacional/>
          <codigo-orgao-financeiro-internacional-nome/>
          <codigo-motivo-sem-cobertura/>
          <codigo-motivo-sem-cobertura-nome/>
        </Grupo-Negociacao>
        <Grupo-LI-Anuencias>
          <Informacoes-LI>
            <data-registro>25/04/2022</data-registro>
            <hora-registro>16:22</hora-registro>
            <data-situacao>16/05/2022</data-situacao>
            <hora-situacao>14:25:40</hora-situacao>
            <codigo-situacao>17</codigo-situacao>
            <nome-situacao>DESEMBARACADA</nome-situacao>
            <data-restricao-embarque/>
            <data-validade-embarque>11/08/2022</data-validade-embarque>
            <data-validade-despacho>09/11/2022</data-validade-despacho>
            <numero-li-substituida/>
            <numero-li-substitutiva/>
          </Informacoes-LI>
          <Informacoes-LI-Vinculada-DI>
            <declaracao-vinculada>2209102544</declaracao-vinculada>
            <adicao-vinculada>001</adicao-vinculada>
            <retificacao/>
          </Informacoes-LI-Vinculada-DI>
          <Informacoes-Cancelamento-Vencimento-LI>
            <motivo/>
            <cpf-importador-efetuou-cancelamento/>
            <data-cancelamento-vencimento/>
            <hora-cancelamento-vencimento/>
          </Informacoes-Cancelamento-Vencimento-LI>
          <lista-anuencias>
            <anuencia>
              <orgao-anuente>MAPA</orgao-anuente>
              <codigo-situacao-anuencia>05</codigo-situacao-anuencia>
              <nome-situacao-anuencia>DEFERIDA</nome-situacao-anuencia>
              <data-diagnostico-anuencia>13/05/2022</data-diagnostico-anuencia>
              <hora-diagnostico-anuencia>14:25</hora-diagnostico-anuencia>
              <data-restricao-embarque/>
              <data-validade-embarque>11/08/2022</data-validade-embarque>
              <data-validade-despacho>09/11/2022</data-validade-despacho>
              <codigo-tratamento-administrativo>01</codigo-tratamento-administrativo>
              <nome-tratamento-administrativo>MERCADORIA</nome-tratamento-administrativo>
              <texto-anuente>Adotado procedimento simplificado para itens 2 e 3 e completo para item 1, conforme IN 67/2018 MAPA. Despacho autorizado.                                                                            </texto-anuente>
            </anuencia>
          </lista-anuencias>
        </Grupo-LI-Anuencias>
      </li-completa>";

			var liMessageText2 = @"<li-completa>
        <Grupo-Dados-Basicos>
          <numero-li>22/1062916-2</numero-li>
          <Importador>
            <importador-tipo>1</importador-tipo>
            <importador-identificador>09.356.580/0001-29</importador-identificador>
            <importador-nome>S. R DOS SANTOS EQUIPAMENTOS LTDA</importador-nome>
            <importador-atividade-economica/>
            <importador-natureza-juridica>SOCIEDADE POR COTAS DE RESPONS. LIMITADA-EMPRESA PRIVADA</importador-natureza-juridica>
            <importador-endereco-logradouro>R ANTONIO GOMES</importador-endereco-logradouro>
            <importador-endereco-numero>277</importador-endereco-numero>
            <importador-endereco-complemento/>
            <importador-endereco-bairro>JARDIM NOSSA SENHOR</importador-endereco-bairro>
            <importador-endereco-cidade>CAMPO MOURAO</importador-endereco-cidade>
            <importador-endereco-uf>PR</importador-endereco-uf>
            <importador-endereco-cep>87309220</importador-endereco-cep>
            <importador-telefone>44 - 30164603</importador-telefone>
            <importador-pais/>
            <importador-pais-nome/>
          </Importador>
          <Outras-Informacoes>
            <pais-procedencia-mercadoria>160</pais-procedencia-mercadoria>
            <pais-procedencia-mercadoria-nome>CHINA, REPUBLICA POPULAR</pais-procedencia-mercadoria-nome>
            <urf-entrada>0917800</urf-entrada>
            <urf-entrada-nome>PORTO DE PARANAGUA</urf-entrada-nome>
            <urf-despacho>0917800</urf-despacho>
            <urf-despacho-nome>PORTO DE PARANAGUA</urf-despacho-nome>
          </Outras-Informacoes>
          <Informacoes-Complementares>
            <texto-informacoes-complementares>DIM00802/22 - IMP01D - AC 210013842                                                                                                                                                                                                                          </texto-informacoes-complementares>
          </Informacoes-Complementares>
        </Grupo-Dados-Basicos>
        <Grupo-Fornecedor>
          <fornecedor-tipo>1</fornecedor-tipo>
          <pais-aquisicao-mercadoria>160</pais-aquisicao-mercadoria>
          <pais-aquisicao-mercadoria-nome>CHINA, REPUBLICA POPULAR</pais-aquisicao-mercadoria-nome>
          <pais-origem-mercadoria>160</pais-origem-mercadoria>
          <pais-origem-mercadoria-nome>CHINA, REPUBLICA POPULAR</pais-origem-mercadoria-nome>
          <Fornecedor>
            <fornecedor-estrangeiro-nome>GUANGDONG ROMAN TECHNOLOGY CO., LTD</fornecedor-estrangeiro-nome>
            <fornecedor-estrangeiro-email/>
            <fornecedor-estrangeiro-responsavel/>
            <fornecedor-estrangeiro-logradouro>XINGUANG ROAD, JINHE INDUSTRIAL ZONE, Z</fornecedor-estrangeiro-logradouro>
            <fornecedor-estrangeiro-numero>18</fornecedor-estrangeiro-numero>
            <fornecedor-estrangeiro-complemento/>
            <fornecedor-estrangeiro-cidade>DONGGUAN</fornecedor-estrangeiro-cidade>
            <fornecedor-estrangeiro-uf>EXTERIOR</fornecedor-estrangeiro-uf>
          </Fornecedor>
          <Fabricante>
            <fabricante-nome/>
            <fabricante-email/>
            <fabricante-responsavel/>
            <fabricante-endereco-logradoudo/>
            <fabricante-endereco-numero/>
            <fabricante-endereco-complemento/>
            <fabricante-endereco-cidade/>
            <fabricante-endereco-estado/>
          </Fabricante>
        </Grupo-Fornecedor>
        <Grupo-Mercadoria>
          <Dados-Gerais>
            <subitem-ncm>8516.32.00</subitem-ncm>
            <subitem-ncm-nome>-- Outros aparelhos para arranjos do cabelo</subitem-ncm-nome>
            <unidade-medida-estatistica>UNIDADE</unidade-medida-estatistica>
            <mercadoria-naladi/>
            <mercadoria-naladi-nome/>
            <moeda>220</moeda>
            <moeda-nome>DOLAR DOS EUA</moeda-nome>
            <incoterm>EXW</incoterm>
            <incoterm-nome>EXW - EX WORKS</incoterm-nome>
          </Dados-Gerais>
          <Condicao-Mercadoria>
            <condicao-mercadoria>N</condicao-mercadoria>
            <condicao-mercadoria-nome>Nenhuma</condicao-mercadoria-nome>
            <tipo-enquadramento-material-usado/>
            <tipo-enquadramento-material-usado-nome/>
            <tipo-operacao-enquadramento-material-usado/>
            <tipo-operacao-enquadramento-material-usado-nome/>
          </Condicao-Mercadoria>
          <lista-destaque-ncm>
            <destaque-ncm>
              <codigo-destaque-ncm>999</codigo-destaque-ncm>
            </destaque-ncm>
          </lista-destaque-ncm>
          <lista-processo-anuente/>
          <Informacoes-Drawback>
            <drawback-regime>4</drawback-regime>
            <drawback-numero-ato-isencao>210013842</drawback-numero-ato-isencao>
            <drawback-numero-ato-suspencao/>
          </Informacoes-Drawback>
          <lista-detalhe-ncm>
            <detalhe-ncm-item-drawback>
              <numero-sequencial-produto>1</numero-sequencial-produto>
              <nome-unidade-medida-comercializada>PECA</nome-unidade-medida-comercializada>
              <peso-liquido-total>245,00000</peso-liquido-total>
              <qtd-mercadoria-unidade-comercializada>500,00000</qtd-mercadoria-unidade-comercializada>
              <qtd-mercadoria-unidade-estatistica>500,00000</qtd-mercadoria-unidade-estatistica>
              <valor-total-local-embarque>9.081,0000000</valor-total-local-embarque>
              <valor-unitario-condicao-venda>18,1620000</valor-unitario-condicao-venda>
              <valor-total-condicao-venda>9.081,0000000</valor-total-condicao-venda>
              <descricao-produto>Prancha alisadora - Modelo: Prancha Premium 1 ¼ - PR009 - Marca: LIZZE - Especificação do Produto: Entrada: 220 Vc.a, 50-60 Hz, 177 W. Classe de proteção contra choque elétrico: Classe II. Grau de proteção contra penetração nociva de água: IPX0. Aparelho para arranjo de cabelo (chapinha).</descricao-produto>
              <marca/>
              <modelo/>
              <numero-serie/>
              <ano-fabricacao/>
              <item-ac-drawback>1</item-ac-drawback>
            </detalhe-ncm-item-drawback>
          </lista-detalhe-ncm>
          <Totalizadores>
            <quantidade-total-medida-estatistica>500,00000</quantidade-total-medida-estatistica>
            <peso-liquido-total-kg>245,00000</peso-liquido-total-kg>
            <valor-total-local-embarque>9.081,0000000</valor-total-local-embarque>
            <valor-total-condicao-venda>9.081,0000000</valor-total-condicao-venda>
          </Totalizadores>
        </Grupo-Mercadoria>
        <Grupo-Negociacao>
          <regime-acordo-tributario>3</regime-acordo-tributario>
          <regime-acordo-tributario-nome>ISENCAO</regime-acordo-tributario-nome>
          <fundamento-legal-regime>16</fundamento-legal-regime>
          <fundamento-legal-regime-nome>DRAWBACK - DL 37/66, ART 78,I (ISENCAO) - DL 37/66, ART. 78,</fundamento-legal-regime-nome>
          <tipo-acordo-tarifario/>
          <tipo-acordo-tarifario-nome/>
          <codigo-acordo-aladi/>
          <codigo-acordo-aladi-nome/>
          <cobertura-cambial>1</cobertura-cambial>
          <cobertura-cambial-nome>COM COBERTURA CAMBIAL E PAGAMENTO FINAL A PRAZO DE ATE' 180</cobertura-cambial-nome>
          <modalidade-pagamento>31</modalidade-pagamento>
          <modalidade-pagamento-nome>FINANCIAMENTO DO FORNECEDOR (SUPPLIER'S CREDIT) - OUTROS</modalidade-pagamento-nome>
          <numero-dias-limite-pagamento/>
          <codigo-orgao-financeiro-internacional/>
          <codigo-orgao-financeiro-internacional-nome/>
          <codigo-motivo-sem-cobertura/>
          <codigo-motivo-sem-cobertura-nome/>
        </Grupo-Negociacao>
        <Grupo-LI-Anuencias>
          <Informacoes-LI>
            <data-registro>25/04/2022</data-registro>
            <hora-registro>11:35</hora-registro>
            <data-situacao>07/06/2022</data-situacao>
            <hora-situacao>12:00:45</hora-situacao>
            <codigo-situacao>17</codigo-situacao>
            <nome-situacao>DESEMBARACADA</nome-situacao>
            <data-restricao-embarque/>
            <data-validade-embarque>24/07/2022</data-validade-embarque>
            <data-validade-despacho>22/10/2022</data-validade-despacho>
            <numero-li-substituida/>
            <numero-li-substitutiva/>
          </Informacoes-LI>
          <Informacoes-LI-Vinculada-DI>
            <declaracao-vinculada>2210701383</declaracao-vinculada>
            <adicao-vinculada>001</adicao-vinculada>
            <retificacao/>
          </Informacoes-LI-Vinculada-DI>
          <Informacoes-Cancelamento-Vencimento-LI>
            <motivo/>
            <cpf-importador-efetuou-cancelamento/>
            <data-cancelamento-vencimento/>
            <hora-cancelamento-vencimento/>
          </Informacoes-Cancelamento-Vencimento-LI>
          <lista-anuencias>
            <anuencia>
              <orgao-anuente>INMETRO</orgao-anuente>
              <codigo-situacao-anuencia>05</codigo-situacao-anuencia>
              <nome-situacao-anuencia>DEFERIDA</nome-situacao-anuencia>
              <data-diagnostico-anuencia>07/05/2022</data-diagnostico-anuencia>
              <hora-diagnostico-anuencia>12:00</hora-diagnostico-anuencia>
              <data-restricao-embarque/>
              <data-validade-embarque>05/08/2022</data-validade-embarque>
              <data-validade-despacho>03/11/2022</data-validade-despacho>
              <codigo-tratamento-administrativo>01</codigo-tratamento-administrativo>
              <nome-tratamento-administrativo>MERCADORIA</nome-tratamento-administrativo>
              <texto-anuente>Processo INMETRO: 2225031                                                                                                                                                                            </texto-anuente>
            </anuencia>
            <anuencia>
              <orgao-anuente>DECEX</orgao-anuente>
              <codigo-situacao-anuencia>05</codigo-situacao-anuencia>
              <nome-situacao-anuencia>DEFERIDA</nome-situacao-anuencia>
              <data-diagnostico-anuencia>25/04/2022</data-diagnostico-anuencia>
              <hora-diagnostico-anuencia>11:35</hora-diagnostico-anuencia>
              <data-restricao-embarque/>
              <data-validade-embarque>24/07/2022</data-validade-embarque>
              <data-validade-despacho>22/10/2022</data-validade-despacho>
              <codigo-tratamento-administrativo>16</codigo-tratamento-administrativo>
              <nome-tratamento-administrativo>REGIME TRIBUTARIO / FUNDAMENTO LEGAL DO REGIME</nome-tratamento-administrativo>
              <texto-anuente/>
            </anuencia>
          </lista-anuencias>
        </Grupo-LI-Anuencias>
      </li-completa>
      ";

			var messageBody = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
									<resposta-consulta-li versao=""1.0"" xmlns=""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html ResultadoConsultaLoteLiWeb.xsd"" >
										<identificador-consulta>251</identificador-consulta>
										<lista-mensagens-e-erros/>
										<lista-li-completa>
											{liMessageText1}
											{liMessageText2}
										</lista-li-completa>
									</resposta-consulta-li>";

			var interchange = ProcessEDIInterchange(messageBody);

			AssertEquals("Should have been 2 message extracted from interchange", 2, interchange.ContainedMessages.Count);

			var liList = XmlObjectSerializer.Deserialize<respostaconsultali>(messageBody);
			var listCompleta = liList.Item as listalicompletatype;
			AssertEDIMessageCreated(interchange.ContainedMessages[0], XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(listCompleta.licompleta[0]), messageNum: "1");
			AssertEDIMessageCreated(interchange.ContainedMessages[1], XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(listCompleta.licompleta[1]), messageNum: "2");
		}

		public void TestProcessEDIInterchangeInvalidMessage()
		{
			var interchange = ProcessEDIInterchange("aaaaaaaa");
			AssertEquals("Interchange status should be set to Failed", EDIInterchangeStatusList.Codes.Failed, interchange.EI_Status);
			AssertEquals("Logger", "Error - There is an error in XML document (1, 1).", logger.Logs.ElementAt(0).ToString());
		}
	}
}
