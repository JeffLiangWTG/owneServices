using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.CDS.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS.Messaging.Wrappers;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class MessageBuilderManager : IMessageBuilderManager
	{
		public IGbCDSMessageBuilder NewMessageBuilder(JobDeclarationMessageSendingObject objectToSend, EU.Business.ErrorCollector errorCollector, CusdecMessageFunction newAmendDelete)
		{
			var cdsExportEntryHeaderWrapper = (IUkCinvWrapper)new GbCDSExportEntryHeaderWrapper(objectToSend.Header);

			switch (objectToSend.MessageType)
			{
				case GbCusDecMessageFunctionsList.Codes.Associate:
					return new AssociateRequestMessageBuilder(cdsExportEntryHeaderWrapper);
				case GbCusDecMessageFunctionsList.Codes.Disassociate:
					return new DisAssociateRequestMessageBuilder(cdsExportEntryHeaderWrapper);
				case GbCusDecMessageFunctionsList.Codes.Close:
					return new CloseRequestMessageBuilder(cdsExportEntryHeaderWrapper);
				case GbCusDecMessageFunctionsList.Codes.QueryDeclaration:
					return new DUCRQueryDeclarationRequestMessageBuilder(cdsExportEntryHeaderWrapper);
				case GbCusDecMessageFunctionsList.Codes.ArrivalAtLocation:
					return new ArrivalActualRequestMessageBuilder(cdsExportEntryHeaderWrapper, Business.GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration);
				case GbCusDecMessageFunctionsList.Codes.DepartureFromLocation:
					return new DepartureRequestMessageBuilder(cdsExportEntryHeaderWrapper, Business.GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration);
				case GbCusDecMessageFunctionsList.Codes.AnticipatedArrivalAtLocation:
					return new ArrivalAnticipatedRequestMessageBuilder(cdsExportEntryHeaderWrapper, Business.GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration);
				case CDSEDIMessageTypeList.Codes.MasterQueryDeclaration:
					return new MUCRQueryDeclarationRequestMessageBuilder(cdsExportEntryHeaderWrapper);
			}

			var functionCode = newAmendDelete.GetCdsFunctionCode();
			var cusEntryHeader = objectToSend.Header;

			if (newAmendDelete is CusdecMessageFunction.New)
			{
				if (cusEntryHeader.EntryInstruction == null)
				{
					errorCollector.AddError("Cannot find any Entry Instructions; this may be because some Commercial Invoice Lines do not cite an Entry Instruction.");
					return null;
				}

				var style = cusEntryHeader.EntryInstruction.CEI_Style;

				if (style.IsEmpty)
				{
					errorCollector.AddError("Entry Instruction is missing a Declaration Type. Please select from dropdown menu.");
					return null;
				}
				if (style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse))
				{
					return new H1MessageBuilder(cusEntryHeader, errorCollector, functionCode);
				}
				if (style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.DeclarationForCustomsWarehousing))
				{
					return new H2MessageBuilder(cusEntryHeader, errorCollector, functionCode);
				}
				if (style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.DeclarationForTemporaryAdmission))
				{
					return new H3MessageBuilder(cusEntryHeader, errorCollector, functionCode);
				}
				if (style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.DeclarationForInwardProcessing))
				{
					return new H4MessageBuilder(cusEntryHeader, errorCollector, functionCode);
				}
				if (style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.DeclarationForGoodsFromTheSpecialFiscalTerritories))
				{
					return new H5MessageBuilder(cusEntryHeader, errorCollector, functionCode);
				}
				if (style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration))
				{
					return cusEntryHeader.IsAnyEntryLineUsingControlledGoodsProcedure ? new I1MessageBuilderControlledGoods(cusEntryHeader, errorCollector, functionCode) :
						new I1MessageBuilder(cusEntryHeader, errorCollector, functionCode);
				}
				if (style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I))
				{
					return new C21IMessageBuilder(cusEntryHeader, errorCollector, functionCode);
				}
				if (style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.ImportClearanceRequestC21N))
				{
					return new C21NMessageBuilder(cusEntryHeader, errorCollector, functionCode);
				}
				if (style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.FinalSupplementaryDeclaration))
				{
					return new FSMessageBuilder(cusEntryHeader, errorCollector, functionCode);
				}
				if (style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.SuperReducedDataSetDeclaration))
				{
					return new H7MessageBuilder(cusEntryHeader, errorCollector, functionCode);
				}
				if (style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.ReducedDataSetDeclaration))
				{
					return new H8MessageBuilder(cusEntryHeader, errorCollector, functionCode);
				}
				if (style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.BulkImportReducedDataSet))
				{
					return new C21BMessageBuilder(cusEntryHeader, errorCollector, functionCode);
				}
				if (style.EqualsIgnoringCase(ExportDeclarationTypeList.Codes.DeclarationForExport))
				{
					return new B1MessageBuilder(cusEntryHeader, errorCollector, functionCode);
				}
				if (style.EqualsIgnoringCase(ExportDeclarationTypeList.Codes.DeclarationForOutwardProcessing))
				{
					return new B2MessageBuilder(cusEntryHeader, errorCollector, functionCode);
				}
				if (style.EqualsIgnoringCase(ExportDeclarationTypeList.Codes.DeclarationForDispatchOfGoods))
				{
					return new B4MessageBuilder(cusEntryHeader, errorCollector, functionCode);
				}
				if (style.EqualsIgnoringCase(ExportDeclarationTypeList.Codes.SimplifiedDeclarationForExport))
				{
					return new C1MessageBuilder(cusEntryHeader, errorCollector, functionCode);
				}
				if (style.EqualsIgnoringCase(ExportDeclarationTypeList.Codes.ExportClearanceRequestC21E))
				{
					return new C21EMessageBuilder(cusEntryHeader, errorCollector, functionCode);
				}
				if (style.EqualsIgnoringCase(ExportDeclarationTypeList.Codes.ExportClearanceRequestC21EEIDRNOP))
				{
					return new CENMessageBuilder(cusEntryHeader, errorCollector, functionCode);
				}
				errorCollector.AddError("CW1 does not support building message type " + style);
				return null;
			}

			if (newAmendDelete is CusdecMessageFunction.Deleted)
			{
				return new CancellationRequestMessageBuilder(objectToSend, functionCode);
			}

			if (newAmendDelete is CusdecMessageFunction.Amended)
			{
				switch (objectToSend.MessageType)
				{
					case CDSEDIMessageTypeList.Codes.AmendDeclaration:
						return new AmendmentMessageBuilder(objectToSend, functionCode);
					case CDSEDIMessageTypeList.Codes.NilAmendment:
						return new NilAmendmentMessageBuilder(objectToSend, functionCode);
					case CDSEDIMessageTypeList.Codes.FecChallenge:
						return new FECAmendmentMessageBuilder(objectToSend, functionCode);
					case CDSEDIMessageTypeList.Codes.ArrivalNotification:
						return new ArrivalAmendmentMessageBuilder(objectToSend, functionCode);
				}
			}

			errorCollector.AddError("CW1 cannot make that type of message, please select a valid option from the dropdown list");
			return null;
		}
	}

	public static class CusdecMessageFunctionExtension
	{
		public static string GetCdsFunctionCode(this CusdecMessageFunction function)
		{
			switch (function)
			{
				case CusdecMessageFunction.New _:
					return CDSDeclarationFunctionCode.Codes.OriginalNew;
				case CusdecMessageFunction.Amended _:
				case CusdecMessageFunction.Deleted _:
					return CDSDeclarationFunctionCode.Codes.Request;
				default:
					return string.Empty;
			}
		}
	}
}
