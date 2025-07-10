using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class AddInfoJobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_RegionOfDestination()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.ZG_RegionOfDestination = RegionOfDestinationList.Codes.Attica;
			AssertNoMessageErrorContaining(declaration.ZG_RegionOfDestinationInfo, ListValidation.InvalidCodeMessageError);
			declaration.ZG_RegionOfDestination = "@1";
			AssertHasMessageErrorContaining(declaration.ZG_RegionOfDestinationInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
