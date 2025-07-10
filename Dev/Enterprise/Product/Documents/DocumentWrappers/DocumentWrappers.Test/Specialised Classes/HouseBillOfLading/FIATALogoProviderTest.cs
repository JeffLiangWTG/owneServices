using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class FIATALogoProviderTest : TransactionedTestCase
	{
		public void TestGetFIATALogo_AU_Authorised()
		{
			using (FreightDataRegistry.Instance.FIATAAuthorised.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var fiataLogProvider = new FIATALogoProvider();
				AssertNotNull(fiataLogProvider.GetFIATALogo("AU"));
			}
		}

		public void TestGetFIATALogo_AU_Unauthorised()
		{
			using (FreightDataRegistry.Instance.FIATAAuthorised.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var fiataLogProvider = new FIATALogoProvider();
				AssertNull(fiataLogProvider.GetFIATALogo("AU"));
			}
		}

		public void TestGetFIATALogo_NoCountryCode()
		{
			using (FreightDataRegistry.Instance.FIATAAuthorised.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var fiataLogProvider = new FIATALogoProvider();
				AssertNull(fiataLogProvider.GetFIATALogo(null));
			}
		}

		public void TestGetFIATATextLogo_SWB_Authorised()
		{
			using (FreightDataRegistry.Instance.FIATAAuthorised.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var fiataLogProvider = new FIATALogoProvider();
				AssertNotNull(fiataLogProvider.GetFIATATextLogo(true));
			}
		}

		public void TestGetFIATATextLogo_SWB_Unauthorised()
		{
			using (FreightDataRegistry.Instance.FIATAAuthorised.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var fiataLogProvider = new FIATALogoProvider();
				AssertNotNull(fiataLogProvider.GetFIATATextLogo(true));
			}
		}
	}
}
