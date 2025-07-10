using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IL.Business
{
	public class ILEDIMessageTypeDecider : TypeDecider, Integration.Customs.IL.IEDIMessageTypeDecider
	{
		public override Type GetTypeForNew() => null;

		public override Type GetTypeForBinding() => null;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var messageType = row[EDIMessageSchema.Constants.EM_MessageType].ToString().Trim();
			var messageSubType = row[EDIMessageSchema.Constants.EM_MessageSubType].ToString().Trim();
			var defaultType = typeof(ILEDIMessage);

			return messageType switch
			{
				ILMessageTypeList.Codes.MAN => messageSubType switch
				{
					ILEDIMessageSubTypeList.Codes.ForwarderManifestRequest => typeof(ILMAN170RequestMessage),
					ILEDIMessageSubTypeList.Codes.ForwarderManifestResponse => typeof(ILMAN171ResponseMessage),
					ILEDIMessageSubTypeList.Codes.ManifestQueryRequest => typeof(ILMAN820RequestMessage),
					ILEDIMessageSubTypeList.Codes.ManifestQueryResponse => typeof(ILMAN821ResponseMessage),
					_ => defaultType
				},

				ILMessageTypeList.Codes.DLO => messageSubType switch
				{
					ILEDIMessageSubTypeList.Codes.DeliveryOrderRequest => typeof(ILDLO120RequestMessage),
					ILEDIMessageSubTypeList.Codes.DeliveryOrderResponse => typeof(ILDLO122ResponseMessage),
					_ => defaultType
				},

				ILMessageTypeList.Codes.GPM => messageSubType switch
				{
					ILEDIMessageSubTypeList.Codes.GatepassMovementRequest => typeof(ILGPM130RequestMessage),
					ILEDIMessageSubTypeList.Codes.GatepassMovementResponse => typeof(ILGPM135ResponseMessage),
					_ => defaultType
				},

				ILMessageTypeList.Codes.DEC => messageSubType switch
				{
					ILEDIMessageSubTypeList.Codes.ImportDeclarationRequest => typeof(ILDEC275RequestMessage),
					ILEDIMessageSubTypeList.Codes.ImportDeclarationResponse => typeof(ILDEC274ResponseMessage),
					ILEDIMessageSubTypeList.Codes.ExportDeclarationRequest => typeof(ILDEC751RequestMessage),
					_ => defaultType
				},

				ILMessageTypeList.Codes.DOC => messageSubType switch
				{
					ILEDIMessageSubTypeList.Codes.SupportingDocumentsRequest => typeof(ILDOC271RequestMessage),
					ILEDIMessageSubTypeList.Codes.SupportingDocumentsResponse => typeof(ILDOC276ResponseMessage),
					ILEDIMessageSubTypeList.Codes.SupportingDocumentsRqDecisionResponse => typeof(ILDOC828ResponseMessage),
					_ => defaultType
				},

				ILMessageTypeList.Codes.XER => typeof(ILXERResponseMessage),

				_ => defaultType
			};
		}
	}
}
