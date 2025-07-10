using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class REXDISCusTempStorageSumALineLookups : REXDISCusTempStorageLineLookups
	{
		public REXDISCusTempStorageSumALineLookups(REXDISCusTempStorageSumALine parent) : base(parent)
		{
		}
		public new REXDISCusTempStorageSumALine Parent => (REXDISCusTempStorageSumALine)base.Parent;

		public override CodeDescriptionPairList OwnerReferenceTypeList => Factory.GetCachedValue(string.Join("|", "DE|REXDISCusTempStorageSumALineLookups|OwnerReferenceTypeList", Parent.Dec?.STH_IdentificationIndicator), () =>
		{
			var resultList = new CodeDescriptionPairList();
			if (Parent.IsAWBDeclaration)
			{
				resultList.AddPair(Business.OwnerReferenceTypeList.Codes.AWB, Business.OwnerReferenceTypeList.Descriptions.AWB);
				resultList.AddPair(Business.OwnerReferenceTypeList.Codes.ULD, Business.OwnerReferenceTypeList.Descriptions.ULD);
			}
			else if (Parent.IsSINDeclaration)
			{
				resultList.AddPair(Business.OwnerReferenceTypeList.Codes.SIN, Business.OwnerReferenceTypeList.Descriptions.SIN);
			}
			return resultList;
		});
	}
}
