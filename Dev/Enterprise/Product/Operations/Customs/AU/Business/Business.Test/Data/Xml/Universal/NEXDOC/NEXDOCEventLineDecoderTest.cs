using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class NEXDOCEventLineDecoderTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			string incomingEvent = @"
<UniversalEvent>
	<Event>
		<ContextCollection>
			<Context>
				<Type>LineNumber</Type>
				<Value>4</Value>
				<SubContextCollection>
					<SubContext>
						<Type>HealthCertificateDescription</Type>
						<Value>DAIRY HEALTH CERTIFICATE</Value>
					</SubContext>
					<SubContext>
						<Type>PrimaryCertificateTemplateCode</Type>
						<Value>ZD035</Value>
					</SubContext>
					<SubContext>
						<Type>PrimaryCertificateEndorsementNumber</Type>
						<Value>455</Value>
					</SubContext>
					<SubContext>
						<Type>SecondaryCertificateTemplateCode</Type>
						<Value>ZD067</Value>
					</SubContext>
					<SubContext>
						<Type>SecondaryCertificateEndorsementNumber</Type>
						<Value>498</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			NewDecoderFromContext(incomingEvent, (NEXDOCEventLineDecoder decoder) =>
			{
				AssertEquals((ZShort)4, decoder.LineNumber);
				AssertEquals("DAIRY HEALTH CERTIFICATE", decoder.HealthCertificateDescription);
				AssertEquals("ZD035", decoder.PrimaryCertificateTemplateCode);
				AssertEquals("455", decoder.PrimaryCertificateEndorsementNumber);
				AssertEquals("ZD067", decoder.SecondaryCertificateTemplateCode);
				AssertEquals("498", decoder.SecondaryCertificateEndorsementNumber);
				AssertEquals(true, decoder.IsValid);
			});
		}

		public void TestIsValid()
		{
			string invalidLine = @"
<UniversalEvent>
	<Event>
		<ContextCollection>
			<Context>
				<Type>LineNumber</Type>
				<Value></Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			NewDecoderFromContext(invalidLine, (NEXDOCEventLineDecoder decoder) =>
			{
				AssertEquals(false, decoder.IsValid);
				AssertEquals((ZShort)0, decoder.LineNumber);
				AssertEquals("", decoder.HealthCertificateDescription);
			});

			string invalidLine2 = @"
<UniversalEvent>
	<Event>
		<ContextCollection>
			<Context>
				<Type>LineNumber</Type>
				<Value>2</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			NewDecoderFromContext(invalidLine2, (NEXDOCEventLineDecoder decoder) =>
			{
				AssertEquals(false, decoder.IsValid);
				AssertEquals((ZShort)2, decoder.LineNumber);
				AssertEquals("", decoder.HealthCertificateDescription);
			});

			string validLine = @"
<UniversalEvent>
	<Event>
		<ContextCollection>
			<Context>
				<Type>LineNumber</Type>
				<Value>4</Value>
				<SubContextCollection>
					<SubContext>
						<Type>HealthCertificateDescription</Type>
						<Value>DAIRY HEALTH CERTIFICATE</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			NewDecoderFromContext(validLine, (NEXDOCEventLineDecoder decoder) =>
			{
				AssertEquals(true, decoder.IsValid);
				AssertEquals((ZShort)4, decoder.LineNumber);
				AssertEquals("DAIRY HEALTH CERTIFICATE", decoder.HealthCertificateDescription);
			});
		}

		XmlEventDeserializer eventDeserializer;

		protected override void SetUp()
		{
			base.SetUp();
			eventDeserializer = new XmlEventDeserializer();
		}

		void NewDecoderFromContext(string contextXml, Action<NEXDOCEventLineDecoder> p)
		{
			using (var xmlEvent = (Event)eventDeserializer.Parse(contextXml))
			{
				var context = xmlEvent.ContextCollection.First();
				var decoder = new NEXDOCEventLineDecoder(context);
				p.Invoke(decoder);
			}
		}
	}
}
