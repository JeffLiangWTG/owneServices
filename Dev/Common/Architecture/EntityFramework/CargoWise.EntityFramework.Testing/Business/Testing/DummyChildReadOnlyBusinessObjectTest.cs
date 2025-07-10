namespace CargoWise.EntityFramework.Testing
{
	sealed class DummyChildReadOnlyBusinessObjectTest : TestCaseWithFactory
	{
		public void TestSupportsClone()
		{
			AssertNotNull("Should be not null", Dummy);
			Assert("SupportsClone", Dummy.SupportsClone());
		}

		public void TestClone()
		{
			Dummy.Z0_Number = 5;
			Dummy.Z0_VarCharMax = "Five";
			DummyChildReadOnlyBusinessObject dummyClone = Dummy.Clone() as DummyChildReadOnlyBusinessObject;

			AssertNotNull("Clone should work", dummyClone);
			AssertEquals("Property Value was copied", 5, dummyClone.Z0_Number);
			AssertEquals("Property Value was copied", "Five", dummyClone.Z0_VarCharMax);
		}

		DummyChildReadOnlyBusinessObject Dummy
		{
			get
			{
				if (fDummy == null)
				{
					fDummy = Factory.New<DummyChildReadOnlyBusinessObject>();
				}
				return fDummy;
			}
		}
		DummyChildReadOnlyBusinessObject fDummy;
	}
}
