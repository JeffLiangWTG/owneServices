using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Management.Testing;
using CusEntryInstruction = Enterprise.Customs.BR.Business.CusEntryInstruction;

namespace Enterprise.Customs.BR.DataTransfer.Universal.Testing
{
	public class BREntryInstructionDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestReader()
		{
			var logger = new TestErrorLogger();
			var helper = new BRDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Brazil);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = "BLT";

			var input = new EntryInstruction
			{
				AddInfoCollection = new List<UniversalDataBuss.DataObjects.Universal.AddInfo>(),
				AddInfoGroupCollection = new List<AddInfoGroup>()
			};

			input.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo
			{
				Key = DataTransfer.Constants.AddInfoKeys.EntryInstruction.UCRNumber,
				Value = "1111111"
			});
			input.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo
			{
				Key = DataTransfer.Constants.AddInfoKeys.EntryInstruction.IsUCROverridden,
				Value = "N"
			});

			var instruction = new BREntryInstructionDataObjectReader(input, logger, helper, Factory, declaration).ReadIntoBusinessObject() as CusEntryInstruction;
			AssertEquals("1111111", instruction.UCRNumber);
			AssertEquals(false, instruction.IsUCROverridden);

			input.AddInfoCollection.Clear();
			input.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo { Key = DataTransfer.Constants.AddInfoKeys.EntryInstruction.UCRNumber, Value = "" });
			input.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo { Key = DataTransfer.Constants.AddInfoKeys.EntryInstruction.IsUCROverridden, Value = "Y" });

			instruction = new BREntryInstructionDataObjectReader(input, logger, helper, Factory, declaration).ReadIntoBusinessObject() as CusEntryInstruction;
			AssertEquals(ZString.Empty, instruction.UCRNumber);
			AssertEquals(ZBool.True, instruction.IsUCROverridden);
		}
	}
}
