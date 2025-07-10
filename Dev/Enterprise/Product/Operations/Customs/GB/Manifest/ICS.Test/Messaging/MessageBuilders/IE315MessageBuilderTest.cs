using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.BrandManager;
using CargoWise.Customs.GB.MessageDefinitions.ICS.HMRC_ICS_IE315;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.ICS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.Messaging.IE315.Testing
{
	public class IE315MessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2015, 8, 22, 14, 0, 0, 123)]
		public void TestBuildIE315Message()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var wrapper = new DeclarationWrapper(manifest);
			var builder = new IE315MessageBuilder(wrapper);
			var buildResult = builder.Build();

			var outputMessageObjectPropertyInfo = builder.GetType()
				.GetProperty("OutputMessageObject", BindingFlags.Instance | BindingFlags.NonPublic);
			var outputMessageObject = (Envelope)(outputMessageObjectPropertyInfo.GetValue(builder));
			var submissionTimestamp = outputMessageObject.Header.Info.SubmissionTimestampValue.ToString("yyyy-MM-ddTHH:mm:ss.fffK");

			var expected = IgnoreBreaksAndIndentations($@"
<Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:ie=""http://ics.dgtaxud.ec/CC315A"" xmlns=""http://www.w3.org/2003/05/soap-envelope"">
	<Header>
		<Info xmlns=""http://www.hmrc.gov.uk/ws/info-header/1"">
			<VendorName URI=""http://www.wisetechglobal.com"">WiseTech Global</VendorName>
			<VendorID>1601</VendorID>
			<VendorProduct Version=""2.0.0.0"">{BrandingFactory.Instance.ProductName}</VendorProduct>
			<ServiceID>1138</ServiceID>
			<ServiceMessageType>HMRC-ICS-IE315-DIRECT</ServiceMessageType>
			<SubmissionTimestamp>{submissionTimestamp}</SubmissionTimestamp>
		</Info>
		<Security>
			<BinarySecurityToken ValueType=""http://www.hmrc.gov.uk#MarkToken"">BinarySecurityToken</BinarySecurityToken>
		</Security>
	</Header>
	<Body>
		<CC315A>
			<ie:MesSenMES3>/00000000</ie:MesSenMES3>
			<ie:MesRecMES6>ICS</ie:MesRecMES6>
			<ie:TesIndMES18>1</ie:TesIndMES18>
			<ie:MesTypMES20>CC315A</ie:MesTypMES20>
			<ie:HEAHEA>
				<ie:RefNumHEA4>C123456</ie:RefNumHEA4>
				<ie:TotNumOfIteHEA305>0</ie:TotNumOfIteHEA305>
				<ie:TotNumOfPacHEA306>0</ie:TotNumOfPacHEA306>
				<ie:TotGroMasHEA307>0</ie:TotGroMasHEA307>
				<ie:DecPlaHEA394>UNIT 3, 480 NUDGEE ROAD</ie:DecPlaHEA394>
				<ie:PlaUnlGOOITE334>ERXXX</ie:PlaUnlGOOITE334>
			</ie:HEAHEA>
			<ie:PERLODSUMDEC>
				<ie:NamPLD1>{BrandingFactory.Instance.ProductSupportName}</ie:NamPLD1>
				<ie:PosCodPLD1>2015</ie:PosCodPLD1>
				<ie:CitPLD1>Alexandria</ie:CitPLD1>
			</ie:PERLODSUMDEC>
		</CC315A>
	</Body>
</Envelope>");

			AssertXMLEquals(IgnoreBreaksAndIndentations(expected), IgnoreBreaksAndIndentations(buildResult));
		}

		string IgnoreBreaksAndIndentations(string input)
		{
			return Regex.Replace(input, @"[\r\n]+\s*", string.Empty);
		}
	}
}
