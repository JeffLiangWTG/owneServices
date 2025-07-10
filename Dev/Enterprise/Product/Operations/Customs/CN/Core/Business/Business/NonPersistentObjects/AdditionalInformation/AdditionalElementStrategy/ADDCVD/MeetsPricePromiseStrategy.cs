using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Customs.CN.Business
{
	public class MeetsPricePromiseStrategy : ADDCVDElementStrategy
	{
		public static string AdditionalElementCode => "345de1dff3d2a625ca17f1a4328f6b32"; // 是否符合价格承诺
		public override bool ProvideList => true;

		public override ICodeDescriptionPairList GetList(BusinessObjectFactory factory, EnteringOrExiting enteringOrExiting) => ConfirmationTypeList.GetYesAndNoList(factory);
	}
}
