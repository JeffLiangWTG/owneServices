using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XtCredential;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Common
{
	[XmlSerializerAssembly("Enterprise.Customs.JP.Common.XmlSerializers")]
	public class MailboxAndRemoteWebPrintClientCredentials : RegistryBusinessObjectTemplate, IxTCredentialProvider
	{
		public MailboxAndRemoteWebPrintClientCredentials() : base()
		{
		}

		public MailboxAndRemoteWebPrintClientCredentials(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
			SendCredentialFactory = factory;
		}

		#region Schema

		public static class Schema
		{
			public const string LocalComputerAlias = "LocalComputerAlias";
			public const string DomainName = "DomainName";
			public const string ReceivingInterval = "ReceivingInterval";
			public const string SendingInterval = "SendingInterval";
			public const string Status = "Status";
			public const string Verbose = "Verbose";
			public const string FailureNotificationGroup = "FailureNotificationGroup";
			public const string DownTimeStart = "DownTimeStart";
			public const string DownTimeEnd = "DownTimeEnd";
			public const int StatusMaxLength = 3;
		}

		#endregion

		#region Local Computer Alias (Machine ID)

		[ResourceStringData("Enterprise.Customs.JP.Business.MailboxAndRemoteWebPrintClientCredentials|LocalComputerAlias", Caption = "Local Computer Alias (Machine ID)")]
		[MaxLength(50)]
		public ZString LocalComputerAlias
		{
			get => localComputerAlias;
			set
			{
				if (LocalComputerAlias != value)
				{
					SetNonPersistentPropertyValue(LocalComputerAliasInfo, ref localComputerAlias, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateLocalComputerAlias();
					}
					LocalComputerAliasInfo.RefreshBinding();
				}
			}
		}

		ZString localComputerAlias;

		public ZPropertyInfo LocalComputerAliasInfo => GetZPropertyInfo(Schema.LocalComputerAlias);

		#endregion

		#region Domain Name (SMTP Client Host)

		[ResourceStringData("Enterprise.Customs.JP.Business.MailboxAndRemoteWebPrintClientCredentials|DomainName", Caption = "Domain Name (SMTP Client Host)")]
		public ZString DomainName
		{
			get => domainName;
			set
			{
				if (domainName != value)
				{
					SetNonPersistentPropertyValue(DomainNameInfo, ref domainName, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateDomainName();
					}
					DomainNameInfo.RefreshBinding();
				}
			}
		}

		ZString domainName;

		public ZPropertyInfo DomainNameInfo => GetZPropertyInfo(Schema.DomainName);

		#endregion

		#region Receiving Interval (Minutes) 

		[ResourceStringData("Enterprise.Customs.JP.Business.MailboxAndRemoteWebPrintClientCredentials|ReceivingInterval", Caption = "Receiving Interval (Minutes)")]
		public ZInt ReceivingInterval
		{
			get => receivingInterval;
			set
			{
				if (receivingInterval != value)
				{
					SetNonPersistentPropertyValue(ReceivingIntervalInfo, ref receivingInterval, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateReceivingInterval();
					}
					ReceivingIntervalInfo.RefreshBinding();
				}
			}
		}

		ZInt receivingInterval;

		public ZPropertyInfo ReceivingIntervalInfo => GetZPropertyInfo(Schema.ReceivingInterval);

		#endregion

		#region Sending Interval (Seconds) 

		[ResourceStringData("Enterprise.Customs.JP.Business.MailboxAndRemoteWebPrintClientCredentials|SendingInterval", Caption = "Sending Interval (Seconds)")]
		public ZInt SendingInterval
		{
			get => sendingInterval;
			set
			{
				if (sendingInterval != value)
				{
					SetNonPersistentPropertyValue(SendingIntervalInfo, ref sendingInterval, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateSendingInterval();
					}
					SendingIntervalInfo.RefreshBinding();
				}
			}
		}

		ZInt sendingInterval;

		public ZPropertyInfo SendingIntervalInfo => GetZPropertyInfo(Schema.SendingInterval);

		#endregion

		#region Status

		[ResourceStringData("Enterprise.Customs.JP.Business.MailboxAndRemoteWebPrintClientCredentials|Status", Caption = "Status")]
		[List(nameof(Lookups) + "." + nameof(MailboxAndRemoteWebPrintClientCredentialsLookups.StatusList))]
		[MaxLength(Schema.StatusMaxLength)]
		[ReadOnly(true)]
		public ZString Status
		{
			get => status;
			set
			{
				if (status != value)
				{
					SetNonPersistentPropertyValue(StatusInfo, ref status, value);
					StatusInfo.RefreshBinding();
				}
			}
		}

		ZString status;

		public ZPropertyInfo StatusInfo => GetZPropertyInfo(Schema.Status);

		#endregion

		#region Verbose

		[ResourceStringData("Enterprise.Customs.JP.Business.MailboxAndRemoteWebPrintClientCredentials|Verbose", Caption = "Verbose Logging")]
		public ZBool Verbose
		{
			get => verbose;
			set
			{
				if (verbose != value)
				{
					SetNonPersistentPropertyValue(VerboseInfo, ref verbose, value);
					VerboseInfo.RefreshBinding();
				}
			}
		}

		ZBool verbose;

		public ZPropertyInfo VerboseInfo => GetZPropertyInfo(Schema.Verbose);

		#endregion

		#region Failure Notification Group

		[RelatedBusinessObject(nameof(NotificationGroup))]
		[MaxLength(GlbGroup.Schema.GG_CodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(MailboxAndRemoteWebPrintClientCredentialsLookups.FailureNotificationGroupList))]
		[ResourceStringData("Enterprise.Customs.JP.Business.MailboxAndRemoteWebPrintClientCredentials|FailureNotificationGroup", Caption = "Failure Notification Group")]
		public ZString FailureNotificationGroup
		{
			get => failureNotificationGroup;
			set
			{
				if (failureNotificationGroup != value)
				{
					CheckMaximumLength(FailureNotificationGroupInfo, value);
					SetNonPersistentPropertyValue(FailureNotificationGroupInfo, ref failureNotificationGroup, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateFailureNotificationGroup();
					}
					FailureNotificationGroupInfo.RefreshBinding();
				}
			}
		}

		ZString failureNotificationGroup;

		public ZPropertyInfo FailureNotificationGroupInfo => GetZPropertyInfo(Schema.FailureNotificationGroup);

		public GlbGroup NotificationGroup => Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, FailureNotificationGroup));

		#endregion

		#region Down Time Start

		[ResourceStringData("Enterprise.Customs.JP.Business.MailboxAndRemoteWebPrintClientCredentials|DownTimeStart", Caption = "Start")]
		public ZDateTime DownTimeStart
		{
			get => downTimeStart;
			set
			{
				if (downTimeStart != value)
				{
					SetNonPersistentPropertyValue(DownTimeStartInfo, ref downTimeStart, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateDownTimeStart();
						Validation.ValidateDownTimeEnd();
					}
					DownTimeStartInfo.RefreshBinding();
				}
			}
		}

		ZDateTime downTimeStart;

		public ZPropertyInfo DownTimeStartInfo => GetZPropertyInfo(Schema.DownTimeStart);

		#endregion

		#region Down Time End

		[ResourceStringData("Enterprise.Customs.JP.Business.MailboxAndRemoteWebPrintClientCredentials|DownTimeEnd", Caption = "End")]
		public ZDateTime DownTimeEnd
		{
			get => downTimeEnd;
			set
			{
				if (downTimeEnd != value)
				{
					SetNonPersistentPropertyValue(DownTimeEndInfo, ref downTimeEnd, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateDownTimeEnd();
						Validation.ValidateDownTimeStart();
					}
					DownTimeEndInfo.RefreshBinding();
				}
			}
		}

		ZDateTime downTimeEnd;

		public ZPropertyInfo DownTimeEndInfo => GetZPropertyInfo(Schema.DownTimeEnd);

		#endregion

		#region Validation

		public MailboxAndRemoteWebPrintClientCredentialsValidation Validation => validation ??= new MailboxAndRemoteWebPrintClientCredentialsValidation(this);
		MailboxAndRemoteWebPrintClientCredentialsValidation validation;

		#endregion

		#region Lookup

		public MailboxAndRemoteWebPrintClientCredentialsLookups Lookups => lookups ??= new MailboxAndRemoteWebPrintClientCredentialsLookups(this);
		MailboxAndRemoteWebPrintClientCredentialsLookups lookups;

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new MailboxAndRemoteWebPrintClientCredentials(fallbackLevel, factory);

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			LocalComputerAlias = reader.ReadElementString(Schema.LocalComputerAlias);
			DomainName = reader.ReadElementString(Schema.DomainName);
			ReceivingInterval = ZInt.ParseSafe(reader.ReadElementString(Schema.ReceivingInterval), defaultReceivingInterval);
			SendingInterval = ZInt.ParseSafe(reader.ReadElementString(Schema.SendingInterval), defaultSendingInterval);
			Status = reader.ReadElementString(Schema.Status);
			Verbose = reader.ReadElementStringAsZBool(Schema.Verbose);
			FailureNotificationGroup = reader.ReadElementString(Schema.FailureNotificationGroup);
			DownTimeStart = reader.ReadElementStringAsZDateTime(Schema.DownTimeStart, DateTimeFormat);
			DownTimeEnd = reader.ReadElementStringAsZDateTime(Schema.DownTimeEnd, DateTimeFormat);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.LocalComputerAlias, LocalComputerAlias);
			writer.WriteElementString(Schema.DomainName, DomainName);
			writer.WriteElementString(Schema.ReceivingInterval, ReceivingInterval.ToString());
			writer.WriteElementString(Schema.SendingInterval, SendingInterval.ToString());
			writer.WriteElementString(Schema.Status, Status);
			writer.WriteElementString(Schema.Verbose, Verbose.ToString());
			writer.WriteElementString(Schema.FailureNotificationGroup, FailureNotificationGroup);
			writer.WriteElementString(Schema.DownTimeStart, DownTimeStart.ToString(DateTimeFormat));
			writer.WriteElementString(Schema.DownTimeEnd, DownTimeEnd.ToString(DateTimeFormat));
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			ReceivingInterval = defaultReceivingInterval;
			SendingInterval = defaultSendingInterval;
			Status = XtCredentialStatusList.Codes.Unregistered;
		}

		const int defaultReceivingInterval = 3;
		const int defaultSendingInterval = 15;

		#endregion

		#region Implement

		public ZBool IsEmpty => LocalComputerAlias.IsEmpty || DomainName.IsEmpty;

		public ZGuid LinkUniqueID { get; set; }

		public XtCredentialAction RequiredAction { get; set; }

		public string Password { get; set; }

		public string OldPassword { get; set; }

		public new BusinessObjectFactory Factory => CurrentFactory;

		public BusinessObjectFactory SendCredentialFactory { get; set; }

		#endregion

		#region IxTCredentialProvider

		ZGuid IxTCredentialProvider.LinkUniqueID => LinkUniqueID;

		XtCredentialAction IxTCredentialProvider.RequiredAction => RequiredAction; 

		string IxTCredentialProvider.Password => Password;

		string IxTCredentialProvider.OldPassword => OldPassword;

		string IxTCredentialProvider.LinkTableName => StmDataSchema.Constants.TableName;

		string IxTCredentialProvider.Identifier => XtCredentialConstants.IdentifierList.JPC;

		DateTime IxTCredentialProvider.ChangeDateTime => ZDateTime.Now.ToDateTime();

		string IxTCredentialProvider.EnterpriseCode
		{
			get
			{
				var key = ObjectFactory.Get<IProductRegistration>()?.Key;
				return $"{key?.EnterpriseCode}{key?.ServerCode}_JPC";
			}
		}

		string IxTCredentialProvider.DatabaseCode => ZString.Empty;

		string IxTCredentialProvider.DatabaseNumber => ZString.Empty;

		string DateTimeFormat => "yyyyMMddHHmm";

		BusinessObjectFactory IxTCredentialProvider.Factory => SendCredentialFactory;

		#endregion
	}
}
