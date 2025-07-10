using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitConsignment))]
	sealed class CusExitConsignmentTest : EnterpriseBusinessObjectTestCase
	{
		public static (CusExitHeader header, CusExitConsignment consignment) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = CusExitHeaderTest.GetNewBusinessObject(factory);
			var consignment = header.CusExitConsignments.AddNew();
			consignment.CXC_Status = "REJ";
			return (header, consignment);
		}

		public void TestImportGoodsItemData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_UCR = "INVUCR1";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 10;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Weight = 150.50m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			invoiceLine.JI_CustomsQuantity = 203.53m;
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Hectograms;

			var mrn = CusEntryNumber.LoadOrCreate(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode);
			mrn.CE_EntryNum = "MRN001";
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var mrn2 = CusEntryNumber.LoadOrCreate(entry2, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode);
			mrn2.CE_EntryNum = "MRN002";

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CNT001";
			container.CO_Seal = "CS001";
			container.CO_SecondSeal = "CS002";

			var equipment = declaration.Equipments.AddNew();
			equipment.CEQ_IdentificationNumber = "EQM001";
			var seal = equipment.Seals.AddNew();
			seal.BK_SealNumber = "ES001";
			var equipment2 = declaration.Equipments.AddNew();
			equipment2.CEQ_IdentificationNumber = "EQM002";
			var seal2 = equipment2.Seals.AddNew();
			seal2.BK_SealNumber = "ES002";
			var seal3 = equipment2.Seals.AddNew();
			seal3.BK_SealNumber = "ES003";

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "MB001";

			var packingGroup = bill.PackingGroups.AddNew();
			packingGroup.CR_CO_Container = container.PK;
			var package = packingGroup.Packages.AddNew();
			package.CW_PackQty = 15;
			package.CW_PackType = "NO";
			package.CW_MarksAndNos = "HELLO WORLD";

			var packagePivot = invoiceLine.PackagesPivot.AddNew();
			packagePivot.CHC_CW = package.PK;
			packagePivot.CHC_Quantity = 12;

			var packingGroup2 = bill.PackingGroups.AddNew();
			packingGroup2.CR_CEQ_Equipment = equipment.PK;
			var package2 = packingGroup2.Packages.AddNew();
			package2.CW_PackQty = 1;
			package2.CW_PackType = "NO";
			var packagePivot2 = invoiceLine.PackagesPivot.AddNew();
			packagePivot2.CHC_CW = package2.PK;

			var packingGroup3 = bill.PackingGroups.AddNew();
			packingGroup3.CR_CEQ_Equipment = equipment2.PK;
			var package3 = packingGroup3.Packages.AddNew();
			package3.CW_PackQty = 1;
			package3.CW_PackType = "NO";
			var packagePivot3 = invoiceLine.PackagesPivot.AddNew();
			packagePivot3.CHC_CW = package3.PK;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 30;
			invoiceLine2.JI_CL = entryLine2.PK;
			var packagePivot4 = invoiceLine2.PackagesPivot.AddNew();
			packagePivot4.CHC_CW = package.PK;
			var packagePivot5 = invoiceLine2.PackagesPivot.AddNew();
			packagePivot5.CHC_CW = package2.PK;

			Factory.Save();
			var seal4 = equipment2.Seals.AddNew();
			seal4.BK_SealNumber = ZString.Empty;
			consignment.CXC_MovementReference = "MRN001";
			consignment.ImportGoodsItemData(declaration);

			AssertEquals(3, header.CusExitContainers.Count);

			var exitContainer = header.CusExitContainers[0];
			AssertEquals("CNT001", exitContainer.CXN_ContainerNumber);
			AssertEquals(false, exitContainer.CXN_IsEquipment);
			var exitContainerAddSealNum = exitContainer.AllSealNumbers;
			AssertEquals(2, exitContainerAddSealNum.Count);
			AssertEquals("CS001", exitContainerAddSealNum[0].BK_SealNumber);

			var exitEquipment = header.CusExitContainers[1];
			AssertEquals("EQM001", exitEquipment.CXN_ContainerNumber);
			AssertEquals(true, exitEquipment.CXN_IsEquipment);
			var exitEquipmentAddSealNum = exitEquipment.AllSealNumbers;
			AssertEquals(1, exitEquipmentAddSealNum.Count);

			var exitEquipment2 = header.CusExitContainers[2];
			AssertEquals("EQM002", exitEquipment2.CXN_ContainerNumber);
			AssertEquals(true, exitEquipment2.CXN_IsEquipment);
			var exitEquipment2AddSealNum = exitEquipment2.AllSealNumbers;
			AssertEquals(2, exitEquipment2AddSealNum.Count);
			AssertEquals("ES002", exitEquipment2AddSealNum[0].BK_SealNumber);

			AssertEquals(2, consignment.CusExitConsignmentItems.Count);
			var consignmentItem = consignment.CusExitConsignmentItems.Single(x => x.CCI_LineNumber == 10);
			invoiceLine.JI_Weight = 150.50m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			invoiceLine.JI_CustomsQuantity = 203.53m;
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Hectograms;
			AssertEquals("consignmentItem.CCI_GrossMass", 150500m, consignmentItem.CCI_GrossMass);
			AssertEquals("consignmentItem.CCI_NetMass", 20.353m, consignmentItem.CCI_NetMass);
			AssertEquals("consignmentItem.CCI_UniqueConsignmentReference", "INVUCR1", consignmentItem.CCI_UniqueConsignmentReference);
			AssertEquals("consignmentItem.CusExitConsignmentPackagePivots.Count", 3, consignmentItem.CusExitConsignmentPackagePivots.Count);
			var pivot = consignmentItem.CusExitConsignmentPackagePivots.Single(x => x.CNP_CXN_Container == exitContainer.PK);
			var pivotPackage = pivot.Package;
			AssertEquals("pivotPackage.CXP_Quantity", 12, pivotPackage.CXP_Quantity);
			AssertEquals("pivotPackage.CXP_PackageType", "NO", pivotPackage.CXP_PackageType);
			AssertEquals("pivotPackage.CXP_MarksAndNumbers", "HELLO WORLD", pivotPackage.CXP_MarksAndNumbers);
			AssertNotEquals("pivotPackage.CXP_Sequence", ZShort.Zero, pivotPackage.CXP_Sequence);

			seal4.Delete();
			AssertNoExceptionThrown(Factory.Save);

			consignment.CusExitConsignmentItems.DeleteAll();
			package.CW_ContainerNoOrEquipmentNo = ZString.Empty;
			consignment.ImportGoodsItemData(declaration);
			AssertEquals(2, consignment.CusExitConsignmentItems.Count);
		}

		public void TestImportGoodsItemData_AdditionalContainerSeals()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;
			var mrn = CusEntryNumber.LoadOrCreate(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode);
			mrn.CE_EntryNum = "MRN001";

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CNT001";
			container.CO_Seal = ZString.Empty;
			container.CO_SecondSeal = "CS002";
			var containerSeal = container.AdditionalSeals.AddNew();
			containerSeal.BK_SealNumber = "CS002";
			var containerSeal2 = container.AdditionalSeals.AddNew();
			containerSeal2.BK_SealNumber = "CS003";
			var containerSeal3 = container.AdditionalSeals.AddNew();
			containerSeal3.BK_SealNumber = ZString.Empty;
			var containerSeal4 = container.AdditionalSeals.AddNew();
			containerSeal4.BK_SealNumber = "CS001";

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "MB001";

			var packingGroup = bill.PackingGroups.AddNew();
			packingGroup.CR_CO_Container = container.PK;
			var package = packingGroup.Packages.AddNew();
			var packagePivot = invoiceLine.PackagesPivot.AddNew();
			packagePivot.CHC_CW = package.PK;

			consignment.CXC_MovementReference = "MRN001";
			consignment.ImportGoodsItemData(declaration);
			AssertEquals(1, header.CusExitContainers.Count);
			var exitContainer = header.CusExitContainers[0];
			AssertEquals("CNT001", exitContainer.CXN_ContainerNumber);
			AssertEquals(false, exitContainer.CXN_IsEquipment);
			AssertArrayEqualsByElements("Contains all seals and CO_SecondSeal", new ZString[] { "CS002", "CS002", "CS003", "CS001" }, exitContainer.AllSealNumbers.Cast<CusExitSeal>().OrderBy(x => x.BK_SequenceNumber).Select(x => x.BK_SealNumber).ToArray());

			container.CO_Seal = "CS003";
			container.CO_SecondSeal = ZString.Empty;
			containerSeal3.BK_SealNumber = "CS004";
			header.CusExitContainers.DeleteAll();
			consignment.ImportGoodsItemData(declaration);
			AssertEquals(1, header.CusExitContainers.Count);
			exitContainer = header.CusExitContainers[0];
			AssertEquals("CNT001", exitContainer.CXN_ContainerNumber);
			AssertEquals(false, exitContainer.CXN_IsEquipment);
			AssertArrayEqualsByElements("Contains all seals and CO_Seal", new ZString[] { "CS003", "CS002", "CS003", "CS004", "CS001" }, exitContainer.AllSealNumbers.Cast<CusExitSeal>().OrderBy(x => x.BK_SequenceNumber).Select(x => x.BK_SealNumber).ToArray());
		}

		public void TestValidation()
		{
			AssertType<CusExitConsignmentValidation>(consignment.Validation);
		}

		public void TestCusAuthorizationUsages()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var authorizationUsage1 = consignment.CusAuthorizationUsages.AddNew();
			authorizationUsage1.AGC_Code = "abc";
			authorizationUsage1.AGC_Number = "123";
			authorizationUsage1.AGC_OH_Owner = orgHeader.PK;
			var authorizationUsage2 = consignment.CusAuthorizationUsages.AddNew();
			authorizationUsage2.AGC_Code = "def";
			authorizationUsage2.AGC_Number = "456";
			authorizationUsage2.AGC_OH_Owner = orgHeader.PK;
			Factory.Save();

			var authorizationUsages = NewFactory().Load<CusExitConsignment>(consignment.PK).CusAuthorizationUsages.Select(x => x.PK).ToArray();
			AssertEquals("authorizationUsages.Length", 2, authorizationUsages.Length);
			AssertContainsExactElementsInAnyOrder(new[] { authorizationUsage1.PK, authorizationUsage2.PK }, authorizationUsages);
		}

		public void TestDelete()
		{
			var authorizationUsage = consignment.CusAuthorizationUsages.AddNew();

			consignment.Delete();
			AssertEquals("CusAuthorizationUsage should be deleted.", true, authorizationUsage.IsDeleted);
		}

		public void TestCXC_MovementReference_ReadOnly()
		{
			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			AssertEquals("CER_Status not set, CER_MessageStatus != SNT", false, consignment.CXC_MovementReferenceInfo.ReadOnly);
			report.CER_MessageStatus = "SNT";
			AssertEquals("CER_MessageStatus = SNT", true, consignment.CXC_MovementReferenceInfo.ReadOnly);
			report.CER_MessageStatus = "ACC";
			AssertEquals("CER_MessageStatus != SNT", false, consignment.CXC_MovementReferenceInfo.ReadOnly);
			report.CER_Status = "ACC";
			AssertEquals("CER_Status != empty", true, consignment.CXC_MovementReferenceInfo.ReadOnly);
		}

		public void TestCusExitConsignmentItems()
		{
			AssertType<ExitControlBase.Business.CusExitConsignmentItemCollection<CusExitConsignmentItem>>(consignment.CusExitConsignmentItems);
		}

		protected override void SetUp()
		{
			base.SetUp();
			(header, consignment) = GetNewBusinessObject(Factory);
		}
		CusExitHeader header;
		CusExitConsignment consignment;

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).consignment;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.CXH_JobReference = exitHeader.PK.ToString().Substring(0, 35);
			var consignment = exitHeader.CusExitConsignments.AddNew();
			consignment.CXC_Status = "REJ";
			return consignment;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => consignment;
	}
}
