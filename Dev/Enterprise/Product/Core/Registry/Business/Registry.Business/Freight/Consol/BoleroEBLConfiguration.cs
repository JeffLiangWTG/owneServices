using System;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public sealed class BoleroEBLConfiguration : RegistryBusinessObjectTemplate
	{
		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			EnableEBLIntegration = ZBool.False;
			GalileoEndPointUrl = Constants.GalileoEndPointUrl;
			GalileoAudience = Constants.GalileoAudience;
			GalileoTestEndPointUrl = Constants.GalileoTestEndPointUrl;
			GalileoTestAudience = Constants.GalileoTestAudience;
		}

		public ZString GetGalileoEndPointUrl()
		{
			return IsProductionSystem ? GalileoEndPointUrl : GalileoTestEndPointUrl;
		}

		public ZString GetGalileoAudience()
		{
			return IsProductionSystem ? GalileoAudience : GalileoTestAudience;
		}

		bool IsProductionSystem => Env.Instance.IsProductionSystem
									&& !ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem()
									&& !Globals.IsTest;

		#region Constants
		public static class Constants
		{
			public const string GalileoEndPointUrl = "https://galileo.boleroserve.net/galileo-portal/jwt/login";
			public const string GalileoAudience = "ab2e99f4-15c9-41c8-a138-7d873c3c4f9f";
			public const string GalileoTestEndPointUrl = "https://galileo.training.boleroserve.net/galileo-portal/jwt/login";
			public const string GalileoTestAudience = "ef46c619-92d6-4f56-af3d-327cf3646508";
		}
		#endregion

		#region Schema

		public static class Schema
		{
			public const string EnableEBLIntegration = "EnableEBLIntegration";
			public const string GalileoEndPointUrl = "GalileoEndPointUrl";
			public const string GalileoAudience = "GalileoAudience";
			public const string GalileoTestEndPointUrl = "GalileoTestEndPointUrl";
			public const string GalileoTestAudience = "GalileoTestAudience";
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BoleroEBLConfiguration
			{
				EnableEBLIntegration = EnableEBLIntegration,
				GalileoEndPointUrl = GalileoEndPointUrl,
				GalileoAudience = GalileoAudience,
				GalileoTestEndPointUrl = GalileoTestEndPointUrl,
				GalileoTestAudience = GalileoTestAudience
			};
		}

		#endregion

		#region Properties

		#region EnableEBLIntegration

		public ZBool EnableEBLIntegration
		{
			get => enableEBLIntegration;
			set
			{
				if (SetNonPersistentPropertyValue(EnableEBLIntegrationInfo, ref enableEBLIntegration, value))
				{
					ValidateEnableEBLIntegration();
					ValidateUrl();
					ValidateUrl(false);
					ValidateGalileoAudience();
					ValidateGalileoAudience(false);
				}
			}
		}
		public ZPropertyInfo EnableEBLIntegrationInfo => GetZPropertyInfo(Schema.EnableEBLIntegration);
		ZBool enableEBLIntegration;

		#endregion

		#region GalileoEndPointUrl

		[ReadOnlyMember(nameof(IsNonSupportUser))]
		public ZString GalileoEndPointUrl
		{
			get { return galileoEndPointUrl; }
			set
			{
				if (SetNonPersistentPropertyValue(GalileoEndPointUrlInfo, ref galileoEndPointUrl, value))
				{
					ValidateUrl();
				}
			}
		}
		ZString galileoEndPointUrl;

		public ZPropertyInfo GalileoEndPointUrlInfo
		{
			get { return GetZPropertyInfo(Schema.GalileoEndPointUrl); }
		}

		#endregion

		#region GalileoAudience

		[ReadOnlyMember(nameof(IsNonSupportUser))]
		public ZString GalileoAudience
		{
			get { return galileoAudience; }
			set
			{
				if (SetNonPersistentPropertyValue(GalileoAudienceInfo, ref galileoAudience, value))
				{
					ValidateGalileoAudience();
				}
			}
		}
		ZString galileoAudience;

		public ZPropertyInfo GalileoAudienceInfo
		{
			get { return GetZPropertyInfo(Schema.GalileoAudience); }
		}

		#endregion

		#region GalileoTestEndPointUrl

		[ReadOnlyMember(nameof(IsNonSupportUser))]
		public ZString GalileoTestEndPointUrl
		{
			get { return galileoTestEndPointUrl; }
			set
			{
				if (SetNonPersistentPropertyValue(GalileoTestEndPointUrlInfo, ref galileoTestEndPointUrl, value))
				{
					ValidateUrl(false);
				}
			}
		}
		ZString galileoTestEndPointUrl;

		public ZPropertyInfo GalileoTestEndPointUrlInfo
		{
			get { return GetZPropertyInfo(Schema.GalileoTestEndPointUrl); }
		}

		#endregion

		#region GalileoTestAudience

		[ReadOnlyMember(nameof(IsNonSupportUser))]
		public ZString GalileoTestAudience
		{
			get { return galileoTestAudience; }
			set
			{
				if (SetNonPersistentPropertyValue(GalileoTestAudienceInfo, ref galileoTestAudience, value))
				{
					ValidateGalileoAudience(false);
				}
			}
		}
		ZString galileoTestAudience;

		public ZPropertyInfo GalileoTestAudienceInfo
		{
			get { return GetZPropertyInfo(Schema.GalileoTestAudience); }
		}

		#endregion

		public bool IsNonSupportUser
		{
			get
			{
				return !User.SupportUserName.Equals(Env.CurrentUser.LoginName, StringComparison.OrdinalIgnoreCase);
			}
		}

		#endregion

		#region Validation

		public void ValidateEnableEBLIntegration()
		{
			EnableEBLIntegrationInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(EnableEBLIntegrationInfo);
		}

		void ValidateUrl(ZString url, ZPropertyInfo propertyInfo)
		{
			propertyInfo.ClearAllNotifications();

			var urlRegex = new Regex(@"^(https?|http)://([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?$", RegexOptions.IgnoreCase);

			if (!url.IsEmpty && !urlRegex.IsMatch(url))
			{
				propertyInfo.AddError(Res.GetString("454A825E-2956-49A5-863A-F008FF662010", "Please enter the correct URL."));
			}
			else if (EnableEBLIntegration && url.IsEmpty)
			{
				MandatoryValidation.CheckEntered(propertyInfo);
			}
		}

		public void ValidateUrl(bool isProductionValidation = true)
		{
			if (isProductionValidation)
			{
				ValidateUrl(GalileoEndPointUrl, GalileoEndPointUrlInfo);
			}
			else
			{
				ValidateUrl(GalileoTestEndPointUrl, GalileoTestEndPointUrlInfo);
			}
		}

		public void ValidateGalileoAudience(bool isProductionValidation = true)
		{
			if (isProductionValidation)
			{
				ValidateAudience(GalileoAudience, GalileoAudienceInfo);
			}
			else
			{
				ValidateAudience(GalileoTestAudience, GalileoTestAudienceInfo);
			}
		}

		void ValidateAudience(ZString audience, ZPropertyInfo propertyInfo)
		{
			propertyInfo.ClearAllNotifications();

			if (EnableEBLIntegration)
			{
				MandatoryValidation.CheckEntered(propertyInfo);
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateEnableEBLIntegration();
			ValidateUrl();
			ValidateUrl(false);
			ValidateGalileoAudience();
			ValidateGalileoAudience(false);
		}

		#endregion

		#region Xml Serialization

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.EnableEBLIntegration, EnableEBLIntegration.ToString());
			writer.WriteElementString(Schema.GalileoEndPointUrl, GalileoEndPointUrl.ToString());
			writer.WriteElementString(Schema.GalileoAudience, GalileoAudience.ToString());
			writer.WriteElementString(Schema.GalileoTestEndPointUrl, GalileoTestEndPointUrl.ToString());
			writer.WriteElementString(Schema.GalileoTestAudience, GalileoTestAudience.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			EnableEBLIntegration = new ZBool(reader.ReadElementString(Schema.EnableEBLIntegration));
			GalileoEndPointUrl = new ZString(reader.ReadElementString(Schema.GalileoEndPointUrl));
			GalileoAudience = new ZString(reader.ReadElementString(Schema.GalileoAudience));
			GalileoTestEndPointUrl = new ZString(reader.ReadElementString(Schema.GalileoTestEndPointUrl));
			GalileoTestAudience = new ZString(reader.ReadElementString(Schema.GalileoTestAudience));
		}

		#endregion
	}
}
