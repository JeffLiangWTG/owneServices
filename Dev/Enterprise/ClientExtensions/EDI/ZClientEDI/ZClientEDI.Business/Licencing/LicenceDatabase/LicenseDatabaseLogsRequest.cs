using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class LicenceDatabaseLogsRequest : NonPersistentBusinessObject, IObsoleteValidation
	{
		public LicenceDatabaseLogsRequest(LicenceDatabase licenceDatabase)
			: base(licenceDatabase != null ? licenceDatabase.Factory : new BusinessObjectFactory())
		{
			this.licenceDatabase = licenceDatabase;
			dateTo = ZDateTime.Now;
			dateFrom = ZDateTime.Now.AddDays(-7);
		}

		readonly LicenceDatabase licenceDatabase;

		internal const string EmailValidationMessage =
			"Destination version not supported - email address for updates is not valid or missing.";

		internal const string InvalidVersionValidationMessage =
			"Destination version not supported - current version GUID is invalid/empty/missing.";

		internal const string OutdatedVersionValidationMessage =
			"Destination version not supported - current version is outdated.";

		internal const string NullLicenceDatabaseValidationMessage =
			"Destination version not supported - licence database is null/invalid.";

		#region Incident

		[List("IncidentList")]
		public ZString IncidentNumber
		{
			get { return incidentNumber; }
			set
			{
				SetNonPersistentPropertyValue(IncidentNumberInfo, ref incidentNumber, value);

				if (!IsValidationSuspended)
				{
					ValidateIncident();
				}
			}
		}
		public ZPropertyInfo IncidentNumberInfo { get { return GetZPropertyInfo(nameof(IncidentNumber)); } }

		ZString incidentNumber;

		public SupportIncidentCollection IncidentList
		{
			get
			{
				if (incidentList == null)
				{
					incidentList = new SupportIncidentCollection(Factory);
				}
				return incidentList;
			}
		}
		SupportIncidentCollection incidentList;

		public void ValidateIncident()
		{
			IncidentNumberInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(IncidentNumberInfo);
			if (!IncidentNumberInfo.HasErrors())
			{
				if (Incident == null)
				{
					IncidentNumberInfo.AddError("Enter a valid Incident.");
				}
			}
		}

		public SupportIncident Incident
		{
			get
			{
				return Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_IncidentNumber, incidentNumber));
			}
		}

		#endregion

		#region ServiceTaskCode

		public ZString ServiceTaskCode
		{
			get { return serviceTaskCode; }
			set
			{
				value = value.Replace(" ", "");

				SetNonPersistentPropertyValue(ServiceTaskCodeInfo, ref serviceTaskCode, value);

				if (!IsValidationSuspended)
				{
					ValidateServiceTaskCode();
				}
			}
		}
		public ZPropertyInfo ServiceTaskCodeInfo { get { return GetZPropertyInfo(nameof(ServiceTaskCode)); } }
		ZString serviceTaskCode;

		public void ValidateServiceTaskCode()
		{
			ServiceTaskCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ServiceTaskCodeInfo);
			Regex serviceTaskCodeRegex = new Regex("^((([a-zA-Z0-9]{3})|(HOST)),)*(([a-zA-Z0-9]{3})|(HOST))$");
			if (!ServiceTaskCodeInfo.HasErrors() && !serviceTaskCodeRegex.IsMatch(serviceTaskCode))
			{
				ServiceTaskCodeInfo.AddError("Enter into this field one or more service task codes (3 letters long each, or the HOST code) separated by commas (,)");
			}
		}

		#endregion

		#region DateFrom

		public ZDateTime DateFrom
		{
			get { return dateFrom; }
			set
			{
				SetNonPersistentPropertyValue(DateFromInfo, ref dateFrom, value);

				if (!IsValidationSuspended)
				{
					ValidateDateFrom();
				}
			}
		}
		public ZPropertyInfo DateFromInfo { get { return GetZPropertyInfo(nameof(DateFrom)); } }
		ZDateTime dateFrom;

		public void ValidateDateFrom()
		{
			DateFromInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DateFromInfo);
			if (!DateFromInfo.HasErrors())
			{
				if (DateFrom < new ZDateTime(2000, 1, 1))
				{
					DateFromInfo.AddError("Date From year must be 2000 or more");
				}
				else if (DateFrom.Date > ZDateTime.Now.Date)
				{
					DateFromInfo.AddError("Date From must not be after today");
				}
			}
		}

		#endregion

		#region DateTo

		public ZDateTime DateTo
		{
			get { return dateTo; }
			set
			{
				SetNonPersistentPropertyValue(DateToInfo, ref dateTo, value);

				if (!IsValidationSuspended)
				{
					ValidateDateTo();
				}
			}
		}
		public ZPropertyInfo DateToInfo { get { return GetZPropertyInfo(nameof(DateTo)); } }
		ZDateTime dateTo;

		public void ValidateDateTo()
		{
			DateToInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DateToInfo);
			if (!DateToInfo.HasErrors())
			{
				if (DateTo < DateFrom)
				{
					DateToInfo.AddError("Date To must not be less than Date From");
				}
				else if (DateTo.Date > ZDateTime.Now.Date)
				{
					DateToInfo.AddError("Date To must not be after today");
				}
			}
		}

		#endregion

		public bool VersionSupported(NotificationCollection validation)
		{
			if (licenceDatabase == null)
			{
				validation.AddError(NullLicenceDatabaseValidationMessage);
				return false;
			}
			return IsSupportingVersion(licenceDatabase, validation);
		}

		public void Send()
		{
			if (IsSupportingVersion(licenceDatabase, new NotificationCollection()))
			{
				if (IsSupportingNewVersion(licenceDatabase, new NotificationCollection()) && licenceDatabase.VersionCanReceiveAllSystemMessages == Customs.Business.TriState.True)
				{
					CreateAndSaveSystemMessage();
				}
				else
				{
					CreateAndSaveLegacyEmail();
				}
			}
		}

		//2019 July 18-ish
		public static readonly VersionNumber NewVersion = new VersionNumber(19, 7, 25, 0);

		// 2014 Oct 16
		public static readonly VersionNumber FirstVersion = new VersionNumber(14, 10, 15, 0);
		public static readonly VersionNumber GprVersion = new VersionNumber(1, 4, 5317, 316);
		public static readonly VersionNumber LpbVersion = new VersionNumber(1, 4, 5401, 4);

		void CreateAndSaveSystemMessage()
		{
			var licenceCode = licenceDatabase.LicenceCodeForSystemMessage;
			var xml = BuildXml();
			var messageCreator = ObjectFactory.Get<IOutgoingSystemMessage>();
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			messageCreator.Create(factory, xml, licenceCode);
			factory.Save();
		}

		void CreateAndSaveLegacyEmail()
		{
			EmailDef email = new EmailDef();
			email.AddRecipientForUserCommunication(licenceDatabase.LD_PublicEmailAddressForUpdate);
			email.Subject = this.Subject;
			Env.OutgoingMailManager.CreateAndSave(email);
		}

		string BuildXml()
		{
			var stringBuilder = new StringBuilder(500);

			using (var writer = XmlWriter.Create(stringBuilder,
				new XmlWriterSettings
				{
					OmitXmlDeclaration = true,
					Indent = true
				}))
			{
				writer.WriteStartElement(SystemMessageList.Descriptions.LogsRequest);

				writer.WriteElementString("DateFromUtc", dateFrom.ToString(LogsRequestProcessor.DateFormat, CultureInfo.InvariantCulture));
				writer.WriteElementString("DateToUtc", dateTo.ToString(LogsRequestProcessor.DateFormat, CultureInfo.InvariantCulture));
				writer.WriteElementString("ServiceTaskCode", ServiceTaskCode);
				writer.WriteElementString("IncidentNumber", Incident.IM_IncidentNumber);
				writer.WriteElementString("HostServerName", licenceDatabase.LD_HostServerName);
				writer.WriteElementString("HostDBName", licenceDatabase.LD_HostDBName);
				writer.WriteElementString("HostConnectionServerName", licenceDatabase.LD_HostConnectionServerName);
				writer.WriteElementString("MaxZipSize", EDIDataRegistry.Instance.LogsRequestMaximumZipSize.Value.ToString(CultureInfo.InvariantCulture));

				writer.WriteEndElement();
			}
			return stringBuilder.ToString();
		}

		public string Subject
		{
			get
			{
				return LogsRequestProcessor.LogsRequestSubject
					+ " " + dateFrom.ToString(LogsRequestProcessor.DateFormat, CultureInfo.InvariantCulture)
					+ " " + dateTo.ToString(LogsRequestProcessor.DateFormat, CultureInfo.InvariantCulture)
					+ " " + ServiceTaskCode
					+ " " + Incident.IM_IncidentNumber
					+ " " + licenceDatabase.LD_HostServerName
					+ " " + licenceDatabase.LD_HostDBName
					+ " " + licenceDatabase.LD_HostConnectionServerName;
			}
		}

		public static bool IsSupportingNewVersion(LicenceDatabase licenceDatabase, NotificationCollection validation)
		{
			if (!licenceDatabase.PublicEmailIsDeployable)
			{
				validation.AddError(EmailValidationMessage);
				return false;
			}

			if (licenceDatabase.CurrentVersion == null)
			{
				validation.AddError(InvalidVersionValidationMessage);
				return false;
			}

			if (licenceDatabase.CurrentVersion.VersionNumber < NewVersion)
			{
				validation.AddError(OutdatedVersionValidationMessage);
				return false;
			}

			return true;
		}

		public static bool IsSupportingVersion(LicenceDatabase licenceDatabase, NotificationCollection validation)
		{
			if (!licenceDatabase.PublicEmailIsDeployable)
			{
				validation.AddError(EmailValidationMessage);
				return false;
			}

			if (licenceDatabase.CurrentVersion == null)
			{
				validation.AddError(InvalidVersionValidationMessage);
				return false;
			}

			if (!CurrentVersionNumberIsValid(licenceDatabase))
			{
				validation.AddError(OutdatedVersionValidationMessage);
				return false;
			}

			return true;
		}

		static bool CurrentVersionNumberIsValid(LicenceDatabase licenceDatabase)
		{
			return (licenceDatabase.CurrentVersion.VersionNumber >= FirstVersion ||
					(licenceDatabase.LD_ReleaseRing == ReleaseRings.Codes.LPB &&
					licenceDatabase.CurrentVersion.VersionNumber >= LpbVersion) ||
					(licenceDatabase.LD_ReleaseRing == ReleaseRings.Codes.GPR &&
					licenceDatabase.CurrentVersion.VersionNumber >= GprVersion));
		}

		protected override void RunPreSaveValidationCore()
		{
			ValidateDateFrom();
			ValidateDateTo();
			ValidateIncident();
			ValidateServiceTaskCode();
			base.RunPreSaveValidationCore();
		}
	}
}
