using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.IL.MessageDefinitions.GPM;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business
{
	public class MessageGatePassMovementDetailsWrapper : IMessageGatePassMovementDetails
	{
		MessageGatePassMovementDetailsWrapper(GatePassMovementDocDataObject gatePassMovement, bool isCancelActionTypeCode)
		{
			this.gatePassMovement = gatePassMovement;
			this.isCancelActionTypeCode = isCancelActionTypeCode;
		}

		internal static MessageGatePassMovementDetailsWrapper NewOrNull(GatePassMovementDocDataObject gatePassMovement, bool isCancelActionTypeCode)
			=> gatePassMovement != null ? new MessageGatePassMovementDetailsWrapper(gatePassMovement, isCancelActionTypeCode) : null;

		IMessageGatePassMovementCargoIdentifier IMessageGatePassMovementDetails.CargoIdentifier
			=> MessageGatePassMovementCargoIdentifierWrapper.NewOrNull(gatePassMovement);

		int? IMessageGatePassMovementDetails.CargoTypeCode => MapCargoType(gatePassMovement.CargoType.Code);

		int IMessageGatePassMovementDetails.CustomerActivityType => Constants.GatePassMovement.ActivityType.Forwarder;

		bool IMessageGatePassMovementDetails.ExportFromDifferentPortIndication => false;

		ICollection<IMessageGatePassMovementDestinationSite> IMessageGatePassMovementDetails.GatepassDestinationSite
			=> new Collection<IMessageGatePassMovementDestinationSite>() { MessageGatePassMovementDestinationSiteWrapper.NewOrNull(gatePassMovement) };

		int IMessageGatePassMovementDetails.GatepassNumber => ZInt.Zero;

		string IMessageGatePassMovementDetails.OriginSiteCode => gatePassMovement.OriginSite.Code;

		int IMessageGatePassMovementDetails.ProcessTypeCode => int.TryParse(gatePassMovement.ProcessType, out var result) ? result : ZInt.Zero;

		DateTime IMessageGatePassMovementDetails.RequestDate => ZDateTime.UtcNow.ToDateTime();

		int IMessageGatePassMovementDetails.UpdateCode
			=> isCancelActionTypeCode ? Constants.GatePassMovement.ActionTypeCode.Cancel : Constants.GatePassMovement.ActionTypeCode.New;

		int? IMessageGatePassMovementDetails.EscortingCustomerId => null;

		int? IMessageGatePassMovementDetails.EscortingCustomerType => null;

		int? IMessageGatePassMovementDetails.InternalPackingQuantity => null;

		bool? IMessageGatePassMovementDetails.BackToPortIndication => null;

		ItpgIdentifier IMessageGatePassMovementDetails.TpgIdentifier => null;

		IMessageGatePassMovementCargoIdentifier IMessageGatePassMovementDetails.CargoIdentityInDestinationPort => null;

		string IMessageGatePassMovementDetails.ExternalId => GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.Israel, OrgCusCode.CodeTypes.VATCode);

		IMessageGatePassMovementOriginalExportDeliveryDocumentIdentification IMessageGatePassMovementDetails.OriginalExportDeliveryDocumentIdentification => null;

		int? MapCargoType(ZString cargoType)
		{
			var map = ZZRefCusMapCombined.GetCW1CodeToCustomsCodeMapping(gatePassMovement.Factory, Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusMaps.CargoTypes, ZDateTime.Now);

			if (map.TryGetValue(cargoType, out var cw1Code))
			{
				return int.TryParse(cw1Code, out var code) ? code : null;
			}

			return null;
		}

		readonly GatePassMovementDocDataObject gatePassMovement;
		readonly bool isCancelActionTypeCode;
	}
}
