using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusEntryHeader.Loader))]
	class LoaderTest : LoaderTestCase
	{
		public void TestFindByEntryNumberAndMessageType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia))
			{
				SetupEntryHeader("ENTRY1", JobMessageTypeList.Codes.Import);
				Factory.Save();
			}

			var entry1 = SetupEntryHeader("ENTRY1", JobMessageTypeList.Codes.Import);
			SetupEntryHeader("ENTRY2", JobMessageTypeList.Codes.Export);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertNull("Empty parameters", new CusEntryHeader.Loader(Factory).FindByEntryNumberAndMessageType(ZString.Empty, ZString.Empty));
				AssertEquals("Found", entry1.PK, new CusEntryHeader.Loader(Factory).FindByEntryNumberAndMessageType("ENTRY1", JobMessageTypeList.Codes.Import).PK);
				AssertNull("Not Found", new CusEntryHeader.Loader(Factory).FindByEntryNumberAndMessageType("ENTRY3", JobMessageTypeList.Codes.Export));
			}

			);
		}

		public void TestFindByEntryNumberAndCurrentCompanyIsNotSupported()
		{
			CombineAssertions(() =>
			{
				AssertNull("Without parameter entryHeaderFilter", new CusEntryHeader.Loader(Factory).FindByEntryNumberAndCurrentCompany("EntryNumber"));
				AssertEquals("Without parameter entryHeaderFilter->LastKeyReported", "AsycudaCustomsFindByEntryNumberAndCurrentCompany Is Not Supported", ErrorReporter.LastKeyReported);
				AssertEquals("Without parameter entryHeaderFilter->LastMessageReported", "This method is not valid for Asycuda Customs; please use FindByEntryNumberAndMessageType.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
				AssertNull("With parameter entryHeaderFilter", new CusEntryHeader.Loader(Factory).FindByEntryNumberAndCurrentCompany("EntryNumber", e => true));
				AssertEquals("With parameter entryHeaderFilter->LastKeyReported", "AsycudaCustomsFindByEntryNumberAndCurrentCompany Is Not Supported", ErrorReporter.LastKeyReported);
				AssertEquals("With parameter entryHeaderFilter->LastMessageReported", "This method is not valid for Asycuda Customs; please use FindByEntryNumberAndMessageType.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}

			);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new CusEntryHeader.Loader(Factory);

		CusEntryHeader SetupEntryHeader(string entryNumber, string messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = messageType;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.EntryNumber = entryNumber;
			return entryHeader;
		}
	}
}
