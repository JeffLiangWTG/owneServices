using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoCusEntryInstruction))]
	sealed class AddInfoCusEntryInstructionTest : BusinessObjectBaseTestCase
	{
		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AddInfoCusEntryInstruction(Factory.New<CusEntryInstruction>());
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			new AddInfoCusEntryInstruction(Factory.New<CusEntryInstruction>());
		}
	}
}
