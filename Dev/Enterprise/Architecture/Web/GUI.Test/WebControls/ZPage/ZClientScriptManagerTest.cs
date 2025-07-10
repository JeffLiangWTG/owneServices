using System.Collections.Specialized;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZClientScriptManagerTest : TestCase
	{
		public void TestRegisterForEventScripts()
		{
			ZTestPage page = new ZTestPage();
			AssertEquals("IsClientForEventScriptBlockRegistered should be false", false, page.ZClientScript.IsClientForEventScriptBlockRegistered("window", "onload", "ForEventTestKeys"));
			AssertEquals("IsClientForEventScriptBlockRegistered should be false", false, page.ZClientScript.IsClientForEventScriptBlockRegistered("window", "onload", "ForEventTestKeys"));

			page.ZClientScript.RegisterClientForEventScriptBlock("window", "onload", GetType(), "ForEventTestKey", "window.alert('Test');");

			AssertEquals("IsClientForEventScriptBlockRegistered should be true after registering", true, page.ZClientScript.IsClientForEventScriptBlockRegistered("window", "onload", "ForEventTestKey"));
			StringCollection registeredKeys = page.ZClientScript.GetRegisteredForEventKeysForTesting("window", "onload");
			AssertNotNull("RegisteredKeys should not be null", registeredKeys);
			AssertEquals("Should only have registered one script key", 1, registeredKeys.Count);
			Assert("Should contain ForEventTestKey", registeredKeys.Contains("ForEventTestKey"));
			AssertMultilineASCIIEquals("Script", "window.alert('Test');", page.ZClientScript.GetForEventScriptForTesting("window", "onload"));

			page.ZClientScript.RegisterClientForEventScriptBlock("window", "onload", GetType(), "ForEventTestKey", "window.alert('Should not add');");

			registeredKeys = page.ZClientScript.GetRegisteredForEventKeysForTesting("window", "onload");
			AssertEquals("Should still contain only one script key", 1, registeredKeys.Count);
			Assert("Should contain ForEventTestKey", registeredKeys.Contains("ForEventTestKey"));
			AssertMultilineASCIIEquals("Duplicate script should not have been added. Key already exists", "window.alert('Test');", page.ZClientScript.GetForEventScriptForTesting("window", "onload"));

			page.ZClientScript.RegisterClientForEventScriptBlock("window", "onload", GetType(), "AnotherTestKey", "window.alert('Second Test');");

			registeredKeys = page.ZClientScript.GetRegisteredForEventKeysForTesting("window", "onload");
			AssertEquals("Should have registered second script key", 2, registeredKeys.Count);
			Assert("Should contain ForEventTestKey", registeredKeys.Contains("ForEventTestKey"));
			Assert("Should contain AnotherTestKey", registeredKeys.Contains("AnotherTestKey"));
			AssertMultilineASCIIEquals("Second Script should have been added", "window.alert('Test');\nwindow.alert('Second Test');", page.ZClientScript.GetForEventScriptForTesting("window", "onload"));
			//Page.OnPreRenderForTesting();
			//AssertEquals("Scripts should have rendered", Page.ClientScriptInternal.IsClientScriptBlockRegistered("ForEventTestKey"));
		}
	}
}
