using System;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class PRAMessagingRegistryTest : TransactionedTestCase
	{
		public void TestAcknowledgementEmailGroup()
		{
			Guid testGroupPK = Guid.NewGuid();
			Registry.RawRegistry.AcknowledgementEmailGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, testGroupPK);
			AssertEquals("AcknowledgementEmailGroup", testGroupPK, Registry.Freight.PRAMessaging.AcknowledgementEmailGroup);
		}

		public void TestAcknowledgementEmailMode()
		{
			Registry.RawRegistry.AcknowledgementEmailMode.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.EmailTo.NoEmails);
			AssertEquals("AcknowledgementEmailMode", Constants.EmailTo.NoEmails, Registry.Freight.PRAMessaging.AcknowledgementEmailMode);
		}

		public void TestImpedimentEmailGroup()
		{
			Guid testGroupPK = Guid.NewGuid();
			Registry.RawRegistry.ImpedimentEmailGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, testGroupPK);
			AssertEquals("ImpedimentEmailGroup", testGroupPK, Registry.Freight.PRAMessaging.ImpedimentEmailGroup);
		}

		public void TestImpedimentEmailMode()
		{
			Registry.RawRegistry.ImpedimentEmailMode.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.EmailTo.NoEmails);
			AssertEquals("ImpedimentEmailMode", Constants.EmailTo.NoEmails, Registry.Freight.PRAMessaging.ImpedimentEmailMode);
		}

		public void TestErrorEmailGroup()
		{
			Guid testGroupPK = Guid.NewGuid();
			Registry.RawRegistry.ErrorEmailGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, testGroupPK);
			AssertEquals("ErrorEmailGroup", testGroupPK, Registry.Freight.PRAMessaging.ErrorEmailGroup);
		}

		public void TestErrorEmailMode()
		{
			Registry.RawRegistry.ErrorEmailMode.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.EmailTo.NoEmails);
			AssertEquals("ErrorEmailMode", Constants.EmailTo.NoEmails, Registry.Freight.PRAMessaging.ErrorEmailMode);
		}

		public void TestPRATestMode()
		{
			Registry.RawRegistry.PRATestMode.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("PRATestMode", true, Registry.Freight.PRAMessaging.PRATestMode);
		}

		public void TestSeaFreightDangerousGoodsContact()
		{
			Guid contactPK = Guid.NewGuid();
			Registry.RawRegistry.PRAMessagingSeaFreightDangerousGoodsContact.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, contactPK);
			AssertEquals("SeaFreightDangerousGoodsContact", contactPK, Registry.Freight.PRAMessaging.SeaFreightDangerousGoodsContact);
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
