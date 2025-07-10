using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(NotificationEmailTemplateRegistryItem))]
	sealed class NotificationEmailTemplateRegistryItemTest : StronglyTypedRegistryItemTestCase<NotificationEmailTemplate>
	{
		protected override StronglyTypedRegistryItem<NotificationEmailTemplate, NotificationEmailTemplate> GetNewRegistryItem()
		{
			return new NotificationEmailTemplateRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, typeof(BusinessObject), "test subject", "test notification email template");
		}

		public void TestGetMaxLength()
		{
			var registryItem = (NotificationEmailTemplateRegistryItem)GetNewRegistryItem();
			var emailTemplate = registryItem.Value;
			emailTemplate.EmailSubject = "Team";
			emailTemplate.EmailBody = @"Company Code: (*CompanyCode*)<br />
User Name: (*UserName*)<br />
Password: (*Password*)<br /><br />";
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailTemplate);

			var captionSource = (IRegistryItemVariantLengthCaptionSource)registryItem;

			AssertEquals("Precondition", emailTemplate.EmailSubjectInfo.MaxLength, captionSource.GetMaxLength(emailTemplate.EmailSubject));
			AssertEquals("Precondition", emailTemplate.EmailBodyInfo.MaxLength, captionSource.GetMaxLength(emailTemplate.EmailBody));

			string chsSubject = "小组";
			string chsBody = @"公司代码： (*CompanyCode*)<br />
用户名： (*UserName*)<br />
密码： (*Password*)<br /><br />";

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mocksChs = Res.UseMockData())
			{
				var keySubject = CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, registryItem.Value.EmailSubject).ResourceKey;
				mocksChs.Put(keySubject, new ResourceStringData(keySubject, chsSubject));
				var keyBody = CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, registryItem.Value.EmailBody).ResourceKey;
				mocksChs.Put(keyBody, new ResourceStringData(keyBody, chsBody));

				AssertEquals(emailTemplate.EmailSubjectInfo.MaxLength, captionSource.GetMaxLength(chsSubject));
				AssertEquals(emailTemplate.EmailBodyInfo.MaxLength, captionSource.GetMaxLength(chsBody));
			}
		}

		public void TestTranslatable()
		{
			var registryItem = GetNewRegistryItem();
			var emailTemplate = registryItem.Value;
			emailTemplate.EmailSubject = "Team";
			emailTemplate.EmailBody = @"Company Code: (*CompanyCode*)<br />
User Name: (*UserName*)<br />
Password: (*Password*)<br /><br />";
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailTemplate);

			string chsSubject = "小组";
			string chsBody = @"公司代码： (*CompanyCode*)<br />
用户名： (*UserName*)<br />
密码： (*Password*)<br /><br />";

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mocksChs = Res.UseMockData())
			{
				var captionSource = (IRegistryItemCaptionSource)registryItem;
				var keySubject = CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, registryItem.Value.EmailSubject).ResourceKey;
				mocksChs.Put(keySubject, new ResourceStringData(keySubject, chsSubject));
				var keyBody = CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, registryItem.Value.EmailBody).ResourceKey;
				mocksChs.Put(keyBody, new ResourceStringData(keyBody, chsBody));

				AssertEquals(chsSubject, registryItem.Value.EmailSubject);
				AssertEquals(chsBody, registryItem.Value.EmailBody);
			}
		}

		public void TestRegistryValue_CurrentLanguageChanged()
		{
			var engSubject = "Team";
			var engBody = @"Company Code: (*CompanyCode*)<br />
User Name: (*UserName*)<br />
Password: (*Password*)<br /><br />";

			var chsSubject = "小组";
			var chsBody = @"公司代码： (*CompanyCode*)<br />
用户名： (*UserName*)<br />
密码： (*Password*)<br /><br />";

			var koreanSubject = "그룹";
			var koreanBody = @"회사 코드: (*CompanyCode*)<br />
사용자 이름: (*UserName*)<br />
암호: (*Password*)<br /><br />";

			var registryItem = (NotificationEmailTemplateRegistryItem)GetNewRegistryItem();
			var emailTemplate = registryItem.Value;
			emailTemplate.EmailSubject = engSubject;
			emailTemplate.EmailBody = engBody;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailTemplate);

			var captionSource = (IRegistryItemVariantLengthCaptionSource)registryItem;

			AssertEquals("Precondition", emailTemplate.EmailSubjectInfo.MaxLength, captionSource.GetMaxLength(emailTemplate.EmailSubject));
			AssertEquals("Precondition", emailTemplate.EmailBodyInfo.MaxLength, captionSource.GetMaxLength(emailTemplate.EmailBody));

			using (var mocksChs = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			using (var mocksEng = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.English).UseMockData())
			using (var mocksKorean = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.Korean).UseMockData())
			{
				var keySubject = CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, engSubject).ResourceKey;
				var keyBody = CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, engBody).ResourceKey;

				mocksChs.Put(keySubject, new ResourceStringData(keySubject, chsSubject));
				mocksChs.Put(keyBody, new ResourceStringData(keyBody, chsBody));

				mocksEng.Put(keySubject, new ResourceStringData(keySubject, engSubject));
				mocksEng.Put(keyBody, new ResourceStringData(keyBody, engBody));

				mocksKorean.Put(keySubject, new ResourceStringData(keySubject, koreanSubject));
				mocksKorean.Put(keyBody, new ResourceStringData(keySubject, koreanBody));

				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					AssertEquals(chsSubject, registryItem.Value.EmailSubject);
					AssertEquals(chsBody, registryItem.Value.EmailBody);
				}

				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.English))
				{
					AssertEquals(engSubject, registryItem.Value.EmailSubject);
					AssertEquals(engBody, registryItem.Value.EmailBody);
				}

				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Korean))
				{
					AssertEquals(koreanSubject, registryItem.Value.EmailSubject);
					AssertEquals(koreanBody, registryItem.Value.EmailBody);
				}

				AssertEquals("Registry should be back to English", engSubject, registryItem.Value.EmailSubject);
				AssertEquals("Registry should be back to English", engBody, registryItem.Value.EmailBody);
			}
		}

		public void TestGetCaptions()
		{
			var registryItem = new NotificationEmailTemplateRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, typeof(BusinessObject), "test subject", "test body");
			var actualCaptions = new List<string>(registryItem.GetCaptions(registryItem.Value));
			AssertEquals("Precondition", 2, actualCaptions.Count);
			AssertEquals(registryItem.Value.EmailSubject, actualCaptions[0]);
			AssertEquals(registryItem.Value.EmailBody, actualCaptions[1]);
		}

		public void TestGetCaptionsHideEmailBody()
		{
			var registryItem = new NotificationEmailTemplateRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, typeof(BusinessObject), "test subject", "", true);
			var actualCaptions = new List<string>(registryItem.GetCaptions(registryItem.Value));
			AssertEquals("Precondition", 2, actualCaptions.Count);
			AssertEquals(registryItem.Value.EmailSubject, actualCaptions[0]);
			AssertEquals(registryItem.Value.EmailBody, actualCaptions[1]);
		}
	}
}
