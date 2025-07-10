using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay.Testing
{
	class EncabezadoBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<EncabezadoBuilder>(new CFEBuilder().EncabezadoBuilder_ExposedForTestOnly);
		}

		[ExpectNoExceptions]
		public void TestBuildEncabezado_eFactura()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var itemDetFacts = Array.Empty<Item_Det_Fact>();

			var emisorMock = new Mock<IEmisorBuilder>();
			emisorMock.Setup(x => x.BuildEmisorInfo(transactionInfo, Factory));

			var totalesMock = new Mock<ITotalesBuilder>();
			var receptorMock = new Mock<IReceptorBuilder>();
			var idDocumentoMock = new Mock<IIdDocumentoBuilder>();

			var encabezadoBuilder = new EncabezadoBuilder();
			encabezadoBuilder.SubstituteEmisorBuilder_ForTestOnly(emisorMock.Object);
			encabezadoBuilder.SubstituteReceptorBuilder_ForTestOnly(receptorMock.Object);
			encabezadoBuilder.SubstituteIdDocumentoBuilder_ForTestOnly(idDocumentoMock.Object);
			encabezadoBuilder.SubstituteTotalesBuilder_ForTestOnly(totalesMock.Object);

			var builder = encabezadoBuilder as IEncabezadoBuilder;
			builder.BuildEFacEncabezadoInfo(transactionInfo, itemDetFacts, Factory);

			emisorMock.Verify(x => x.BuildEmisorInfo(transactionInfo, Factory), Times.Once);
			receptorMock.Verify(x => x.BuildEFacReceptor(transactionInfo), Times.Once);
			idDocumentoMock.Verify(x => x.BuildEFacIdDocumento(transactionInfo), Times.Once);
			totalesMock.Verify(x => x.BuildTotales(transactionInfo, itemDetFacts), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestBuildEncabezado_eTicket()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var itemDetFacts = Array.Empty<Item_Det_Fact>();

			var emisorMock = new Mock<IEmisorBuilder>();
			var receptorMock = new Mock<IReceptorBuilder>();
			var totalesMock = new Mock<ITotalesBuilder>();

			var idDocumentoMock = new Mock<IIdDocumentoBuilder>();

			var encabezadoBuilder = new EncabezadoBuilder();
			encabezadoBuilder.SubstituteEmisorBuilder_ForTestOnly(emisorMock.Object);
			encabezadoBuilder.SubstituteReceptorBuilder_ForTestOnly(receptorMock.Object);
			encabezadoBuilder.SubstituteIdDocumentoBuilder_ForTestOnly(idDocumentoMock.Object);
			encabezadoBuilder.SubstituteTotalesBuilder_ForTestOnly(totalesMock.Object);

			var builder = encabezadoBuilder as IEncabezadoBuilder;
			builder.BuildETicketEncabezadoInfo(transactionInfo, itemDetFacts, Factory);

			emisorMock.Verify(x => x.BuildEmisorInfo(transactionInfo, Factory), Times.Once);
			receptorMock.Verify(x => x.BuildETicketReceptor(transactionInfo), Times.Once);
			idDocumentoMock.Verify(x => x.BuildETicketIdDocumento(transactionInfo), Times.Once);
			totalesMock.Verify(x => x.BuildTotales(transactionInfo, itemDetFacts), Times.Once);
		}
	}
}
