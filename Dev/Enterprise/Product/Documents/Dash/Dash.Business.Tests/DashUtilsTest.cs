using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Dash.Business.Testing;
using Enterprise.Dash.Integration.Services;
using Enterprise.Registry.Business;

namespace Enterprise.Dash.Business.Tests
{
	sealed class DashUtilsTest : TestCaseWithFactory
	{
		DashBusinessObjectTestHelpers Helpers;
		IDashUtils DashUtils;

		protected override void SetUp()
		{
			base.SetUp();

			Helpers = new DashBusinessObjectTestHelpers(Factory);
			DashUtils = ObjectFactory.Get<IDashUtils>();
		}

		public void TestEmptyWhenNotAValidGuid()
		{
			var url = DashUtils.GetCorrectionToolUrl("notaguid");
			AssertEquals(string.Empty, url);
		}

		public void TestEmptyWhenNotAValidDocument()
		{
			var url = DashUtils.GetCorrectionToolUrl("88e60a90-47c1-4415-9bca-aeefa34640cf");
			AssertEquals(string.Empty, url);
		}

		public void TestGenerateURLWhenPKIsValid()
		{
			var eDoc1 = Helpers.CreateStorageFile();

			var dashDocument = Helpers.CreateDashDocument(eDoc1.PK, "CIV");

			Factory.Save();

			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://mysite/"))
			{
				var url = DashUtils.GetCorrectionToolUrl(eDoc1.PK.ToString());
				AssertEquals($"https://mysite/DIN/Desktop#/formFlow/4cf2f4339cda4e7fb8316f6cf823df83/{dashDocument.PK}", url);
			}
		}

		public void TestGenerateURLWhenDuplicated()
		{
			var eDoc1 = Helpers.CreateStorageFile();

			var dashDocument1 = Helpers.CreateDashDocument(eDoc1.PK, "CIV");
			dashDocument1.DDD_IsObsolete = true;
			var dashDocument2 = Helpers.CreateDashDocument(eDoc1.PK, "CIV");

			Factory.Save();

			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://mysite/"))
			{
				var url = DashUtils.GetCorrectionToolUrl(eDoc1.PK.ToString());
				AssertEquals($"https://mysite/DIN/Desktop#/formFlow/4cf2f4339cda4e7fb8316f6cf823df83/{dashDocument2.PK}", url);
			}
		}
	}
}
