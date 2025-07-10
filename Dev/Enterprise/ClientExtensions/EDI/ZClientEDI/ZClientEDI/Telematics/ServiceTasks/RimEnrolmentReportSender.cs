using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using WTG.Foundation.Http;

namespace Enterprise.Client.EDI.Telematics.ServiceTasks
{
	class RimEnrolmentReportSender
	{
		public RimEnrolmentReportSender(ILogger logger)
			: this(
				new BusinessObjectFactory { NameForDebugging = "Telematics RIM Enrollment report sending task" },
				new EnrolmentAccessor(),
				new EnrolmentProcessor(),
				new EnrolmentSender(ObjectFactory.Get<IHttpClientFactory>(), logger,  TimeSpan.FromMilliseconds(30000)))
		{
		}

		internal RimEnrolmentReportSender(
			BusinessObjectFactory factory,
			IEnrolmentAccessor enrolmentAccessor,
			IEnrolmentProcessor enrolmentProcessor,
			IEnrolmentSender enrolmentSender)
		{
			this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
			this.enrolmentAccessor = enrolmentAccessor ?? throw new ArgumentNullException(nameof(enrolmentAccessor));
			this.enrolmentProcessor = enrolmentProcessor ?? throw new ArgumentNullException(nameof(enrolmentProcessor));
			this.enrolmentSender = enrolmentSender ?? throw new ArgumentNullException(nameof(enrolmentSender));
		}

		public void Run(CancellationToken token, DateTimeOffset currentTime)
		{
			using (var transaction = ((IDbConnected)factory).Connection.BeginTransactionWithManager())
			{
				var enrollmentPeriodEnd = new DateTimeOffset(currentTime.Year, currentTime.Month, 1, 0, 0, 0, TimeSpan.Zero);
				var enrollmentPeriodStart = enrollmentPeriodEnd.AddMonths(-1);
				var enrolledDevices = enrolmentAccessor.GetEnrollments(factory, enrollmentPeriodStart, enrollmentPeriodEnd);
				var enrollmentReports = enrolmentProcessor.Process(enrolledDevices, enrollmentPeriodStart, enrollmentPeriodEnd);
				enrolmentSender.Send(factory, enrollmentReports);
				factory.Save();
				transaction.CommitTransaction();
			}
		}

		readonly BusinessObjectFactory factory;
		readonly IEnrolmentAccessor enrolmentAccessor;
		readonly IEnrolmentProcessor enrolmentProcessor;
		readonly IEnrolmentSender enrolmentSender;
	}
}
