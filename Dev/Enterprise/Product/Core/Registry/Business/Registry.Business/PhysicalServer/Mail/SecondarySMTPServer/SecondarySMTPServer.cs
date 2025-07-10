using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class SecondarySMTPServer : RegistryBusinessObjectTemplate
	{
		public SecondarySMTPServer()
			: base()
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string SMTPServer = "SMTPServer";
			public const string SMTPPort = "SMTPPort";
			public const string SMTPSecureConnection = "SMTPSecureConnection";
			public const string SMTPUsername = "SMTPUsername";
			public const string SMTPPassword = "SMTPPassword";
			public const string AllowEmailsToBeSentFromUsersAddress = "AllowEmailsToBeSentFromUsersAddress";
			public const string SMTPSenderAddress = "SMTPSenderAddress";
			public const string SupportedDomains = "SupportedDomains";
		}

		#endregion

		#region Properties

		#region SMTPServer

		ZString smtpServer;
		[MaxLength(250)]
		public ZString SMTPServer
		{
			get { return smtpServer; }
			set
			{
				if (smtpServer != value)
				{
					SetNonPersistentPropertyValue(SMTPServerInfo, ref smtpServer, value);
				}
				ValidateSMTPServer();
			}
		}

		public ZPropertyInfo SMTPServerInfo
		{
			get { return GetZPropertyInfo(Schema.SMTPServer); }
		}

		#endregion

		#region SMTPPort

		ZInt smtpPort = 25;
		public ZInt SMTPPort
		{
			get { return smtpPort; }
			set
			{
				if (smtpPort != value)
				{
					SetNonPersistentPropertyValue(SMTPPortInfo, ref smtpPort, value);
				}
			}
		}

		public ZPropertyInfo SMTPPortInfo
		{
			get { return GetZPropertyInfo(Schema.SMTPPort); }
		}

		#endregion

		#region SMTPSecureConnection

		ZString smtpSecureConnection = SecureConnectionTypes.None;
		[List(nameof(SecureConnectionTypeList))]
		public ZString SMTPSecureConnection
		{
			get { return smtpSecureConnection; }
			set
			{
				if (smtpSecureConnection != value)
				{
					SetNonPersistentPropertyValue(SMTPSecureConnectionInfo, ref smtpSecureConnection, value);
				}
				ValidateSMTPSecureConnection();
			}
		}

		public ZPropertyInfo SMTPSecureConnectionInfo
		{
			get { return GetZPropertyInfo(Schema.SMTPSecureConnection); }
		}

		SecureConnectionTypes secureConnectionTypeList;
		public SecureConnectionTypes SecureConnectionTypeList
		{
			get
			{
				if (secureConnectionTypeList == null)
				{
					secureConnectionTypeList = new SecureConnectionTypes();
				}
				return secureConnectionTypeList;
			}
		}

		#endregion

		#region SMTPUsername

		ZString smtpUsername;
		public ZString SMTPUsername
		{
			get { return smtpUsername; }
			set
			{
				if (smtpUsername != value)
				{
					SetNonPersistentPropertyValue(SMTPUsernameInfo, ref smtpUsername, value);
				}
				ValidateSMTPUsername();
			}
		}

		public ZPropertyInfo SMTPUsernameInfo
		{
			get { return GetZPropertyInfo(Schema.SMTPUsername); }
		}

		#endregion

		#region SMTPPassword

		ZString smtpPassword;
		[Password]
		public ZString SMTPPassword
		{
			get { return smtpPassword; }
			set
			{
				if (smtpPassword != value)
				{
					SetNonPersistentPropertyValue(SMTPPasswordInfo, ref smtpPassword, value);
				}
			}
		}

		public ZPropertyInfo SMTPPasswordInfo
		{
			get { return GetZPropertyInfo(Schema.SMTPPassword); }
		}

		#endregion

		#region AllowEmailsToBeSentFromUsersAddress

		ZBool allowEmailsToBeSentFromUsersAddress = true;
		public ZBool AllowEmailsToBeSentFromUsersAddress
		{
			get { return allowEmailsToBeSentFromUsersAddress; }
			set
			{
				if (allowEmailsToBeSentFromUsersAddress != value)
				{
					SetNonPersistentPropertyValue(AllowEmailsToBeSentFromUsersAddressInfo, ref allowEmailsToBeSentFromUsersAddress, value);
				}
			}
		}

		public ZPropertyInfo AllowEmailsToBeSentFromUsersAddressInfo
		{
			get { return GetZPropertyInfo(Schema.AllowEmailsToBeSentFromUsersAddress); }
		}

		#endregion

		#region SMTPSenderAddress

		ZString smtpSenderAddress;
		public ZString SMTPSenderAddress
		{
			get { return smtpSenderAddress; }
			set
			{
				if (smtpSenderAddress != value)
				{
					SetNonPersistentPropertyValue(SMTPSenderAddressInfo, ref smtpSenderAddress, value);
				}
				ValidateSMTPSenderAddress();
			}
		}

		public ZPropertyInfo SMTPSenderAddressInfo
		{
			get { return GetZPropertyInfo(Schema.SMTPSenderAddress); }
		}

		#endregion

		#region SupportedDomains

		ZString supportedDomains;
		public ZString SupportedDomains
		{
			get { return supportedDomains; }
			set
			{
				var formattedValue = value;
				if (!value.IsEmpty)
				{
					var supportedDomainsArray = value.ToString().Split(new string[] { ",", System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
					formattedValue = string.Join(", ", supportedDomainsArray);
				}

				if (supportedDomains != formattedValue)
				{
					SetNonPersistentPropertyValue(SupportedDomainsInfo, ref supportedDomains, formattedValue);
				}

				ValidateSupportedDomains();
			}
		}

		public ZPropertyInfo SupportedDomainsInfo
		{
			get { return GetZPropertyInfo(Schema.SupportedDomains); }
		}

		#endregion

		#endregion

		#region Validation

		void ValidateAll()
		{
			ValidateSMTPServer();
			ValidateSMTPUsername();
			ValidateSMTPSenderAddress();
			ValidateSMTPSecureConnection();
			ValidateSupportedDomains();
		}

		void ValidateSMTPServer()
		{
			if (!IsValidationSuspended)
			{
				SMTPServerInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(SMTPServerInfo);
				if (!SMTPServerInfo.HasErrors())
				{
					CheckSMTPServerSettingIsUniqueInCollection();
				}
			}
		}

		void ValidateSMTPUsername()
		{
			if (!IsValidationSuspended)
			{
				CheckSMTPServerSettingIsUniqueInCollection();
			}
		}

		void CheckSMTPServerSettingIsUniqueInCollection()
		{
			var errorMessage = Res.GetString("36edc83c-4a85-4808-abdb-6acd011e062c", "SMTP Server + SMTP User Name has already been set up. SMTP Server settings must be unique.");

			RemoveRowError(errorMessage);
			if (ParentCollections.Count > 0)
			{
				var parentCollection = GetParentCollection(this, typeof(SecondarySMTPServerCollection));
				if (parentCollection != null)
				{
					var hasDuplicateSetting = parentCollection.OfType<SecondarySMTPServer>()
														.Any(o => o != this &&
																o.SMTPServer == SMTPServer &&
																o.SMTPUsername == SMTPUsername);
					if (hasDuplicateSetting)
					{
						AddRowError(errorMessage);
					}
				}
			}
		}

		void ValidateSMTPSenderAddress()
		{
			if (!IsValidationSuspended)
			{
				SMTPSenderAddressInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(SMTPSenderAddressInfo);
				EmailAddressValidation.ValidateEmailAddress(SMTPSenderAddressInfo);
			}
		}

		void ValidateSMTPSecureConnection()
		{
			if (!IsValidationSuspended)
			{
				SMTPSecureConnectionInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(SMTPSecureConnectionInfo);
				ListValidation.ErrorIfInvalidCode(SMTPSecureConnectionInfo, SecureConnectionTypeList);
			}
		}

		void ValidateSupportedDomains()
		{
			if (!IsValidationSuspended)
			{
				SupportedDomainsInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(SupportedDomainsInfo);
				EnglishCharactersValidation.ErrorIfNotWesternEuropean(SupportedDomainsInfo);

				if (!SupportedDomainsInfo.HasErrors())
				{
					var supportedDomainCollection = SupportedDomains.Split(',').Select(o => o.Trim()).ToList();
					if (supportedDomainCollection.Count > 0)
					{
						var invalidDomains = supportedDomainCollection.Distinct().Where(d => !IsValidDomainWithOutScheme(d)).ToList();
						if (invalidDomains.Count > 0)
						{
							SupportedDomainsInfo.AddError(Res.GetString("3bc24eaf-08f8-47d7-bc77-f7b1e3d56f8a", "Invalid Domains: {0}.", string.Join(", ", invalidDomains)));
						}

						var duplicatedDomains = supportedDomainCollection.GroupBy(o => o).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
						if (duplicatedDomains.Count == 0)
						{
							var parentCollection = GetParentCollection(this, typeof(SecondarySMTPServerCollection));
							if (parentCollection != null)
							{
								var otherSupportedDomains = parentCollection.OfType<SecondarySMTPServer>().Where(o => o != this).SelectMany(o => o.SupportedDomains.Split(',')).ToList();
								if (otherSupportedDomains != null && otherSupportedDomains.Count > 0)
								{
									duplicatedDomains = supportedDomainCollection.Intersect(otherSupportedDomains).ToList();
								}
							}
						}

						if (duplicatedDomains.Count > 0)
						{
							SupportedDomainsInfo.AddError(Res.GetString("b748489d-adf6-4b8b-8e5e-9e26cf1c75fd", "Duplicated Domains: {0}. Supported Domain must be unique.", string.Join(", ", duplicatedDomains)));
						}
					}
				}
			}
		}

		bool IsValidDomainWithOutScheme(string domain)
		{
			var pattern = (NoResString)"^((?!-)[A-Za-z0-9-]{1,63}(?<!-)\\.)+[A-Za-z]{2,6}$";

			Regex regex = new Regex(pattern);
			return !string.IsNullOrEmpty(domain) && regex.IsMatch(domain);
		}

		#endregion

		#region Override

		protected override void RunPreSaveValidationCore()
		{
			ValidateAll();
			base.RunPreSaveValidationCore();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new SecondarySMTPServer();
			using (clone.GetValidationSuspender())
			{
				clone.SMTPServer = SMTPServer;
				clone.SMTPPort = SMTPPort;
				clone.SMTPSecureConnection = SMTPSecureConnection;
				clone.SMTPUsername = SMTPUsername;
				clone.SMTPPassword = SMTPPassword;
				clone.AllowEmailsToBeSentFromUsersAddress = AllowEmailsToBeSentFromUsersAddress;
				clone.SMTPSenderAddress = SMTPSenderAddress;
				clone.SupportedDomains = supportedDomains;
			}
			return clone;
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			using (GetValidationSuspender())
			{
				SMTPServer = reader.ReadElementString(Schema.SMTPServer);
				SMTPPort = reader.ReadElementStringAsZInt(Schema.SMTPPort);
				SMTPSecureConnection = reader.ReadElementString(Schema.SMTPSecureConnection);
				SMTPUsername = reader.ReadElementString(Schema.SMTPUsername);
				SMTPPassword = reader.ReadElementString(Schema.SMTPPassword);
				AllowEmailsToBeSentFromUsersAddress = reader.ReadElementStringAsZBool(Schema.AllowEmailsToBeSentFromUsersAddress);
				SMTPSenderAddress = reader.ReadElementString(Schema.SMTPSenderAddress);
				SupportedDomains = reader.ReadElementString(Schema.SupportedDomains);
			}
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.SMTPServer, SMTPServer);
			writer.WriteElementString(Schema.SMTPPort, SMTPPort.ToString());
			writer.WriteElementString(Schema.SMTPSecureConnection, SMTPSecureConnection);
			writer.WriteElementString(Schema.SMTPUsername, SMTPUsername);
			writer.WriteElementString(Schema.SMTPPassword, SMTPPassword);
			writer.WriteElementString(Schema.AllowEmailsToBeSentFromUsersAddress, AllowEmailsToBeSentFromUsersAddress.ToString());
			writer.WriteElementString(Schema.SMTPSenderAddress, SMTPSenderAddress);
			writer.WriteElementString(Schema.SupportedDomains, SupportedDomains);
		}

		#endregion
	}
}
