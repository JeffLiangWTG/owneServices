using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.H7.Business
{
	public class MessagingProvider : ASYCUDA.Business.MessagingProvider
	{
		public override ASYCUDA.Business.MessageStatusProvider MessageStatusProvider => new MessageStatusProvider();

		public override CodeDescriptionPairList GetCustomsEntryNumberTypeList(BusinessObjectFactory factory,
			string countryCode) => GetCachedBillEntryNumberTypeList(factory);

		public static CodeDescriptionPairList GetCachedBillEntryNumberTypeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("EUH7BillEntryNumberTypeList", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(CusEntryNumberTypes.Standard.MovementReferenceNumber, Res.GetString("EC786A23-2F2E-480E-92CD-FA3990CC9A1C", "Movement Reference Number"));
				result.AddPair(CusEntryNumberTypes.EU.LocalReferenceNumber, Res.GetString("299EC58C-7786-4F25-94E3-343D84180969", "Local Reference Number"));
				return result;
			});
		}
	}
}
