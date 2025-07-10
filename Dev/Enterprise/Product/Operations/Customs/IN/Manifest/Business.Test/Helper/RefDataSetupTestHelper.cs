using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

public sealed class RefDataSetupTestHelper
{
	public static void SetupCertificateTokenData(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingCusCodeType("CERAU", "Certificate Authority", "IN");
		helper.CreateNewOrGetExistingCusCodeType("CHPST", "Chipset Manufacturer", "IN");

		helper.CreateNewOrGetExistingCusCodeList("IN", "CERAU", "eMudhra", "eMudhra Limited", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var chipset = helper.CreateNewOrGetExistingCusCodeList("IN", "CHPST", "WatchData", "WatchData", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		helper.CreateNewOrGetExistingCusCodeListAttribute(chipset.PK, "Chipsetdll", "TRUSTKEYP11_ND_v34.dll");
		factory.Save();
	}

	public static void SetupUNLOCOData(BusinessObjectFactory factory)
	{
		var refUNLOCO = factory.New<RefUNLOCO>();
		refUNLOCO.RL_Code = "INABC";
		refUNLOCO.RL_IATA = "XYZ";

		refUNLOCO = factory.New<RefUNLOCO>();
		refUNLOCO.RL_Code = "INLMN";
		refUNLOCO.RL_IATA = ZString.Empty;

		factory.Save();
	}
}
