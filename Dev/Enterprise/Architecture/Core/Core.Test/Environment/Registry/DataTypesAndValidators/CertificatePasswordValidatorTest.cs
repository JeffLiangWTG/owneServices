using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Integration.Customs.AU;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(CertificatePasswordValidator))]
	sealed class CertificatePasswordValidatorTest : StringRegistryDataTypeTest
	{
		[ExpectNoExceptions]
		[TestDate]
		public void TestCertificatePasswordValidatorCorrectPassword()
		{
			certificatesHelper.SetupValidCompanyCertificatesForTest();
			IRegistryItem item = new RegistryItemImpl("TestItem", (NoResString)"TestCategory/TestSubCat", (NoResString)"Caption", (NoResString)"Hint", DataType, RegistryStorageFlags.All);
			item.DataType.Validate(item, certificatesHelper.AUCCompanyCertificatePasswordForTest, EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
		}
		[ExpectException(typeof(RegistryValidationException))]
		public void TestCertificatePasswordValidatorWrongPassword()
		{
			certificatesHelper.SetupValidCompanyCertificatesForTest();
			IRegistryItem item = new RegistryItemImpl("TestItem", (NoResString)"TestCategory/TestSubCat", (NoResString)"Caption", (NoResString)"Hint", DataType, RegistryStorageFlags.All);
			item.DataType.Validate(item, "lalala", EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
		}

		[ExpectException(typeof(RegistryValidationException))]
		public void TestCertificatePasswordValidatorInvalidCertificate()
		{
			EnvProxy.Instance.Registry.AUCCompanyCertificateData = new byte[] { 1, 2, 3, 4 };
			IRegistryItem item = new RegistryItemImpl("TestItem", (NoResString)"TestCategory/TestSubCat", (NoResString)"Caption", (NoResString)"Hint", DataType, RegistryStorageFlags.All);
			item.DataType.Validate(item, certificatesHelper.AUCCompanyCertificatePasswordForTest, EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
		}

		protected override StringRegistryDataType GetNewDataType()
		{
			return new CertificatePasswordValidator();
		}

		protected override object[] GetInvalidSamples()
		{
			return Array.Empty<object>(); // No longer care about min/max values as the base does.
		}

		protected override void SetUp()
		{
			base.SetUp();
			certificatesHelper = ObjectFactory.New<ICertificateManagerHelper>(new BusinessObjectFactory());
		}

		ICertificateManagerHelper certificatesHelper;
	}
}
