using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class KafkaSecurity : AutoKafkaSecurity
	{
		public KafkaSecurity() { }

		public KafkaSecurity(BusinessObjectFactory factory) : base(factory)
		{ }

		[List("Lookups.SecurityProtocols")]
		public override ZString SecurityProtocol
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return base.SecurityProtocol;
			}
			set
			{
				base.SecurityProtocol = value;
				RefreshConfig();
				if (!IsValidationSuspended)
				{
					ValidateSettings();
				}
			}
		}

		void RefreshConfig()
		{
			SslCaLocationInfo.RefreshBinding();
			SaslUsernameInfo.RefreshBinding();
			SaslPasswordInfo.RefreshBinding();
		}

		void ValidateSettings()
		{
			ValidateSaslUsername();
			ValidateSaslPassword();
		}

		public override void ValidateSecurityProtocol()
		{
			base.ValidateSecurityProtocol();
			MandatoryValidation.CheckEntered(SecurityProtocolInfo);
			ListValidation.ErrorIfInvalidCode(SecurityProtocolInfo, Lookups.SecurityProtocols);
		}

		public override ZString SslCaLocation
		{
			get
			{
				if (SslCaLocation_ReadOnly)
				{
					return string.Empty;
				}
				return base.SslCaLocation;
			}
			set
			{
				base.SslCaLocation = value;
			}
		}

		protected override bool SslCaLocation_ReadOnly
		{
			get
			{
				return SecurityProtocol == KafkaSecurityProtocolOptions.PLAINTEXT
					|| SecurityProtocol == KafkaSecurityProtocolOptions.SASL_PLAINTEXT;
			}
		}

		public override ZString SaslUsername
		{
			get
			{
				if (SaslUsername_ReadOnly)
				{
					return string.Empty;
				}
				return base.SaslUsername;
			}
			set
			{
				base.SaslUsername = value;
			}
		}

		public override ZString SaslPassword
		{
			get
			{
				if (SaslPassword_ReadOnly)
				{
					return string.Empty;
				}
				return base.SaslPassword;
			}
			set
			{
				base.SaslPassword = value;
			}
		}

		public override void ValidateSaslUsername()
		{
			base.ValidateSaslUsername();
			if (!SaslUsername_ReadOnly && string.IsNullOrEmpty(SaslUsername))
			{
				SaslUsernameInfo.AddError(Res.GetString("C9594BA7-030A-470F-996A-34F9B59B3310", "SASL Username can not be null."));
			}
		}

		public override void ValidateSaslPassword()
		{
			base.ValidateSaslPassword();
			if (!SaslPassword_ReadOnly && string.IsNullOrEmpty(SaslPassword))
			{
				SaslPasswordInfo.AddError(Res.GetString("CF9012AD-ED7C-49DE-A358-A7ED8DA814A9", "SASL Password can not be null."));
			}
		}

		protected override bool SaslPassword_ReadOnly
		{
			get
			{
				return SecurityProtocol == KafkaSecurityProtocolOptions.PLAINTEXT
					|| SecurityProtocol == KafkaSecurityProtocolOptions.SSL;
			}
		}

		protected override bool SaslUsername_ReadOnly
		{
			get
			{
				return SecurityProtocol == KafkaSecurityProtocolOptions.PLAINTEXT
					|| SecurityProtocol == KafkaSecurityProtocolOptions.SSL;
			}
		}

		public KafkaSecurityLookups Lookups
		{
			get { return lookups ?? (lookups = new KafkaSecurityLookups(this)); }
		}
		KafkaSecurityLookups lookups;

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			SecurityProtocol = KafkaSecurityProtocolOptions.PLAINTEXT;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new KafkaSecurity(factory);
		}
	}
}
