using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.Telematics.ServiceTasks;
using Enterprise.Client.EDI.Telematics.Tca;
using Moq;

namespace ZClientEDI.Test.Telematics.ServiceTasks
{
	class RimEnrolmentReportSenderTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			enrolmentAccessor = new Mock<IEnrolmentAccessor>();
			enrolmentProcessor = new Mock<IEnrolmentProcessor>();
			enrolmentSender = new Mock<IEnrolmentSender>();
			rimEnrollmentReportSender = new RimEnrolmentReportSender(Factory, enrolmentAccessor.Object, enrolmentProcessor.Object, enrolmentSender.Object);
		}

		public void TestCallsAccessorWithExpectedParams()
		{
			// Arrange
			var report = new EnrolmentReportType();
			var token = new CancellationToken();
			var time = new DateTimeOffset(2020, 2, 1, 5, 5, 5, TimeSpan.Zero);
			enrolmentAccessor.Setup(accessor => accessor.GetEnrollments(It.IsAny<IFactory>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
				.Returns(new List<ClientTelRimRegistration>());
			enrolmentProcessor.Setup(processor => processor.Process(It.IsAny<IEnumerable<ClientTelRimRegistration>>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
				.Returns(report);
			enrolmentSender.Setup(sender => sender.Send(It.IsAny<BusinessObjectFactory>(), It.IsAny<EnrolmentReportType>()));

			var expectedStart = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
			var expectedEnd = new DateTimeOffset(2020, 2, 1, 0, 0, 0, 0, TimeSpan.Zero);

			// Act
			AssertNoExceptionThrown(() => rimEnrollmentReportSender.Run(token, time));

			// Assert
			enrolmentAccessor.Verify(accessor => accessor.GetEnrollments(
				Factory,
				expectedStart,
				expectedEnd));
			enrolmentAccessor.VerifyNoOtherCalls();
		}

		public void TestCallsProcessorWithExpectedParams()
		{
			// Arrange
			var report = new EnrolmentReportType();
			var token = new CancellationToken();
			var time = new DateTimeOffset(2020, 1, 1, 5, 5, 5, TimeSpan.Zero);
			enrolmentAccessor.Setup(accessor => accessor.GetEnrollments(It.IsAny<IFactory>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
				.Returns(new List<ClientTelRimRegistration>());
			enrolmentProcessor.Setup(processor => processor.Process(It.IsAny<IEnumerable<ClientTelRimRegistration>>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
				.Returns(report);
			enrolmentSender.Setup(sender => sender.Send(It.IsAny<BusinessObjectFactory>(), It.IsAny<EnrolmentReportType>()));

			var expectedStart = new DateTimeOffset(2019, 12, 1, 0, 0, 0, TimeSpan.Zero);
			var expectedEnd = new DateTimeOffset(2020, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

			// Act
			AssertNoExceptionThrown(() => rimEnrollmentReportSender.Run(token, time));

			// Assert
			enrolmentProcessor.Verify(processor => processor.Process(
				It.IsAny<IEnumerable<ClientTelRimRegistration>>(),
				expectedStart,
				expectedEnd));
			enrolmentProcessor.VerifyNoOtherCalls();
		}

		public void TestCallsSenderWithExpectedParams()
		{
			// Arrange
			var report = new EnrolmentReportType();
			var token = new CancellationToken();
			var time = new DateTimeOffset(2020, 1, 1, 5, 5, 5, TimeSpan.Zero);
			enrolmentAccessor.Setup(accessor => accessor.GetEnrollments(It.IsAny<IFactory>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
				.Returns(new List<ClientTelRimRegistration>());
			enrolmentProcessor.Setup(processor => processor.Process(It.IsAny<IEnumerable<ClientTelRimRegistration>>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
				.Returns(report);
			enrolmentSender.Setup(sender => sender.Send(It.IsAny<BusinessObjectFactory>(), It.IsAny<EnrolmentReportType>()));

			// Act
			AssertNoExceptionThrown(() => rimEnrollmentReportSender.Run(token, time));

			// Assert
			enrolmentSender.Verify(sender => sender.Send(Factory, report));
			enrolmentSender.VerifyNoOtherCalls();
		}

		Mock<IEnrolmentAccessor> enrolmentAccessor;
		Mock<IEnrolmentProcessor> enrolmentProcessor;
		Mock<IEnrolmentSender> enrolmentSender;
		RimEnrolmentReportSender rimEnrollmentReportSender;
	}
}
