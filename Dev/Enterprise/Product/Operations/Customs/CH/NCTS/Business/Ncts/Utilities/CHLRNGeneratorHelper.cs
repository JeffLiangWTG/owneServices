using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CH.NCTS.Business;

public static class CHLRNGeneratorHelper
{
	public static ZString GenerateLocalReferenceNumber(BusinessObjectFactory factory, INumberFountainProxy numberFountain)
	{
		return GenerateLocalReferenceNumber(factory, numberFountain, EnvironmentHelper.GetBusinessPartnerId());
	}

	public static ZString GenerateLocalReferenceNumber(BusinessObjectFactory factory, INumberFountainProxy numberFountain, OrgHeader orgHeader)
	{
		return GenerateLocalReferenceNumber(factory, numberFountain, orgHeader?.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.BID) ?? ZString.Empty);
	}

	static ZString GenerateLocalReferenceNumber(BusinessObjectFactory factory, INumberFountainProxy numberFountain, ZString bidNumber)
	{
		var result = ZString.Empty;
		if (!bidNumber.IsEmpty)
		{
			var sequenceNumber = numberFountain.GetNext(factory).ToString().PadLeft(10, '0');
			result = string.Format("{0}{1}{2}", ZDateTime.Now.ToString("yy"), bidNumber.SubstringSafe(0, 10).PadLeft(10, '0'), sequenceNumber);
		}

		return result;
	}
}
