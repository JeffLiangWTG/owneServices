using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoCusEntryInstruction))]
	public class AddInfoCusEntryInstructionTest : EU.Business.Declaration.Testing.AddInfoCusEntryInstructionTest
	{
		public new void TestParent()
		{
			Assertion.AssertType<CusEntryInstruction>(((AddInfoCusEntryInstruction)GetNewBusinessObject()).Parent);
		}

		public void TestZG_TransNatureList()
		{
			AssertEquals("Lookups.TransNatureList", GetAddInfoCusEntryInstruction().ZG_TransNatureInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestZG_BypassCodeList()
		{
			AssertEquals("Lookups.ValuationBypassCodeList", GetAddInfoCusEntryInstruction().ZG_BypassCodeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
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
}
