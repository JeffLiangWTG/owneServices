using System;

#region TestCase
#if DEBUG
using NUnit.Framework;
#endif
#endregion

namespace SourceSafeTestProject2
{
	public class TestFile2
	{
		public TestFile2()
		{
		}

		#region Implementation
		
		
		
		#endregion
	}

	#region TestCase for TestFile2
	#if DEBUG

	public class TestFile2Test : TestCase
	{
		[ExpectNoExceptions]
		public void TestEmpty()
		{
		}
	}

	#endif
	#endregion
}
