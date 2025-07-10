using CargoWise.Types;
using Enterprise.Customs.EU.DataTransfer.Universal;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DataTransfer.Universal.Testing
{
	sealed class CustomsEntryHeaderDataObjectWriterTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		[TestDate(2020, 1, 11)]
		public void TestGetEntryHeaderAddInfoCollection()
		{
			entryHeader.CH_ExitedStatus = ExportControlStatusList.Codes.SOR;
			Factory.SaveForTesting();
			var result = cusEntryHeaderDataObjectWriter.GetDataObject(entryHeader);
			AssertEquals("GuaranteedAmount is temporarily set to 0", "0", result.AddInfoCollection.GetZStringValue(new ZString("GuaranteedAmount")));
			AssertEquals("AddInfo should contain EXITDATE", "11/01/2020", result.AddInfoCollection.GetZStringValue(new ZString("EXITDATE")));
		}

		public void TestGetEntryHeaderAddInfoCollection_EntryStatusDescription()
		{
			entryHeader.CH_EntryStatus = "100";
			Factory.SaveForTesting();
			var result = cusEntryHeaderDataObjectWriter.GetDataObject(entryHeader);
			AssertEquals("ENTRYSTATUSDESCRIPTION should be BAE as the code 100 has a linked description.", "BAE", result.AddInfoCollection.GetZStringValue(new ZString("ENTRYSTATUSDESCRIPTION")));

			entryHeader.CH_EntryStatus = "560";
			Factory.SaveForTesting();
			result = cusEntryHeaderDataObjectWriter.GetDataObject(entryHeader);
			AssertEquals("ENTRYSTATUSDESCRIPTION should be null as there is no description link to code 560", null, result.AddInfoCollection.GetZStringValue(new ZString("ENTRYSTATUSDESCRIPTION")));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeaderDataObjectWriter = new CustomsEntryHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, entryHeader)), new UniversalDataObjectWriterHelper(entryHeader.Factory, Core.Constants.CountryCodes.France));
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		CustomsEntryHeaderDataObjectWriter cusEntryHeaderDataObjectWriter;
	}
}
