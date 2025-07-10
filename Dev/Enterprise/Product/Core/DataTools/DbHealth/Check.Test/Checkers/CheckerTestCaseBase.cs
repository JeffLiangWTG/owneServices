using NUnit.Framework;

namespace Enterprise.DbHealth.Check
{
	[TestsSubclassesOf(typeof(IChecker), ExcludePrivate = true)]
	public abstract class CheckerTestCaseBase : TestCase
	{
		#region CheckerToTest

		protected abstract IChecker GetNewCheckerInstance();

		protected IChecker CheckerToTest
		{
			get
			{
				if (checkerToTest == null)
				{
					checkerToTest = GetNewCheckerInstance();
				}

				return checkerToTest;
			}
		}

		protected IChecker checkerToTest;

		#endregion
	}
}
