using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoCusEntryInstruction))]
	public class AddInfoCusEntryInstructionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestZG_Article86_3_UCC()
		{
			var bo = (AddInfoCusEntryInstruction)GetNewBusinessObject();
			AssertEquals("Lookups.YesNoList", bo.ZG_Article86_3_UCCInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestParent()
		{
			AssertType<CusEntryInstruction>(((AddInfoCusEntryInstruction)GetNewBusinessObject()).Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			return new AddInfoCusEntryInstruction(instruction.CEI_AddInfoInfo);
		}
	}
}
