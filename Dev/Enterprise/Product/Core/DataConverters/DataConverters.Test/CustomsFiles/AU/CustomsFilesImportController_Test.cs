using CargoWise.EntityFramework;
using Enterprise.DataConverters.CustomsFiles;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DataConverters.Testing.CustomsFiles.AU
{
	[TestedType(typeof(CustomsFilesImportController))]
	sealed internal class CustomsFilesImportController_Test : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.ImportCustomsFilesData;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var result = Factory.New<OrgHeader>();
			result.MainAddress.OA_Address1 = "Basher Test Address";
			result.OH_Code = "BSH: Basher";
			Factory.Save();
			return result;
		}
	}
}
