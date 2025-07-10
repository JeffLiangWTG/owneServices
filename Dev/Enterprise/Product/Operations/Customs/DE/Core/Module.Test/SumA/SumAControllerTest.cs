using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Messaging;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(SumAController))]
	sealed class SumAControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		public void TestGetNewBusinessEntityInLocalFactoryForSumA()
		{
			var controller = new SumAController();
			using (var form = controller.ShowNewForm() as ZForm)
			{
				var jobHeader = (CusTempStorageJobHeader)form.BusinessEntity;
				CombineAssertions(() =>
				{
					AssertEquals("SJH_AppCode", TemporaryStorageApplicationCodeList.Codes.SumA, ((CusTempStorageJobHeader)form.BusinessEntity).SJH_AppCode);
					AssertNull("CUSPRLCusTempStorageDec", jobHeader.CUSPRLCusTempStorageDec);
					AssertEquals("SetDefaultValuesForSumA was called", PreviousReferenceType.Codes._OHNE, jobHeader.SJH_PreviousReferenceType);
				});
			}
		}

		public void TestGetNewBusinessEntityInLocalFactoryForReExport()
		{
			var controller = new SumAController(TemporaryStorageApplicationCodeList.Codes.REX);
			using (var form = controller.ShowNewForm() as ZForm)
			{
				var jobHeader = (CusTempStorageJobHeader)form.BusinessEntity;
				CombineAssertions(() =>
				{
					AssertEquals("SJH_AppCode", TemporaryStorageApplicationCodeList.Codes.REX, jobHeader.SJH_AppCode);
					AssertNotNull("REXDISCusTempStorageDec", jobHeader.REXDISCusTempStorageDec);
					AssertEquals("SetDefaultValuesForSumA wasn't called", ZString.Empty, jobHeader.SJH_PreviousReferenceType);
				});
			}
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.TemporaryStorage;

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;
	}
}
