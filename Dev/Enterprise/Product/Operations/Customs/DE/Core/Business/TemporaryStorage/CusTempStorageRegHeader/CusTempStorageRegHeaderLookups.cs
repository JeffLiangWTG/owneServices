using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageRegHeaderLookups : EU.TemporaryStorage.Business.CusTempStorageRegHeaderLookups, IRegisterReportStatusListProvider
	{
		public CusTempStorageRegHeaderLookups(CusTempStorageRegHeader parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList PreviousReferenceTypeList => Factory.GetCachedValue<PreviousReferenceType>();

		public override CustomsOfficeCodeCollection CustomsOfficeList => CustomsOfficeCodeCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Germany, ZDateTime.Today);

		public override CodeDescriptionPairList StatusList => Factory.GetCachedValue<CustomsStatusList>();

		public ReadOnlyCodeDescriptionPairList ReportStatusList
		{
			get
			{
				var statusList = new CodeDescriptionPairList(StatusList);
				statusList.AddPair(TemporaryStorageRegStatusCodes.Code.NCM, TemporaryStorageRegStatusCodes.Description.NCM);
				return statusList;
			}
		}
	}
}
