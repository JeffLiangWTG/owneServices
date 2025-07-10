using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.BR.DataTransfer.Universal.Testing
{
	public class BREntryInstructionDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestAddInfos()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.UCRNumber = "9CN91330302765207767NTINVGWAB190311";
			instruction.IsUCROverridden = false;

			var writer = new BREntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new BRDataObjectWriterHelper(Factory));
			var result = writer.GetDataObject(instruction);

			AssertEquals("UCRNumber", "9CN91330302765207767NTINVGWAB190311", result.AddInfoCollection.GetZStringValue(DataTransfer.Constants.AddInfoKeys.EntryInstruction.UCRNumber));
			AssertEquals("IsUCROverridden", "N", result.AddInfoCollection.GetZStringValue(DataTransfer.Constants.AddInfoKeys.EntryInstruction.IsUCROverridden));

			instruction.IsUCROverridden = true;
			result = writer.GetDataObject(instruction);

			AssertEquals("UCRNumber", ZString.Empty, result.AddInfoCollection.GetZStringValue(DataTransfer.Constants.AddInfoKeys.EntryInstruction.UCRNumber));
			AssertEquals("IsUCROverridden", "Y", result.AddInfoCollection.GetZStringValue(DataTransfer.Constants.AddInfoKeys.EntryInstruction.IsUCROverridden));
		}

		public void TestOrganizationAddressCollection()
		{
			var orgJustificationContact = Factory.NewWithValidTestData<OrgHeader>();
			orgJustificationContact.OH_Code = "TEST01";
			orgJustificationContact.OH_FullName = "JustificationContactDetailAddress";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.JustificationContactDetailAddress.E2_OA_Address = orgJustificationContact.MainAddress.PK;

			var writer = new BREntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new BRDataObjectWriterHelper(Factory));
			var result = writer.GetDataObject(instruction);

			AssertEquals(result.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == "JustificationContactDetailAddress").CompanyName, orgJustificationContact.OH_FullName);
		}
	}
}
