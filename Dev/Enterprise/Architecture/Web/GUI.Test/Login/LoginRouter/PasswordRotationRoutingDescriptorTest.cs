using System;
using System.Data;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Login.Testing
{
	[HttpContextEnabledTest]
	sealed class PasswordRotationRoutingDescriptorTest : TestCaseWithFactory
	{
		public void TestRoutingUrl()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.Person.SetHashedPassword("p123");
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_PER = contact2.OC_PER;
			Factory.Save();

			var descriptor1 = new PasswordRotationRoutingDescriptor(contact1);
			var descriptor2 = new PasswordRotationRoutingDescriptor(contact2);

			var utcNow = ZDateTime.UtcNow;
			var uriParts1 = descriptor1.RoutingUrl.ToString().Split('?');
			AssertEquals("~/Admin/ResetMasterPassword.aspx", uriParts1[0]);
			var queryDictionary1 = HttpUtility.ParseQueryString(uriParts1[1]);
			AssertNotNullOrEmpty(queryDictionary1["ResetKey"]);
			AssertToken(queryDictionary1["ResetKey"], utcNow);
			AssertEquals("exp", queryDictionary1["ref"]);

			utcNow = ZDateTime.UtcNow;
			var uriParts2 = descriptor2.RoutingUrl.ToString().Split('?');
			AssertEquals("~/Admin/ResetPassword.aspx", uriParts2[0]);
			var queryDictionary2 = HttpUtility.ParseQueryString(uriParts2[1]);
			AssertNotNullOrEmpty(queryDictionary2["ResetKey"]);
			AssertToken(queryDictionary2["ResetKey"], utcNow);
			AssertEquals("exp", queryDictionary2["ref"]);
		}

		void AssertToken(string token, ZDateTime utcNow)
		{
			var accessToken = Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Token, token));
			var validTotalSeconds = (accessToken.SAT_ExpiresAt - utcNow).TotalSeconds;
			Assert(validTotalSeconds > (10 * 60) - 30 && validTotalSeconds < (10 * 60) + 30);
			AssertEquals(1, accessToken.SAT_RemainingUseCount);
		}

		[TestDate(2021, 10, 10)]
		public void TestIsRoutingRequired()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.SetHashedPassword("c123");
			SimpleStmALog.New(contact1, "WPC", null, ZDateTime.Today.AddDays(-60));

			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.Person.SetHashedPassword("p123");
			SimpleStmALog.New(contact2.Person, "WPC", null, ZDateTime.Today.AddDays(-31));

			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.SetHashedPassword("c456");
			SimpleStmALog.New(contact3, "WPC", null, ZDateTime.Today.AddDays(-30));

			var contact4 = Factory.NewWithValidTestData<OrgContact>();
			contact4.SetHashedPassword("c789");
			SimpleStmALog.New(contact4, "WPC", "ref is not blank...", ZDateTime.Today.AddDays(-99));

			var contact5 = Factory.NewWithValidTestData<OrgContact>();
			contact5.SetHashedPassword("c012");

			var contact6 = Factory.NewWithValidTestData<OrgContact>();
			contact6.RemovePasswordAndHash();

			Factory.Save();

			void assertIsRoutingRequired(string msg, OrgContact contact, bool expected)
			{
				AssertEquals(msg, expected, new PasswordRotationRoutingDescriptor(contact).IsRoutingRequired);
			}

			AssertEquals(0, WebDataRegistry.Instance.WebPasswordRotationDays.Value);
			AssertEquals(DateTime.MinValue, WebDataRegistry.Instance.WebPasswordRotationEffectiveDate.Value);
			assertIsRoutingRequired("#1", contact1, false);
			assertIsRoutingRequired("#2", contact2, false);
			assertIsRoutingRequired("#3", contact3, false);
			assertIsRoutingRequired("#4", contact4, false);
			assertIsRoutingRequired("#5", contact5, false);
			assertIsRoutingRequired("#6", contact6, false);

			WebDataRegistry.Instance.WebPasswordRotationDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 30);
			assertIsRoutingRequired("#7", contact1, true);
			assertIsRoutingRequired("#8", contact2, true);
			assertIsRoutingRequired("#9", contact3, false);
			assertIsRoutingRequired("#10", contact4, false);
			assertIsRoutingRequired("#11", contact5, false);
			assertIsRoutingRequired("#12", contact6, false);

			WebDataRegistry.Instance.WebPasswordRotationEffectiveDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(-30).ToDateTime());
			assertIsRoutingRequired("#13", contact1, true);
			assertIsRoutingRequired("#14", contact2, true);
			assertIsRoutingRequired("#15", contact3, false);
			assertIsRoutingRequired("#16", contact4, false);
			assertIsRoutingRequired("#17", contact5, false);
			assertIsRoutingRequired("#18", contact6, false);

			WebDataRegistry.Instance.WebPasswordRotationEffectiveDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(-31).ToDateTime());
			assertIsRoutingRequired("#19", contact1, true);
			assertIsRoutingRequired("#20", contact2, true);
			assertIsRoutingRequired("#21", contact3, false);
			assertIsRoutingRequired("#22", contact4, true);
			assertIsRoutingRequired("#23", contact5, true);
			assertIsRoutingRequired("#24", contact6, false);
		}

		class SimpleStmALog : AutoStmALog
		{
			public SimpleStmALog(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public static SimpleStmALog New(BusinessObject parent, string eventCode, string refString, ZDateTime postedTimeUtc)
			{
				var log1 = parent.Factory.New<SimpleStmALog>();
				log1.SL_Parent = parent.PK;
				log1.SL_Table = parent.TablePrefix;
				log1.SL_SE_NKEvent = eventCode;
				log1.SL_Reference = refString ?? "";
				log1.SL_EventTime = postedTimeUtc;
				log1.SL_GS_NKUser = "ZZ";
				log1.SL_PostedTimeUtc = postedTimeUtc;
				return log1;
			}
		}
	}
}
