using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	abstract class CusEntryInstructionLookupsAbstractTest<T> : BusinessObjectLookupsTestCase where T : CusEntryInstructionLookups
	{
		public void TestDeclarationTypeList_Cached()
		{
			var list = lookups.DeclarationTypeList;
			if (list.Count == 0)
			{
				Assert("All good", true);
			}
			else
			{
				AssertSame(list, lookups.DeclarationTypeList);
			}
		}

		public void TestDeclarationTypeList()
		{
			CombineAssertions(() =>
			{
				var list = instruction.Lookups.DeclarationTypeList;
				var expectedData = GetExpectedData();
				AssertEquals("Count", expectedData.Length, list.Count);
				for (var i = 0; i < expectedData.Length; i++)
				{
					var data = expectedData[i];
					var pair = list[i];
					AssertEquals("Code", data.code, pair.Code);
					AssertEquals("Description", data.desc, pair.Description);
				}
			});
		}

		protected abstract (string code, string desc)[] GetExpectedData();

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageType;
			instruction = jobDeclaration.CustomsEntryInstructions[0];
			lookups = (T)instruction.Lookups;
		}

		protected JobDeclaration jobDeclaration;
		protected CusEntryInstruction instruction;
		protected T lookups;

		protected abstract string MessageType { get; }
	}
}
