using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class BONotificationTest : TestCaseWithFactory
	{
		public void TestAddErrorsFromBusinessObjectValidationIncludingChildren()
		{
			var factory = new BusinessObjectFactory();
			var buffer = new NotificationBuffer(null);
			var bO = factory.New<DummyBusinessObject>();
			bO.AddRowError("MY Pants Are On Fire");
			bO.AddRowWarning("Henry's Pants Are On Fire");
			BONotification.AddErrorsFromBusinessObjectValidationIncludingChildren(buffer, bO);
			ZString result = buffer.AsString;
			ZString expected = @"Error: DummyBizo: MY Pants Are On Fire
";
			AssertEquals(expected, result);
		}

		public void TestAddWarningsFromBusinessObjectValidationIncludingChildren()
		{
			var factory = new BusinessObjectFactory();
			var buffer = new NotificationBuffer(null);
			var bO = factory.New<DummyBusinessObject>();
			bO.AddRowError("MY Pants Are On Fire");
			bO.AddRowWarning("Henry's Pants Are On Fire");
			BONotification.AddWarningsFromBusinessObjectValidationIncludingChildren(buffer, bO);
			ZString result = buffer.AsString;
			ZString expected = @"Warning: DummyBizo: Henry's Pants Are On Fire
";
			AssertEquals(expected, result);
		}
	}
}
