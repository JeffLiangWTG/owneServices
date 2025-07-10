using System.Linq;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(CustomsProcedureWrapper))]
public sealed class CustomsProcedureWrapperTest : DataProviderTestCase<CustomsProcedureWrapper>
{
	public void TestProcedure()
	{
		AssertNull("Procedure is null", Provider.Procedure);
	}

	public void TestPreviousProcedure()
	{
		AssertNull("Previous procedure is null.", Provider.PreviousProcedure);
	}

	public void TestAdditionalProcedures()
	{
		CombineAssertions("Should split ABL_Procedure using '+' as delimiter and filters empty codes", () =>
		{
			AssertEquals("AdditionalProcedures count", 2, Provider.AdditionalProcedures.Count);
			AssertEquals("First AdditionalProcedure", "C07", Provider.AdditionalProcedures.First());
			AssertEquals("Second AdditionalProcedure", "F48", Provider.AdditionalProcedures.Skip(1).First());
		});
	}

	protected override CustomsProcedureWrapper GetProvider()
	{
		header = Factory.New<AsycudaManifestHeader>();
		bill = header.Bills.AddNew();
		bill.ABL_Procedure = "C07++F48+";

		return new CustomsProcedureWrapper(bill);
	}

	AsycudaManifestHeader header;
	AsycudaBill bill;
}
