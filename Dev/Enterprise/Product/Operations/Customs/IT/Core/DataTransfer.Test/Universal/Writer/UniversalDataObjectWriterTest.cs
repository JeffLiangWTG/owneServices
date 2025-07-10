using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.IT.DataTransfer.Universal.Testing;

sealed class UniversalDataObjectWriterTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
{
	public void TestPopulateAddInfoCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_TransportMode = "SEA";

		declaration.ZG_PreClearing = ZBool.True;
		var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
		AssertEquals("ZG_PreClearing should be", "Y", declarationData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "PreClearing").Value);

		declaration.ZG_PreClearing = ZBool.False;
		declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
		AssertEquals("ZG_PreClearing should be", "N", declarationData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "PreClearing").Value);

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.ZG_TempProcLimitDate = new ZDateTime(2019, 01, 01);
		declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
		var entryInstructionData = declarationData.EntryInstructionCollection.ElementAt(0);
		AssertEquals("ZG_TempProcLimitDate should be", "2019-01-01 00:00:00.000", entryInstructionData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "TempProcLimitDate").Value);

		entryInstruction.ZG_TempProcLimitDate = ZDateTime.Empty;
		declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
		entryInstructionData = declarationData.EntryInstructionCollection.ElementAt(0);
		AssertNull("ZG_TempProcLimitDate should be null", entryInstructionData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "TempProcLimitDate"));
	}
}
