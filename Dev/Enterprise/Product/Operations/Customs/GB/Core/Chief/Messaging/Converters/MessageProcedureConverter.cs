using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Common;

namespace Enterprise.Customs.GB.Chief.Messaging.Converters
{
	public static class MessageProcedureConverter
	{
		public static MessageProcedure GetMessageProcedure(ZString messageCode, ZString subStyle)
		{
			switch (messageCode.ToString())
			{
				// Export Message Codes

				case ExportSADDeclarationTypeList.Codes.ExportLCPPreShipment:
					return MessageProcedure.LCPPSA;
				case ExportSADDeclarationTypeList.Codes.ExportSDPPreShipment:
					return MessageProcedure.SDPPSA;
				case ExportSADDeclarationTypeList.Codes.ExportSupplementaryDeclaration:
					return MessageProcedure.SuppDec;
				case ExportSADDeclarationTypeList.Codes.ExportFullDeclaration:
					return MessageProcedure.FullDec;
				case ExportSADDeclarationTypeList.Codes.ExportClearanceRequest:
					return MessageProcedure.C21;
				case ExportSADDeclarationTypeList.Codes.ExitSummaryDeclaration:
					return MessageProcedure.EXS;
				default:
					return MessageProcedure.UnknownCargoWiseShowError;

				// Import message Codes
				case ImportSADDeclarationTypeList.Codes.ImportClearanceRequest:
					return MessageProcedure.C21;
				case ImportSADDeclarationTypeList.Codes.ImportFullDeclaration:
					return SimplifiedFrontierHelper.IsSimplifiedFrontierSubstyle(subStyle) ? MessageProcedure.SFD : MessageProcedure.FullDec;
				case ImportSADDeclarationTypeList.Codes.ImportFullWarehouse:
					return MessageProcedure.WRD;
				case ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration:
					return MessageProcedure.SDI;
				case ImportSADDeclarationTypeList.Codes.ImportSupplementaryWarehouse:
					return MessageProcedure.SDW;
			}
		}
	}
}
