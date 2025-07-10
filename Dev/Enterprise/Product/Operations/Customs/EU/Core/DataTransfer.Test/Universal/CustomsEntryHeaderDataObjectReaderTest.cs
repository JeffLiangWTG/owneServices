using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	sealed class CustomsEntryHeaderDataObjectReaderTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestEntryHeaderDataObjectReaderType()
		{
			var logger = new TestErrorLogger();
			var helper = new UniversalDataObjectReaderHelper(Factory, "IT", "IT");
			var reader = new CustomsEntryHeaderDataObjectReaderForTest(new UniversalCustoms.EntryHeader(), logger, helper, declaration, ZGuid.Empty);

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			AssertType<CustomsEntryNumberDataObjectReader>("EntryNumber reader type", reader.CreateCustomsEntryNumberDataObjectReaderExposed(
				new UniversalCustoms.EntryNumber(),
				logger,
				helper,
				cusEntryHeader));
		}

		public void TestPopulateEntryPayInfo()
		{
			var universalEntryHeader = new UniversalCustoms.EntryHeader
			{
				PaymentInformationCollection = new List<UniversalCustoms.EntryHeaderPaymentInformation>
				{
					new UniversalCustoms.EntryHeaderPaymentInformation
					{
						IncomingPaymentResponseNumber = "123",
						PaymentAmount = 2m,
						PaymentParty = new CodeDescriptionPair { Code = "A" },
						TransactionType = new CodeDescriptionPair { Code = "B" }
					},
					new UniversalCustoms.EntryHeaderPaymentInformation
					{
						IncomingPaymentResponseNumber = "456",
						PaymentAmount = 2m,
						PaymentParty = new CodeDescriptionPair { Code = "C" },
						TransactionType = new CodeDescriptionPair { Code = "D" }
					},
				}
			};

			var testLogger = new TestErrorLogger();
			var dataObjectReaderHelper = new UniversalDataObjectReaderHelper(Factory, "IT", "IT");
			var dataObjectReader = new CustomsEntryHeaderDataObjectReader(universalEntryHeader, testLogger, dataObjectReaderHelper, declaration, ZGuid.Empty);
			var entryHeaderBO = dataObjectReader.ReadIntoBusinessObject();
			AssertEquals("EntryPayInfo Items Count", 2, entryHeaderBO.EntryPayInfos.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;
	}

	sealed class CustomsEntryHeaderDataObjectReaderForTest : CustomsEntryHeaderDataObjectReader
	{
		public CustomsEntryHeaderDataObjectReaderForTest(
			UniversalCustoms.EntryHeader entryHeaderDataObject,
			IXmlImportLogger logger,
			UniversalDataObjectReaderHelper helper,
			BaseJobDeclaration declaration,
			ZGuid primeEntryPK,
			List<ZString> matchingKeys = null) : base(entryHeaderDataObject, logger, helper, declaration, primeEntryPK, matchingKeys)
		{
		}

		public CustomsEntryNumberDataObjectReader<Customs.Business.CusEntryHeader> CreateCustomsEntryNumberDataObjectReaderExposed(
			UniversalCustoms.EntryNumber entryNumberDataObject,
			IXmlImportLogger logger,
			UniversalDataObjectReaderHelper helper,
			Customs.Business.CusEntryHeader entryHeader) => CreateCustomsEntryNumberDataObjectReader(entryNumberDataObject, logger, helper, entryHeader);
	}
}
