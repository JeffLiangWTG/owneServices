namespace Enterprise.Dat.Implementation.Testing
{
	[DummyAll]
	sealed class TestClassWithAttribute
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
