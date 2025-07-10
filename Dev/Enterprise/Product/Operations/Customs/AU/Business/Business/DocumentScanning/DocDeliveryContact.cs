using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DocDeliveryContact : MasterFiles.Business.DocDeliveryContact
	{
		public DocDeliveryContact(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override CodeDescriptionPairList GetNotifyModes()
		{
			CodeDescriptionPairList result;

			if (ZArchitecture.Environment.DataRegistry.Instance.AUCustoms.NEXDOCSDisableQRPView)
			{
				result = new CodeDescriptionPairList();
				result.AddPair(Core.Constants.ContactNotifyModes.Print, ResString.GetMultilingualString("683FFC50-18B5-4977-930F-948A7D7ABC66", "Print"));
			}
			else
			{
				result = base.GetNotifyModes();
			}

			return result;
		}
	}
}
