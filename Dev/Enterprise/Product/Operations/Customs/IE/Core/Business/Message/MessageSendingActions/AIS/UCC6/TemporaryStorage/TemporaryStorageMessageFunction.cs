using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class DeclarationMessageFunction : TemporaryStorageMessageFunction
	{
		protected override ZString MessageTypeCore => IETemporaryStorageMessageTypeList.Codes.Declaration;
	}

	public class InvalidationMessageFunction : TemporaryStorageMessageFunction
	{
		protected override ZString MessageTypeCore => IETemporaryStorageMessageTypeList.Codes.Invalidation;
	}

	public class AmendmentMessageFunction : TemporaryStorageMessageFunction
	{
		protected override ZString MessageTypeCore => IETemporaryStorageMessageTypeList.Codes.Amendment;
	}

	public class PresentationNotificationMessageFunction : TemporaryStorageMessageFunction
	{
		protected override ZString MessageTypeCore => IETemporaryStorageMessageTypeList.Codes.PresentationNotification;
	}

	public class GoodsStatusReportDeclarationMessageFunction : TemporaryStorageMessageFunction
	{
		protected override ZString MessageTypeCore => IETemporaryStorageMessageTypeList.Codes.GoodsStatusReportDeclaration;
	}
}
