using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Documents;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Moq;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.CAED.Testing
{
	public class CAEDWrapperTest : TestCaseWithFactory
	{
		public void TestPortSystem()
		{
			caed.PortSystem = "MGI";
			AssertEquals("MGI", wrapper.PortSystem);
		}

		public void TestMessageType()
		{
			AssertEquals(MessageSubTypeList.Codes.CAED, wrapper.MessageType);
		}

		public void TestSenderUser()
		{
			caed.SendingPartySICCode = "CI5_001";
			AssertEquals("CI5_001", wrapper.SenderUser);
		}

		public void TestSenderTiersProf()
		{
			caed.SendingPartyID = "EASYLOG";
			AssertEquals("EASYLOG", wrapper.SenderTiersProf);
		}

		public void TestRecipientUser()
		{
			caed.RecipientSICCode = "CI5_008";
			AssertEquals("CI5_008", wrapper.RecipientUser);
		}

		public void TestRecipientTiersProf()
		{
			caed.RecipientID = "SJCORP";
			AssertEquals("SJCORP", wrapper.RecipientTiersProf);
		}

		public void TestCTOUser()
		{
			caed.CTOPartySICCode = "CI5_023";
			AssertEquals("CI5_023", wrapper.CTOUser);
		}

		public void TestDeclarationType()
		{
			caed.DeclarationType = "T1";
			AssertEquals("T1", wrapper.DeclarationType);
		}

		public void TestCommonAccessReference()
		{
			caed.CommonAccessRef = "ECV003";
			AssertEquals("ECV003", wrapper.CommonAccessReference);
		}

		public void TestJobReference()
		{
			caed.JobNumber = "NCT001";
			AssertEquals("NCT001", wrapper.JobReference);
		}

		public void TestCustomsDepartureOffice()
		{
			var departureOfficeMock = new Mock<ICodeDescription>();
			departureOfficeMock.Setup(x => x.Code).Returns("FR004000");
			caed.CustomsOfficeCodeOfDeparture = departureOfficeMock.Object;
			AssertEquals("FR004000", wrapper.CustomsDepartureOffice);
		}

		public void TestTotalNumberOfPackages()
		{
			caed.TotalNumberOfPacks = 21;
			AssertEquals(21, wrapper.TotalNumberOfPackages);
		}

		public void TestHarborDuesAmount()
		{
			caed.PortDuesAmount = 80.20m;
			AssertEquals("Decimal should be truncated.", 80, wrapper.HarborDuesAmount);
		}

		public void TestHarborDuesCurrency()
		{
			var currencyMock = new Mock<ICodeDescription>();
			currencyMock.Setup(x => x.Code).Returns("EUR");
			caed.PortDuesCurrency = currencyMock.Object;
			AssertEquals("EUR", wrapper.HarborDuesCurrency);
		}

		public void TestAppliesToAllPack()
		{
			caed.AppliesToAllPacks = false;
			AssertEquals(false, wrapper.AppliesToAllPacks);
		}

		public void TestDeclarantSiretNumber()
		{
			caed.DeclarantsSIRETNumber = "FR1234567";
			AssertEquals("FR1234567", wrapper.DeclarantsSIRETNumber);
		}

		public void TestContainersNumber()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			var list = new List<ZString>();

			list.Add("container");
			list.Add("container2");
			list.Add("");

			caed.Containers = list;
			AssertEquals(2, wrapper.Containers.Count);
			AssertEquals("container", wrapper.Containers.ElementAt(0));
			AssertEquals("container2", wrapper.Containers.ElementAt(1));
		}

		protected override void SetUp()
		{
			base.SetUp();
			caed = new CAEDDataObject();
			wrapper = new CAEDWrapper(caed);
		}

		CAEDDataObject caed;
		CAEDWrapper wrapper;
	}
}
