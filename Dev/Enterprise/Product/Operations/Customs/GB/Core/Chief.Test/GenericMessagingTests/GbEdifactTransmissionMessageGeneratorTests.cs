using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.Chief.Testing
{
	class GbEdifactTransmissionMessageGeneratorTests : TestCaseWithFactory
	{
		public void TestGenerateMessagesOver56kIsNotAllowed()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			var ccsukGenerator = new GbEdifactTransmissionMessageGeneratorForTest(new Customs.Business.CusdecMessageFunction.New());
			var generator = ccsukGenerator as IMessageGenerator<CusEntryHeader>;
			GBCustomsDataRegistry.Instance.ChiefMaximumMessageSizeInDecimalBytes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 69);
			generator.Generate(entry);
			List<string> errors = ccsukGenerator.ErrorCollectorForTest.GetErrors().ToList();
			Assert("Error should be shown about size of payload, and this error should not start with 'Missing data for mandatory field'", errors[0].Contains("CHIEF supports messages up to 69 characters"));
		}
	}

	class GbEdifactTransmissionMessageGeneratorForTest : GbChiefEdifactTransmissionMessageGenerator
	{
		public GbEdifactTransmissionMessageGeneratorForTest(Customs.Business.CusdecMessageFunction declarationMessageFunction)
			: base(declarationMessageFunction)
		{ }

		internal ErrorCollector ErrorCollectorForTest { get { return base.errorCollector; } }

		protected override void GenerateMessageTextButDoNotSave(CusEntryHeader entryHeader)
		{
			generatedMessageText = "I am over 69 bytes .. blah  adijahd akjdh akjd asjkdaskd askjdhkajhdkjas dhjkash sakhdajdh akjsdh skajd hsak";
			errorCollector = new ErrorCollector();
		}

		protected override ZString GetApplicationReference(CusEntryHeader entryHeader)
		{
			return "XXX";
		}

		protected override ZString GetApplicationCode(Customs.Business.CusEntryHeader entry)
		{
			return "YYY";
		}
	}
}
