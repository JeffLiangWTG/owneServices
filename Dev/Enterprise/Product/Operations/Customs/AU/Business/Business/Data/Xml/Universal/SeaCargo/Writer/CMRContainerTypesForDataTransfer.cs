using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business;

public class CMRContainerTypesForDataTransfer : CMRContainerTypes, CargoWise.Integration.ICodeDescriptionPairList
{
	string CargoWise.Integration.ICodeDescriptionPairList.GetDescriptionFromCode(string code)
	{
		var newCode = new CMRContainerUtilities().GetContainerTypeMappingOrOriginal(code);
		return this.GetDescriptionFromCode(newCode);
	}
}
