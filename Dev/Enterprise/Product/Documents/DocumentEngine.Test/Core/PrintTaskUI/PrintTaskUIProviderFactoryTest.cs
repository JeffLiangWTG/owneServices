using System;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class PrintTaskUIProviderFactoryTest : TestCase
	{
		public void TestNewForAllUITypes()
		{
			foreach (PrintTaskUIProviderTypes uiType in Enum.GetValues(typeof(PrintTaskUIProviderTypes)))
			{
				AssertNotNull(PrintTaskUIProviderFactory.CreateForTesting(uiType));
			}
		}

		public void TestGetDefaultUIType()
		{
			bool originalGlobalsIsUserInteractive = Globals.IsUserInteractive;
			bool originalGlobalsIsWeb = Globals.IsWeb;

			try
			{
				Globals.IsUserInteractive = true;
				Globals.IsWeb = false;
				AssertEquals("PrintTaskUIProviderFactory.GetDefaultUITypeForTesting()", PrintTaskUIProviderTypes.WinForms, PrintTaskUIProviderFactory.GetDefaultUITypeForTesting());

				Globals.IsUserInteractive = true;
				Globals.IsWeb = true;
				AssertEquals("PrintTaskUIProviderFactory.GetDefaultUITypeForTesting()", PrintTaskUIProviderTypes.Web, PrintTaskUIProviderFactory.GetDefaultUITypeForTesting());

				Globals.IsUserInteractive = false;
				Globals.IsWeb = false;
				AssertEquals("PrintTaskUIProviderFactory.GetDefaultUITypeForTesting()", PrintTaskUIProviderTypes.Unattended, PrintTaskUIProviderFactory.GetDefaultUITypeForTesting());

				Globals.IsUserInteractive = false;
				Globals.IsWeb = true;
				AssertEquals("PrintTaskUIProviderFactory.GetDefaultUITypeForTesting()", PrintTaskUIProviderTypes.Unattended, PrintTaskUIProviderFactory.GetDefaultUITypeForTesting());
			}
			finally
			{
				Globals.IsUserInteractive = originalGlobalsIsUserInteractive;
				Globals.IsWeb = originalGlobalsIsWeb;
			}
		}
	}
}
