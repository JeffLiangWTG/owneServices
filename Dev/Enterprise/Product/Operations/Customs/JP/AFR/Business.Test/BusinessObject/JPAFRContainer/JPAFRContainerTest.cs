using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(JPAFRContainer))]
	class JPAFRContainerTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBill()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var container = bill1.Containers.AddNew();
			AssertEquals(bill1, container.Bill);

			container.JPC_JPB_Bill = bill2.PK;
			AssertEquals(bill2, container.Bill);
		}

		public void TestJPC_SearchExclusionIdForCheckBoxInfo()
		{
			var testHeader = Factory.New<JPAFRHeader>();
			var testBill = testHeader.Bills.AddNew();
			var testContainer = testBill.Containers.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Default SearchExclutionId", "", testContainer.JPC_SearchExclusionId);
				AssertEquals("Default SearchExclutionIdForCheckBox", false, testContainer.JPC_SearchExclusionIdForCheckBox);

				testContainer.JPC_SearchExclusionId = "a";
				AssertEquals("Unknown SearchExclutionId", "a", testContainer.JPC_SearchExclusionId);
				AssertEquals("Unknown SearchExclutionIdForCheckBox", false, testContainer.JPC_SearchExclusionIdForCheckBox);

				testContainer.JPC_SearchExclusionId = "A";
				AssertEquals("Acceptable SearchExclutionId", "A", testContainer.JPC_SearchExclusionId);
				AssertEquals("Acceptable SearchExclutionIdForCheckBox", true, testContainer.JPC_SearchExclusionIdForCheckBox);
			});

			CombineAssertions(() =>
			{
				testContainer.JPC_SearchExclusionIdForCheckBox = false;
				AssertEquals("Changing checkbox to false - SearchExclutionIdForCheckbox", false, testContainer.JPC_SearchExclusionIdForCheckBox);
				AssertEquals("Changing checkbox to false - SearchExclutionId", "", testContainer.JPC_SearchExclusionId);

				testContainer.JPC_SearchExclusionIdForCheckBox = true;
				AssertEquals("Changing checkbox to true - SearchExclutionIdForCheckbox", true, testContainer.JPC_SearchExclusionIdForCheckBox);
				AssertEquals("Changing checkbox to true - SearchExclutionId", "A", testContainer.JPC_SearchExclusionId);
			});
		}

		public void TestDefaultDataFromMatchingContainer()
		{
			var refCont1 = Factory.New<RefContainer>();
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();

			var bill1Container1 = bill1.Containers.AddNew();
			bill1Container1.JPC_ContainerNum = "CONT1";
			bill1Container1.JPC_CCCApplicationId = "1";
			bill1Container1.JPC_IsEmpty = ZBool.True;
			bill1Container1.JPC_OperatorCode = "1";
			bill1Container1.JPC_OwnershipCode = "1";
			bill1Container1.JPC_RC_ContainerType = refCont1.PK;
			bill1Container1.JPC_Seal1 = "1";
			bill1Container1.JPC_Seal2 = "1";
			bill1Container1.JPC_SearchExclusionId = "1";
			bill1Container1.JPC_TypeOfService = "1";
			bill1Container1.JPC_VanningType = "1";
			var bill2 = header.Bills.AddNew();
			var bill2Container1 = Factory.New<JPAFRContainer>();
			bill2Container1.JPC_ContainerNum = "CONT1";
			bill2.Containers.Add(bill2Container1);
			var bill3 = header.Bills.AddNew();
			var bill3Container1 = bill3.Containers.AddNew();
			bill3Container1.JPC_ContainerNum = "CONT1";

			AssertEquals("CONT1", bill3Container1.JPC_ContainerNum);
			AssertEquals("1", bill3Container1.JPC_CCCApplicationId);
			AssertEquals(ZBool.True, bill3Container1.JPC_IsEmpty);
			AssertEquals("1", bill3Container1.JPC_OperatorCode);
			AssertEquals("1", bill3Container1.JPC_OwnershipCode);
			AssertEquals(refCont1.PK, bill3Container1.JPC_RC_ContainerType);
			AssertEquals("1", bill3Container1.JPC_Seal1);
			AssertEquals("1", bill3Container1.JPC_Seal2);
			AssertEquals("1", bill3Container1.JPC_SearchExclusionId);
			AssertEquals("1", bill3Container1.JPC_TypeOfService);
			AssertEquals("1", bill3Container1.JPC_VanningType);
			AssertEquals(false, bill3Container1.JPC_SearchExclusionIdForCheckBox);

			AssertEquals("CONT1", bill1Container1.JPC_ContainerNum);
			AssertEquals("1", bill1Container1.JPC_CCCApplicationId);
			AssertEquals(ZBool.True, bill1Container1.JPC_IsEmpty);
			AssertEquals("1", bill1Container1.JPC_OperatorCode);
			AssertEquals("1", bill1Container1.JPC_OwnershipCode);
			AssertEquals(refCont1.PK, bill1Container1.JPC_RC_ContainerType);
			AssertEquals("1", bill1Container1.JPC_Seal1);
			AssertEquals("1", bill1Container1.JPC_Seal2);
			AssertEquals("1", bill1Container1.JPC_SearchExclusionId);
			AssertEquals("1", bill1Container1.JPC_TypeOfService);
			AssertEquals(false, bill1Container1.JPC_SearchExclusionIdForCheckBox);

			AssertEquals("CONT1", bill2Container1.JPC_ContainerNum);
			AssertEquals("", bill2Container1.JPC_CCCApplicationId);
			AssertEquals(ZBool.False, bill2Container1.JPC_IsEmpty);
			AssertEquals("", bill2Container1.JPC_OperatorCode);
			AssertEquals("New or Copied containers to default to CarrierSupplied for Ownership Code", ContainerOwnershipCodeList.Codes.CarrierSupplied, bill2Container1.JPC_OwnershipCode);
			AssertEquals(ZGuid.Empty, bill2Container1.JPC_RC_ContainerType);
			AssertEquals("", bill2Container1.JPC_Seal1);
			AssertEquals("", bill2Container1.JPC_Seal2);
			AssertEquals("", bill2Container1.JPC_SearchExclusionId);
			AssertEquals("", bill2Container1.JPC_TypeOfService);
			AssertEquals("", bill2Container1.JPC_VanningType);
			AssertEquals(false, bill2Container1.JPC_SearchExclusionIdForCheckBox);
		}

		public void TestUpdateMatchingContainers()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			var bill1Container1 = bill1.Containers.AddNew();
			var bill1Container2 = bill1.Containers.AddNew();
			var bill1Container3 = bill1.Containers.AddNew();
			AssertEquals("Ownership Code defaults to CarrierSupplied", ContainerOwnershipCodeList.Codes.CarrierSupplied, bill1Container1.JPC_OwnershipCode);
			AssertEquals("Ownership Code defaults to CarrierSupplied", ContainerOwnershipCodeList.Codes.CarrierSupplied, bill1Container2.JPC_OwnershipCode);
			AssertEquals("Ownership Code defaults to CarrierSupplied", ContainerOwnershipCodeList.Codes.CarrierSupplied, bill1Container3.JPC_OwnershipCode);
			bill1Container1.JPC_ContainerNum = "CONT1";
			bill1Container2.JPC_ContainerNum = "CONT2";
			bill1Container3.JPC_ContainerNum = "CONT1";
			var bill2 = header.Bills.AddNew();
			var bill2Container1 = bill2.Containers.AddNew();
			var bill2Container2 = bill2.Containers.AddNew();
			var bill2Container3 = bill1.Containers.AddNew();
			bill2Container1.JPC_ContainerNum = "CONT1";
			bill2Container2.JPC_ContainerNum = "CONT2";
			bill2Container3.JPC_ContainerNum = "CONT1";
			var bill3 = header.Bills.AddNew();
			var bill3Container1 = bill3.Containers.AddNew();
			var bill3Container2 = bill3.Containers.AddNew();
			var bill3Container3 = bill1.Containers.AddNew();
			bill3Container1.JPC_ContainerNum = "CONT1";
			bill3Container2.JPC_ContainerNum = "CONT2";
			bill3Container3.JPC_ContainerNum = "CONT1";

			var container = Factory.New<RefContainer>();

			foreach (var data in new[]
				{
					new object[] { JPAFRContainer.Schema.JPC_CCCApplicationId, "1" },
					new object[] { JPAFRContainer.Schema.JPC_IsEmpty, ZBool.True },
					new object[] { JPAFRContainer.Schema.JPC_OperatorCode, "A" },
					new object[] { JPAFRContainer.Schema.JPC_RC_ContainerType, container.PK },
					new object[] { JPAFRContainer.Schema.JPC_Seal1, "A" },
					new object[] { JPAFRContainer.Schema.JPC_Seal2, "A" },
					new object[] { JPAFRContainer.Schema.JPC_SearchExclusionId, "A" },
					new object[] { JPAFRContainer.Schema.JPC_TypeOfService, "A" },
					new object[] { JPAFRContainer.Schema.JPC_VanningType, "A" },
				})
			{
				var fieldName = data[0].ToString();
				var value = data[1];

				bill1Container1[fieldName] = data[1];
				AssertEquals("bill1Container2." + fieldName, true, ((IZType)bill1Container2[fieldName]).IsDefault);
				AssertEquals("bill1Container3." + fieldName, value, bill1Container3[fieldName]);
				AssertEquals("bill2Container1." + fieldName, value, bill2Container1[fieldName]);
				AssertEquals("bill2Container2." + fieldName, true, ((IZType)bill2Container2[fieldName]).IsDefault);
				AssertEquals("bill2Container3." + fieldName, value, bill2Container3[fieldName]);
				AssertEquals("bill3Container1." + fieldName, value, bill3Container1[fieldName]);
				AssertEquals("bill3Container2." + fieldName, true, ((IZType)bill3Container2[fieldName]).IsDefault);
				AssertEquals("bill3Container3." + fieldName, value, bill3Container3[fieldName]);
			}

			bill1Container1.JPC_ContainerNum = "CONT3";
			AssertEquals("CONT2", bill1Container2.JPC_ContainerNum);
			AssertEquals("CONT1", bill1Container3.JPC_ContainerNum);
			AssertEquals("CONT1", bill2Container1.JPC_ContainerNum);
			AssertEquals("CONT2", bill2Container2.JPC_ContainerNum);
			AssertEquals("CONT1", bill2Container3.JPC_ContainerNum);
			AssertEquals("CONT1", bill3Container1.JPC_ContainerNum);
			AssertEquals("CONT2", bill3Container2.JPC_ContainerNum);
			AssertEquals("CONT1", bill3Container3.JPC_ContainerNum);

			bill1Container1.ShouldChangeMatchedContainersOverride = (c, o, n) => false;
			bill1Container1.JPC_ContainerNum = "CONT1";
			AssertEquals("CONT2", bill1Container2.JPC_ContainerNum);
			AssertEquals("CONT1", bill1Container3.JPC_ContainerNum);
			AssertEquals("CONT1", bill2Container1.JPC_ContainerNum);
			AssertEquals("CONT2", bill2Container2.JPC_ContainerNum);
			AssertEquals("CONT1", bill2Container3.JPC_ContainerNum);
			AssertEquals("CONT1", bill3Container1.JPC_ContainerNum);
			AssertEquals("CONT2", bill3Container2.JPC_ContainerNum);
			AssertEquals("CONT1", bill3Container3.JPC_ContainerNum);

			bill1Container1.ShouldChangeMatchedContainersOverride = (c, o, n) => true;
			bill1Container1.JPC_ContainerNum = "CONT3";
			AssertEquals("CONT2", bill1Container2.JPC_ContainerNum);
			AssertEquals("CONT3", bill1Container3.JPC_ContainerNum);
			AssertEquals("CONT3", bill2Container1.JPC_ContainerNum);
			AssertEquals("CONT2", bill2Container2.JPC_ContainerNum);
			AssertEquals("CONT3", bill2Container3.JPC_ContainerNum);
			AssertEquals("CONT3", bill3Container1.JPC_ContainerNum);
			AssertEquals("CONT2", bill3Container2.JPC_ContainerNum);
			AssertEquals("CONT3", bill3Container3.JPC_ContainerNum);

			bill1Container1.ShouldChangeMatchedContainersOverride = (c, o, n) => true;
			bill1Container1.JPC_ContainerNum = "CONT1";
			AssertEquals("CONT2", bill1Container2.JPC_ContainerNum);
			AssertEquals("CONT1", bill1Container3.JPC_ContainerNum);
			AssertEquals("CONT1", bill2Container1.JPC_ContainerNum);
			AssertEquals("CONT2", bill2Container2.JPC_ContainerNum);
			AssertEquals("CONT1", bill2Container3.JPC_ContainerNum);
			AssertEquals("CONT1", bill3Container1.JPC_ContainerNum);
			AssertEquals("CONT2", bill3Container2.JPC_ContainerNum);
			AssertEquals("CONT1", bill3Container3.JPC_ContainerNum);

			bill1Container1.JPC_SearchExclusionIdForCheckBox = true;
			AssertEquals(false, bill1Container2.JPC_SearchExclusionIdForCheckBox);
			AssertEquals(true, bill1Container3.JPC_SearchExclusionIdForCheckBox);
			AssertEquals(true, bill2Container1.JPC_SearchExclusionIdForCheckBox);
			AssertEquals(false, bill2Container2.JPC_SearchExclusionIdForCheckBox);
			AssertEquals(true, bill2Container3.JPC_SearchExclusionIdForCheckBox);
			AssertEquals(true, bill3Container1.JPC_SearchExclusionIdForCheckBox);
			AssertEquals(false, bill3Container2.JPC_SearchExclusionIdForCheckBox);
			AssertEquals(true, bill3Container3.JPC_SearchExclusionIdForCheckBox);
			AssertEquals("", bill1Container2.JPC_SearchExclusionId);
			AssertEquals("A", bill1Container3.JPC_SearchExclusionId);
			AssertEquals("A", bill2Container1.JPC_SearchExclusionId);
			AssertEquals("", bill2Container2.JPC_SearchExclusionId);
			AssertEquals("A", bill2Container3.JPC_SearchExclusionId);
			AssertEquals("A", bill3Container1.JPC_SearchExclusionId);
			AssertEquals("", bill3Container2.JPC_SearchExclusionId);
			AssertEquals("A", bill3Container3.JPC_SearchExclusionId);
		}

		public void TestICanDeleteMember()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var container = bill.Containers.AddNew();

			AssertEquals(true, ((ICanDelete)container).CanDelete);

			var consol = Factory.New<ForwardingConsol>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			AssertEquals(false, ((ICanDelete)container).CanDelete);
			AssertEquals("Container values are copied from the Consol. If you want to delete this record, please do it in the Consol, or you may tick 'Override Freight Defaults'.", ((ICanDelete)container).ReasonForNotAbleToDelete);

			header.JPH_OverrideFreightDefaults = true;
			AssertEquals(true, ((ICanDelete)container).CanDelete);
		}

		public void TestIsBillShippingLineEntry()
		{
			CombineAssertions(() =>
			{
				var testContainer = Factory.New<JPAFRContainer>();
				Assert("Container Only", !testContainer.IsBillShippingLineEntry);

				var testBill = Factory.New<JPAFRBills>();
				testContainer = testBill.Containers.AddNew();
				Assert("Container Withh a StandAlone Bill", !testContainer.IsBillShippingLineEntry);

				var testHeader = Factory.New<JPAFRHeader>();
				testHeader.JPH_IsShippingLineEntry = true;
				testBill = testHeader.Bills.AddNew();
				testContainer = testBill.Containers.AddNew();
				Assert("Container Withh a VOCC Bill", testContainer.IsBillShippingLineEntry);

				testHeader = Factory.New<JPAFRHeader>();
				testHeader.JPH_IsShippingLineEntry = false;
				testBill = testHeader.Bills.AddNew();
				testContainer = testBill.Containers.AddNew();
				Assert("Container Withh a NVOCC Bill", !testContainer.IsBillShippingLineEntry);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_JobReference = "AFRTEST0001";
			var bill = header.Bills.AddNew();
			return bill.Containers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_JobReference = "AFRTEST0002";
			var bill = header.Bills.AddNew();
			return bill.Containers.AddNew();
		}
	}

	class JPAFRContainerImportBillsFromSailingTest : JPAFRImportFromSailingTestBase
	{
		public void TestImporting()
		{
			SetUpSailingEnvironmentForImporting();
			SetUpHeaderEnvironmentForImporting();
			header.ImportBillsOfLadingLinkedToTheSameSailing(new BillImportActionCollection(header.Bills));
			var container = header.Bills.FirstOrDefault(x => x.JPB_BillNumber == "SCACAAA").Containers[0];
			AssertEquals("ABCD1111", container.JPC_ContainerNum);
			AssertEquals(refContainer.PK, container.JPC_RC_ContainerType);
			AssertEquals("123", container.JPC_Seal1);
			AssertEquals("321123212332134", container.JPC_Seal2);
		}
	}
}
