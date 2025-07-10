using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.CN.DataTransfer.Universal.Testing
{
	class CNEntryInstructionDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestReader()
		{
			Registry.Business.eServices.eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var logger = new TestErrorLogger();
			var helper = new CNDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.China);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			declaration.JE_ApplicationCode = "BLT";

			var input = new EntryInstruction
			{
				AddInfoCollection = new List<UniversalDataBuss.DataObjects.Universal.AddInfo>(),
				AddInfoGroupCollection = new List<AddInfoGroup>()
			};

			input.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo
			{
				Key = "LevyType",
				Value = "101"
			});
			input.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo
			{
				Key = Constants.AddInfoKeys.EntryInstruction.CustomsMessageRemarks,
				Value = "Remarks"
			});
			input.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo
			{
				Key = Constants.AddInfoKeys.EntryInstruction.BillOfLading,
				Value = "BILL001"
			});
			input.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo
			{
				Key = Constants.AddInfoKeys.EntryInstruction.BillOfLadingDate,
				Value = ZDateTime.Today.ToShortDateString()
			});
			input.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo
			{
				Key = Constants.AddInfoKeys.EntryInstruction.CIQRequires,
				Value = "true"
			});

			var addinfo1 = new UniversalDataBuss.DataObjects.Universal.AddInfo
			{
				Key = "DocumentType",
				Value = "13"
			};
			var addinfo2 = new UniversalDataBuss.DataObjects.Universal.AddInfo
			{
				Key = "NumberOfCopies",
				Value = "2"
			};
			var addinfo3 = new UniversalDataBuss.DataObjects.Universal.AddInfo
			{
				Key = "NumberOfOriginals",
				Value = "1"
			};
			var addInfoGroup = new AddInfoGroup
			{
				Type = new CodeDescriptionPair
				{
					Code = "RQD"
				},
				AddInfoCollection = new List<UniversalDataBuss.DataObjects.Universal.AddInfo>
					{
						addinfo1,
						addinfo2,
						addinfo3,
					}
			};
			input.AddInfoGroupCollection.Add(addInfoGroup);

			var output = new CNEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration).ReadIntoBusinessObject() as Business.CusEntryInstruction;
			Factory.SaveForTesting();

			AssertEquals("101", output.CEI_LevyType);
			AssertEquals("Remarks", output.CustomsMessageRemarks);

			AssertEquals("BILL001", output.BillOfLading);
			AssertEquals(ZDateTime.Today, output.BillOfLadingDate);

			AssertEquals(1, output.CIQRequiredDocuments.Count);
			Assert(output.CIQRequiredDocuments.Cast<CIQRequiredDocument>().Any(doc => doc.XC_DocumentType == "13" && doc.XC_NumberOfCopies == 2 && doc.XC_NumberOfOriginals == 1));

			input.AddInfoCollection.Clear();
			input.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo { Key = "LevyType", Value = "" });
			input.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo { Key = Constants.AddInfoKeys.EntryInstruction.CustomsMessageRemarks, Value = "" });
			input.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo { Key = Constants.AddInfoKeys.EntryInstruction.BillOfLading, Value = "" });
			input.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo { Key = Constants.AddInfoKeys.EntryInstruction.BillOfLadingDate, Value = "" });

			output = new CNEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration).ReadIntoBusinessObject() as Business.CusEntryInstruction;
			Factory.SaveForTesting();

			AssertEquals(ZString.Empty, output.CEI_LevyType);
			AssertEquals(ZString.Empty, output.CustomsMessageRemarks);

			AssertEquals(ZString.Empty, output.BillOfLading);
			AssertEquals(ZDateTime.Empty, output.BillOfLadingDate);
		}
	}
}
