using CargoWise.Customs.ES.MessageDefinitions.Version1.Adua.Internet.Es.Aeat.Dit.Adu.Aden.Enswsv5;
using CargoWise.Customs.ES.MessageDefinitions.Version1.COMPLEX_ICS;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.AltaH7V1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.TD;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.AnulaImportacionV1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.PreDeclaIncompletaV1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LexpedicionModificaV1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.TD;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using NUnit.Framework;
using WS = CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Adua.Internet.Es.Aeat.Advu.Jdit.Ws;

namespace Enterprise.Customs.ES.Messaging.Testing
{
	class XmlObjectCleanerTest : TestCase
	{
		public void TestRemoveEmptyXmlElements_WithSomeEmptyValues()
		{
			var xmlObjectWithSomeEmptyValues = new AltaH7V1Ent
			{
				Message = new MessageEntTd()
				{
					MessageIdentification = "SomeId",
					MessageRecipient = "",
					PreparationDate = "25/05/2021",
				},
				Declaration = new DeclarationTd()
				{
					SupervisingCustomsOffice = "SomeOffice",
					GrossMass = 10m,
					ReferenceNumberUcr = " ",
					TranspCostToDest = new ValueTd() { CurrencyCode = "EUR" },
				},
			};

			SerialiseAndAssert(xmlObjectWithSomeEmptyValues, @$"<?xml version=""1.0"" encoding=""utf-8""?>
<{XMLTestFileConstants.XmlElementNamespace}AltaH7V1Ent xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adip/jdit/ws/h7/AltaH7V1Ent.xsd"">
  <Message>
    <messageIdentification>SomeId</messageIdentification>
    <preparationDate>25/05/2021</preparationDate>
  </Message>
  <Declaration>
    <supervisingCustomsOffice>SomeOffice</supervisingCustomsOffice>
    <grossMass>10</grossMass>
    <TranspCostToDest>
      <amount>0</amount>
      <currencyCode>EUR</currencyCode>
    </TranspCostToDest>
  </Declaration>
</{XMLTestFileConstants.XmlElementNamespace}AltaH7V1Ent>");
		}

		public void TestRemoveEmptyXmlElements_WithAllValuesEmpty()
		{
			var xmlObjectWithAllValuesEmpty = new AltaH7V1Ent
			{
				Message = new MessageEntTd()
				{
					MessageIdentification = "",
					MessageRecipient = "  ",
					PreparationDate = "",
				},
				Declaration = new DeclarationTd()
				{
					SupervisingCustomsOffice = "    ",
					GrossMass = 0m,
					TranspCostToDest = new ValueTd() { CurrencyCode = "" },
				},
			};

			SerialiseAndAssert(xmlObjectWithAllValuesEmpty, @$"<?xml version=""1.0"" encoding=""utf-8""?>
<{XMLTestFileConstants.XmlElementNamespace}AltaH7V1Ent xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adip/jdit/ws/h7/AltaH7V1Ent.xsd"">
  <Declaration>
    <grossMass>0</grossMass>
    <TranspCostToDest>
      <amount>0</amount>
    </TranspCostToDest>
  </Declaration>
</{XMLTestFileConstants.XmlElementNamespace}AltaH7V1Ent>");
		}

		public void TestRemoveEmptyXmlElements_WithNullValues()
		{
			var xmlObjectWithSomeNullValues = new Cc313A
			{
				Id = null,
				Heahea = null,
				Cusoffsent740 =
				[
					new Cusoffsent740Type() { RefNumSubenr909 = "AA" },
					null,
				]
			};

			SerialiseAndAssert(xmlObjectWithSomeNullValues, @$"<?xml version=""1.0"" encoding=""utf-8""?>
<{XMLTestFileConstants.XmlElementNamespace}CC313A xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/ADUA/internet/es/aeat/dit/adu/aden/enswsv5/IE313V5Ent.xsd"">
  <MesTypMES20>CC304A</MesTypMES20>
  <CUSOFFSENT740>
    <RefNumSUBENR909>AA</RefNumSUBENR909>
  </CUSOFFSENT740>
  <Signature p2:nil=""true"" xmlns:p2=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.w3.org/2000/09/xmldsig#"" />
</{XMLTestFileConstants.XmlElementNamespace}CC313A>");
		}

		public void TestEmptyXmlElementsWithAttributesAreNotRemoved()
		{
			var xmlObjectWithEmptyElementsContainingNonEmptyAttributes = new AnulaImportacionV1Ent
			{
				SegmentosDeServicio = new SegmDeServicioTd()
				{
					Id = "SomeId",
					Test = "  ",
				},
				NumeroDeReferencia = "",
			};

			SerialiseAndAssert(xmlObjectWithEmptyElementsContainingNonEmptyAttributes, @$"<?xml version=""1.0"" encoding=""utf-8""?>
<{XMLTestFileConstants.XmlElementNamespace}AnulaImportacionV1Ent xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/ADUA/internet/es/aeat/dit/adu/adip/ws/AnulaImportacionV1Ent.xsd"">
  <SegmentosDeServicio Id=""SomeId"" />
</{XMLTestFileConstants.XmlElementNamespace}AnulaImportacionV1Ent>");
		}

		public void TestRemoveEmptyXmlElements_WithCollectionOfElements()
		{
			var xmlObjectWithElementArray = new PreDeclaIncompletaV1Ent
			{
				C05NumeroDePartidas = 2,
				Partida =
				[
					new PartidaTd
					{
						C31DescripcionDeLaMercancia = "",
						C31Contenedores = ["  ", ""],
					},
					new PartidaTd
					{
						C32NumeroDePartida = 1,
						C34PaisOrigen = "AU",
						C31Contenedores = ["C1", "", "C2"],
					}
				],
			};

			SerialiseAndAssert(xmlObjectWithElementArray, @$"<?xml version=""1.0"" encoding=""utf-8""?>
<{XMLTestFileConstants.XmlElementNamespace}PreDeclaIncompletaV1Ent xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/ADUA/internet/es/aeat/advu/jdit/ws/PreDeclaIncompletaV1Ent.xsd"">
  <Operacion>A</Operacion>
  <C05NumeroDePartidas>2</C05NumeroDePartidas>
  <Partida>
    <C32NumeroDePartida>0</C32NumeroDePartida>
  </Partida>
  <Partida>
    <C32NumeroDePartida>1</C32NumeroDePartida>
    <C31Contenedores>C1</C31Contenedores>
    <C31Contenedores>C2</C31Contenedores>
    <C34PaisOrigen>AU</C34PaisOrigen>
  </Partida>
</{XMLTestFileConstants.XmlElementNamespace}PreDeclaIncompletaV1Ent>");
		}

		public void TestRemoveEmptyXmlElements_WithNonExplicitlyMarkedElements()
		{
			var xmlObjectWithElementArray = new T2LexpedicionModificaV1Ent
			{
				NumeroDePartidasDeOrden = 2,
				PartidasDeOrdenAutorizadas =
				[
					new PartidasDeOrdenTipo
					{
						CodigoMercancia = "",
						Bultos =
						[
							new BultosTipo
							{
								ClaseDeEmbalaje = "BX",
								NumeroDeBultos = 4,
								NumeroDeBultosValueSpecified = true,
							},
							new BultosTipo
							{
								ClaseDeEmbalaje = "BY",
								NumeroDePiezas = 6,
								NumeroDePiezasValueSpecified = true,
							},
						],
						Contenedores = ["C1", "C2", "     "],
					},
					new PartidasDeOrdenTipo
					{
						NumeroDeOrdenDeLaPartida = 1,
						CodigoMercancia = "001",
						Contenedores = ["", "   "],
					}
				],
			};

			SerialiseAndAssert(xmlObjectWithElementArray, @$"<?xml version=""1.0"" encoding=""utf-8""?>
<{XMLTestFileConstants.XmlElementNamespace}T2LexpedicionModificaV1Ent xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/dit/adu/adtl/T2LexpedicionModificaV1Ent.xsd"">
  <numeroDePartidasDeOrden>2</numeroDePartidasDeOrden>
  <numeroTotalDeBultos>0</numeroTotalDeBultos>
  <indicadorDeContenedores>0</indicadorDeContenedores>
  <partidasDeOrdenAutorizadas>
    <numeroDeOrdenDeLaPartida>0</numeroDeOrdenDeLaPartida>
    <masaBrutaEnKG>0</masaBrutaEnKG>
    <masaNetaEnKG>0</masaNetaEnKG>
    <bultos>
      <claseDeEmbalaje>BX</claseDeEmbalaje>
      <numeroDeBultos>4</numeroDeBultos>
    </bultos>
    <bultos>
      <claseDeEmbalaje>BY</claseDeEmbalaje>
      <numeroDePiezas>6</numeroDePiezas>
    </bultos>
    <contenedores>C1</contenedores>
    <contenedores>C2</contenedores>
  </partidasDeOrdenAutorizadas>
  <partidasDeOrdenAutorizadas>
    <numeroDeOrdenDeLaPartida>1</numeroDeOrdenDeLaPartida>
    <codigoMercancia>001</codigoMercancia>
    <masaBrutaEnKG>0</masaBrutaEnKG>
    <masaNetaEnKG>0</masaNetaEnKG>
  </partidasDeOrdenAutorizadas>
  <Signature p2:nil=""true"" xmlns:p2=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.w3.org/2000/09/xmldsig#"" />
</{XMLTestFileConstants.XmlElementNamespace}T2LexpedicionModificaV1Ent>");
		}

		public void TestEnumElementsAreNotRemoved()
		{
			var xmlObjectWithEnumElement = new PreDeclaIncompletaV1Ent
			{
				Operacion = WS.OperacionTd.M,
				NumeroDeReferencia = " ",
				C08Importador = new WS.Cas08ImportadorTd
				{
					C08ImportadorParticular = "   ",
				},
			};

			SerialiseAndAssert(xmlObjectWithEnumElement, @$"<?xml version=""1.0"" encoding=""utf-8""?>
<{XMLTestFileConstants.XmlElementNamespace}PreDeclaIncompletaV1Ent xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/ADUA/internet/es/aeat/advu/jdit/ws/PreDeclaIncompletaV1Ent.xsd"">
  <Operacion>M</Operacion>
  <C05NumeroDePartidas>0</C05NumeroDePartidas>
</{XMLTestFileConstants.XmlElementNamespace}PreDeclaIncompletaV1Ent>");
		}

		public void TestRemoveEmptyXmlElements_MandatoryElements_C47TributoIndicadorMaxMinNor()
		{
			var xmlObjectWithElementArray = new CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ImportacionCompletaV1Ent.ImportacionCompletaV1Ent
			{
				C05NumeroDePartidas = 1,
				ServDatadoEnCeutaMelilla = "E",
				Partida =
				[
					new CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ImportacionCompletaV1Ent.PartidaTd
					{
						C31DescripcionDeLaMercancia = "",
						C47TributoDeclarado =
						[
							new Cas47TributoDeclaradoTd
							{
								C47TributoClase = "AA",
								C47TributoIndicadorMaxMinNor = ""
							}
						]
					},
				],
			};

			SerialiseAndAssert(xmlObjectWithElementArray, @$"<?xml version=""1.0"" encoding=""utf-8""?>
<{XMLTestFileConstants.XmlElementNamespace}ImportacionCompletaV1Ent xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/ADUA/internet/es/aeat/dit/adu/adip/ws/ImportacionCompletaV1Ent.xsd"">
  <ServDatadoEnCeutaMelilla>E</ServDatadoEnCeutaMelilla>
  <C05NumeroDePartidas>1</C05NumeroDePartidas>
  <C06TotalBultos>0</C06TotalBultos>
  <C19TransporteEnContenedores>0</C19TransporteEnContenedores>
  <CBImporteTotalTributos>0</CBImporteTotalTributos>
  <Partida>
    <C32NumeroDePartida>0</C32NumeroDePartida>
    <C35MasaBrutaEnKg>0</C35MasaBrutaEnKg>
    <C38MasaNetaEnKg>0</C38MasaNetaEnKg>
    <C42ValorFactura>0</C42ValorFactura>
    <C46ValorEstadistico>0</C46ValorEstadistico>
    <C47TributoDeclarado>
      <C47TributoClase>AA</C47TributoClase>
      <C47TributoBaseImponible>0</C47TributoBaseImponible>
      <C47TributoTipoImpositivo>0</C47TributoTipoImpositivo>
      <C47TributoIndicadorMaxMinNor />
      <C47TributoCuota>0</C47TributoCuota>
    </C47TributoDeclarado>
    <C47ImporteTotal>0</C47ImporteTotal>
  </Partida>
</{XMLTestFileConstants.XmlElementNamespace}ImportacionCompletaV1Ent>");
		}

		public void TestRemoveEmptyXmlElements_MandatoryElements_NifDeclarante()
		{
			var xmlObjectWithElementArray = new CargoWise.Customs.ES.MessageDefinitions.Version1.InboxNotification.LISTADECV4ENT.ListaDecV4Ent
			{
				TipoRespuesta = "response",
				Declarante = new CargoWise.Customs.ES.MessageDefinitions.Version1.InboxNotification.LISTADECV4ENT.DeclaranteType()
				{
					NifDeclarante = "",
					NombreDeclarante = "DeclarantName",
				},
			};

			SerialiseAndAssert(xmlObjectWithElementArray, @$"<?xml version=""1.0"" encoding=""utf-8""?>
<{XMLTestFileConstants.XmlElementNamespace}ListaDecV4Ent tipoRespuesta=""response"" xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adht/band/ws/li/ListaDecV4Ent.xsd"">
  <{XMLTestFileConstants.XmlElementNamespace}declarante>
    <{XMLTestFileConstants.XmlElementNamespace}NifDeclarante />
    <{XMLTestFileConstants.XmlElementNamespace}NombreDeclarante>DeclarantName</{XMLTestFileConstants.XmlElementNamespace}NombreDeclarante>
  </{XMLTestFileConstants.XmlElementNamespace}declarante>
</{XMLTestFileConstants.XmlElementNamespace}ListaDecV4Ent>");
		}

		public void TestRemoveEmptyXmlElements_MandatoryElements_NombreDeclarante()
		{
			var xmlObjectWithElementArray = new CargoWise.Customs.ES.MessageDefinitions.Version1.InboxNotification.LISTADECV4ENT.ListaDecV4Ent
			{
				TipoRespuesta = "response",
				Declarante = new CargoWise.Customs.ES.MessageDefinitions.Version1.InboxNotification.LISTADECV4ENT.DeclaranteType()
				{
					NifDeclarante = "DeclarantID",
					NombreDeclarante = "",
				},
			};

			SerialiseAndAssert(xmlObjectWithElementArray, @$"<?xml version=""1.0"" encoding=""utf-8""?>
<{XMLTestFileConstants.XmlElementNamespace}ListaDecV4Ent tipoRespuesta=""response"" xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adht/band/ws/li/ListaDecV4Ent.xsd"">
  <{XMLTestFileConstants.XmlElementNamespace}declarante>
    <{XMLTestFileConstants.XmlElementNamespace}NifDeclarante>DeclarantID</{XMLTestFileConstants.XmlElementNamespace}NifDeclarante>
    <{XMLTestFileConstants.XmlElementNamespace}NombreDeclarante />
  </{XMLTestFileConstants.XmlElementNamespace}declarante>
</{XMLTestFileConstants.XmlElementNamespace}ListaDecV4Ent>");
		}

		void SerialiseAndAssert<T>(T xmlObject, string expectedXml)
		{
			var xmlCleaner = new XmlObjectCleaner();
			xmlCleaner.RemoveEmptyXmlElements(xmlObject);
			var serialisedObject = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(xmlObject);
#if NET
			serialisedObject = serialisedObject.Replace(" xmlns=\"\"", "");
#endif
			AssertEquals(expectedXml, serialisedObject);
		}
	}
}
