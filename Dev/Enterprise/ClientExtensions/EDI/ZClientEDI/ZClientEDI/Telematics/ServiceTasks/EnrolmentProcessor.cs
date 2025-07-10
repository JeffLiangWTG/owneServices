using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.Telematics.Tca;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.Telematics.ServiceTasks
{
	class EnrolmentProcessor : IEnrolmentProcessor
	{
		public EnrolmentReportType Process(IEnumerable<ClientTelRimRegistration> registrations, DateTimeOffset enrollmentPeriodStart, DateTimeOffset enrollmentPeriodEnd)
		{
			var enrollmentSummary = registrations
				.Select(registration => new EnrolmentReportType.EnrolmentSummaryType
					{
						installedDevice = new TcaCommonXml.DeviceIdentityType
						{
							id = registration.ClientDevice.CDH_Identifier,
						},
						enrolment = new[] {
							new EnrolmentReportType.EnrolmentReferenceType1
							{
								enrolmentIdentifier = registration.TRR_EnrolmentId,
								scheme = registration.TRR_EnrolmentScheme,
								@operator = new TcaCommonXml.OperatorIdentificationType
								{
									companyName = registration.ClientCompany.LCC_Name,
									abn = registration.OrganisationCustomCodes.OK_CustomsRegNo,
								},
							},
						},
						Item = new TcaCommonXml.VehicleInformationType
						{
							identity = new TcaCommonXml.VehicleIdentityType
							{
								ItemElementName = TcaCommonXml.ItemChoiceType.vin,
								Item = registration.TRR_VehicleIdentificationNumber,
							},
							registration = new TcaCommonXml.VehicleRegistrationType
							{
								number = registration.TRR_VehicleRegistration,
								stateCode = (TcaCommonXml.RegistrationStateEnum)Enum.Parse(typeof(TcaCommonXml.RegistrationStateEnum), registration.TRR_VehicleRegistrationState),
							},
						},
						entryDateTime = registration.TRR_StartTime.ToDateTime(),
						exitDateTime = registration.TRR_EndTime.IsEmpty
							? new DateTime()
							: registration.TRR_EndTime.ToDateTime(),
						entryDateTimeSpecified = (
							registration.TRR_StartTime >= enrollmentPeriodStart &&
							registration.TRR_StartTime <= enrollmentPeriodEnd),
						exitDateTimeSpecified = (
							!registration.TRR_EndTime.IsEmpty &&
							registration.TRR_EndTime >= enrollmentPeriodStart &&
							registration.TRR_EndTime < enrollmentPeriodEnd)
					});

			return new EnrolmentReportType
			{
				application = new TcaCommonXml.ApplicationReferenceType
				{
					name = "RIM",
					version = "1.02",
				},
				reportPeriod = new EnrolmentReportType.DateTimePeriodType
				{
					startDateTime = enrollmentPeriodStart.DateTime,
					endDateTime = enrollmentPeriodEnd.DateTime,
				},
				serviceProvider = new TcaCommonXml.ServiceProviderInformationType
				{
					identity = new TcaCommonXml.CompanyIdentificationType
					{
						abn = Env.CurrentCompany.BusinessRegNo1,
						companyName = WiseTechCompanyName,
					},
				},
				enrolmentSummary = enrollmentSummary.ToArray(),
			};
		}

		const string WiseTechCompanyName = "WiseTech Global";
	}
}
