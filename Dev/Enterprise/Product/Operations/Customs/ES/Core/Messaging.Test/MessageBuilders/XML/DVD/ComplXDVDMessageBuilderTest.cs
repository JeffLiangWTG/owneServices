using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DeclaComplemVinculV2Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(ComplXDVDMessageBuilder))]
	class ComplXDVDMessageBuilderTest : DVDCommonMessageBuilderTest<ComplXDVDMessageBuilder, IComplXDVDMessageDataProvider, DeclaComplemVinculV2Ent>
	{
		#region Tests
		public override void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When provider is null", () => CreateMessageBuilderWithNullProvider());
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public override void TestCreateEDIMessage()
		{
			var messageBuilder = CreateMessageBuilder();

			CombineAssertions(() =>
			{
				AssertEquals("messageBuilder.MessageType", ExpectedMessageType, messageBuilder.MessageType);
				AssertEquals("messageBuilder.MessageSubType", ExpectedMessageSubType, messageBuilder.MessageSubType);
				AssertEquals("messageBuilder.Provider", mockProvider.Object, messageBuilder.Provider);
				AssertUnsignedMessageText(messageBuilder.UnsignedMessageText);
				AssertSignedMessageText(messageBuilder.GetSignedMessageText());
			});
		}

		public override void TestPopulateSegmentosDeServicioIsTestFalse()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("IndicadorTest", messageText);
		}

		public void TestPopulateDeclarantAndRepresentative()
		{
			mockProvider.Setup(m => m.DeclarantAndRepresentative).Returns((IComplXDVDDeclarantAndRepresentative)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateDeclarantAndRepresentative_Declarant()
		{
			mockDeclarante.Setup(m => m.Declarant).Returns((IPartyNameProvider)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateDeclarantAndRepresentative_Representative()
		{
			mockDeclarante.Setup(m => m.Representative).Returns((IPartyNameProvider)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateLines()
		{
			mockProvider.Setup(m => m.Lines).Returns((IReadOnlyCollection<IComplXDVDLine>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateLine()
		{
			mockProvider.Setup(m => m.Lines).Returns(new IComplXDVDLine[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateLinePackages()
		{
			mockLine1.Setup(m => m.Packages).Returns((IReadOnlyCollection<IDVDCommonPackage>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateLinePackage()
		{
			mockLine1.Setup(m => m.Packages).Returns(new IDVDCommonPackage[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateLineVehicles()
		{
			mockLine1.Setup(m => m.Vehicles).Returns((IReadOnlyCollection<IVehicleCommon>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateLineVehicle()
		{
			mockLine1.Setup(m => m.Vehicles).Returns(new IVehicleCommon[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		#endregion

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.TypeXDvdH2;

		protected override ComplXDVDMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new ComplXDVDMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

		protected override ComplXDVDMessageBuilder CreateMessageBuilderWithNullProvider() => new ComplXDVDMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.DVDTestFilePath, "TestComplXDVD.txt");

		protected override void SetUp()
		{
			base.SetUp();
			mockProvider.Setup(m => m.MRN).Returns("20ES00999830001277");

			mockProvider.Setup(m => m.TotalPackages).Returns(7);
			mockProvider.Setup(m => m.TotalGrossMass).Returns(1);

			mockDeclarante = SetUpDeclarantAndRepresentative("ESA78587268", "TARIC, S.A.", "ESA78587269", "TARIC, S.A.", "2");
			mockProvider.Setup(m => m.DeclarantAndRepresentative).Returns(mockDeclarante.Object);

			var mockPackage1 = SetUpPackage("GR", "RTDA", 2);
			var mockPackage2 = SetUpPackage("3A", "RTDA Y OTRAS COSAS", 5);
			var mockPackage3 = SetUpPackage("FR", "OTHER", 0);
			var packages = new IDVDCommonPackage[] { mockPackage1.Object, mockPackage2.Object, mockPackage3.Object };

			var mockVehicle1 = SetUpVehicle("VS8ZAZB7861ZB6913", "RENAULT", "LAGUNA 2007");
			var mockVehicle2 = SetUpVehicle("XX", "Ford", "Focus");
			var vehicles = new IVehicleCommon[] { mockVehicle1.Object, mockVehicle2.Object };

			mockLine1 = SetUpLine(1, packages, Enumerable.Empty<IVehicleCommon>(), "NAR", 1, ZDecimal.Zero);
			var mockLine2 = SetUpLine(2, Enumerable.Empty<IDVDCommonPackage>(), vehicles, ZString.Empty, ZDecimal.Zero, 1);
			mockProvider.Setup(m => m.Lines).Returns(new IComplXDVDLine[] { mockLine1.Object, mockLine2.Object });
		}
		Mock<IComplXDVDLine> mockLine1;

		Mock<IComplXDVDDeclarantAndRepresentative> mockDeclarante;

		#region Structures SetUp

		Mock<IComplXDVDLine> SetUpLine(ZInt lineNum, IEnumerable<IDVDCommonPackage> packages, IEnumerable<IVehicleCommon> vehicles,
											ZString depositUnitOfMeasureCodeEU, ZDecimal depositUnitOfMeasureQuantity, ZDecimal supplementaryQuantity)
		{
			var mockLine = new Mock<IComplXDVDLine>();

			mockLine.Setup(m => m.LineNumber).Returns(lineNum);
			mockLine.Setup(m => m.Packages).Returns((IReadOnlyCollection<IDVDCommonPackage>)packages);
			mockLine.Setup(m => m.Vehicles).Returns((IReadOnlyCollection<IVehicleCommon>)vehicles);
			mockLine.Setup(m => m.DepositUnitOfMeasureCodeEU).Returns(depositUnitOfMeasureCodeEU);
			mockLine.Setup(m => m.DepositUnitOfMeasureQuantity).Returns(depositUnitOfMeasureQuantity);
			mockLine.Setup(m => m.GrossMassKg).Returns(1);
			mockLine.Setup(m => m.NetMassKg).Returns(0.15);
			mockLine.Setup(m => m.SupplementaryQuantity).Returns(supplementaryQuantity);
			return mockLine;
		}

		Mock<IComplXDVDDeclarantAndRepresentative> SetUpDeclarantAndRepresentative(ZString nid, ZString razonSocial, ZString cauInid, ZString cauRazaonSocial, ZString tipoAutorizacion)
		{
			var mockDeclarante = new Mock<IComplXDVDDeclarantAndRepresentative>();
			var mockDec = new Mock<IPartyNameProvider>();
			mockDec.Setup(m => m.Id).Returns(nid);
			mockDec.Setup(m => m.Name).Returns(razonSocial);
			mockDeclarante.Setup(m => m.Declarant).Returns(mockDec.Object);
			var mockRep = new Mock<IPartyNameProvider>();
			mockRep.Setup(m => m.Id).Returns(cauInid);
			mockRep.Setup(m => m.Name).Returns(cauRazaonSocial);
			mockDeclarante.Setup(m => m.Representative).Returns(mockRep.Object);
			mockDeclarante.Setup(m => m.RepresentativeTypeAuthorization).Returns(tipoAutorizacion);

			return mockDeclarante;
		}

		protected Mock<IVehicleCommon> SetUpVehicle(ZString chassis, ZString brand, ZString model)
		{
			var mockVehicle = new Mock<IVehicleCommon>();
			mockVehicle.Setup(m => m.Chassis).Returns(chassis);
			mockVehicle.Setup(m => m.Brand).Returns(brand);
			mockVehicle.Setup(m => m.Model).Returns(model);
			return mockVehicle;
		}
		#endregion
	}
}
