using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ComplXAESConsignmentWrapper : IComplXAESConsignment
	{
		public ComplXAESConsignmentWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
			modeOfTransportAtBorder = declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportMode, false);
		}
		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		readonly ZString modeOfTransportAtBorder;

		public ZString InlandModeOfTransport => declaration.GetExportCustomsOffice() == declaration.GetExitCustomsOffice()
															? ZString.Empty
															: declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportModeInland, false);

		public ZString ModeOfTransportAtBorder => modeOfTransportAtBorder;

		public ITransportMediumInfoCommon ActiveBorderTransportMeans => activeBorderTransportMeans ??= !modeOfTransportAtBorder.IsEmpty ? AESWrappersHelper.GetActiveBorderTransportMeans(declaration) : null;
		TransportMediumInfoCommonWrapper activeBorderTransportMeans;

		public ZString TransportChargesMoP => entryHeader.RandomHeader.ZG_TransportChargesMethodOfPayment;
	}
}
