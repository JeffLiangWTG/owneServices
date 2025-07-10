using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Documents;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Moq;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DOA.Testing
{
	public class DOAWrapperTest : TestCaseWithFactory
	{
		public void TestPortSystem()
		{
			doa.PortSystem = "MGI";
			AssertEquals("MGI", wrapper.PortSystem);
		}

		public void TestMessageType()
		{
			AssertEquals(MessageSubTypeList.Codes.DOA, wrapper.MessageType);
		}

		public void TestSenderUser()
		{
			doa.SendingPartySICCode = "CI5_001";
			AssertEquals("CI5_001", wrapper.SenderUser);
		}

		public void TestSenderTiersProf()
		{
			doa.SendingPartyID = "EASYLOG";
			AssertEquals("EASYLOG", wrapper.SenderTiersProf);
		}

		public void TestRecipientUser()
		{
			doa.RecipientSICCode = "CI5_008";
			AssertEquals("CI5_008", wrapper.RecipientUser);
		}

		public void TestRecipientTiersProf()
		{
			doa.RecipientID = "SJCORP";
			AssertEquals("SJCORP", wrapper.RecipientTiersProf);
		}

		public void TestCTOUser()
		{
			doa.CTOPartySICCode = "CI5_023";
			AssertEquals("CI5_023", wrapper.CTOUser);
		}

		public void TestDeclarationNumber()
		{
			doa.DeclarationNumber = "MRN0028";
			AssertEquals("MRN0028", wrapper.DeclarationNumber);
		}

		public void TestDeclarationType()
		{
			doa.DeclarationType = "T1";
			AssertEquals("T1", wrapper.DeclarationType);
		}

		public void TestCommonAccessReference()
		{
			doa.CommonAccessRef = "ECV003";
			AssertEquals("ECV003", wrapper.CommonAccessReference);
		}

		public void TestEquipmentReference()
		{
			doa.EquipmentRef = "CNT006";
			AssertEquals("CNT006", wrapper.EquipmentReference);
		}

		public void TestContainers()
		{
			doa.Containers = new List<Container> { new Container("CNT006"), new Container("CNT007") };
			AssertContainsExactElementsInAnyOrder(new List<ZString> { "CNT006", "CNT007" }, wrapper.Containers);
		}

		public void TestDeclarationReference()
		{
			doa.DeclarationReference = "NCT004";
			AssertEquals("NCT004", wrapper.DeclarationReference);
		}

		public void TestDeclarationStatus()
		{
			doa.DeclarationStatus = "BAE";
			AssertEquals("BAE", wrapper.DeclarationStatus);
		}

		public void TestJobReference()
		{
			doa.JobNumber = "NCT001";
			AssertEquals("NCT001", wrapper.JobReference);
		}

		public void TestPrelodged()
		{
			doa.IsPrelodged = true;
			AssertEquals(true, wrapper.Prelodged);
		}

		public void TestCustomsDepartureOffice()
		{
			var departureOfficeMock = new Mock<ICodeDescription>();
			departureOfficeMock.Setup(x => x.Code).Returns("FR004000");
			doa.CustomsOfficeCodeOfDeparture = departureOfficeMock.Object;
			AssertEquals("FR004000", wrapper.CustomsDepartureOffice);
		}

		public void TestCustomsDestinationOffice()
		{
			var destinationOfficeMock = new Mock<ICodeDescription>();
			destinationOfficeMock.Setup(x => x.Code).Returns("FR002300");
			doa.CustomsOfficeCodeOfDestination = destinationOfficeMock.Object;
			AssertEquals("FR002300", wrapper.CustomsDestinationOffice);
		}

		public void TestTotalNumberOfPackages()
		{
			doa.TotalNumberOfPacks = 21;
			AssertEquals(21, wrapper.TotalNumberOfPackages);
		}

		public void TestTotalGrossWeightInKilograms()
		{
			doa.TotalGrossWeightInKilograms = 13.68m;
			AssertEquals("Decimal should be truncated.", 13, wrapper.TotalGrossWeightInKilograms);
		}

		public void TestTotalNetWeightInKilograms()
		{
			doa.TotalNetWeightInKilograms = 12.80m;
			AssertEquals("Decimal should be truncated.", 12, wrapper.TotalNetWeightInKilograms);
		}

		public void TestHasSeal()
		{
			doa.HasSeal = true;
			AssertEquals(true, wrapper.HasSeal);
		}

		public void TestHarborDuesAmount()
		{
			doa.PortDuesAmount = 80.20m;
			AssertEquals("Decimal should be truncated.", 80, wrapper.HarborDuesAmount);
		}

		public void TestHarborDuesCurrency()
		{
			var currencyMock = new Mock<ICodeDescription>();
			currencyMock.Setup(x => x.Code).Returns("EUR");
			doa.PortDuesCurrency = currencyMock.Object;
			AssertEquals("EUR", wrapper.HarborDuesCurrency);
		}

		public void TestTariffs()
		{
			var tariff1Mock = new Mock<ICodeDescription>();
			tariff1Mock.Setup(x => x.Code).Returns("304000");
			tariff1Mock.Setup(x => x.Description).Returns("Coffee");
			var tariff2Mock = new Mock<ICodeDescription>();
			tariff2Mock.Setup(x => x.Code).Returns("708000");
			tariff2Mock.Setup(x => x.Description).Returns("Milk");
			doa.Tariffs = new List<ICodeDescription>
			{
				tariff1Mock.Object,
				tariff2Mock.Object
			};

			AssertContainsExactElementsInExactOrder(new ZString[]
			{
				"304000", "708000"
			}, wrapper.Tariffs.Select(x => x.Code));
			AssertContainsExactElementsInExactOrder(new ZString[]
			{
				"Coffee", "Milk"
			}, wrapper.Tariffs.Select(x => x.Description));
		}

		protected override void SetUp()
		{
			base.SetUp();
			doa = new DOADataObject();
			wrapper = new DOAWrapper(doa);
		}

		DOADataObject doa;
		DOAWrapper wrapper;
	}
}
