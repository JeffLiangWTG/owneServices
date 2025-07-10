using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;
using CoreConstants = Enterprise.Core.Constants;

namespace Enterprise.Customs.JP.AFR.DocumentWrappers.Testing
{
	sealed class DocJPAFRContainerTest : DocBaseWrapperTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return DocJPAFRContainer.New(Container, DocJPAFRBills.New(Bill, Factory), 1, Factory);
		}

		DocBaseWrapper GetNewDummyDocumentWrapper()
		{
			return DocJPAFRContainer.NewDummy(DocJPAFRBills.New(Bill, Factory), Factory);
		}

		public void TestContainerOwnershipCode()
		{
			var testContainerWrapper = GetNewDocumentWrapper() as DocJPAFRContainer;
			AssertEquals("Ownership code now defaults to Carrier Owned", "2", testContainerWrapper.ContainerOwnershipCode);
			Container.JPC_OwnershipCode = "0";
			AssertEquals("0", testContainerWrapper.ContainerOwnershipCode);
			Container.JPC_OwnershipCode = "1";
			AssertEquals("1", testContainerWrapper.ContainerOwnershipCode);
			Container.JPC_OwnershipCode = "2";
			AssertEquals("2", testContainerWrapper.ContainerOwnershipCode);
			Container.JPC_OwnershipCode = "3";
			AssertEquals("3", testContainerWrapper.ContainerOwnershipCode);
			Container.JPC_OwnershipCode = "4";
			AssertEquals("4", testContainerWrapper.ContainerOwnershipCode);
			Container.JPC_OwnershipCode = "5";
			AssertEquals("5", testContainerWrapper.ContainerOwnershipCode);
			Container.JPC_OwnershipCode = "X";
			AssertEquals("X", testContainerWrapper.ContainerOwnershipCode);
		}

		public void TestContainerTypeCode()
		{
			var testContainerWrapper = GetNewDocumentWrapper() as DocJPAFRContainer;
			AssertEquals("", testContainerWrapper.ContainerTypeCode);

			var testRefContainer = Factory.New<RefContainer>();
			testRefContainer.RC_Code = "TCONT1";
			testRefContainer.RC_ContainerType = "XXX";
			Container.JPC_RC_ContainerType = testRefContainer.PK;

			CombineAssertions(() =>
			{
				AssertEquals("SN", testContainerWrapper.ContainerTypeCode);
				testRefContainer.RC_ContainerType = CoreConstants.ContainerTypes.DryStorage;
				AssertEquals("GP", testContainerWrapper.ContainerTypeCode);
				testRefContainer.RC_ContainerType = CoreConstants.ContainerTypes.Refrigerated;
				AssertEquals("RT", testContainerWrapper.ContainerTypeCode);
				testRefContainer.RC_ContainerType = CoreConstants.ContainerTypes.OpenTop;
				AssertEquals("UT", testContainerWrapper.ContainerTypeCode);
				testRefContainer.RC_ContainerType = CoreConstants.ContainerTypes.FlatRack;
				AssertEquals("PF", testContainerWrapper.ContainerTypeCode);
				testRefContainer.RC_ContainerType = CoreConstants.ContainerTypes.Bolster;
				AssertEquals("PL", testContainerWrapper.ContainerTypeCode);
				testRefContainer.RC_ContainerType = CoreConstants.ContainerTypes.Tank;
				AssertEquals("TN", testContainerWrapper.ContainerTypeCode);
			});
		}

		public void TestContainerSizeCode()
		{
			var testContainerWrapper = GetNewDocumentWrapper() as DocJPAFRContainer;
			AssertEquals("", testContainerWrapper.ContainerSizeCode);

			var testRefContainer = Factory.New<RefContainer>();
			testRefContainer.RC_Code = "TCONT1";
			testRefContainer.RC_Length = 0;
			testRefContainer.RC_Height = 0;
			Container.JPC_RC_ContainerType = testRefContainer.PK;

			CombineAssertions(() =>
			{
				AssertEquals("99", testContainerWrapper.ContainerSizeCode);
				testRefContainer.RC_Length = 10;
				testRefContainer.RC_Height = 8;
				AssertEquals("10", testContainerWrapper.ContainerSizeCode);
				testRefContainer.RC_Length = 20;
				testRefContainer.RC_Height = 8.5;
				AssertEquals("22", testContainerWrapper.ContainerSizeCode);
				testRefContainer.RC_Length = 40;
				testRefContainer.RC_Height = 9;
				AssertEquals("44", testContainerWrapper.ContainerSizeCode);
				testRefContainer.RC_Length = 50;
				testRefContainer.RC_Height = 9.5;
				AssertEquals("95", testContainerWrapper.ContainerSizeCode);
				testRefContainer.RC_Height = 10.5;
				AssertEquals("96", testContainerWrapper.ContainerSizeCode);
				testRefContainer.RC_Height = 4.25;
				AssertEquals("98", testContainerWrapper.ContainerSizeCode);
			});
		}

		public void TestContainerSeal()
		{
			var testContainerWrapper = GetNewDocumentWrapper() as DocJPAFRContainer;
			AssertEquals("", testContainerWrapper.ContainerSeal);
			Container.JPC_Seal1 = "Seal1";
			Container.JPC_Seal2 = "";
			AssertEquals("Seal1", testContainerWrapper.ContainerSeal);
			Container.JPC_Seal1 = "";
			Container.JPC_Seal2 = "Seal2";
			AssertEquals("Seal2", testContainerWrapper.ContainerSeal);
			Container.JPC_Seal1 = "Seal1";
			Container.JPC_Seal2 = "Seal2";
			AssertEquals("Seal1;Seal2", testContainerWrapper.ContainerSeal);
		}

		public void TestContainerNumber()
		{
			var testContainerWrapper = GetNewDocumentWrapper() as DocJPAFRContainer;
			AssertEquals("", testContainerWrapper.ContainerNumber);
			Container.JPC_ContainerNum = "Cont1";
			AssertEquals("Cont1", testContainerWrapper.ContainerNumber);
			Container.JPC_ContainerNum = "Cont2";
			AssertEquals("Cont2", testContainerWrapper.ContainerNumber);
		}

		public void TestContainerSequence()
		{
			var testCont1 = Bill.Containers.AddNew();
			testCont1.JPC_ContainerNum = "Cont1";
			var testCont2 = Bill.Containers.AddNew();
			testCont2.JPC_ContainerNum = "Cont2";

			var docBill = DocJPAFRBills.New(Bill, Factory);
			AssertEquals(2, docBill.Containers.Count);
			AssertEquals(1, docBill.Containers.OfType<DocJPAFRContainer>().FirstOrDefault(cont => cont.ContainerNumber == "Cont1").ContainerSequence);
			AssertEquals(2, docBill.Containers.OfType<DocJPAFRContainer>().FirstOrDefault(cont => cont.ContainerNumber == "Cont2").ContainerSequence);
		}

		public void TestDummyContainer()
		{
			var testContainerWrapper = GetNewDummyDocumentWrapper() as DocJPAFRContainer;
			AssertEquals(ZString.Empty, testContainerWrapper.ContainerOwnershipCode);
			AssertEquals(ZString.Empty, testContainerWrapper.ContainerTypeCode);
			AssertEquals(ZString.Empty, testContainerWrapper.ContainerSizeCode);
			AssertEquals(ZString.Empty, testContainerWrapper.ContainerNumber);
			AssertEquals(1, testContainerWrapper.ContainerSequence);
		}

		JPAFRContainer Container
		{
			get { return container ?? (container = Factory.New<JPAFRContainer>()); }
		}
		JPAFRContainer container;

		JPAFRBills Bill
		{
			get { return bill ?? (bill = Factory.New<JPAFRBills>()); }
		}
		JPAFRBills bill;
	}
}
