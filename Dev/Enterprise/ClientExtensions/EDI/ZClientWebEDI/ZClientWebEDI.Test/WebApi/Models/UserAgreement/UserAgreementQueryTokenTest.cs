using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(UserAgreementQueryToken))]
	public class UserAgreementQueryTokenTest : TestCaseWithFactory
	{
		public void TestFromJson()
		{
			var contactGuid = ZGuid.NewZGuid();
			var databaseGuid = ZGuid.NewZGuid();
			var jsonStringWithInValidID = $"{{\"Source\": \"Campaign\",\"Type\": \"VER\",\"FromContact\": \"{contactGuid}\",\"Enterprise\": \"123123123123\",\"Database\": \"{databaseGuid}\",\"AgreementType\": \"CWN\",\"RecipientName\": \"RN\",\"RecipientJobTitle\": \"RJ\",\"RecipientEmail\": \"RE\",\"SendAgreementCopy\": true}}";
			var token = UserAgreementQueryToken.FromJson(jsonStringWithInValidID);
			AssertEquals("Campaign", token.Source);
			AssertEquals("VER", token.Type);
			AssertEquals(contactGuid, token.FromContact);
			AssertEquals(databaseGuid, token.Database);

			AssertEquals("CWN", token.AgreementType);
			AssertEquals("RN", token.RecipientName);
			AssertEquals("RE", token.RecipientEmail);
			AssertEquals("RJ", token.RecipientJobTitle);

			Assert(token.Enterprise.IsEmpty);
			Assert(token.SendAgreementCopy);

			AssertNoExceptionThrown(() =>
			{
				var token2 = UserAgreementQueryToken.FromJson("g&*R(*	8I&^$*&j6t8&^HT87o6*TE^H(*ET6");
				AssertNull(token2);
			});
		}
	}
}
