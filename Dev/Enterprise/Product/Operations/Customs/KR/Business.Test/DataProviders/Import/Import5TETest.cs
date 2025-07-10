using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class Import5TETest : XMLMessageTestHelper<Import5TETest>
	{
		[TestDate(2021, 03, 26)]
		public void TestFullData()
		{
			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "", "");
			payer.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "5808500065", Core.Constants.CountryCodes.KoreaSouth);
			payer.CustomsCodes.AddNew(IdentificationType.KoreanRegNoForResident, "5808500123", Core.Constants.CountryCodes.KoreaSouth);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaidBy = ZString.Empty;
			declaration.JE_OH_DutyPayer = payer.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "4207218022173M";

			entry.EntryNumbers.AddNew().CE_EntryType = ElectronicDocumentTypeList.Codes._5TE;

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(entry.RegistryCompanyPK, Guid.Empty, Guid.Empty, "42072");

			var import5TE = new Import5TECreator().Create(entry, "정정으로 인한 동기화신청");

			var result = new GOVCBR5TEMessageBuilder(import5TE).GenerateMessage();
			var fileReader = new TestFileReader(typeof(Import5TETest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5TE_D1.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
