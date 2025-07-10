using CargoWise.Common;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common
{
	public abstract class InboundMessageDocumentWrapper<T>(IJPInboundMessageParseResult parseResult, BusinessObjectFactory factory) : DocumentEngineCore.DocWrappers.DocumentWrapper(null, factory) where T : IJPInboundMessageDataProvider
	{
		protected readonly T messageProvider = (T)Argument.NotNull(parseResult, nameof(parseResult)).MessageProvider;

		protected readonly IJPInboundMessageHeader responseHeader = parseResult.ResponseHeader;

		#region Common Fields

		public ZString ProcedureCode => responseHeader?.ProcedureCode ?? ZString.Empty;

		public ZString OutputInformationCode => responseHeader?.OutputInformationCode ?? ZString.Empty;

		public ZString ReceivedDateTime => responseHeader?.ReceivedDateTime?.ToNACCSDateTime() ?? ZString.Empty;

		public ZString UserCode => responseHeader?.UserCode ?? ZString.Empty;

		public ZString UserMailAddress => responseHeader?.UserMailAddress ?? ZString.Empty;

		public ZString Subject => responseHeader?.Subject ?? ZString.Empty;

		public ZString MessageTag => responseHeader?.MessageTag ?? ZString.Empty;

		public ZString DivisionNumber => responseHeader?.DivisionNumber ?? ZString.Empty;

		public ZString LastMessage => responseHeader?.LastMessage ?? ZString.Empty;

		public ZString MessageType => responseHeader?.MessageType ?? ZString.Empty;

		public ZString InputReference => responseHeader?.InputReference ?? ZString.Empty;

		public ZString SplitReference => responseHeader?.SplitReference ?? ZString.Empty;

		public ZString ManagementType => responseHeader?.ManagementType ?? ZString.Empty;

		public ZString MessageLength => responseHeader?.MessageLength ?? ZString.Empty;

		#endregion

		public ZString TemplateType => GetTemplateTypeCore();

		public ZString ShipmentType => GetShipmentTypeCore();

		public ZString TransportMode => OutputInformationCode.Left(1);

		protected virtual ZString GetTemplateTypeCore() => ZString.Empty;

		protected virtual ZString GetShipmentTypeCore() => ZString.Empty;
	}
}
