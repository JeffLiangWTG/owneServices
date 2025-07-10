using System;
using System.Linq;
using System.Xml;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Test
{
	sealed class XMLHelperTest : TestCase
	{
		public void TestGetExistingRedirects()
		{
			var dummyConfigXmlPath = DummyConfigXmlPath;
			var config = new XmlDocument();
			config.Load(dummyConfigXmlPath);

			var existingRedirects = XMLHelper.GetExistingRedirects(dummyConfigXmlPath);
			AssertEquals("All dependentAssembly read", existingRedirects.Count(), 3);
		}

		public void TestGetElement()
		{
			var dummyConfigXmlPath = DummyConfigXmlPath;
			var config = new XmlDocument();
			config.Load(dummyConfigXmlPath);
			const string firstRedirectExpected = "<bindingRedirect oldVersion=\"0.0.0.0-1.9.0.0\" newVersion=\"1.9.0.0\" xmlns=\"urn:schemas-microsoft-com:asm.v1\" />";

			var firstRedirect = XMLHelper.GetExistingRedirects(dummyConfigXmlPath).First();
			var firstBindingRedirect = XMLHelper.GetElement(firstRedirect, "bindingRedirect").ToString();
			AssertEquals("GetElement bindingRedirect returns correct bindingredirect ", firstRedirectExpected, firstBindingRedirect);
		}

		public void TestGetAttribute()
		{
			var dummyConfigXmlPath = DummyConfigXmlPath;
			var config = new XmlDocument();
			config.Load(dummyConfigXmlPath);
			const string expectedName = "BouncyCastle.Crypto";

			var firstRedirect = XMLHelper.GetExistingRedirects(dummyConfigXmlPath).First();
			var firstAssemblyName = XMLHelper.GetAttribute(firstRedirect, "assemblyIdentity", "name");
			AssertEquals("GetAttribute returns correct Attribute Values ", expectedName, firstAssemblyName);
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		string DummyConfigXmlPath => resourceRetriever.Value.SaveResourceToFile("DummyConfig.xml");
	}
}
