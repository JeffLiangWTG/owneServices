using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.MX.Manifest.Business.XmlSerializers")]
	public class MXWsVucem : RegistryBusinessObjectTemplate
	{
		#region Constructors and Schema

		public MXWsVucem() : base()
		{
		}

		public MXWsVucem(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		protected abstract class Schema
		{
			public const string AirModeWSResponse = "AirModeWSResponse";
			public const string AirModeWSUsername = "AirModeWSUsername";
			public const string AirModeWSPassword = "AirModeWSPassword";
			public const string SeaModeWSResponse = "SeaModeWSResponse";
			public const string SeaModeWSUsername = "SeaModeWSUsername";
			public const string SeaModeWSPassword = "SeaModeWSPassword";
		}

		#endregion

		#region Properties

		ZString pAirModeWSResponse;
		[ResourceStringData("Enterprise.Customs.MX.Manifest.Business.MXWsVucem|AirModeWSResponse", Caption = "URL for the Air Mode")]
		public ZString AirModeWSResponse
		{
			get => pAirModeWSResponse;
			set
			{
				if (AirModeWSResponse != value)
				{
					SetNonPersistentPropertyValue(AirModeWSResponseInfo, ref pAirModeWSResponse, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateURLWSAirMode();
					}
					AirModeWSResponseInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AirModeWSResponseInfo => GetZPropertyInfo(Schema.AirModeWSResponse);

		ZString pAirModeWSUsername;
		[ResourceStringData("Enterprise.Customs.MX.Manifest.Business.MXWsVucem|AirModeWSUsername", Caption = "Username for the Air Mode")]
		public ZString AirModeWSUsername
		{
			get => pAirModeWSUsername;
			set
			{
				if (AirModeWSUsername != value)
				{
					SetNonPersistentPropertyValue(AirModeWSUsernameInfo, ref pAirModeWSUsername, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateUsernameWSAirMode();
					}
					AirModeWSUsernameInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AirModeWSUsernameInfo => GetZPropertyInfo(Schema.AirModeWSUsername);

		ZString pAirModeWSPassword;
		[ResourceStringData("Enterprise.Customs.MX.Manifest.Business.MXWsVucem|AirModeWSPassword", Caption = "Password for the Air Mode")]
		public ZString AirModeWSPassword
		{
			get => pAirModeWSPassword;
			set
			{
				if (AirModeWSPassword != value)
				{
					SetNonPersistentPropertyValue(AirModeWSPasswordInfo, ref pAirModeWSPassword, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidatePasswordWSAirMode();
					}
					AirModeWSPasswordInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AirModeWSPasswordInfo => GetZPropertyInfo(Schema.AirModeWSPassword);

		ZString pSeaModeWSResponse;
		[ResourceStringData("Enterprise.Customs.MX.Manifest.Business.MXWsVucem|SeaModeWSResponse", Caption = "URL for the Sea Mode")]
		public ZString SeaModeWSResponse
		{
			get => pSeaModeWSResponse;
			set
			{
				if (SeaModeWSResponse != value)
				{
					SetNonPersistentPropertyValue(SeaModeWSResponseInfo, ref pSeaModeWSResponse, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateURLWSSeaMode();
					}
					SeaModeWSResponseInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo SeaModeWSResponseInfo => GetZPropertyInfo(Schema.SeaModeWSResponse);

		ZString pSeaModeWSUsername;
		[ResourceStringData("Enterprise.Customs.MX.Manifest.Business.MXWsVucem|SeaModeWSUsername", Caption = "Username for the Sea Mode")]
		public ZString SeaModeWSUsername
		{
			get => pSeaModeWSUsername;
			set
			{
				if (SeaModeWSUsername != value)
				{
					SetNonPersistentPropertyValue(SeaModeWSUsernameInfo, ref pSeaModeWSUsername, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateUsernameWSSeaMode();
					}
					SeaModeWSUsernameInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo SeaModeWSUsernameInfo => GetZPropertyInfo(Schema.SeaModeWSUsername);

		ZString pSeaModeWSPassword;
		[ResourceStringData("Enterprise.Customs.MX.Manifest.Business.MXWsVucem|SeaModeWSPassword", Caption = "Password for the Sea Mode")]
		public ZString SeaModeWSPassword
		{
			get => pSeaModeWSPassword;
			set
			{
				if (SeaModeWSPassword != value)
				{
					SetNonPersistentPropertyValue(SeaModeWSPasswordInfo, ref pSeaModeWSPassword, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidatePasswordWSSeaMode();
					}
					SeaModeWSPasswordInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo SeaModeWSPasswordInfo => GetZPropertyInfo(Schema.SeaModeWSPassword);

		#endregion

		#region Validations

		MXWsVucemValidation fValidation;
		public MXWsVucemValidation Validation => fValidation ?? (fValidation = new MXWsVucemValidation(this));

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		#region Overrides

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			AirModeWSResponse = reader.ReadElementString(Schema.AirModeWSResponse);
			AirModeWSUsername = reader.ReadElementString(Schema.AirModeWSUsername);
			AirModeWSPassword = reader.ReadElementString(Schema.AirModeWSPassword);
			SeaModeWSResponse = reader.ReadElementString(Schema.SeaModeWSResponse);
			SeaModeWSUsername = reader.ReadElementString(Schema.SeaModeWSUsername);
			SeaModeWSPassword = reader.ReadElementString(Schema.SeaModeWSPassword);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.AirModeWSResponse, AirModeWSResponse);
			writer.WriteElementString(Schema.AirModeWSUsername, AirModeWSUsername);
			writer.WriteElementString(Schema.AirModeWSPassword, AirModeWSPassword);
			writer.WriteElementString(Schema.SeaModeWSResponse, SeaModeWSResponse);
			writer.WriteElementString(Schema.SeaModeWSUsername, SeaModeWSUsername);
			writer.WriteElementString(Schema.SeaModeWSPassword, SeaModeWSPassword);
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new MXWsVucem(fallbackLevel, factory);
		}

		#endregion
	}
}
