using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Module
{
	public class ImportFromSumARegisterFilterStripBusinessObjectLookups
	{
		public ImportFromSumARegisterFilterStripBusinessObjectLookups(ImportFromSumARegisterFilterStripBusinessObject filterBizObj)
		{
			this.filterBizObj = filterBizObj;
		}

		readonly ImportFromSumARegisterFilterStripBusinessObject filterBizObj;

		BusinessObjectFactory Factory => filterBizObj.Factory;

		public CodeDescriptionPairList OwnerReferenceTypeList => Factory.GetCachedValue("65716E22-29DC-4939-9A80-A447B780622E", () =>
			new CodeDescriptionPairList
			{
				new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.AWB, Business.OwnerReferenceTypeList.Descriptions.AWB),
				new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.ULD, Business.OwnerReferenceTypeList.Descriptions.ULD),
				new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.ZZZ, Business.OwnerReferenceTypeList.Descriptions.ZZZ)
			});
	}
}
