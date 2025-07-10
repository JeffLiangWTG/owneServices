using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Telematics.Business.Registry;
using Enterprise.Telematics.ServiceTasks;
using Enterprise.ZArchitecture.Schema;
using WTG.Foundation.Http;
using WTG.Telematics.Common;
using WTG.Telematics.Data.CargoWiseOne;
using IHttpClientFactory = WTG.Foundation.Http.IHttpClientFactory;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.Telematics.Tca
{
	class RimEnrolmentRequestProcessor : IRimEnrolmentRequestProcessor
	{
		public RimEnrolmentRequestProcessor(IHttpClientFactory httpClientFactory, IEHubMessageSender eHubMessageSender, TimeSpan timeout)
		{
			this.eHubMessageSender = eHubMessageSender ?? throw new ArgumentNullException(nameof(eHubMessageSender));
			this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			this.timeout = timeout;
		}

		public bool TryProcessEnrolmentRequest(
			ClientDeviceHeader clientDeviceHeader,
			string schemeCode,
			string vin,
			string vehicleRegistration,
			string vehicleRegistrationState,
			string deviceLocation,
			DateTimeOffset installationDateTimeOffset,
			DateTimeOffset commencementDateTimeOffset,
			DateTimeOffset approvalDateTimeOffset,
			out string message)
		{
			var result = ProcessEnrollmentRequest(new BusinessObjectFactory(), clientDeviceHeader, schemeCode, vin, vehicleRegistration, vehicleRegistrationState, deviceLocation, installationDateTimeOffset, commencementDateTimeOffset, approvalDateTimeOffset);
			message = result.message;
			return result.status;
		}

		public bool TryProcessEnrolmentCancellation(IEnumerable<ClientTelRimRegistration> selectedRegistrations, DateTimeOffset cancellationTime, out string message)
		{
			var registrations = selectedRegistrations.ToList();
			if (registrations.Count != 1)
			{
				message = Res.GetString("367e5d07-7ef1-499d-b779-7a01a0e16cb5", "Registrations are processed one at a time. Please select a single registration to cancel");
				return false;
			}

			var result = ProcessEnrollmentCancellation(new BusinessObjectFactory(), registrations.Single(), cancellationTime);
			message = result.message;
			return result.status;
		}

		internal (bool status, string message) ProcessEnrollmentRequest(
			BusinessObjectFactory factory,
			ClientDeviceHeader clientDeviceHeader,
			string schemeCode,
			string vin,
			string vehicleRegistration,
			string vehicleRegistrationState,
			string deviceLocation,
			DateTimeOffset installationDateTimeOffset,
			DateTimeOffset commencementDateTimeOffset,
			DateTimeOffset approvalDateTimeOffset)
		{
			using (var transaction = ((IDbConnected)factory).Connection.BeginTransactionWithManager())
			{
				var clientCompany = factory.LoadTop1<ClientCompany>(new ZQuery(ClientCompanySchema.LCC_LD, clientDeviceHeader.Licence.PK));
				var orgCusCode = GetOrgCusCode(factory, clientCompany);

				var missingFields = CheckForMissingFields(clientCompany, orgCusCode);
				if (missingFields.status)
				{
					return (false, missingFields.message);
				}

				var enrolmentForm = GenerateEnrolmentForm(factory, clientDeviceHeader, clientCompany, orgCusCode, schemeCode, vin, vehicleRegistration, vehicleRegistrationState, EnrolmentFormType.EnrolmentStatusEnum.APPROVED, commencementDateTimeOffset, approvalDateTimeOffset, installationDateTimeOffset, deviceLocation);
				var result = ProcessEnrolmentRequestWithTca(factory, enrolmentForm, enrolmentForm.identifier, clientDeviceHeader, schemeCode);
				if (result.status)
				{
					CreateClientTelRimRegistration(factory, enrolmentForm, clientDeviceHeader, clientCompany, orgCusCode, schemeCode, vin, vehicleRegistration, vehicleRegistrationState, commencementDateTimeOffset, installationDateTimeOffset);
					NotifyClientSystemOfEnrollment(factory, clientDeviceHeader, commencementDateTimeOffset);
					factory.Save();
				}
				transaction.CommitTransaction();
				return result;
			}
		}

		internal (bool status, string message) ProcessEnrollmentCancellation(BusinessObjectFactory factory, ClientTelRimRegistration registration, DateTimeOffset cancellationTime)
		{
			using (var transaction = ((IDbConnected)factory).Connection.BeginTransactionWithManager())
			{
				if (!registration.TRR_EndTime.IsEmpty)
				{
					return (false, Res.GetString("480072AB-1E78-4CCC-B0C1-B5E8E1AB4F96", $"Registration {registration.TRR_EnrolmentId} has already been closed"));
				}

				var enrolmentForm = GenerateCancellationForm(
					factory,
					registration.ClientDevice,
					registration.ClientCompany,
					registration.OrganisationCustomCodes,
					registration.TRR_EnrolmentScheme,
					registration.TRR_VehicleIdentificationNumber,
					registration.TRR_VehicleRegistration,
					registration.TRR_VehicleRegistrationState,
					EnrolmentFormType.EnrolmentStatusEnum.CANCELLED,
					registration.TRR_StartTime,
					cancellationTime,
					registration.TRR_InstallationDateTimeOffset,
					cancellationTime);

				var result = SendEnrolmentRequest(enrolmentForm, enrolmentForm.identifier);
				if (result.status)
				{
					registration.TRR_EndTime = cancellationTime;
					NotifyClientSystemOfCancellation(factory, registration.ClientDevice, cancellationTime);
				}
				transaction.CommitTransaction();
				return result;
			}
		}

		(bool status, string message) ProcessEnrolmentRequestWithTca(BusinessObjectFactory factory, EnrolmentFormType message, string enrolmentId, ClientDeviceHeader clientDevice, string schemeCode)
		{
			if (HasExistingRegistration(factory, clientDevice, schemeCode))
			{
				return (false, Res.GetString("4e2e6110-2d2d-4cac-87f8-e8d39a9668e9", "Existing Registration for {0}", clientDevice.CDH_Identifier));
			}

			return SendEnrolmentRequest(message, enrolmentId);
		}

		bool HasExistingRegistration(BusinessObjectFactory factory, ClientDeviceHeader clientDevice, string scheme)
		{
			var query = new ZQuery(ClientTelRimRegistrationSchema.TRR_CDH_ClientDeviceHeader, clientDevice.PK);
			query.AddToFilter(ClientTelRimRegistrationSchema.TRR_EnrolmentScheme, scheme);
			query.AddToFilter(ClientTelRimRegistrationSchema.TRR_EndTime, ZDateTimeOffset.Empty);
			return factory.Exists(typeof(ClientTelRimRegistration), query);
		}

		(bool status, string message) SendEnrolmentRequest(EnrolmentFormType message, string enrolmentId)
		{
			var request = TcaXmlSerializer.SerializeToTelematicsRimData(message, "http://www.tca.gov.au/schemas/tde/core/enrolment/2018-07");
			var content = new StringContent(request, Encoding.UTF8, "application/xml");
			var response = HttpClient.PutAsync($"enrolment/{enrolmentId}", content).Result;
			if (!response.IsSuccessStatusCode)
			{
				return (false, Res.GetString("aaf69df7-f334-4c18-bb27-d9f36e96621c", "TCA registration failure, status code: {0} response: {1}", response.StatusCode, response.Content.ReadAsStringAsync().Result));
			}
			return (true, string.Empty);
		}

		static void CreateClientTelRimRegistration(BusinessObjectFactory factory, EnrolmentFormType message, ClientDeviceHeader clientDevice, ClientCompany clientCompany, OrgCusCode orgCusCode, string scheme, string vin, string vehicleRegistration, string vehicleRegistrationState, DateTimeOffset registrationTime, DateTimeOffset installationDateTimeOffset)
		{
			var enrolment = factory.New<ClientTelRimRegistration>();
			enrolment.TRR_EnrolmentScheme = scheme;
			enrolment.TRR_EnrolmentId = message.identifier;
			enrolment.TRR_StartTime = registrationTime;
			enrolment.TRR_InstallationDateTimeOffset = installationDateTimeOffset;
			enrolment.TRR_VehicleIdentificationNumber = vin;
			enrolment.TRR_VehicleRegistration = vehicleRegistration;
			enrolment.TRR_VehicleRegistrationState = vehicleRegistrationState;
			enrolment.TRR_CDH_ClientDeviceHeader = clientDevice.PK;
			enrolment.TRR_LCC_ClientCompany = clientCompany.PK;
			enrolment.TRR_OK_OrgCusCode = orgCusCode.PK;
		}

		void NotifyClientSystemOfEnrollment(BusinessObjectFactory factory, ClientDeviceHeader clientDevice, DateTimeOffset registrationTime)
		{
			var cw1RimRegistrationMessage = new RimRegistrationMessage
			{
				DeviceId = clientDevice.CDH_Identifier,
				DeviceAssignmentTime = registrationTime,
			};
			eHubMessageSender.Send(factory, new[] { XmlDataSerializer.Serialize(cw1RimRegistrationMessage) }, new[] { clientDevice.Licence.LicenceCodeForSystemMessage });
		}

		void NotifyClientSystemOfCancellation(BusinessObjectFactory factory, ClientDeviceHeader clientDevice, DateTimeOffset revokeTime)
		{
			var cw1RimCancellationMessage = new RevokeRimRegistrationMessage
			{
				DeviceId = clientDevice.CDH_Identifier,
				DeviceRevokeTime = revokeTime,
			};
			eHubMessageSender.Send(factory, new[] { XmlDataSerializer.Serialize(cw1RimCancellationMessage) }, new[] { clientDevice.Licence.LicenceCodeForSystemMessage });
		}

		HttpClient HttpClient
		{
			get
			{
				var httpClient = httpClientFactory
					.CreateNew(
						new HttpClientHandlerWithDiagnostics(new CookieContainer())
						{
							Credentials = new NetworkCredential(TelematicsConfigurationRegistry.Instance.TcaRimUsername.Value, TelematicsConfigurationRegistry.Instance.TcaRimPassword.Value),
						},
						timeout);
				httpClient.BaseAddress = new Uri(TelematicsConfigurationRegistry.Instance.TcaRimUrl.Value);
				return httpClient;
			}
		}

		static (bool status, string message) CheckForMissingFields(ClientCompany clientCompany, OrgCusCode orgCusCode)
		{
			if (clientCompany.LCC_State.IsEmpty ||
				clientCompany.LCC_PostCode.IsEmpty ||
				clientCompany.LCC_City.IsEmpty ||
				clientCompany.LCC_Phone.IsEmpty ||
				clientCompany.LCC_Address1.IsEmpty ||
				clientCompany.LCC_Name.IsEmpty)
			{
				return (true, Res.GetString("76c52d2b-e42c-40ed-8763-9d8273106f44", "Client Company must be registered with all required fields: Name, Address, Phone, City, Postcode and State"));
			}

			if (orgCusCode == null ||
				orgCusCode.OK_CodeType != OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber ||
				orgCusCode.OK_CustomsRegNo.IsEmpty)
			{
				return (true, Res.GetString("5d89a0be-997e-4d4f-98ba-e154d97b4d0a", "Related Organization must be registered with an Australian Business Number"));
			}

			return (false, string.Empty);
		}

		static OrgCusCode GetOrgCusCode(BusinessObjectFactory factory, ClientCompany clientCompany)
		{
			return factory.LoadTop1<OrgCusCode>(
				new ZQuery(
					new ZQuery(OrgCusCodeSchema.OK_OH, clientCompany.LCC_OH),
					JoinCondition.And,
					new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber)));
		}

		static EnrolmentFormType GenerateEnrolmentForm(
			BusinessObjectFactory factory,
			ClientDeviceHeader clientDeviceHeader,
			ClientCompany clientCompany,
			OrgCusCode orgCusCode,
			string scheme,
			string vehicleVin,
			string vehicleRegistration,
			string vehicleRegistrationState,
			EnrolmentFormType.EnrolmentStatusEnum enrolmentStatus,
			ZDateTimeOffset commencementDateTimeOffset,
			ZDateTimeOffset approvalDateTimeOffset,
			ZDateTimeOffset installationDateTimeOffset,
			string deviceLocation)
		{
			var enrolmentForm = GenerateBaseEnrolmentForm(factory, clientDeviceHeader, clientCompany, orgCusCode, scheme, vehicleVin, vehicleRegistration, vehicleRegistrationState, enrolmentStatus, commencementDateTimeOffset, approvalDateTimeOffset);
			enrolmentForm.serviceProviderSection.primaryUnitInstallation.installedDevice.Single().installationDateTime = installationDateTimeOffset.ToDateTime();
			enrolmentForm.serviceProviderSection.primaryUnitInstallation.installedDevice.Single().deviceLocation = deviceLocation;
			return enrolmentForm;
		}

		static EnrolmentFormType GenerateCancellationForm(
			BusinessObjectFactory factory,
			ClientDeviceHeader clientDeviceHeader,
			ClientCompany clientCompany,
			OrgCusCode orgCusCode,
			string scheme,
			string vehicleVin,
			string vehicleRegistration,
			string vehicleRegistrationState,
			EnrolmentFormType.EnrolmentStatusEnum enrolmentStatus,
			ZDateTimeOffset commencementDateTimeOffset,
			ZDateTimeOffset approvalDateTimeOffset,
			ZDateTimeOffset installationDateTimeOffset,
			ZDateTimeOffset cessationDateTimeOffset)
		{
			var enrolmentForm = GenerateBaseEnrolmentForm(factory, clientDeviceHeader, clientCompany, orgCusCode, scheme, vehicleVin, vehicleRegistration, vehicleRegistrationState, enrolmentStatus, commencementDateTimeOffset, approvalDateTimeOffset);
			enrolmentForm.serviceProviderSection.primaryUnitInstallation.installedDevice.Single().installationDateTime = installationDateTimeOffset.ToDateTime();
			enrolmentForm.cessationDateTime = cessationDateTimeOffset.ToDateTime();
			enrolmentForm.cessationDateTimeSpecified = true;
			return enrolmentForm;
		}

		static EnrolmentFormType GenerateBaseEnrolmentForm(
			BusinessObjectFactory factory,
			ClientDeviceHeader clientDeviceHeader,
			ClientCompany clientCompany,
			OrgCusCode orgCusCode,
			string scheme,
			string vehicleVin,
			string vehicleRegistration,
			string vehicleRegistrationState,
			EnrolmentFormType.EnrolmentStatusEnum enrolmentStatus,
			ZDateTimeOffset commencementDateTimeOffset,
			ZDateTimeOffset approvalDateTimeOffset)
		{
			var enrolmentForm = new EnrolmentFormType
			{
				application = new TcaCommonXml.ApplicationReferenceType
				{
					name = "RIM",
					version = "1.02",
				},
				identifier = TelematicsRimRegistrationNumberStrategy.GetRegistrationNumber(factory),
				enrolmentProcess = EnrolmentFormType.EnrolmentProcessEnum.ASP,
				authoritySection = new EnrolmentFormType.AuthoritySectionType
				{
					authority = new TcaCommonXml.AuthorityInformationType
					{
						authorityCode = "NSW",
					},
					scheme = scheme,
				},
				statusCode = enrolmentStatus,
				commencementDateTime = commencementDateTimeOffset.ToDateTime(),
				operatorSection = new EnrolmentFormType.OperatorSectionType
				{
					@operator = new EnrolmentFormType.OperatorInformationType
					{
						identity = new TcaCommonXml.OperatorIdentificationType
						{
							abn = orgCusCode.OK_CustomsRegNo,
							name = clientCompany.LCC_Name,
							companyName = clientCompany.LCC_Name,
						},
						postalAddress = new TcaCommonXml.AddressType
						{
							lineOne = clientCompany.LCC_Address1,
							lineTwo = clientCompany.LCC_Address2,
							locality = clientCompany.LCC_City,
							postCode = clientCompany.LCC_PostCode,
							stateCode = (TcaCommonXml.StateEnum)Enum.Parse(typeof(TcaCommonXml.StateEnum), clientCompany.LCC_State.ToString(), true),
						},
						businessHoursPhone = clientCompany.LCC_Phone,
					},
					primaryUnitInformation = new TcaCommonXml.PrimaryUnitInformationType
					{
						identity = new TcaCommonXml.VehicleIdentityType
						{
							ItemElementName = TcaCommonXml.ItemChoiceType.vin,
							Item = vehicleVin,
						},
						registration = new TcaCommonXml.VehicleRegistrationType
						{
							number = vehicleRegistration,
							stateCode = (TcaCommonXml.RegistrationStateEnum)Enum.Parse(typeof(TcaCommonXml.RegistrationStateEnum), vehicleRegistrationState, true),
						},
					},
				},
				serviceProviderSection = new TcaCommonXml.ServiceProviderSectionType
				{
					serviceProvider = new TcaCommonXml.ServiceProviderInformationType
					{
						identity = new TcaCommonXml.CompanyIdentificationType
						{
							abn = Env.CurrentCompany.BusinessRegNo1,
							companyName = WiseTechCompanyName,
						},
					},
					primaryUnitInstallation = new TcaCommonXml.PrimaryUnitInstallationType
					{
						installedDevice = new[]
							{
								new TcaCommonXml.DeviceInstallationType
								{
									deviceIdentity = new TcaCommonXml.DeviceIdentityType
									{
										id = clientDeviceHeader.CDH_Identifier,
										type = TcaCommonXml.DeviceTypeType.IVU,
										typeSpecified = true,
									},
								},
							},
						vehicleIdentity = new TcaCommonXml.VehicleIdentityType
						{
							ItemElementName = TcaCommonXml.ItemChoiceType.vin,
							Item = vehicleVin,
						},
					},
					issuedDateTime = approvalDateTimeOffset.ToDateTime()
				},
				approvalSection = new EnrolmentFormType.ApprovalSectionType
				{
					issuedDateTime = approvalDateTimeOffset.ToDateTime(),
					approved = true,
				},
			};

			return enrolmentForm;
		}

		readonly IHttpClientFactory httpClientFactory;
		readonly IEHubMessageSender eHubMessageSender;
		readonly TimeSpan timeout;
		const string WiseTechCompanyName = "WiseTech Global";
	}
}
