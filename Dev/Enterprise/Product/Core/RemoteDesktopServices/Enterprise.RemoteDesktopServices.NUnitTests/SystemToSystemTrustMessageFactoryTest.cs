using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using Enterprise.RemoteDesktopServices.Server;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	[Serializable]
	class SystemToSystemTrustMessageFactoryTest
	{
		[Test]
		public void TestDefaultClaims()
		{
			var clientId = Guid.NewGuid();
			var audience = Guid.NewGuid();
			var message = SystemToSystemTrustMessageFactory.Create(RSA.Create(2048), clientId, audience, new Uri("https://127.0.0.1"));
			var token = new JwtSecurityToken(message.AccessToken);
			Assert.That(token.Claims.Select(x => x.Type), Is.EquivalentTo(new[] { "jti", "sub", "iss", "aud", "exp", "iat", "nbf", }));
			Assert.That($"{clientId}", Is.EqualTo(token.Claims.First(x => x.Type == "sub").Value));
			Assert.That($"{clientId}", Is.EqualTo(token.Claims.First(x => x.Type == "iss").Value));
			Assert.That($"{audience}", Is.EqualTo(token.Claims.First(x => x.Type == "aud").Value));
		}

		[Test]
		public void TestCustomJson()
		{
			string jsonString = @"
{
  ""name"": ""MadameUppercut"",
  ""age"": ""39"",
  ""secretIdentity"": ""JaneWilson"",
  ""powers"": [
    ""Milliontonnepunch"",
    ""Damageresistance"",
    {
      ""powerName"": ""Superhumanreflexes"",
      ""powerLevel"": 100,
      ""details"": {
        ""origin"": ""Genetic Mutation"",
        ""duration"": ""Permanent""
      }
    }
  ]
}";
			var payload = JwtPayload.Deserialize(jsonString);
			var message = SystemToSystemTrustMessageFactory.Create(RSA.Create(2048), Guid.NewGuid(), Guid.NewGuid(), new Uri("https://127.0.0.1"), payload);
			var token = new JwtSecurityToken(message.AccessToken);
			Assert.That(token.Claims.Select(x => x.Type).ToList(), Does.Contain("sub"));
			Assert.That(token.Claims.Select(x => x.Type).ToList(), Does.Contain("name"));
			var tokenJson = token.Payload.SerializeToJson();
			var normalizedSourceJsonString = NormalizeJson(jsonString);
			// we expect the json passed as input to be in the token. We will remove the first character '{' since it will be appended to existing json
			Assert.That(tokenJson, Does.Contain(normalizedSourceJsonString.Substring(1)));
		}

		class A
		{
			public string stringData = "some string";
			public float someInt = 42;
			public List<B> bList = new List<B> { new B(), new B() { intData = 6 } };
		}

		class B
		{
			public int intData = 5;
			public string OtherString { get; } = "other string";
		}

		[Test]
		public void TestCustomJson_DemonstrateObjectSerialization()
		{
			var a = new A();
			var jsonString = JsonSerializer.Serialize(a, new JsonSerializerOptions { IncludeFields = true });
			var payload = JwtPayload.Deserialize(jsonString);
			var message = SystemToSystemTrustMessageFactory.Create(RSA.Create(2048), Guid.NewGuid(), Guid.NewGuid(), new Uri("https://127.0.0.1"), payload);
			var token = new JwtSecurityToken(message.AccessToken);
			var tokenJson = Base64UrlEncoder.Decode(token.RawPayload);
			var normalizedSourceJsonString = NormalizeJson(jsonString);
			// we expect the json passed as input to be in the token. We will remove the first character '{' since it will be appended to existing json
			Assert.That(tokenJson, Does.Contain(normalizedSourceJsonString.Substring(1)));
		}

		[Test]
		public void TestCustomJsonContainsCriticalClaims()
		{
			string jsonString = @"
{
  ""sub"": ""123456"",
  ""name"": ""MadameUppercut""
}";
			var payload = JwtPayload.Deserialize(jsonString);
			var message = SystemToSystemTrustMessageFactory.Create(RSA.Create(2048), Guid.NewGuid(), Guid.NewGuid(), new Uri("https://127.0.0.1"), payload);
			var token = new JwtSecurityToken(message.AccessToken);
			Assert.That(token.Claims.Select(x => x.Type).ToList(), Does.Contain("sub"));
			Assert.That(token.Claims.First(x => x.Type == "sub").Value, Is.Not.EqualTo("123456"));
			Assert.That(token.Claims.Select(x => x.Type).ToList(), Does.Contain("name"));
			Assert.That(token.Claims.First(x => x.Type == "name").Value, Is.EqualTo("MadameUppercut"));
		}

		string NormalizeJson(string jsonString)
		{
			// serialize and deserialize to remove carriage return and tabs
			return JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(jsonString));
		}
	}
}
