using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(OppositeInformationDataProvider))]
sealed class OppositeInformationDataProviderTest : BasePassarDataProviderTest<OppositeInformationDataProvider>
{
	protected override OppositeInformationDataProvider CreateDataProvider() => OppositeInformationDataProvider.New(Declaration);

	public void TestNew()
	{
		AssertNull(OppositeInformationDataProvider.New(null));
	}

	public void TestReferenceNumber() => CombineAssertions(() =>
	{
		Declaration.JE_DeclarationReference = "B00183715";
		Declaration.JE_OwnerRef = "CH000001";

		AssertEquals("ReferenceNumber = JE_OwnerRef if not empty", Declaration.JE_OwnerRef, DataProvider.ReferenceNumber);

		ResetDataProvider();

		Declaration.JE_OwnerRef = ZString.Empty;
		AssertEquals("ReferenceNumber = JE_DeclarationReference if JE_OwnerRef empty", Declaration.JE_DeclarationReference, DataProvider.ReferenceNumber);

		ResetDataProvider();

		Declaration.JE_DeclarationReference = ZString.Empty;
		AssertNull("ReferenceNumber = null if both JE_OwnerRef and JE_DeclarationReference empty", DataProvider.ReferenceNumber);
	});

	public void TestText()
	{
		AssertEquals($"CW-{GlbCompany.CurrentCompany.LicenceKeyIdentifier}", DataProvider.Text);
	}

	public void TestUnusedProperties()
	{
		AssertNull("Detail", DataProvider.Detail);
	}
}
