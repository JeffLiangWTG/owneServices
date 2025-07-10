using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.Testing
{
	internal class SysMergeValueObjectHelperTest : TestCaseWithFactory
	{
		public void TestImportAndExportCPDecAnswers()
		{
			ZGuid cpDecPk = new ZGuid("7BC0AF7B-6FA8-483e-9170-DE081D80851C");
			Xsd.CPDecAnswerCollection cpDecsXSD = new Xsd.CPDecAnswerCollection();
			Xsd.CPDecAnswer cpDec = cpDecsXSD.AddNew();
			cpDec.CPDecNum = 30;
			cpDec.AnswerCode = "N";
			cpDec.PK = cpDecPk.ToString();
			cpDec.Permit = "xxx";

			ZGuid parentPK = ZGuid.NewZGuid();

			AssertNull("precondition: CusEntryCPDec with pk '7BC0AF7B-6FA8-483e-9170-DE081D80851C' doesn't exist", Factory.Load<BaseCusEntryCPDec>(cpDecPk));
			SysMergeValueObjectHelper.ImportCPDecAnswers(Factory, cpDecsXSD, parentPK, OrgSupplierPartSchema.Constants.Prefix);

			BaseCusEntryCPDec cpDecAnsBizObj = Factory.Load<BaseCusEntryCPDec>(cpDecPk);
			AssertNotNull("CusEntryCPDec with pk '7BC0AF7B-6FA8-483e-9170-DE081D80851C' is created", cpDecAnsBizObj);
			CombineAssertions(
				delegate
				{
					AssertEquals("Permit", cpDec.Permit, cpDecAnsBizObj.ON_Permit);
					AssertEquals("Answer", "N", cpDecAnsBizObj.ON_AnswerCode);
					AssertEquals("Parent Table Code", OrgSupplierPartSchema.Constants.Prefix, cpDecAnsBizObj.ON_ParentTableCode);
					AssertEquals("CpDec PK", cpDecPk, cpDecAnsBizObj.PK);
					AssertEquals("CpDec Parent PK", parentPK, cpDecAnsBizObj.ON_ParentID);
					AssertEquals("Cp Dec Number", cpDec.CPDecNum, cpDecAnsBizObj.ON_CPDecNum);
				}
			);

			cpDecAnsBizObj.ON_AnswerCode = "Y";

			//Export
			cpDecsXSD = new Xsd.CPDecAnswerCollection();
			SysMergeValueObjectHelper.ExportCPDecAnswers(Factory, cpDecsXSD, parentPK, OrgSupplierPartSchema.Constants.Prefix);
			AssertEquals("no of elements in cpDecCollection", 1, cpDecsXSD.Count);
			AssertEquals("cpDecPK", cpDecAnsBizObj.PK.ToString(), cpDecsXSD[0].PK);
			AssertEquals("cpDeC Permit ", cpDecAnsBizObj.ON_Permit, cpDecsXSD[0].Permit);
			AssertEquals("cpDecNumber", cpDecAnsBizObj.ON_CPDecNum, cpDecsXSD[0].CPDecNum);
			AssertEquals("cpDecAnswer", "Y", cpDecsXSD[0].AnswerCode);
		}
	}
}
