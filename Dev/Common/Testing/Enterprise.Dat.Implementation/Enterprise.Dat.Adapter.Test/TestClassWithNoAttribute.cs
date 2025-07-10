namespace Enterprise.Dat.Implementation.Testing
{
	sealed class TestClassWithNoAttribute
	{
		[DummyMethod]
		[DummyAll]
		public void TestMethodWithAttribute()
		{
			_ = 1;
		}

		public void TestMethodWithNoAttribute()
		{
			_ = 1;
		}
	}
}
