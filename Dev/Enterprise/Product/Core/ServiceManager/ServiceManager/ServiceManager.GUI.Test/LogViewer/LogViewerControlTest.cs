using System;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.ServiceManager.GUI.Testing
{
	[TestedType(typeof(LogViewerControl))]
	class LogViewerControlTest : TransactionedTestCase
	{
		[RequiresSTA]
		public void TestIsUsingSearchBasedLogViewerControl()
		{
			Test(LoggingMethods.FSL, typeof(FileBasedLogViewer));
			Test(LoggingMethods.KAF, typeof(SearchBasedLogViewer));
			Test(LoggingMethods.ELK, typeof(SearchBasedLogViewer));

			static void Test(string loggingMethod, Type expected)
			{
				// Arrange
				using (SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateRegistryValueForLoggingMethod(loggingMethod)))
				{
					var bindingSource = new ServiceTaskLogViewer();
					using var logViewerControl = new LogViewerControl();
					logViewerControl.SetDataBinding(bindingSource, "");

					// Act
					var viewerControl = logViewerControl.FindSingleOrDefault<ZUserControl>("StrategyLogViewerControl");

					// Assert
					AssertNotNull("viewerControl", viewerControl);
					AssertEquals(expected, viewerControl.DataSourceType);
				}
			}

			static SystemDefinableCodeDescriptionBoolWithExtraBoolCollection CreateRegistryValueForLoggingMethod(string loggingMethod)
			{
				var value = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
				{
					new SystemDefinableCodeDescriptionBoolWithExtraBool
					{
						Code = loggingMethod, Bool = true, Bool2 = true, SystemDefined = true,
					},
				};
				value.SetDefaultCode(loggingMethod, true);

				return value;
			}
		}
	}
}
