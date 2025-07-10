using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class REXDISCusTempStorageReExportLineLookups : REXDISCusTempStorageLineLookups
	{
		public REXDISCusTempStorageReExportLineLookups(REXDISCusTempStorageReExportLine parent) : base(parent)
		{
		}

		public new REXDISCusTempStorageReExportLine Parent => (REXDISCusTempStorageReExportLine)base.Parent;

		public override CodeDescriptionPairList OwnerReferenceTypeList => Factory.GetCachedValue("DE|REXDISCusTempStorageReExportLineLookups|OwnerReferenceTypeList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(new OwnerReferenceTypeList());
			result.RemoveCode(Business.OwnerReferenceTypeList.Codes.REG);
			return result;
		});
	}
}
