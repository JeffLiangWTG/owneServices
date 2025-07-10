using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class CusContainerLookupsTest : TestCaseWithFactory
{
	public void TestNormalCO_FCL_LCL_NCT_List()
	{
		CombineAssertions(() =>
		{
			var containerModeList = lookup.CO_FCL_LCL_NCT_List;
			AssertEquals("Normal CO_FCL_LCL_NCT_List", "FCL, LCL, FCX, BBK", containerModeList.CodesAsString);
			NUnit.Framework.Assert.That(containerModeList, Is.SameAs(lookup.CO_FCL_LCL_NCT_List), "Cached");
		});
	}

	public void TestAEDeclarationApplicationCodeDubaiCO_FCL_LCL_NCT_List()
	{
		CombineAssertions(() =>
		{
			declaration.JE_ApplicationCode = "AED";
			var containerModeList = lookup.CO_FCL_LCL_NCT_List;
			AssertEquals("AEDeclarationApplicationCodeDubai CO_FCL_LCL_NCT_List", "GEN, FCL, LCL, MIX, BKL, BKS, ROR, LVC, EMT", containerModeList.CodesAsString);
			NUnit.Framework.Assert.That(containerModeList, Is.SameAs(lookup.CO_FCL_LCL_NCT_List), "Cached");
		});
	}

	public void TestTransferApplicationCodeCO_FCL_LCL_NCT_List()
	{
		CombineAssertions(() =>
		{
			var containerModeList = lookup.CO_FCL_LCL_NCT_List;
			AssertEquals("Normal CO_FCL_LCL_NCT_List", "FCL, LCL, FCX, BBK", containerModeList.CodesAsString);

			declaration.JE_ApplicationCode = "AED";
			containerModeList = lookup.CO_FCL_LCL_NCT_List;
			AssertEquals("AEDeclarationApplicationCodeDubai CO_FCL_LCL_NCT_List", "GEN, FCL, LCL, MIX, BKL, BKS, ROR, LVC, EMT", containerModeList.CodesAsString);

			declaration.JE_ApplicationCode = "ITF";
			containerModeList = lookup.CO_FCL_LCL_NCT_List;
			AssertEquals("Normal CO_FCL_LCL_NCT_List", "FCL, LCL, FCX, BBK", containerModeList.CodesAsString);
		});
	}

	public void TestContainer()
	{
		AssertEquals(lookup.Container, container);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		container = Factory.New<CusContainer>();
		lookup = new CusContainerLookups(container);
		container.CO_JE = declaration.PK;
	}

	CusContainerLookups lookup;
	CusContainer container;
	JobDeclaration declaration;
}
