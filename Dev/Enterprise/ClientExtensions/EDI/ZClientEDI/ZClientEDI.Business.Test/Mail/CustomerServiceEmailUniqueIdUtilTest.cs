using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Environment;
using MailManager;

namespace Enterprise.Client.EDI.Mail.Business.Test
{
	class CustomerServiceEmailUniqueIdUtilTest : TestCaseWithFactory
	{
		public void TestGetUniqueID()
		{
			var mailItem = Factory.NewWithValidTestData<MailItem>();
			mailItem.MI_From = "From@Domain.Com";
			mailItem.MI_Direction = DirectionList.Codes.Receive;
			var supportIncident = Factory.New<SupportIncident>();
			Factory.Save();
			var generateUniqueEmailId = CustomerServiceEmailUniqueIdUtil.GenerateUniqueEmailID(supportIncident);
			Assert("generateUniqueEmailId should not be empty", !string.IsNullOrEmpty(generateUniqueEmailId));
			mailItem.MI_Body = string.Format(IncidentConstants.MarkContent, generateUniqueEmailId);
			var uniqueId = CustomerServiceEmailUniqueIdUtil.GetUniqueID(mailItem);
			AssertEquals(generateUniqueEmailId, uniqueId);
		}

		public void TestGetMarkId()
		{
			var mark1 = "<div name='CustomerServiceIdentifier' id='mark1UniqueId'/>Please don't edit the content of this email as your message may not be receive";
			var mark2 = "<div name=\"CustomerServiceIdentifier\" id=\"mark2UniqueId\"/>Please don't edit the content of this email as your message may not be receive";
			var mark3 = "<div id='mark3UniqueId' name='CustomerServiceIdentifier'></div>Please don't edit the content of this email as your message may not be receive";
			var mark4 = "<div id='mark3UniqueId' name='CustomerServiceIdentifier'>Please don't edit the content of this email as your message may not be receive</div>";
 
			var mark5 = @"<div id='XhD1x1&#43;nNXHhgGFV7Z87ewsQR518bn/1DWkAh9zLsM5bWv82zBazroHOrq8&#43;bg230ZCTk5KL&#43;lBLdU4k&#43;p3IDQ==' name='CustomerServiceIdentifier'>Please don't edit the content of this email as your message may not be receive</div>";
			var mark6 = @"<div id='Y1W0kFwUf/hiUzfn0HH&#43;ctmphQL6sLzMe4wYMYhPB&#43;bOsl8KjSivvjWY3wzL95mEPhCA/ZwGM0uY/Z&#43;30p41iA==' name='CustomerServiceIdentifier'>Please don't edit the content of this email as your message may not be receive</div>";
			var mark7 = @"<div id='c9beUBgcWS3xxaiA9mwHNUruZ12u7gDfLm8XgAGtCpIfqq6NBcepWdyPvSNnh0B+lu8+ztEVmyUyI2VRc5kIEA==' name='CustomerServiceIdentifier'>Please don't edit the content of this email as your message may not be receive</div>";

			var methodInfo = typeof(CustomerServiceEmailUniqueIdUtil).GetMethod("GetMarkId",
				System.Reflection.BindingFlags.IgnoreCase
				| System.Reflection.BindingFlags.NonPublic
				| System.Reflection.BindingFlags.Static);
			AssertEquals("mark1UniqueId", methodInfo.Invoke(null, new object[] { mark1 }));
			AssertEquals("mark2UniqueId", methodInfo.Invoke(null, new object[] { mark2 }));
			AssertEquals("mark3UniqueId", methodInfo.Invoke(null, new object[] { mark3 }));
			AssertEquals("mark3UniqueId", methodInfo.Invoke(null, new object[] { mark4 }));

			AssertEquals("Result should be html decoded", "XhD1x1+nNXHhgGFV7Z87ewsQR518bn/1DWkAh9zLsM5bWv82zBazroHOrq8+bg230ZCTk5KL+lBLdU4k+p3IDQ==", methodInfo.Invoke(null, new object[] { mark5 }));
			AssertEquals("Result should be html decoded", "Y1W0kFwUf/hiUzfn0HH+ctmphQL6sLzMe4wYMYhPB+bOsl8KjSivvjWY3wzL95mEPhCA/ZwGM0uY/Z+30p41iA==", methodInfo.Invoke(null, new object[] { mark6 }));
			AssertEquals("Unescaped characters should be read normally", "c9beUBgcWS3xxaiA9mwHNUruZ12u7gDfLm8XgAGtCpIfqq6NBcepWdyPvSNnh0B+lu8+ztEVmyUyI2VRc5kIEA==", methodInfo.Invoke(null, new object[] { mark7 }));
		}

		public void TestGetUniqueIdFromBody()
		{
			var mailItem = Factory.NewWithValidTestData<MailItem>();
			mailItem.MI_From = "From@Domain.Com";
			mailItem.MI_Direction = DirectionList.Codes.Receive;
			var supportIncident = Factory.New<SupportIncident>();
			Factory.Save();
			var generateUniqueEmailId = CustomerServiceEmailUniqueIdUtil.GenerateUniqueEmailID(supportIncident);
			Assert("generateUniqueEmailId should not be empty", !string.IsNullOrEmpty(generateUniqueEmailId));
			mailItem.MI_Body = $"Hello I would like some help fixing this issue. Heres a random number apparently its important; {generateUniqueEmailId}. Anyway, yes please fix it is causing major problems.";
			var uniqueId = CustomerServiceEmailUniqueIdUtil.GetUniqueIDFromBody(supportIncident, mailItem);
			AssertEquals(generateUniqueEmailId, uniqueId);
		}

		public void TestGetUniqueIdFromBodyWhenEmailOnlyHaveHtmlEncodedToken()
		{
			var supportIncident = Factory.NewWithPrimaryKey<SupportIncident>(Guid.Parse("2B2E0E63-4F09-4473-A761-F6AD9B6CE002"));
			supportIncident.IM_IncidentNumber = "CS00012345";
			Factory.Save();

			var mailItem = Factory.NewWithValidTestData<MailItem>();
			mailItem.MI_From = "support@wisetechglobal.com";
			mailItem.MI_Direction = DirectionList.Codes.Receive;
			mailItem.MI_Subject = "Archie Test Email Subject";
			mailItem.MI_ContentType = EmailContentTypes.HTML.ContentTypeCode;
			mailItem.MI_Body = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01//EN"" ""http://www.w3.org/TR/htm=
l4/strict.dtd""><html><head>
<meta http-equiv=3D""Content-Type"" content=3D""text/html; charset=3Dutf-8"">
<meta http-equiv=3D""Content-Style-Type"" content=3D""text/css"">
<meta name=3D""Generator"" content=3D""FlexCel 7.21.0.0"">
<title>
Archie Test Email Title
</title>
</head>
<body>
<table class=3D""flxmain_table"" border=3D""0"" cellpadding=3D""0"" cellspacing=
=3D""0"" style=3D""width:466.52pt"" summary=3D""Excel Sheet: 1 - New Carrier Con=
nection"">
 <tr style=3D""height:51.17pt;"">
 =20
  <td class=3D""flx7"" style=3D""width:466.52pt;"" colspan=3D""26"" rowspan=3D""1""=
><span class=3D""flx7"" style=3D""height:51.17pt;""><br><span style=3D""font-siz=
e:8pt;"">Please keep the following token in all email communication to ensur=
e that your responses are received:&nbsp;</span><span style=3D""font-weight:=
bold;font-size:8pt;"">[hrn1VoomID6y30ZUk7dnG49k9LWiuPLlsjKAy6HR3V3MO7j58HX98IW&#43;EJ8k1qjQHvMAt2ldHHXm9VsFtloeXw==</span><span style=3D""font-size:=
8pt;"">]</span></span></td>
</tr>
</table>
</html>";

			var expectedToken = CustomerServiceEmailUniqueIdUtil.GenerateUniqueEmailID(supportIncident);
			Assert("HTML original HTML content doesn't contain the token", !mailItem.MI_Body.Contains(expectedToken));
			var uniqueId = CustomerServiceEmailUniqueIdUtil.GetUniqueIDFromBody(supportIncident, mailItem);
			AssertEquals("After decoding HTML content we can get the token", expectedToken, uniqueId);
		}

		public void TestGetUniqueIdFromBodyWhenEmailHavePlainTextContentType()
		{
			var supportIncident = Factory.NewWithPrimaryKey<SupportIncident>(Guid.Parse("2B2E0E63-4F09-4473-A761-F6AD9B6CE002"));
			supportIncident.IM_IncidentNumber = "CS00012345";
			Factory.Save();

			var mailItem = Factory.NewWithValidTestData<MailItem>();
			mailItem.MI_From = "support@wisetechglobal.com";
			mailItem.MI_Direction = DirectionList.Codes.Receive;
			mailItem.MI_Subject = "Archie Test Email Subject";
			mailItem.MI_ContentType = EmailContentTypes.PlainText.ContentTypeCode;
			mailItem.MI_Body = @"this is a test email body with email token hrn1VoomID6y30ZUk7dnG49k9LWiuPLlsjKAy6HR3V3MO7j58HX98IW+EJ8k1qjQHvMAt2ldHHXm9VsFtloeXw==";

			var expectedToken = CustomerServiceEmailUniqueIdUtil.GenerateUniqueEmailID(supportIncident);
			var uniqueId = CustomerServiceEmailUniqueIdUtil.GetUniqueIDFromBody(supportIncident, mailItem);
			AssertEquals("should get token from plain text email content", expectedToken, uniqueId);
		}
	}
}
