using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.IT.DataTransfer.Universal.Testing;

sealed class CustomsEntryHeaderDataObjectReaderTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
{
	public void TestCustomsChannelMapping()
	{
		var expectedCustomsChannelEntryHeaderAddInfo = string.Format("*{0}=IT", CusEntryHeader.Schema.CustomsChannel);
		var entryHeaderDataObject = SetupEntryHeader(
			"EX$",
			"MS2",
			"ES3",
			"BG32423",
			1034.43m,
			new ZDateTime(2012, 3, 4),
			new ZDateTime(2012, 3, 5),
			AddInfoCollectionCreator.CreateCollection(expectedCustomsChannelEntryHeaderAddInfo));
		var entryHeaderBO = (CusEntryHeader)new CustomsEntryHeaderDataObjectReader(
			entryHeaderDataObject,
			new TestErrorLogger(),
			new UniversalDataObjectReaderHelper(Factory, "IT", "IT"),
			declaration,
			ZGuid.Empty).ReadIntoBusinessObject();
		AssertEquals("entryHeaderBO.CustomsChannel", "IT", entryHeaderBO.CustomsChannel.ToString());
	}

	public void TestEntryHeaderDataObjectReaderType()
	{
		var logger = new TestErrorLogger();
		var helper = new UniversalDataObjectReaderHelper(Factory, "IT", "IT");
		var reader = new CustomsEntryHeaderDataObjectReaderForTest(
			new UniversalCustoms.EntryHeader(),
			logger,
			helper,
			declaration,
			ZGuid.Empty);
		var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

		AssertType<CustomsEntryNumberDataObjectReader>("EntryNumber reader type", reader.CreateCustomsEntryNumberDataObjectReaderExposed(
			new UniversalCustoms.EntryNumber(),
			logger,
			helper,
			cusEntryHeader));
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	JobDeclaration declaration;

	UniversalCustoms.EntryHeader SetupEntryHeader(
		ZString type,
		ZString messageStatus,
		ZString entryStatus,
		ZString? reference,
		ZDecimal? totalAmountPaid,
		ZDateTime? entrySubmittedDate,
		ZDateTime? entryReleaseDate,
		List<AddInfo> addInfoCollection = null)
	{
		return SetupEntryHeader(
			new EntryType() { Code = type, Description = type + " DESC" },
			new CodeDescriptionPair() { Code = messageStatus, Description = messageStatus + " DESC" },
			new EntryStatus() { Code = entryStatus, Description = entryStatus + " DESC" },
			reference,
			totalAmountPaid,
			entrySubmittedDate,
			entryReleaseDate,
			addInfoCollection);
	}

	UniversalCustoms.EntryHeader SetupEntryHeader(
		EntryType type,
		CodeDescriptionPair messageStatus,
		EntryStatus entryStatus,
		ZString? reference,
		ZDecimal? totalAmountPaid,
		ZDateTime? entrySubmittedDate,
		ZDateTime? entryReleaseDate,
		List<AddInfo> addInfoCollection = null)
	{
		return new UniversalCustoms.EntryHeader()
		{
			Type = type,
			MessageStatus = messageStatus,
			EntryStatus = entryStatus,
			Reference = reference,
			TotalAmountPaid = totalAmountPaid,
			EntrySubmittedDate = entrySubmittedDate,
			EntryReleaseDate = entryReleaseDate,
			AddInfoCollection = addInfoCollection
		};
	}
}

class CustomsEntryHeaderDataObjectReaderForTest : CustomsEntryHeaderDataObjectReader
{
	public CustomsEntryHeaderDataObjectReaderForTest(
		UniversalCustoms.EntryHeader entryHeaderDataObject,
		IXmlImportLogger logger,
		UniversalDataObjectReaderHelper helper,
		Customs.Business.BaseJobDeclaration declaration,
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
