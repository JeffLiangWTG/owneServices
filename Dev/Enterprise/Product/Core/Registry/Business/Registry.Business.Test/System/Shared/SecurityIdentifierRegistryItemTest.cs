using System;
using System.Security.Principal;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing;

[TestedType(typeof(SecurityIdentifierRegistryItem))]
sealed class SecurityIdentifierRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
{
	static readonly SecurityIdentifier defaultValue = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);

	protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
	{
		var registryItem = new SecurityIdentifierRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.Default, defaultValue);
		return registryItem;
	}

	public void TestSecurityIdentifier_DefaultValue()
	{
		var registryItem = GetNewRegistryItem() as SecurityIdentifierRegistryItem;
		AssertEquals(defaultValue, registryItem?.SecurityIdentifier);
	}

	public void TestSetValue_ValidSid()
	{
		var expectedSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
		var registryItem = GetNewRegistryItem() as SecurityIdentifierRegistryItem;
		registryItem?.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Everyone");
		AssertEquals(expectedSid, registryItem?.SecurityIdentifier);
	}

	public void TestSetValue_InvalidSid()
	{
		var registryItem = GetNewRegistryItem() as SecurityIdentifierRegistryItem;
		var e = AssertExceptionThrown<RegistryValidationException>(() => registryItem?.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "InvalidSid"));
		AssertEquals("AD name 'InvalidSid' not found.", e.Message);
		AssertEquals(defaultValue, registryItem?.SecurityIdentifier);
	}

	public void TestSetValue_Null()
	{
		var registryItem = GetNewRegistryItem() as SecurityIdentifierRegistryItem;
		registryItem?.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
		AssertEquals(defaultValue, registryItem?.SecurityIdentifier);
	}
}
