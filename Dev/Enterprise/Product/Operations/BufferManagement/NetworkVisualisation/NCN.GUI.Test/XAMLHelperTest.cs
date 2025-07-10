using System;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	class XAMLHelperTest : TestCase
	{
		public void TestXAMLHelperHasParserRegistered()
		{
			try
			{
				var selector = XAMLHelper.CreateAndPopulateAlternativeSelector();
			}
			catch (Exception e)
			{
				Fail(e.Message);
			}

			Assert("Creating the selector would normally create an exception and fail the previous assertion.", true);
			ErrorReporter.Clear();
		}
	}
}
