using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.CA.Services;
using Enterprise.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class AIRSValidationServiceSettings : IAIRSValidationServiceSettings
	{
		public AIRSValidationServiceSettings(JobDeclaration declaration)
		{
			Argument.NotNull(declaration, nameof(declaration));
			FrenchPreferred = GetFrenchPreferred(declaration);
			Key = GetKey(declaration);
			UserName = GetUserName(declaration);
			Password = GetPassword(declaration);
			SchemaVersion = declaration.IsIID ? AIRSValidationServiceSchemaVersion.IID : AIRSValidationServiceSchemaVersion.OGD;
		}

		bool GetFrenchPreferred(JobDeclaration declaration)
		{
			return CACustomsDataRegistry.Instance.FrenchLanguageIndicator.GetFallBackValueAtAllLevels(declaration.CompanyPK.ToGuid(), declaration.JE_GB.ToGuid(), Guid.Empty);
		}

		string GetKey(JobDeclaration declaration)
		{
			return CACustomsDataRegistry.Instance.AIRSValidationKey.GetFallBackValueAtAllLevels(declaration.CompanyPK.ToGuid(), declaration.JE_GB.ToGuid(), Guid.Empty);
		}

		string GetUserName(JobDeclaration declaration)
		{
			return CACustomsDataRegistry.Instance.AIRSValidationRequestUserName.GetFallBackValueAtAllLevels(declaration.CompanyPK.ToGuid(), declaration.JE_GB.ToGuid(), Guid.Empty);
		}

		string GetPassword(JobDeclaration declaration)
		{
			return CACustomsDataRegistry.Instance.AIRSValidationRequestPassword.GetFallBackValueAtAllLevels(declaration.CompanyPK.ToGuid(), declaration.JE_GB.ToGuid(), Guid.Empty);
		}

		#region IAIRSValidationServiceSettings Members

		public bool FrenchPreferred
		{
			get; private set;
		}

		public string Key
		{
			get; private set;
		}

		public string UserName
		{
			get; private set;
		}

		public string Password
		{
			get; private set;
		}

		public string SchemaVersion
		{
			get;
		}

		public ZString Uri
		{
			get
			{
				var result = CACustomsDataRegistry.Instance.AIRSValidationRequestURL.Value;
				if (string.IsNullOrEmpty(result))
				{
					result = "https://avs-svs.inspection.gc.ca/avs/bvs.svc/secure";
				}
				return result;
			}
		}

		public ZString WebProxyUri => CACustomsDataRegistry.Instance.WebProxyAddress.Value;
		#endregion

		public ZString CheckValidationServiceSetting()
		{
			var sbuilder = new ZStringBuilder();
			if (string.IsNullOrEmpty(Key))
			{
				sbuilder.Append(Res.GetString("A1769F82-F1F7-4DFF-85FA-8BDB13D943BF", "AIRS Validation Key hasn't been setup. The key is allocated by CFIA. Please set it up in Registry -> {0}", ((IRegistryItemInternals)CACustomsDataRegistry.Instance.AIRSValidationKey).Location));
			}
			if (!Uri.IsEmpty && !UrlValidation.IsValidUrl(Uri))
			{
				sbuilder.Append(Res.GetString("C17A2AE7-F4D8-4B58-8453-2F2062903650", "An invalid URL entered in Registry -> {0}", ((IRegistryItemInternals)CACustomsDataRegistry.Instance.AIRSValidationRequestURL).Location));
			}
			if (!WebProxyUri.IsEmpty && !UrlValidation.IsValidUrl(WebProxyUri))
			{
				sbuilder.Append(Res.GetString("14C95CA6-5766-4999-BDE3-4A4ECEB4868E", "An invalid URL entered in Registry -> {0}", ((IRegistryItemInternals)CACustomsDataRegistry.Instance.WebProxyAddress).Location));
			}
			return sbuilder.ToStringWithNewLineBetweenAppends();
		}
	}
}
