using System;
using System.IO;
using System.Reflection;
using Enterprise.CryptoUtilities;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class FileUpLoaderX509CertificateRegistryEditorInfoTest : TransactionedTestCase
	{
		public void TestConstructor()
		{
			DummyFileUpLoaderX509CertificateRegistryEditorInfo editorInfo = new DummyFileUpLoaderX509CertificateRegistryEditorInfo();
			AssertEquals("IsPrivateKeyPair", false, editorInfo.IsPrivateKeyPair);
			AssertEquals("PrivateKeyRegistryItem", null, editorInfo.PrivateKeyRegistryItem);

			StringRegistryItem registryItem = new StringRegistryItem("", null, null, null, RegistryStorageFlags.System);
			editorInfo = new DummyFileUpLoaderX509CertificateRegistryEditorInfo(registryItem);
			AssertEquals("IsPrivateKeyPair", true, editorInfo.IsPrivateKeyPair);
			AssertEquals("PrivateKeyRegistryItem", registryItem, editorInfo.PrivateKeyRegistryItem);
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "PrivateKeyRegistryItem cannot be null.")]
		public void TestThrowExceptionIfPrivateKeyRegistryItemIsNull()
		{
			FileUpLoaderX509CertificateRegistryEditorInfo editorInfo = new FileUpLoaderX509CertificateRegistryEditorInfo(null);
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "The DataType of PrivateKeyRegistryItem must be a StringRegistryDataType.")]
		public void TestThrowExceptionIfPrivateKeyRegistryItemDataTypeIsNotStringRegistryDataType()
		{
			GuidRegistryItem registryItem = new GuidRegistryItem("", (MultilingualString)null, null, null, RegistryStorageFlags.System);
			FileUpLoaderX509CertificateRegistryEditorInfo editorInfo = new FileUpLoaderX509CertificateRegistryEditorInfo(registryItem);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetCertificate()
		{
			string testFilesDirectory = Path.Combine(BaseSourcePath, "Enterprise", "Tools", "StandAlone", "CryptoUtilities", "CryptoUtilities.Test", "TestFiles");
			string testFile = Path.Combine(testFilesDirectory, "Conf.cer");

			DummyFileUpLoaderX509CertificateRegistryEditorInfo editorInfo = new DummyFileUpLoaderX509CertificateRegistryEditorInfo();

			using (Certificate certificate = (Certificate)editorInfo.GetCertificate(File.ReadAllBytes(testFile)))
			{
				AssertNotNull("Certificate should not be null.", certificate);
			}

			testFile = Path.Combine(testFilesDirectory, "SEDI_Test_EncryptionKeyPair.p12");

			Guid companyGuid = Guid.NewGuid();
			StringRegistryItem registryItem = new StringRegistryItem("", null, null, null, RegistryStorageFlags.Company);
			registryItem.SetValue(companyGuid, Guid.Empty, Guid.Empty, "4Customs&SEDI");

			editorInfo = new DummyFileUpLoaderX509CertificateRegistryEditorInfo(registryItem);
			editorInfo.FallbackLevel = new FallbackLevel(companyGuid, Guid.Empty, Guid.Empty);

			using (Store store = (Store)editorInfo.GetCertificate(File.ReadAllBytes(testFile)))
			{
				AssertNotNull("Store should not be null.", store);
			}
		}

		#region class DummyFileUpLoaderX509CertificateRegistryEditorInfo

		class DummyFileUpLoaderX509CertificateRegistryEditorInfo : FileUpLoaderX509CertificateRegistryEditorInfo
		{
			public DummyFileUpLoaderX509CertificateRegistryEditorInfo()
			{
			}

			public DummyFileUpLoaderX509CertificateRegistryEditorInfo(IRegistryItem privateKeyRegistryItem) : base(privateKeyRegistryItem)
			{
			}

			public bool IsPrivateKeyPair
			{
				get { return (bool)typeof(FileUpLoaderX509CertificateRegistryEditorInfo).GetField("IsPrivateKeyPair", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this); }
			}

			public IRegistryItem PrivateKeyRegistryItem
			{
				get { return (IRegistryItem)typeof(FileUpLoaderX509CertificateRegistryEditorInfo).GetField("PrivateKeyRegistryItem", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this); }
			}
		}

		#endregion
	}
}
