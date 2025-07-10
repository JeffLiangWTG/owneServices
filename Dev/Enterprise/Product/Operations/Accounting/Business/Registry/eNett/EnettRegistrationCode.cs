using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class EnettRegistrationCode : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string RegistrationCode = "RegistrationCode";
			public const int RegistrationCode_MaxLength = 100;
			public const string AuthenticationCode = "AuthenticationCode";
			public const int AuthenticationCode_MaxLength = 100;
			public const string OrganisationPK = "OrganisationPK";
			public const string CustomHouseUsername = "CustomHouseUsername";
			public const int CustomHouseUsername_MaxLength = 100;
			public const string CustomHousePassword = "CustomHousePassword";
			public const int CustomHousePassword_MaxLength = 100;
		}

		#endregion

		public EnettRegistrationCode(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public EnettRegistrationCode()
		{
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateOrganisation();
		}

		void ValidateOrganisation()
		{
			if ((RegistrationCode != ZString.Empty || AuthenticationCode != ZString.Empty) && (OrganisationPK == ZGuid.Empty))
			{
				if (!OrganisationPKInfo.HasError(Res.GetString("c2fc1ef5-da2d-46a4-9194-11f3411dae5a", "ComPay Organization must be entered")))
				{
					OrganisationPKInfo.AddError(Res.GetString("c2fc1ef5-da2d-46a4-9194-11f3411dae5a", "ComPay Organization must be entered"));
				}
			}
			else
			{
				OrganisationPKInfo.ClearAllNotifications();
			}
		}

		#region Bound Properties

		ZString registrationCode = "";
		[MaxLength(Schema.RegistrationCode_MaxLength)]
		public ZString RegistrationCode
		{
			get
			{
				return registrationCode;
			}
			set
			{
				if (registrationCode != value)
				{
					SetNonPersistentPropertyValue(RegistrationCodeInfo, ref registrationCode, value);
				}
			}
		}

		public ZPropertyInfo RegistrationCodeInfo
		{
			get { return GetZPropertyInfo(Schema.RegistrationCode); }
		}

		ZString fCustomHouseUsername = "";
		[MaxLength(Schema.CustomHouseUsername_MaxLength)]
		public ZString CustomHouseUsername
		{
			get
			{
				return fCustomHouseUsername;
			}
			set
			{
				if (fCustomHouseUsername != value)
				{
					SetNonPersistentPropertyValue(CustomHouseUsernameInfo, ref fCustomHouseUsername, value);
				}
			}
		}

		public ZPropertyInfo CustomHouseUsernameInfo
		{
			get { return GetZPropertyInfo(Schema.CustomHouseUsername); }
		}

		ZString fCustomHousePassword = "";
		[MaxLength(Schema.CustomHousePassword_MaxLength)]
		public ZString CustomHousePassword
		{
			get
			{
				return fCustomHousePassword;
			}
			set
			{
				if (fCustomHousePassword != value)
				{
					SetNonPersistentPropertyValue(CustomHousePasswordInfo, ref fCustomHousePassword, value);
				}
			}
		}

		public ZPropertyInfo CustomHousePasswordInfo
		{
			get { return GetZPropertyInfo(Schema.CustomHousePassword); }
		}

		ZString authenticationCode = "";
		[MaxLength(Schema.AuthenticationCode_MaxLength)]
		public ZString AuthenticationCode
		{
			get
			{
				return authenticationCode;
			}
			set
			{
				if (authenticationCode != value)
				{
					SetNonPersistentPropertyValue(AuthenticationCodeInfo, ref authenticationCode, value);
				}
			}
		}

		public ZPropertyInfo AuthenticationCodeInfo
		{
			get { return GetZPropertyInfo(Schema.AuthenticationCode); }
		}

		[List("OrganisationsList")]
		public ZGuid OrganisationPK
		{
			get
			{
				return organisationPK;
			}
			set
			{
				if (organisationPK != value)
				{
					SetNonPersistentPropertyValue(OrganisationPKInfo, ref organisationPK, value);
				}
			}
		}
		ZGuid organisationPK;

		public ZPropertyInfo OrganisationPKInfo
		{
			get { return GetZPropertyInfo(Schema.OrganisationPK); }
		}

		#endregion

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.RegistrationCode, RegistrationCode);
			writer.WriteElementString(Schema.AuthenticationCode, AuthenticationCode);
			writer.WriteElementString(Schema.OrganisationPK, OrganisationPK.ToString());
			writer.WriteElementString(Schema.CustomHouseUsername, CustomHouseUsername);
			writer.WriteElementString(Schema.CustomHousePassword, CustomHousePassword);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			RegistrationCode = new ZString(reader.ReadElementString(Schema.RegistrationCode));
			AuthenticationCode = new ZString(reader.ReadElementString(Schema.AuthenticationCode));
			OrganisationPK = new ZGuid(reader.ReadElementString(Schema.OrganisationPK));
			CustomHouseUsername = new ZString(reader.ReadElementString(Schema.CustomHouseUsername));
			CustomHousePassword = new ZString(reader.ReadElementString(Schema.CustomHousePassword));
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EnettRegistrationCode(fallbackLevel, factory);
		}

		#region Organisation List

		BusinessObjectCollection fOrganisationsList;
		public BusinessObjectCollection OrganisationsList
		{
			get { return fOrganisationsList ?? (fOrganisationsList = new OrgHeaderCollection(CurrentFactory)); }
		}

		#endregion
	}
}