using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

abstract class CommonPackageCheckStrategyTest : TestCaseWithFactory
{
	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = "BLT";

		var container = declaration.CusContainers.AddNew();
		container.CO_ContainerNumber = "OOCL0000006";

		var declarationBill = declaration.Bills.AddNew();
		declarationBill.CU_BillType = BillTypeList.Codes.MasterBill;
		declarationBill.CU_BillNum = "999";

		packageCT_IND = declaration.Packages.AddNew();
		packageCT_IND.CW_PackQty = 1;
		packageCT_IND.CW_PackType = "CT";
		packageCT_IND.CW_MarksAndNos = "IND";
		packageCT_IND.CW_HouseBill = declarationBill.CU_BillUniqueCode;

		packageCT_B = declaration.Packages.AddNew();
		packageCT_B.CW_PackQty = 1;
		packageCT_B.CW_PackType = "CT";
		packageCT_B.CW_MarksAndNos = "B";
		packageCT_B.CW_HouseBill = declarationBill.CU_BillUniqueCode;
	}

	protected JobDeclaration declaration;

	protected BasePackage packageCT_IND;
	protected BasePackage packageCT_B;
}
