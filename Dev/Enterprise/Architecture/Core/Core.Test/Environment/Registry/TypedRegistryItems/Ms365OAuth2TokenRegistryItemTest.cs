using System;
using Enterprise.Integration;
using Enterprise.MailManager.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(Ms365OAuth2TokenRegistryItem))]
	sealed class Ms365OAuth2TokenRegistryItemTest : StronglyTypedRegistryItemTestCase<Ms365OAuth2Token>
	{
		protected override StronglyTypedRegistryItem<Ms365OAuth2Token, Ms365OAuth2Token> GetNewRegistryItem()
		{
			return new Ms365OAuth2TokenRegistryItem("", null, null, null, EmailType.Incoming, null, null, null, RegistryStorageFlags.All, RegistryOptions.Default);
		}

		protected override Ms365OAuth2Token ValidValue
		{
			get => new Ms365OAuth2Token { Identifier = "Id", Token = new byte[] { 1, 2 }, User = "email@test.com" };
		}

		public void TestConstructorSetPropertiess()
		{
			var ms365OAuth2TenantId = new StringRegistryItem("ms365OAuth2TenantId", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, Guid.NewGuid().ToString());
			var ms365ApplicationIdForIncoming = new StringRegistryItem("ms365ApplicationIdForIncoming", null, null, null, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue, Guid.NewGuid().ToString());
			var useGraphApiForIncoming = new BooleanRegistryItem("useGraphApiForIncoming", null, null, null, RegistryStorageFlags.All, true);

			var ms365OAuth2TokenRegistryItem = new Ms365OAuth2TokenRegistryItem("Ms365OAuth2TokenRegistryItem", null, null, null, EmailType.Outgoing, ms365OAuth2TenantId, ms365ApplicationIdForIncoming, useGraphApiForIncoming, RegistryStorageFlags.All, RegistryOptions.Default);

			AssertEquals(EmailType.Outgoing, ms365OAuth2TokenRegistryItem.EmailType);
			AssertEquals(ms365OAuth2TenantId, ms365OAuth2TokenRegistryItem.Ms365OAuth2TenantId);
			AssertEquals(ms365ApplicationIdForIncoming, ms365OAuth2TokenRegistryItem.Ms365ApplicationIdForIncoming);
			AssertEquals(useGraphApiForIncoming, ms365OAuth2TokenRegistryItem.UseGraphApiForIncoming);
		}
	}
}
