using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class AzureOpenIDConnectConfiguration : RegistryBusinessObjectTemplate
	{
		#region schema

		public static class Schema
		{
			public const string Code = "Code";
			public const string AuthorityUrl = "AuthorityUrl";
			public const string ClientID = "ClientID";
		}

		#endregion

		#region EnvironmentList

		public AzureB2CEnvironmentCodeDescriptionList AzureB2CEnvironmentList => azureB2CEnvironmentList ?? (azureB2CEnvironmentList = new AzureB2CEnvironmentCodeDescriptionList());
		AzureB2CEnvironmentCodeDescriptionList azureB2CEnvironmentList;

		#endregion

		#region Code

		[List("AzureB2CEnvironmentList")]
		public ZString Code
		{
			get => code;
			set
			{
				SetNonPersistentPropertyValue(CodeInfo, ref code, value);
				if (!IsValidationSuspended)
				{
					ValidateCode();
				}
			}
		}

		public ZPropertyInfo CodeInfo => GetZPropertyInfo(Schema.Code);

		ZString code;

		#endregion

		#region AuthorityUrl

		public ZString AuthorityUrl
		{
			get => authorityUrl;
			set
			{
				SetNonPersistentPropertyValue(AuthorityUrlInfo, ref authorityUrl, value);
				if (!IsValidationSuspended)
				{
					ValidateUrl();
				}
			}
		}

		public ZPropertyInfo AuthorityUrlInfo => GetZPropertyInfo(Schema.AuthorityUrl);

		ZString authorityUrl;

		#endregion

		#region ClientID

		public ZString ClientID
		{
			get => clientID;
			set
			{
				SetNonPersistentPropertyValue(ClientIDInfo, ref clientID, value);
				if (!IsValidationSuspended)
				{
					ValidateClientID();
				}
			}
		}

		public ZPropertyInfo ClientIDInfo => GetZPropertyInfo(Schema.ClientID);

		ZString clientID;

		#endregion

		#region clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AzureOpenIDConnectConfiguration()
			{
				Code = Code,
				AuthorityUrl = AuthorityUrl,
				ClientID = ClientID,
			};
		}

		#endregion

		#region Xml Serialisation

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.Code);
			AuthorityUrl = reader.ReadElementString(Schema.AuthorityUrl);
			ClientID = reader.ReadElementString(Schema.ClientID);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.AuthorityUrl, AuthorityUrl);
			writer.WriteElementString(Schema.ClientID, ClientID);
		}

		#endregion

		#region Validation

		public void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CodeInfo);
			ListValidation.ErrorIfInvalidCode(CodeInfo);
			if (!Code.IsEmpty)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(CodeInfo);
			}
		}

		public void ValidateUrl()
		{
			AuthorityUrlInfo.ClearAllNotifications();
			var pattern = @"^(https?|http)://([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?$";
			var regex = new Regex(pattern, RegexOptions.IgnoreCase);
			MandatoryValidation.CheckEntered(AuthorityUrlInfo);

			if (!regex.IsMatch(authorityUrl))
			{
				AuthorityUrlInfo.AddError(Res.GetString("1E95EB2C-481D-4D64-969F-A1190E985464", "Please enter the correct URL."));
			}
		}

		public void ValidateClientID()
		{
			ClientIDInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ClientIDInfo);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCode();
			ValidateUrl();
			ValidateClientID();
		}

		#endregion
	}
}
