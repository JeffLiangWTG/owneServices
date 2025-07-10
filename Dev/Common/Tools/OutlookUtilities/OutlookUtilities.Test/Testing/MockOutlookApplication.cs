using System;

namespace Enterprise.Interop.OutlookIntegration.Testing
{
	sealed class MockOutlookApplication : OutlookApplication
	{
		public Exception ExceptionToThrowOnGetComOutlookApplication;

		protected override Outlook._DApplication GetComOutlookApplication()
		{
			if (ExceptionToThrowOnGetComOutlookApplication != null)
			{
				throw ExceptionToThrowOnGetComOutlookApplication;
			}
			return new MockNativeOutlookApplication();
		}
	}
}
