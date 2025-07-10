using CargoWise.Types;
using Enterprise.Customs.EU.DataTransfer.Universal;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.IT.DataTransfer.Universal.Testing;

sealed class CustomsEntryHeaderDataObjectWriterTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
{
	public void TestGetEntryHeaderAddInfoCollection_ForTotalInnerPackages()
	{
		entryHeader.CustomsChannel = "IT";
		var result = cusEntryHeaderDataObjectWriter.GetDataObject(entryHeader);
		AssertEquals("CustomsChannel value is expected to be same in AddInfoCollection with the value in CusEntryHeader.", "IT", result.AddInfoCollection.GetZStringValue(new ZString("CustomsChannel")));
	}

	public void TestCustomsEntryNumberDataObjectWriterType()
	{
		AssertType<CustomsEntryNumberDataObjectWriter>("EntryNumber writer type", cusEntryHeaderDataObjectWriter.GetNewCustomsEntryNumberDataObjectWriterExposed());
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		cusEntryHeaderDataObjectWriter = new CustomsEntryHeaderDataObjectWriterForTest(new DataWritingManager(new ActionInfo(null, entryHeader)), new UniversalDataObjectWriterHelper(entryHeader.Factory, Core.Constants.CountryCodes.Italy));
	}
	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	CustomsEntryHeaderDataObjectWriterForTest cusEntryHeaderDataObjectWriter;
}

class CustomsEntryHeaderDataObjectWriterForTest : CustomsEntryHeaderDataObjectWriter
{
	public CustomsEntryHeaderDataObjectWriterForTest(IDataWritingManager manager, UniversalDataObjectWriterHelper helper)
	: base(manager, helper)
	{
	}

	public Customs.DataTransfer.Universal.CustomsEntryNumberDataObjectWriter GetNewCustomsEntryNumberDataObjectWriterExposed() => GetNewCustomsEntryNumberDataObjectWriter();
}
