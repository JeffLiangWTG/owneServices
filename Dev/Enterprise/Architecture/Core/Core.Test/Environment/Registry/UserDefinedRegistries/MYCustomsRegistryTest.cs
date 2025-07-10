using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class MYCustomsRegistryTest : TransactionedTestCase
	{
		public void TestUseRankAlphaForK4K5()
		{
			AssertEquals("RankAlphaExportDirectory", false, Registry.MYCustoms.UseRankAlphaForK4K5);
			Registry.MYCustoms.UseRankAlphaForK4K5 = true;
			AssertEquals("RankAlphaExportDirectory", true, Registry.MYCustoms.UseRankAlphaForK4K5);
		}

		public void TestMYMessageOutputDirectory()
		{
			AssertEquals("RankAlphaExportDirectory", "", Registry.MYCustoms.MYMessageOutputDirectory);
			Registry.MYCustoms.MYMessageOutputDirectory = "splaty";
			AssertEquals("RankAlphaExportDirectory", "splaty", Registry.MYCustoms.MYMessageOutputDirectory);
		}

		public void TestImportContinuingPermission()
		{
			Registry.RawRegistry.MYCustomsImportContinuingPermission.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "12345678901234567890");
			AssertEquals("ImportContinuingPermission", "12345678901234567890", Registry.MYCustoms.ImportContinuingPermission);
		}

		public void TestMYCustomsSenderID()
		{
			Registry.RawRegistry.MYCustomsSenderID.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "12345678901234567890");
			AssertEquals("MYCustomsSenderID", "12345678901234567890", Registry.MYCustoms.MYCustomsSenderID);
		}

		public void TestMYDagangPassword()
		{
			Registry.RawRegistry.MYDagangPassword.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "12345678901234567890");
			AssertEquals("MYDagangPassword", "12345678901234567890", Registry.MYCustoms.MYDagangPassword);
		}

		public void TestMYSenderPassword()
		{
			Registry.RawRegistry.MYSenderPassword.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "12345678901234567890");
			AssertEquals("MYSenderPassword", "12345678901234567890", Registry.MYCustoms.MYSenderPassword);
		}

		public void TestIsTestMode()
		{
			Registry.RawRegistry.MYIsTestMode.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("IsTestMode", true, Registry.MYCustoms.IsTestMode);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Registry = new DataRegistry();
		}

		DataRegistry Registry;

		#endregion
	}
}
