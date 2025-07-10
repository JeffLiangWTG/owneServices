
namespace CargoWise.Common.Testing
{
	using System;
	using NUnit.Framework;

	public class TestEnumUtils : TestCase
	{
		[Flags]
		enum TestEnum
		{
			A = 1 << 0,
			B = 1 << 1,
			C = 1 << 2,
			D = 1 << 3
		}

		public void TestHasAnyFlag()
		{
			Assert((TestEnum.A | TestEnum.B | TestEnum.C).HasAnyFlag(TestEnum.B));
			Assert(!(TestEnum.A | TestEnum.B | TestEnum.C).HasAnyFlag(TestEnum.D));
			Assert((TestEnum.A | TestEnum.B | TestEnum.C).HasAnyFlag(TestEnum.D, TestEnum.C));
		}
	}
}
