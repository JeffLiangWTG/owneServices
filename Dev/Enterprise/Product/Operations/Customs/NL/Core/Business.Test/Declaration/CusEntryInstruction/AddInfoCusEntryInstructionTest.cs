using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(AddInfoCusEntryInstruction))]
sealed class AddInfoCusEntryInstructionTest : EU.Business.Declaration.Testing.AddInfoCusEntryInstructionTest
{
	public new void TestParent()
	{
		Assertion.AssertType<CusEntryInstruction>(((AddInfoCusEntryInstruction)GetNewBusinessObject()).Parent);
	}

	public void TestZG_TransNatureList()
	{
		AssertEquals("Lookups.TransNatureList", GetAddInfoCusEntryInstruction().ZG_TransNatureInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
	}

	public void TestZG_TransNatureCaption() => CombineAssertions(() =>
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(GetAddInfoCusEntryInstruction().ZG_TransNatureInfo, null);
		AssertEquals("Caption", "Tran. Nature", resourceStringData.Caption);
		AssertEquals("FullDescription", "[UCC 8/5] Transaction Nature", resourceStringData.FullDescription);
	});

	public void TestLookups()
	{
		AssertType<AddInfoCusEntryInstructionLookups>(typeof(AddInfoCusEntryInstruction).GetMethod("GetNewLookups", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(GetAddInfoCusEntryInstruction(), null));
	}

	public void TestValidation()
	{
		AssertType<AddInfoCusEntryInstructionValidation>(typeof(AddInfoCusEntryInstruction).GetMethod("GetNewValidation", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(GetAddInfoCusEntryInstruction(), null));
	}

	protected override BusinessObject GetNewBusinessObject() => GetAddInfoCusEntryInstruction();

	protected override void SetUp()
	{
		base.SetUp();
		cusEntryInstruction = Factory.New<CusEntryInstruction>();
	}

	CusEntryInstruction cusEntryInstruction;

	AddInfoCusEntryInstruction GetAddInfoCusEntryInstruction() => new AddInfoCusEntryInstruction(cusEntryInstruction.CEI_AddInfoInfo);
}
