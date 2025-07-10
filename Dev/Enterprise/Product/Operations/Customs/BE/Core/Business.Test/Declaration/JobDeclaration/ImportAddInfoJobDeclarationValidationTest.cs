using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class ImportAddInfoJobDeclarationValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidateZG_RegionOfDestination()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		CombineAssertions(() =>
		{
			foreach (var codeDescription in declaration.AddInfoLookups.RegionOfDestinationList)
			{
				declaration.ZG_RegionOfDestination = ((ICodeDescription)codeDescription).Code;
				AssertNoNotifications("Correct input of Region of Destination should not trigger error", declaration.ZG_RegionOfDestinationInfo);
			}
			declaration.ZG_RegionOfDestination = "(";
			AssertHasMessageErrors("Region of Destination should report a message about invalid input '('", declaration.ZG_RegionOfDestinationInfo);
		});
	}
}
