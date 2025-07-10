using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Business.Testing
{
	class GbTransmissionMessageGeneratorTest : TestCaseWithFactory
	{
		public void TestPlaceholders()
		{
			IMessageGenerator<CusEntryHeader> generator = new GbTransmissionMessageGeneratorForTest();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "Hello";
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var message = entry.Messages.AddNew();
			var requestedMessageTextWithPlaceholders = "SYSCAR: <<SYSCAR>>; Dec Ref: <<DECRF>>;   Box Seven: <<BOX7>>.";
			generator.PutReferenceNumberIntoMessageFromPlaceholder(message, requestedMessageTextWithPlaceholders, entry);
			AssertMatch(new Regex("Dec Ref: Hello;   Box Seven: HELLO."), message.EM_MessageText);
			declaration.JE_OwnerRef = "Goodbye";
			generator.PutReferenceNumberIntoMessageFromPlaceholder(message, requestedMessageTextWithPlaceholders, entry);
			AssertMatch(new Regex("Dec Ref: Hello;   Box Seven: GOODBYE."), message.EM_MessageText);
			var withSyscar = GbTransmissionMessageGenerator.PutBizoPkIntoSysCarPlaceholder(requestedMessageTextWithPlaceholders, entry);
			AssertMatch(new Regex("SYSCAR: [A-F0-9]{32};"), withSyscar);
		}

		public void TestAfterFullSuccess_PutBizoPkIntoSysCarPlaceholder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "Hello";
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var message = Factory.New<EDIMessage>();
			var builderRes = new BuilderResult(entry, Enumerable.Empty<string>(), result => { })
			{
				Message = message
			};
			var generator = new GbTransmissionMessageGeneratorForTest();
			const string requestedMessageTextWithPlaceholders = "SYSCAR: <<SYSCAR>>; Dec Ref: <<DECRF>>;   Box Seven: <<BOX7>>.";
			generator.PutReferenceNumberIntoMessageFromPlaceholder(message, requestedMessageTextWithPlaceholders, entry);
			generator.AfterFullSuccess(builderRes);

			var msg = entry.Messages[0];
			var withSyscar = GbTransmissionMessageGenerator.PutBizoPkIntoSysCarPlaceholder(requestedMessageTextWithPlaceholders, entry);
			AssertMatch(new Regex("SYSCAR: [A-F0-9]{32};"), withSyscar);
			AssertContains(GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(entry), msg.EM_MessageText);
		}

		public void TestBox7ReferenceCleanup()
		{
			IMessageGenerator<CusEntryHeader> generator = new GbTransmissionMessageGeneratorForTest();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "!Sp3cial'Chars@Are.Removed[^]";
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var message = entry.Messages.AddNew();
			var requestedMessageTextWithPlaceholders = "Dec Ref: <<DECRF>>;   Box Seven: <<BOX7>>";
			generator.PutReferenceNumberIntoMessageFromPlaceholder(message, requestedMessageTextWithPlaceholders, entry);
			AssertEquals("Dec Ref: !Sp3cial'Chars@Are.Removed[^];   Box Seven: SP3CIALCHARSAREREMOVED", message.EM_MessageText);
		}
	}

	class GbTransmissionMessageGeneratorForTest : GbTransmissionMessageGenerator
	{
		public override ZString MakePrettyForInterpretation(EDIMessage message)
		{
			throw new NotSupportedException("Don't call this in test");
		}

		public override Enterprise.Messaging.Business.MessageBuilders.IBuilderResult Generate(CusEntryHeader entryHeader)
		{
			throw new NotSupportedException("Don't call this in test");
		}
	}
}
