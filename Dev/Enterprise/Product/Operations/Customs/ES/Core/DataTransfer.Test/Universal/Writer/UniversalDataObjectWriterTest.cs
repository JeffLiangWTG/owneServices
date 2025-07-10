using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ES.DataTransfer.Universal.Testing
{
	public class UniversalDataObjectWriterTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestPopulateAddInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.ZG_AuthPerDeclaration = ZBool.True;
			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			AssertEquals("ZG_AuthPerDeclaration should be", "Y", declarationData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "AuthPerDeclaration").Value);

			declaration.ZG_AuthPerDeclaration = ZBool.False;
			declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			AssertEquals("ZG_AuthPerDeclaration should be", "N", declarationData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "AuthPerDeclaration").Value);
		}
	}
}
