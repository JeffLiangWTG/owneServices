using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	class CustomsEntryHeaderDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestGetNewCustomsEntryLineDataObjectWriter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var writer = new CustomsEntryHeaderDataObjectWriterForTest(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new UniversalDataObjectWriterHelper(declaration));
			writer.GetDataObject(entry);
			AssertType<CustomsEntryLineDataObjectWriter>("Customs entry line data object writer type", writer.GetNewCustomsEntryLineDataObjectWriterExposed());
		}
	}

	class CustomsEntryHeaderDataObjectWriterForTest : CustomsEntryHeaderDataObjectWriter
	{
		public CustomsEntryHeaderDataObjectWriterForTest(IDataWritingManager manager, UniversalDataObjectWriterHelper helper) : base(manager, helper)
		{
		}

		public Customs.DataTransfer.Universal.CustomsEntryLineDataObjectWriter GetNewCustomsEntryLineDataObjectWriterExposed() => GetNewCustomsEntryLineDataObjectWriter();
	}
}
