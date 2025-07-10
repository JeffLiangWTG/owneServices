using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(ServiceTaskLogViewer))]
	public class ServiceTaskLogViewerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsUsingSearchBasedLogViewerControl()
		{
			Test(LoggingMethods.FSL, false);
			Test(LoggingMethods.KAF, true);
			Test(LoggingMethods.ELK, true);

			static void Test(string loggingMethod, bool expected)
			{
				// Arrange
				using (SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateRegistryValueForLoggingMethod(loggingMethod)))
				{
					// Act
					var logViewer = new ServiceTaskLogViewer();

					// Assert
					AssertEquals(expected, logViewer.IsUsingSearchBasedLogViewerControl);
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
