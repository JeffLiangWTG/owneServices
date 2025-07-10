using System.ComponentModel;
using System.Reflection;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AvsWebServiceUriRegistryBusinessObject))]
	sealed class AvsWebServiceUriRegistryBusinessObjectTest : RegistryBusinessObjectTestCaseBase
	{
		protected override bool RequiresFactory => false;
		protected override bool RequiresFallbackLevel => false;

		public void TestCodeDescriptionBoolProperties()
		{
			var bizO = new AvsWebServiceUriRegistryBusinessObject();

			bizO.Code = "code1";
			bizO.EnglishDescription = "http://description1.com/";
			bizO.Bool = false;

			AssertEquals("code1", bizO.Type);
			AssertEquals("http://description1.com/", bizO.ServiceUri);
			AssertEquals(false, bizO.EnableSystemToSystemTrustAuthentication);

			bizO.Type = "code2";
			bizO.ServiceUri = "http://description2.com/";
			bizO.EnableSystemToSystemTrustAuthentication = true;

			AssertEquals("code2", bizO.Code);
			AssertEquals("http://description2.com/", bizO.EnglishDescription);
			AssertEquals(true, bizO.Bool);
		}

		public void TestGetClone()
		{
			var bizO = new AvsWebServiceUriRegistryBusinessObject();

			bizO.Type = "Primary";
			bizO.ServiceUri = "https://url.com/v2/";
			bizO.EnableSystemToSystemTrustAuthentication = true;

			var clone = (AvsWebServiceUriRegistryBusinessObject)bizO.Clone(null, null);

			AssertEquals("Primary", clone.Type);
			AssertEquals("https://url.com/v2/", clone.ServiceUri);
			AssertEquals(true, clone.EnableSystemToSystemTrustAuthentication);
		}

		public void TestCodeMaxLength()
		{
			AssertEquals(10, new AvsWebServiceUriRegistryBusinessObject().Code_MaxLength);
		}

		public void TestCodeIsReadonlyForGUI()
		{
			var readOnly = typeof(AvsWebServiceUriRegistryBusinessObject)
				.GetProperty("Code")
				.GetCustomAttribute<ReadOnlyAttribute>();

			Assert(readOnly.IsReadOnly);
		}

		public void TestFormatServiceUri()
		{
			var bizO = new AvsWebServiceUriRegistryBusinessObject();
			var rawFormatServiceUriMethod = typeof(AvsWebServiceUriRegistryBusinessObject).GetMethod("FormatServiceUri", BindingFlags.Instance | BindingFlags.NonPublic);

			var doNothingFormat = false;
			var mockAvsWebServiceUriBizO = new Mock<AvsWebServiceUriRegistryBusinessObject>() { CallBase = true };
			mockAvsWebServiceUriBizO.Protected()
				.As<IMockFormatServiceUriBecauseCanNotInheritInTestAssembly>()
				.Setup(x => x.FormatServiceUri(It.IsAny<string>()))
				.Returns<string>(uri => doNothingFormat ? uri : (string)rawFormatServiceUriMethod.Invoke(bizO, new [] { uri }));

			doNothingFormat = true;
			mockAvsWebServiceUriBizO.Object.ServiceUri = "https://fake.com/v2       ";
			doNothingFormat = false;
			AssertEquals("https://fake.com/v2/", mockAvsWebServiceUriBizO.Object.ServiceUri);

			mockAvsWebServiceUriBizO.Object.ServiceUri = "     https://fake.com/v3  ";
			doNothingFormat = true;
			AssertEquals("https://fake.com/v3/", mockAvsWebServiceUriBizO.Object.ServiceUri);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new AvsWebServiceUriRegistryBusinessObject();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new AvsWebServiceUriRegistryBusinessObject();
		}

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			if (info.Name != "ServiceUri" && info.Name != "EnglishDescription")
			{
				base.TestBizObjectField(info);
			}
		}
	}

	interface IMockFormatServiceUriBecauseCanNotInheritInTestAssembly
	{
		string FormatServiceUri(string uri);
	}
}
