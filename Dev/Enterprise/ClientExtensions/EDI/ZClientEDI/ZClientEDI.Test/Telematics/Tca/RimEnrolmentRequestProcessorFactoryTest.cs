using System;
using Enterprise.Client.EDI.Telematics.Tca;
using NUnit.Framework;

namespace ZClientEDI.Test.Telematics.Tca
{
	public class RimEnrolmentRequestProcessorFactoryTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			rimEnrollmentRequestProcessorFactory = new RimEnrolmentRequestProcessorFactory();
		}

		IRimEnrolmentRequestProcessorFactory rimEnrollmentRequestProcessorFactory;

		public void TestNotNull()
		{
			// Arrange
			// Act
			var result = rimEnrollmentRequestProcessorFactory.GetProcessor(new TimeSpan());

			// Assert
			AssertNotNull(result);
		}
	}
}
