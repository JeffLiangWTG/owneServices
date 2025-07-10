using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using WTG.RtfConverter;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.Licensing
{
	public class LicenseAgreement : AutoLicenseAgreement
	{
		public LicenseAgreement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("LicenseAgreement.VersionNumber", Caption = "Version", FullDescription = "The version number of the License Agreement")]
		public ZString VersionNumber => FormattableString.Invariant($"{LAG_MajorVersion}.{LAG_MinorVersion}");

		public ZPropertyInfo VersionNumberInfo => GetZPropertyInfo(nameof(VersionNumber));

		[ResourceStringData("LicenseAgreement|LAG_ContentBlob", Caption = "License Agreement Content")]
		public ZBlob LAG_ContentBlob
		{
			get
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				return ZBlob.FromUTF8(htmlToRtfConverter.Convert(LAG_Content.Replace(System.Environment.NewLine, string.Empty)));
			}
		}

		public ZBlob LAG_ContentBlob_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(LAG_ContentBlob);
			}
		}

		public ZPropertyInfo LAG_ContentBlobInfo
		{
			get { return GetZPropertyInfo(nameof(LAG_ContentBlob)); }
		}

		public static IEnumerable<string> GetIncompleteAgreementStatuses()
		{
			yield return LicenseAgreementStatusList.Codes.Pending;
			yield return LicenseAgreementStatusList.Codes.Queued;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LAG_Status = LicenseAgreementStatusList.Codes.Pending;
		}

		public static LicenseAgreement ImportNewAgreement(BusinessObjectFactory factory, UserAgreementResponseData response, string productCode)
		{
			var newAgreement = factory.New<LicenseAgreement>();
			using (newAgreement.GetValidationSuspender())
			{
				newAgreement.LAG_MajorVersion = response.VersionNumber.ToString();
				newAgreement.LAG_MinorVersion = response.MinorVersionNumber.ToString();
				newAgreement.LAG_VariantCode = response.Variant ?? string.Empty;
				newAgreement.LAG_Content = response.Content;
				newAgreement.LAG_Title = response.Title;
				newAgreement.LAG_Type = productCode;
				newAgreement.LAG_Status = LicenseAgreementStatusList.Codes.Pending;
				newAgreement.LAG_EffectiveStartUtc = ZDateTime.UtcNow;
				newAgreement.LAG_EffectiveEndUtc = ZDateTime.Empty;
			}

			return newAgreement;
		}
	}
}
