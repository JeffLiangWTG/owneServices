using System;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class LicenceTypeChangerTest : TransactionedTestCase
	{
		public void TestSetPRDSystemLicence()
		{
			AssertSystemLicence(DatabaseTypes.Codes.Production, () => LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production));
		}

		public void TestSetTSTSystemLicence()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Training);
			AssertSystemLicence(DatabaseTypes.Codes.Test, () => LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test));
		}

		public void TestSetTRNSystemLicence()
		{
			AssertSystemLicence(DatabaseTypes.Codes.Training, () => LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Training));
		}

		public void TestSetDEMSystemLicence()
		{
			AssertSystemLicence(DatabaseTypes.Codes.Demo, () => LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Demo));
		}

		public void TestSetEDUSystemLicence()
		{
			AssertSystemLicence(DatabaseTypes.Codes.Education, () => LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Education));
		}

		void AssertSystemLicence(string systemTypeCode, Action setLicence)
		{
			Assert(ObjectFactory.Get<IProductRegistration>().Key.DatabaseType != systemTypeCode);

			setLicence();
			Assert(ObjectFactory.Get<IProductRegistration>().Key.DatabaseType == systemTypeCode);
		}
	}
}
