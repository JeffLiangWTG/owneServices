using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
#if NETCOREAPP
using TimeProvider = Enterprise.ZArchitecture.Core.TimeProvider;
#endif

namespace Enterprise.BufferManagement.Business
{
	public static class BMSAdminEmailNotifier
	{
		struct EmailDTO
		{
			public EmailDTO(string subject, string body, GuidRegistryItem notificationGroup)
			{
				NotificationGroup = notificationGroup;
				Subject = subject;
				Body = body;
			}

			public GuidRegistryItem NotificationGroup { get; private set; }
			public string Subject { get; private set; }
			public string Body { get; private set; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Immutable type")]
		static readonly Lazy<DigestReporter<string, EmailDTO>> lazy = new Lazy<DigestReporter<string, EmailDTO>>(() => CreateReporter(new TimeProvider()));

		static DigestReporter<string, EmailDTO> Reporter
		{
			get
			{
				var reporter = lazy.Value;
#if DEBUG
				reporter = reporterForTest ?? reporter;
#endif
				return reporter;
			}
		}

		public static void Notify(string key, string subject, string body, GuidRegistryItem notificationGroup)
		{
			Reporter.Report(key, new EmailDTO(subject, body, notificationGroup));
		}

		public static void ClearReports()
		{
			Reporter.ClearReports();
		}

		#region Testing

#if DEBUG

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "There is nothing bad in having two reporters at the same time temporarily as all reports are guaranteed to be delivered even in this case.")]
		static DigestReporter<string, EmailDTO> reporterForTest;

		public static void MockTimeProviderForTesting(ITimeProvider timeProvider = null)
		{
			if (reporterForTest != null)
			{
				reporterForTest.Dispose();
			}

			reporterForTest = timeProvider != null ? CreateReporter(timeProvider) : null;
		}
#endif

		#endregion

		#region Implementation

		static DigestReporter<string, EmailDTO> CreateReporter(ITimeProvider timeProvider)
		{
			return DigestReporterProvider.GetDigestReporter<string, EmailDTO>(
				processDigestHandler: Report,
				accumulateTimeGetter: GetAccumulateTime,
				timeProvider: timeProvider,
				releaseTimeGetter: GetReleaseTime,
				reportFirst: () => false,
				threshold: GetThreashold);
		}

		static void Report(string key, EmailDTO email, IEnumerable<DateTime> occurrences)
		{
			var bodyBuilder = new ZStringBuilder();
			bodyBuilder.AppendLine(email.Body);
			bodyBuilder.AppendLine();
			bodyBuilder.AppendLine(Res.GetString("353CE029-102B-48FD-A66E-6ADCA088F47E", "Registered at the following UTC time:"));
			foreach (var occurrence in occurrences)
			{
				bodyBuilder.AppendLine(occurrence.ToString(CultureInfo.InvariantCulture));
			}

			var emailDef = new BMSEmailDef(email.Subject, bodyBuilder.ToString(), email.NotificationGroup);
			emailDef.Send();
		}

		static TimeSpan GetAccumulateTime() => TimeSpan.FromMinutes(BMSRegistry.Instance.NotificationPeriod.Value);

		static int GetThreashold() => BMSRegistry.Instance.NotificationThreshold.Value;

		static TimeSpan GetReleaseTime() => TimeSpan.FromMinutes(GetAccumulateTime().TotalMinutes);

		#endregion
	}
}
