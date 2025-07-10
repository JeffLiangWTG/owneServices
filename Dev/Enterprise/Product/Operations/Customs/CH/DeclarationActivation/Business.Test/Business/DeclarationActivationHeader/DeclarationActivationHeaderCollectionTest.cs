using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.DeclarationActivation.Business.Testing;

[TestedType(typeof(DeclarationActivationHeaderCollection))]
sealed class DeclarationActivationHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<DeclarationActivationHeaderCollection>
{
	public void TestAllow()
	{
		var collection = GetCollectionToTest();
		collection.SetReadOnlyIncludingChildren(false);
		var bindingList = collection as IBindingList;
		AssertEquals("AllowNew", true, bindingList.AllowNew);
		AssertEquals("AllowEdit", true, bindingList.AllowEdit);
		AssertEquals("AllowRemove", true, bindingList.AllowRemove);
	}

	public void TestCompanyFilter()
	{
		var otherCompany = Factory.New<GlbCompany>();
		var header1 = Factory.New<DeclarationActivationHeader>();
		header1.CXH_GC_Company = GlbCompany.CurrentCompany.PK;
		var header2 = Factory.New<DeclarationActivationHeader>();
		header2.CXH_GC_Company = otherCompany.PK;
		Factory.Save();
		var collection = new DeclarationActivationHeaderCollection(new BusinessObjectFactory());
		AssertEquals("Count", 1, collection.Count);
		AssertEquals("Header", header1.PK, collection[0].PK);
	}

	public void TestGetFilter() => CombineAssertions(() =>
	{
		var otherCompany = Factory.New<GlbCompany>();
		otherCompany.GC_Code = "C99";
		var otherBranch = GlbCompany.GetCurrentCompany(Factory).Branches.AddNew();
		otherBranch.GB_Code = "B99";

		var header1 = CreateHeader();
		var header2 = CreateHeader(applicationCode: CusExitHeaderApplicationCodeList.Codes.ExitControl);
		var header3 = CreateHeader(companyPK: otherCompany.PK);
		var header4 = CreateHeader(branchPK: otherBranch.PK);
		Factory.Save();

		var collection = new DeclarationActivationHeaderCollection(Factory);

		AssertEquals("Expected Header", true, collection.Contains(header1));
		AssertEquals("Wrong ApplicationCode", false, collection.Contains(header2));
		AssertEquals("Wrong company", false, collection.Contains(header3));
		AssertEquals("Other branch", true, collection.Contains(header4));

		DeclarationActivationHeader CreateHeader(string applicationCode = CusExitHeaderApplicationCodeList.Codes.CHDeclarationActivation, ZGuid? companyPK = null, ZGuid? branchPK = null)
		{
			var header = Factory.New<DeclarationActivationHeader>();
			header.CXH_ApplicationCode = applicationCode;
			header.CXH_GC_Company = companyPK ?? GlbCompany.CurrentCompany.PK;
			header.CXH_GB_Branch = branchPK ?? GlbBranch.CurrentBranch.PK;
			return header;
		}
	});

	protected override DeclarationActivationHeaderCollection GetCollectionToTest()
	{
		var header = Factory.New<DeclarationActivationHeader>();
		Factory.Save();
		return new DeclarationActivationHeaderCollection(Factory);
	}
}
