using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class AuthorizationModeAndSettings : RegistryBusinessObjectTemplate
	{
		public AuthorizationModeAndSettings()
			: base()
		{
		}

		public AuthorizationModeAndSettings(FallbackLevel fallbackLevel)
			: base(fallbackLevel, null)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AuthorizationModeAndSettings(fallbackLevel);
		}

		protected override void CopyCollectionsToClone(RegistryBusinessObjectTemplate clone, FallbackLevel currentFallbackLevel, BusinessObjectFactory factory)
		{
			base.CopyCollectionsToClone(clone, currentFallbackLevel, factory);
			var cloneAMS = (clone as AuthorizationModeAndSettings);
			cloneAMS.AuthorisationSettings.AddRange(this.AuthorisationSettings.Clone(currentFallbackLevel, factory) as PaymentSixLevelAuthorisationSettingsCollection);
		}

		protected override void SetCustomDefaultValuesCore()
		{
			this.AuthorizationMode = Constants.AuthorizationMode.Codes.Default;
			base.SetCustomDefaultValuesCore();
		}

		#region Properties

		[MaxLength(3)]
		[List(nameof(AuthorizationModesList))]
		public ZString AuthorizationMode
		{
			get { return authorizationMode; }
			set
			{
				SetNonPersistentPropertyValue(AuthorizationModeInfo, ref authorizationMode, value);
				if (!IsValidationSuspended)
				{
					ValidateAuthorizationMode();
				}
			}
		}
		ZString authorizationMode;

		public ZPropertyInfo AuthorizationModeInfo => GetZPropertyInfo(nameof(AuthorizationMode), "Authorization Mode");

		public void ValidateAuthorizationMode()
		{
			AuthorizationModeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AuthorizationModeInfo);
			ListValidation.ErrorIfInvalidCode(AuthorizationModeInfo);
		}

		public CodeDescriptionPairList AuthorizationModesList
		{
			get
			{
				if (authorizationModesList == null)
				{
					authorizationModesList = new CodeDescriptionPairList();
					authorizationModesList.AddPair(Constants.AuthorizationMode.Codes.Default, Constants.AuthorizationMode.Descriptions.Default);
					authorizationModesList.AddPair(Constants.AuthorizationMode.Codes.TwoApprovers, Constants.AuthorizationMode.Descriptions.TwoApprovers);
					authorizationModesList.AddPair(Constants.AuthorizationMode.Codes.SequentialApprovers, Constants.AuthorizationMode.Descriptions.SequentialApprovers);
				}
				return authorizationModesList;
			}
		}
		CodeDescriptionPairList authorizationModesList;

		#endregion

		public PaymentSixLevelAuthorisationSettingsCollection AuthorisationSettings
		{
			get
			{
				if (authorisationSettings == null)
				{
					authorisationSettings = new PaymentSixLevelAuthorisationSettingsCollection();
					RegisterEditableChildObject(authorisationSettings);
				}

				return authorisationSettings;
			}
		}

		PaymentSixLevelAuthorisationSettingsCollection authorisationSettings;

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(nameof(AuthorizationMode), AuthorizationMode);
			SettingsSerialiser.Serialize(writer, AuthorisationSettings);
		}

		ZXmlSerializer SettingsSerialiser => requirementsSerializer ?? (requirementsSerializer = ZXmlSerializer.New(typeof(PaymentSixLevelAuthorisationSettingsCollection)));
		ZXmlSerializer requirementsSerializer;

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			AuthorizationMode = reader.ReadElementString(nameof(AuthorizationMode));
			authorisationSettings = (PaymentSixLevelAuthorisationSettingsCollection)SettingsSerialiser.Deserialize(reader);
			RegisterEditableChildObject(authorisationSettings);
		}

		#endregion
	}
}
