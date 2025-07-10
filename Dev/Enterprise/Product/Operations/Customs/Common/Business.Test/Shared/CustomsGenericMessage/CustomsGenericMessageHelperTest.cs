using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	class CustomsGenericMessageHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCreateGMDInterchange_EDIMessage()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = "TS1";
			message.EM_MessageType = "MT1";
			message.EM_MessageSubType = "MST";
			message.EM_ApplicationReference = "APPREF123";
			message.EM_MessageOwner = "MSGOWNER1";
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			message.EM_MessageText = @"
	<CustomsGenericMessage>
		<ApplicationCode>TS1</ApplicationCode>
		<Type>MT1</Type>
		<SubType>MST</SubType>
		<Owner>MSGOWNER1</Owner>
		<Reference>APPREF123</Reference>
		<Data><Greeting>HELLO</Greeting></Data>
	</CustomsGenericMessage>
";
			var interchange = CustomsGenericMessageHelper.CreateGMDInterchange(message, "ABCHELLO");
			NUnit.Framework.Assert.That(message.EM_EI, Is.EqualTo(interchange.PK), "message.EM_EI");
			AssertInterchange(interchange, "ABCHELLO", "TS1", @"
	<CustomsGenericMessage>
		<ApplicationCode>TS1</ApplicationCode>
		<Type>MT1</Type>
		<SubType>MST</SubType>
		<Owner>MSGOWNER1</Owner>
		<Reference>APPREF123</Reference>
		<Data><Greeting>HELLO</Greeting></Data>
	</CustomsGenericMessage>
");
			message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = "TS1";
			message.EM_MessageType = "MT1";
			message.EM_MessageSubType = "MST";
			message.EM_ApplicationReference = "APPREF123";
			message.EM_MessageOwner = "MSGOWNER1";
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			message.EM_MessageText = @"
	<CustomsGenericMessage>
		<ApplicationCode>TS1</ApplicationCode>
		<Type>MT1</Type>
		<SubType>MST</SubType>
		<Owner>MSGOWNER1</Owner>
		<Reference>APPREF123</Reference>
		<Data><Greeting>HELLO</Greeting></Data>
	</CustomsGenericMessage>
";
			interchange = CustomsGenericMessageHelper.CreateGMDInterchange(message, "ABCHELLO", true);
			NUnit.Framework.Assert.That(message.EM_EI, Is.EqualTo(interchange.PK), "message.EM_EI");
			AssertInterchange(interchange, "ABCHELLO", "TS1", @"
	&lt;CustomsGenericMessage&gt;
		&lt;ApplicationCode&gt;TS1&lt;/ApplicationCode&gt;
		&lt;Type&gt;MT1&lt;/Type&gt;
		&lt;SubType&gt;MST&lt;/SubType&gt;
		&lt;Owner&gt;MSGOWNER1&lt;/Owner&gt;
		&lt;Reference&gt;APPREF123&lt;/Reference&gt;
		&lt;Data&gt;&lt;Greeting&gt;HELLO&lt;/Greeting&gt;&lt;/Data&gt;
	&lt;/CustomsGenericMessage&gt;
");
		}

		[ExpectNoExceptions]
		public void TestCreateGMDInterchange_EDIMessageDiffBodyText()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = "TS1";
			message.EM_MessageType = "MT1";
			message.EM_MessageSubType = "MST";
			message.EM_ApplicationReference = "APPREF123";
			message.EM_MessageOwner = "MSGOWNER1";
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			message.EM_MessageText = "<Greeting>HELLO</Greeting>";
			var interchange = CustomsGenericMessageHelper.CreateGMDInterchange(message, "ABCHELLO", GenerateCGMFormat(message));
			NUnit.Framework.Assert.That(message.EM_EI, Is.EqualTo(interchange.PK), "message.EM_EI");
			AssertInterchange(interchange, "ABCHELLO", "TS1", @"
	<CustomsGenericMessage>
		<ApplicationCode>TS1</ApplicationCode>
		<Type>MT1</Type>
		<SubType>MST</SubType>
		<Owner>MSGOWNER1</Owner>
		<Reference>APPREF123</Reference>
		<Data><Greeting>HELLO</Greeting></Data>
	</CustomsGenericMessage>
");
			message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = "TS1";
			message.EM_MessageType = "MT1";
			message.EM_MessageSubType = "MST";
			message.EM_ApplicationReference = "APPREF123";
			message.EM_MessageOwner = "MSGOWNER1";
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			message.EM_MessageText = "<Greeting>HELLO</Greeting>";
			interchange = CustomsGenericMessageHelper.CreateGMDInterchange(message, "ABCHELLO", GenerateCGMFormat(message), true);
			NUnit.Framework.Assert.That(message.EM_EI, Is.EqualTo(interchange.PK), "message.EM_EI");
			AssertInterchange(interchange, "ABCHELLO", "TS1", @"
	&lt;CustomsGenericMessage&gt;
		&lt;ApplicationCode&gt;TS1&lt;/ApplicationCode&gt;
		&lt;Type&gt;MT1&lt;/Type&gt;
		&lt;SubType&gt;MST&lt;/SubType&gt;
		&lt;Owner&gt;MSGOWNER1&lt;/Owner&gt;
		&lt;Reference&gt;APPREF123&lt;/Reference&gt;
		&lt;Data&gt;&lt;Greeting&gt;HELLO&lt;/Greeting&gt;&lt;/Data&gt;
	&lt;/CustomsGenericMessage&gt;
");
		}

		static ZString GenerateCGMFormat(EDIMessage message)
		{
			return FormattableString.Invariant($@"
	<CustomsGenericMessage>
		<ApplicationCode>{message.EM_ApplicationCode}</ApplicationCode>
		<Type>{message.EM_MessageType}</Type>
		<SubType>{message.EM_MessageSubType}</SubType>
		<Owner>{message.EM_MessageOwner}</Owner>
		<Reference>{message.EM_ApplicationReference}</Reference>
		<Data>{message.EM_MessageText}</Data>
	</CustomsGenericMessage>
");
		}

		[ExpectNoExceptions]
		public void TestCreatRefDbRepoMessage()
		{
			var recipient = Environment.Env.Instance.IsProductionSystem ? "CUSTOMS_DATA_REPO" : "CUSTOMS_DATA_REPO_TEST";

			var interchange = CustomsGenericMessageHelper.CreatRefDbRepoMessage(Factory, "123", "456", "789", "000 > 1");
			AssertInterchange(interchange, recipient, "RDM", @"<RefDbRepoMessage><Source>123</Source><SubSource>456</SubSource><ContentType>789</ContentType><Data>000 > 1</Data></RefDbRepoMessage>");

			interchange = CustomsGenericMessageHelper.CreatRefDbRepoMessage(Factory, "123", "456", "789", "000 > 1", true);
			AssertInterchange(interchange, recipient, "RDM", @"<RefDbRepoMessage><Source>123</Source><SubSource>456</SubSource><ContentType>789</ContentType><Data>000 &gt; 1</Data></RefDbRepoMessage>");
		}

		[ExpectNoExceptions]
		public void TestCreateGMDInterchange_FreeText()
		{
			var interchange = CustomsGenericMessageHelper.CreateGMDInterchange(Factory, "ABCHELLO", "TST", "<Greeting>HELLO</Greeting>");
			AssertInterchange(interchange, "ABCHELLO", "TST", "<Greeting>HELLO</Greeting>");
			interchange = CustomsGenericMessageHelper.CreateGMDInterchange(Factory, "ABCHELLO", "TST", "<Greeting>HELLO</Greeting>", true);
			AssertInterchange(interchange, "ABCHELLO", "TST", "&lt;Greeting&gt;HELLO&lt;/Greeting&gt;");
		}

		[ExpectNoExceptions]
		public void TestPopulateGMDInterchange()
		{
			var message = EdiMessageForTesting;
			var interchange = Factory.New<EDIInterchange>();
			CustomsGenericMessageHelper.PopulateGMDInterchange(interchange, "ABCHELLO", "TS1", "<GREETING>HI</GREETING>");
			AssertInterchange(interchange, "ABCHELLO", "TS1", "<GREETING>HI</GREETING>");
			CustomsGenericMessageHelper.PopulateGMDInterchange(interchange, "ABCHELLO", "TS1", "<GREETING>HI</GREETING>", true);
			AssertInterchange(interchange, "ABCHELLO", "TS1", "&lt;GREETING&gt;HI&lt;/GREETING&gt;");
		}

		[ExpectNoExceptions]
		void AssertInterchange(EDIInterchange interchange, ZString recipient, ZString interchangeType, ZString bodyText)
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(interchange.EI_ApplicationCode, Is.EqualTo(EDIInterchange.ApplicationCodes.GenericMessageDelivery).Using(CustomComparers.TypeComparison), "interchange.EI_ApplicationCode");
				NUnit.Framework.Assert.That(interchange.EI_InterchangeType, Is.EqualTo(interchangeType), "interchange.EI_InterchangeType");
				NUnit.Framework.Assert.That(interchange.EI_ReceiveTransmit, Is.EqualTo(EDIInterchange.Direction.Transmit).Using(CustomComparers.TypeComparison), "interchange.EI_ReceiveTransmit");
				NUnit.Framework.Assert.That(interchange.EI_From, Is.EqualTo(GlbCompany.CurrentCompany.LicenceKeyIdentifier), "interchange.EI_From");
				NUnit.Framework.Assert.That(interchange.EI_To, Is.EqualTo(recipient), "interchange.EI_To");
				NUnit.Framework.Assert.That(interchange.EI_Status, Is.EqualTo(Messaging.Integration.EDIInterchangeStatusList.Codes.eHubQueued).Using(CustomComparers.TypeComparison), "interchange.EI_Status");
				NUnit.Framework.Assert.That(interchange.EI_BodyText, CustomConstraints.MultilineASCIIEquals(bodyText), "interchange.EI_BodyText");
				NUnit.Framework.Assert.That(interchange.EI_SessionGUID, Is.Not.EqualTo(ZGuid.Empty), "interchange.EI_SessionGUID");
			});
		}

		EDIMessage EdiMessageForTesting
		{
			get
			{
				if (ediMessageForTesting == null)
				{
					ediMessageForTesting = Factory.New<EDIMessage>();
					ediMessageForTesting.EM_ApplicationCode = "TS1";
					ediMessageForTesting.EM_MessageType = "MT1";
					ediMessageForTesting.EM_MessageSubType = "MST";
					ediMessageForTesting.EM_ApplicationReference = "APPREF123";
					ediMessageForTesting.EM_MessageOwner = "MSGOWNER1";
					ediMessageForTesting.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				}
				return ediMessageForTesting;
			}
		}
		EDIMessage ediMessageForTesting;
	}
}
