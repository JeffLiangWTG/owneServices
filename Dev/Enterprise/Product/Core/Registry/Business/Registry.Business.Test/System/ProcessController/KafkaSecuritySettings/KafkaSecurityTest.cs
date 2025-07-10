using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(KafkaSecurity))]
	sealed class KafkaSecurityTest : RegistryBusinessObjectTemplateTestCase<KafkaSecurity>
	{
		public void TestTimeIntervalReadOnlyness()
		{
			KafkaSecurity.SecurityProtocol = KafkaSecurityProtocolOptions.PLAINTEXT;
			AssertEquals(true, KafkaSecurity.SslCaLocationInfo.ReadOnly);
			AssertEquals(true, KafkaSecurity.SaslUsernameInfo.ReadOnly);
			AssertEquals(true, KafkaSecurity.SaslPasswordInfo.ReadOnly);

			KafkaSecurity.SecurityProtocol = KafkaSecurityProtocolOptions.SSL;
			AssertEquals(false, KafkaSecurity.SslCaLocationInfo.ReadOnly);
			AssertEquals(true, KafkaSecurity.SaslUsernameInfo.ReadOnly);
			AssertEquals(true, KafkaSecurity.SaslPasswordInfo.ReadOnly);

			KafkaSecurity.SecurityProtocol = KafkaSecurityProtocolOptions.SASL_PLAINTEXT;
			AssertEquals(true, KafkaSecurity.SslCaLocationInfo.ReadOnly);
			AssertEquals(false, KafkaSecurity.SaslUsernameInfo.ReadOnly);
			AssertEquals(false, KafkaSecurity.SaslPasswordInfo.ReadOnly);

			KafkaSecurity.SecurityProtocol = KafkaSecurityProtocolOptions.SASL_SSL;
			AssertEquals(false, KafkaSecurity.SslCaLocationInfo.ReadOnly);
			AssertEquals(false, KafkaSecurity.SaslUsernameInfo.ReadOnly);
			AssertEquals(false, KafkaSecurity.SaslPasswordInfo.ReadOnly);
		}

		public void TestProtocolValidation()
		{
			KafkaSecurity.SecurityProtocol = "";
			AssertHasError(KafkaSecurity.SecurityProtocolInfo, "Please enter a value.");

			KafkaSecurity.SecurityProtocol = "Invalid";
			AssertHasError(KafkaSecurity.SecurityProtocolInfo, "Enter a valid selection.");
		}

		public void TestSaslValidation()
		{
			KafkaSecurity.SecurityProtocol = KafkaSecurityProtocolOptions.SASL_SSL;
			KafkaSecurity.SaslUsername = string.Empty;
			KafkaSecurity.SaslPassword = string.Empty;
			AssertHasErrors("SASL Username can not be null.", KafkaSecurity.SaslUsernameInfo);
			AssertHasErrors("SASL Password can not be null.", KafkaSecurity.SaslPasswordInfo);

			KafkaSecurity.SecurityProtocol = KafkaSecurityProtocolOptions.SASL_PLAINTEXT;
			KafkaSecurity.SaslUsername = string.Empty;
			KafkaSecurity.SaslPassword = string.Empty;
			AssertHasErrors("SASL Username can not be null.", KafkaSecurity.SaslUsernameInfo);
			AssertHasErrors("SASL Password can not be null.", KafkaSecurity.SaslPasswordInfo);
		}

		KafkaSecurity KafkaSecurity
		{
			get { return kafkaSecurity ?? (kafkaSecurity = new KafkaSecurity(Factory)); }
		}
		KafkaSecurity kafkaSecurity;

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override KafkaSecurity GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override KafkaSecurity GetBusinessObjectToSerialise()
		{
			return new KafkaSecurity(Factory)
			{
				SecurityProtocol = KafkaSecurityProtocolOptions.SASL_SSL,
				SaslUsername = "user",
				SaslPassword = "letmein"
			};
		}
	}
}
