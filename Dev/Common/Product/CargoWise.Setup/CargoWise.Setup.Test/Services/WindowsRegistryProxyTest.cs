using CargoWise.Setup.Services;
using Microsoft.Win32;
using NUnit.Framework;

namespace CargoWise.Setup.Test.Services
{
	[Property("DAT:CapabilityRequirements", "ADMIN")]
	internal class WindowsRegistryProxyTest
	{
		[SetUp]
		[TearDown]
		public void EnsureTestKeyDoesNotExist()
		{
			try
			{
				Registry.LocalMachine.DeleteSubKey(registryPathForTest);
			}
			catch (ArgumentException ex) when (ex.Message == "Cannot delete a subkey tree because the subkey does not exist.")
			{
				// nothing to do
			}
		}

		protected const string registryPathForTest = "Software\\FakeForUnitTest\\";
		protected const string fullRegistryPathForTest = $"HKEY_LOCAL_MACHINE\\{registryPathForTest}";

		[Test]
		public void TestSetAndGetValue()
		{
			var item = new WindowsRegistryProxy();
			item.SetLocalMachineValue(RegistryView.Default, registryPathForTest, "myKey", "valuueeee");

			var value = item.GetValue(fullRegistryPathForTest, "myKey", null);
			Assert.That(value, Is.EqualTo("valuueeee"));
		}

		[Test]
		public void TestGetValueNoValue()
		{
			var item = new WindowsRegistryProxy();
			var value = item.GetValue(fullRegistryPathForTest, "asdfasdf", 123);
			Assert.That(value, Is.EqualTo(123));
		}

		[Test]
		public void TestGetValueNoPath()
		{
			var item = new WindowsRegistryProxy();
			item.SetLocalMachineValue(RegistryView.Default, registryPathForTest, "otherKey", "valuueeee");
			var value = item.GetValue(fullRegistryPathForTest, "asdfasdf", 123);
			Assert.That(value, Is.EqualTo(123));
		}

		[Test]
		public void TestSetValueWhenPathAndKeyExist()
		{
			var item = new WindowsRegistryProxy();
			item.SetLocalMachineValue(RegistryView.Default, registryPathForTest, "myKey", "valuueeee");

			item.SetLocalMachineValue(RegistryView.Default, registryPathForTest, "myKey", "newvalue");

			var value = item.GetValue(fullRegistryPathForTest, "myKey", null);
			Assert.That(value, Is.EqualTo("newvalue"));
		}

		[Test]
		public void TestDefaultKey()
		{
			var item = new WindowsRegistryProxy();
			item.SetLocalMachineValue(RegistryView.Default, registryPathForTest, "", "newvalue");

			var value = item.GetValue(fullRegistryPathForTest, "", null);
			Assert.That(value, Is.EqualTo("newvalue"));
		}
	}
}
