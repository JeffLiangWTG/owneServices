using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AUTariffFilterStrip))]
	sealed class AUTariffFilterStripTest : TransactionedTestCase
	{
		public void TestCreateTariffFindBox()
		{
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var tariffFilterStrip = new AUTariffFilterStrip())
			using (var tariffFindBox = tariffFilterStrip.CreateTariffFindBox(TariffModuleFilterType.Export))
			{
				AssertType<UniversalTariffExportFindBox>(tariffFindBox);
			}

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var tariffFilterStrip = new AUTariffFilterStrip())
			using (var tariffFindBox = tariffFilterStrip.CreateTariffFindBox(TariffModuleFilterType.Export))
			{
				AssertType<AHECCFindBox>(tariffFindBox);
			}

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var tariffFilterStrip = new AUTariffFilterStrip())
			using (var tariffFindBox = tariffFilterStrip.CreateTariffFindBox(TariffModuleFilterType.Import))
			{
				AssertType<UniversalTariffImportFindBox>(tariffFindBox);
			}

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (var tariffFilterStrip = new AUTariffFilterStrip())
			using (var tariffFindBox = tariffFilterStrip.CreateTariffFindBox(TariffModuleFilterType.Import))
			{
				AssertType<AUCClassFindBox>(tariffFindBox);
			}
		}
	}
}
