using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusReconDeclarationCollection))]
	sealed class CusReconDeclarationCollectionTest : ActiveBusinessObjectCollectionTestCase<CusReconDeclarationCollection>
	{
		protected override CusReconDeclarationCollection GetCollectionToTest() => new CusReconDeclarationCollection(Factory, GlbCompany.CurrentCompany );

		public void TestCusReconDeclaration()
		{
			var factory = new BusinessObjectFactory();
			var currentCompany = SetGlbCompany("DKR");
			var selBranch = SetGlbBranch(currentCompany, "SEL");
			var pusBranch = SetGlbBranch(currentCompany, "PUS");
			var otherCompany = SetGlbCompany("DJP");
			var demBranch = SetGlbBranch(otherCompany, "OSK");
			SetCusReconDeclaration("11598210000001U", selBranch.PK);
			SetCusReconDeclaration("11598210000002U", pusBranch.PK);
			SetCusReconDeclaration("11598210000003U", demBranch.PK);
			factory.Save();

			var result = new CusReconDeclarationCollection(factory, currentCompany);
			AssertEquals(2, result.Count);
			result = new CusReconDeclarationCollection(factory, otherCompany);
			AssertEquals(1, result.Count);

			GlbCompany SetGlbCompany(string code)
			{
				var company = factory.New<GlbCompany>();
				company.GC_Code = code;
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
				return company;
			}
			GlbBranch SetGlbBranch(GlbCompany company, string code)
			{
				var branch = company.Branches.AddNew();
				branch.GB_Code = code;
				return branch;
			}
			void SetCusReconDeclaration(string jobreferenceNumber, ZGuid branchPK)
			{
				var request = factory.New<CusReconDeclaration>();
				request.CRD_ApplicationCode = "KRC";
				request.CRD_JobReferenceNumber = jobreferenceNumber;
				request.CRD_CustomsOffice = "010";
				request.CRD_MessageStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
				request.CRD_GB_Branch = branchPK;
			}
		}
	}
}
