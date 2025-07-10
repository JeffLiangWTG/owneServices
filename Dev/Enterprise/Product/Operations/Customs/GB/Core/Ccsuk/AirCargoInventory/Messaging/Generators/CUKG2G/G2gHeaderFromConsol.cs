using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Generators
{
	class G2gHeaderFromConsol : IG2gHeader
	{
		public G2gHeaderFromConsol(CustomsExportConsolIntegrationWrapper consolWrapper)
		{
			forwardingConsol = consolWrapper.ForwardingConsol;
			mawbExportAddInfo = consolWrapper.MawbExportHelper;
		}

		ZString IG2gHeader.MasterUcr
		{
			get { return mawbExportAddInfo.ME_MasterUCR; }
		}

		ZString IG2gHeader.MawpAndMawn
		{
			get { return forwardingConsol.JK_MasterBillNum; }
		}

		ZString IG2gHeader.CustomsAuthorisationReference
		{
			get { return G2gUtilities.GetFirstNumber(G2gUtilities.CodeType.CAR, forwardingConsol.Numbers); }
		}

		ZString IG2gHeader.MasterSOE
		{
			get { return mawbExportAddInfo.ME_ChiefMasterStyleOfEntry; }
		}

		ZString IG2gHeader.Airport
		{
			get { return mawbExportAddInfo.ME_ExportLocation; }
		}

		ZString IG2gHeader.Shed
		{
			get { return mawbExportAddInfo.ME_ExportShed; }
		}

		ZString IG2gHeader.AgentBadge
		{
			get { return G2gUtilities.GetAgentFromPima(mawbExportAddInfo.ME_Profile); }
		}

		ZString IG2gHeader.AgentType
		{
			get { return G2gUtilities.GetAgentTypeFromPima(GetCredentialForPima()?.PIMA ?? ZString.Empty); }
		}

		IEnumerable<IG2gConsignment> IG2gHeader.Consignments
		{
			get
			{
				foreach (ForwardingShipment shipment in forwardingConsol.Shipments)
				{
					if (shipment.JS_RL_NKOrigin.StartsWith(Core.Constants.CountryCodes.UnitedKingdom))
					{
						yield return new G2gConsignmentFromShipment(shipment, forwardingConsol.IsDirect, this);  // we rely on Freight's validation to ensure that a direct consol has exactly one shipment
					}
				}
			}
		}

		public CredentialsSetting GetCredentialForPima()
		{
			var credentials = GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			return credentials.Cast<CredentialsSetting>().FirstOrDefault(cred => cred.BadgeCode == mawbExportAddInfo.ME_Profile);
		}

		readonly ForwardingConsol forwardingConsol;
		readonly MawbExportAddInfo mawbExportAddInfo;
	}
}
