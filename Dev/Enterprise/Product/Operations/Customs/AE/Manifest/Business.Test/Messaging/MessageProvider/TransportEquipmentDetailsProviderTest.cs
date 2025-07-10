using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class TransportEquipmentDetailsProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentDetailsProvider>
{
	public void TestConstructor()
	{
		NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new TransportEquipmentDetailsProvider(null), "Container is null");
		AssertNoExceptionThrown("Container is valid", () => new TransportEquipmentDetailsProvider(container));
	}

	[ExpectNoExceptions]
	public void TestEquipmentIdentifier()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(GetProvider().EquipmentIdentifier, Is.Null.Or.Empty, "Identifier not set - should be [null] or [empty]");

			container.ACN_ContainerNumber = "ABC123";
			NUnit.Framework.Assert.That(GetProvider().EquipmentIdentifier, Is.EqualTo("ABC123"), "Identifier set");
		});
	}

	[ExpectNoExceptions]
	public void TestEquipmentType()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(GetProvider().EquipmentType, Is.Null.Or.Empty, "Container type not set - should be [null] or [empty]");

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "20FR";
			container.ACN_RC_ContainerType = refContainer.PK;
			NUnit.Framework.Assert.That(GetProvider().EquipmentType, Is.EqualTo("20FR"), "Container type set");
		});
	}

	[ExpectNoExceptions]
	public void TestEquipmentIndicator()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(GetProvider().EquipmentIndicator, Is.Null.Or.Empty, "Empty/Full not set - should be [null] or [empty]");
			void VerifyEquipmentIndicator(string indicator, string expectedCode)
			{
				container.ACN_EmptyFullIndicator = indicator;
				var provider = GetProvider();
				NUnit.Framework.Assert.That(provider.EquipmentIndicator, Is.EqualTo(expectedCode), $"EquipmentIndicator for {indicator}");
			}
			VerifyEquipmentIndicator("FCL", "5");
			VerifyEquipmentIndicator("MT", "4");
			VerifyEquipmentIndicator("LCL", "7");
		});
	}

	protected override TransportEquipmentDetailsProvider GetProvider() => new TransportEquipmentDetailsProvider(container);

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<AsycudaManifestHeader>();
		container = header.Containers.AddNew();
	}
	AsycudaContainer container;
}
