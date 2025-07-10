namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	partial class CMRCusSCAOceanBillDataObjectWriterTest
	{
		protected override CusSCAPivot AddPackage1(CusSCAContainer container, CusSCAHouse bill)
		{
			var result = base.AddPackage1(container, bill);
			result.HouseBill.Pivot.Add(result);
			result.CV_NetWeight = 1m;
			result.CV_HazardousGoods = true;
			result.CV_FumigationCert = true;
			result.CV_PersonalEffects = true;
			result.CV_Timber = true;
			result.CV_PerishableGoods = true;
			result.CV_IsSAC = true;
			result.CV_Flammable = true;
			return result;
		}

		protected override CusSCAPivot AddPackage2(CusSCAContainer container, CusSCAHouse bill)
		{
			var result = base.AddPackage2(container, bill);
			result.HouseBill.Pivot.Add(result);
			result.CV_CargoStatus = "ACA";
			result.CV_NetWeight = 2m;
			return result;
		}
	}
}
