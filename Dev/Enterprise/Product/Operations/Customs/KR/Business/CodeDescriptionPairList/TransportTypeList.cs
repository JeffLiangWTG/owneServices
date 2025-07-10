using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class TransportTypeList : CodeDescriptionPairList,
		Integration.Customs.KR.IKRTransportTypeCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public TransportTypeList()
		{
			AddPair(Codes.Air, Descriptions.Air);
			AddPair(Codes.Sea, Descriptions.Sea);
			AddPair(Codes.Mail, Descriptions.Mail);
		}

		public static class Codes
		{
			public const string Air = Customs.Business.TransportTypeList.Codes.Air;
			public const string Sea = Customs.Business.TransportTypeList.Codes.Sea;
			public const string Mail = Customs.Business.TransportTypeList.Codes.Mail;
		}

		public static class Descriptions
		{
			public static string Air => Customs.Business.TransportTypeList.Descriptions.Air;
			public static string Sea => Customs.Business.TransportTypeList.Descriptions.Sea;
			public static string Mail => Customs.Business.TransportTypeList.Descriptions.Mail;
		}

		ReadOnlyCodeDescriptionPairList DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider.GetCodeDescriptionPairList() => new TransportTypeList();

		public static CodeDescriptionPairList GetTransportTypeList(BusinessObjectFactory factory, ZString messageType)
		{
			return factory.GetCachedValue("TransportTypeList" + messageType, () =>
			{
				var result = new TransportTypeList();
				if (messageType == KRJobMessageTypeList.Codes.Import)
				{
					result.RemoveCode(TransportTypeList.Codes.Mail);
				}
				return result;
			});
		}
	}
}
