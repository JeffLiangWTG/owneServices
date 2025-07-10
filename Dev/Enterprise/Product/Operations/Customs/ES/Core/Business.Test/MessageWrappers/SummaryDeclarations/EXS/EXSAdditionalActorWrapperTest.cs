using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Testing
{
	class EXSAdditionalActorWrapperTest : WrapperHelperTest<EXSAdditionalActorWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNotNull("CusReference not null", new EXSAdditionalActorWrapper(cusReference));
			});
		}

		public void TestRole()
		{
			AssertEquals("Role empty", string.Empty, wrapper.Role);
			cusReference.CFR_Code = "CS";
			wrapper = new EXSAdditionalActorWrapper(cusReference);
			AssertEquals("Role", "CS", wrapper.Role);
		}

		public void TestId()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Id empty", string.Empty, wrapper.Id);
				cusReference.CFR_Reference = OrgHeaderData.Code;
				wrapper = new EXSAdditionalActorWrapper(cusReference);
				AssertEquals("Id with data", "ORGTEST", wrapper.Id);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			var entryInstructions = declaration.CustomsEntryInstructions.AddNew();
			cusReference = entryInstructions.CusSupplyChainActorReferences.AddNew();

			wrapper = new EXSAdditionalActorWrapper(cusReference);
		}

		CusReference cusReference;
		EXSAdditionalActorWrapper wrapper;

		protected override EXSAdditionalActorWrapper GetProvider() => wrapper;
	}
}
