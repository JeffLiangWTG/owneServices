using System.Globalization;
using System.Text;
using System.Xml;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class LicenceUsageRequest : NonPersistentBusinessObject, IObsoleteValidation
	{
		public LicenceUsageRequest(LicenceDatabase licenceDatabase)
			: base(licenceDatabase != null ? licenceDatabase.Factory : null)
		{
			this.licenceDatabase = licenceDatabase;
			ZDateTime lastMonth = ZDateTime.Now.AddMonths(-1);
			dateFrom = new ZDateTime(lastMonth.Year, lastMonth.Month, 1);
			dateTo = dateFrom.AddMonths(1).AddDays(-1);
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
				else if (DateFrom > ZDateTime.Now.Date)
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
				else if (DateTo > ZDateTime.Now.Date)
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
				if (licenceDatabase.VersionCanReceiveAllSystemMessages == Customs.Business.TriState.True)
				{
					CreateAndSaveSystemMessage();
				}
				else
				{
					CreateAndSaveLegacyEmail();
				}
			}
		}

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
			var email = new EmailDef();
			email.AddRecipientForUserCommunication(licenceDatabase.LD_PublicEmailAddressForUpdate);
			email.Subject = LegacyLicenceUsageRequestSubject
				+ " " + dateFrom.ToString(DateFormat, CultureInfo.InvariantCulture)
				+ " " + dateTo.ToString(DateFormat, CultureInfo.InvariantCulture)
				+ " " + Env.CurrentUser.Initials.Trim();
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
				writer.WriteStartElement(SystemMessageList.Descriptions.LicenceUsageRequest);

				writer.WriteElementString(DateFromName, DateFrom.ToString(DateFormat, CultureInfo.InvariantCulture));
				writer.WriteElementString(DateToName, DateTo.ToString(DateFormat, CultureInfo.InvariantCulture));
				writer.WriteElementString(RequestedByName, Env.CurrentUser.Initials.Trim());

				writer.WriteEndElement();
			}
			return stringBuilder.ToString();
		}

		public const string DateFormat = "yyyy/MM/dd";
		public const string LegacyLicenceUsageRequestSubject = "Request Enterprise Report";

		public const string DateFromName = "DateFrom";
		public const string DateToName = "DateTo";
		public const string RequestedByName = "RequestedBy";

		// 2010 Mar 21
		public static readonly VersionNumber FirstVersion = new VersionNumber(1, 4, 3732, 0);

		public static bool IsSupportingVersion(LicenceDatabase licenceDatabase, NotificationCollection validation)
		{
			if (licenceDatabase.CurrentVersion == null)
			{
				validation.AddError(InvalidVersionValidationMessage);
				return false;
			}

			if (licenceDatabase.VersionCanReceiveAllSystemMessages != Customs.Business.TriState.True)
			{
				if (!licenceDatabase.PublicEmailIsDeployable)
				{
					validation.AddError(EmailValidationMessage);
					return false;
				}
				if (licenceDatabase.CurrentVersion.VersionNumber < FirstVersion)
				{
					validation.AddError(OutdatedVersionValidationMessage);
					return false;
				}
			}
			return true;
		}

		protected override void RunPreSaveValidationCore()
		{
			ValidateDateFrom();
			ValidateDateTo();
			base.RunPreSaveValidationCore();
		}
	}
}
