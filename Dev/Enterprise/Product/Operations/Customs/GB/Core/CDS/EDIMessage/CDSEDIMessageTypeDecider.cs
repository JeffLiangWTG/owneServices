using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSEDIMessageTypeDecider : TypeDecider, Integration.Customs.GB.GBCDS.IGBCDSEDIMessageTypeDecider
	{
		public override Type GetTypeForNew() => null;

		public override Type GetTypeForBinding() => null;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var messageType = row[EDIMessageSchema.Constants.EM_MessageType].ToString().Trim();
			switch (messageType)
			{
				case CDSEDIMessageTypeList.Codes.SynchronousResponse:
					return typeof(CDSSynchronousResponseEDIMessage);
				case CDSEDIMessageTypeList.Codes.EHubErrorResponse:
					return typeof(CDSErrorResponseEDIMessage);
				case CDSEDIMessageTypeList.Codes.Response:
					return typeof(CDSResponseEDIMessage);
				case CDSEDIMessageTypeList.Codes.InventoryLinkingControlResponse:
					return typeof(CDSInventoryLinkingControlResponseEDIMessage);
				case CDSEDIMessageTypeList.Codes.InventoryLinkingMovementResponse:
					return typeof(CDSInventoryLinkingMovementResponseEDIMessage);
				case CDSEDIMessageTypeList.Codes.InventoryLinkingMovementTotalsResponse:
					return typeof(CDSInventoryLinkingMovementTotalsResponseEDIMessage);
				case CDSEDIMessageTypeList.Codes.InventoryLinkingQueryResponse:
					return typeof(CDSInventoryLinkingQueryResponseEDIMessage);
				case CDSEDIMessageTypeList.Codes.InventoryLinkingConsolidationRequest:
					return typeof(CDSInventoryLinkingConsolidationRequestEDIMessage);
				case CDSEDIMessageTypeList.Codes.InventoryLinkingMovementRequest:
					return typeof(CDSInventoryLinkingMovementRequestEDIMessage);
				case CDSEDIMessageTypeList.Codes.InventoryLinkingQueryRequest:
					return typeof(CDSInventoryLinkingQueryRequestEDIMessage);
				case CDSEDIMessageTypeList.Codes.NewDeclaration:
					return typeof(CDSNewDeclarationEDIMessage);
				case CDSEDIMessageTypeList.Codes.AmendDeclaration:
					return typeof(CDSAmendDeclarationEDIMessage);
				case CDSEDIMessageTypeList.Codes.CancelDeclaration:
					return typeof(CDSCancelDeclarationEDIMessage);
				case CDSEDIMessageTypeList.Codes.NewAmendment:
					return typeof(CDSAmendmentComparisonEDIMessage);
				case CDSEDIMessageTypeList.Codes.QueryResponse:
					return typeof(CDSDeclarationInfoResponseEDIMessage);
				case CDSEDIMessageTypeList.Codes.ArrivalNotification:
					return typeof(CDSArrivalAmendmentDeclarationEDIMessage);
				case CDSEDIMessageTypeList.Codes.FecChallenge:
					return typeof(CDSFECAmendmentDeclarationEDIMessage);
				case CDSEDIMessageTypeList.Codes.NilAmendment:
					return typeof(CDSNilAmendmentDeclarationEDIMessage);
				case CDSEDIMessageTypeList.Codes.CDSPentantAcaMessage:
					return typeof(CDSPentantAcaMessage);
				case CDSEDIMessageTypeList.Codes.DocumentUploadConfirmation:
					return typeof(CDSDocumentUploadConfirmationResponse);
				case CDSEDIMessageTypeList.Codes.MasterQueryDeclaration:
					return typeof(CDSInventoryLinkingMasterQueryRequestEDIMessage);
				default:
					{
						var applicationCode = row[EDIMessageSchema.Constants.EM_ApplicationCode].ToString().Trim();
						switch (applicationCode)
						{
							case EDIMessage.ApplicationCodes.GbCDSViaCCSUK:
								return typeof(GbEDIMessage);
							case EDIMessage.ApplicationCodes.GbCDSDISQuery:
								return typeof(CDSDISQueryMessage);
							default:
								return typeof(CDSEDIMessage);
						}
					}
			}
		}
	}
}
