using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	sealed class CustomsEntryHeaderDataObjectWriterTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestCustomsEntryNumberDataObjectWriterType()
		{
			AssertType<CustomsEntryNumberDataObjectWriter>("EntryNumber writer type", cusEntryHeaderDataObjectWriter.GetNewCustomsEntryNumberDataObjectWriterExposed());
		}

		public void TestPopulatePaymentInformationCollection()
		{
			entryHeader.EntryPayInfos.AddNew();
			entryHeader.EntryPayInfos.AddNew();

			var dataObject = cusEntryHeaderDataObjectWriter.GetDataObject(entryHeader);
			var paymentInformationCollection = dataObject.PaymentInformationCollection;
			AssertNotNull("PaymentInformationCollection", paymentInformationCollection);
			AssertEquals("PaymentInformationCollection Count", 2, paymentInformationCollection.Count);
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

		sealed class CustomsEntryHeaderDataObjectWriterForTest : CustomsEntryHeaderDataObjectWriter
		{
			public CustomsEntryHeaderDataObjectWriterForTest(IDataWritingManager manager, UniversalDataObjectWriterHelper helper)
			: base(manager, helper)
			{
			}

			public Customs.DataTransfer.Universal.CustomsEntryNumberDataObjectWriter GetNewCustomsEntryNumberDataObjectWriterExposed() => GetNewCustomsEntryNumberDataObjectWriter();
		}
	}
}
