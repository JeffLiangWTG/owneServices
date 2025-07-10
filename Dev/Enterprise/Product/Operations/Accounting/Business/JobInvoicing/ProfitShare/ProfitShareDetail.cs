using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	public class ProfitShareDetail : NonPersistentBusinessObject, IObsoleteValidation, ICustomLabelsConfigOrgProvider, ISourceIdentifierProvider
	{
		public ProfitShareDetail(OrgHeader profitShareParty, BusinessObjectFactory factory, string partyType)
			: base(factory)
		{
			this.ProfitShareParty = profitShareParty;
			this.PartyType = partyType;
		}

		public ProfitShareDetail()
		{
			// This empty constructor is needed for document wrapper functionality
		}

		public readonly OrgHeader ProfitShareParty;
		public readonly string PartyType;

		#region Shipment Details

		public IJobCostingPlugIn Consol { get; set; }

		public ProfitShareShipmentDetailCollection ProfitShareShipmentDetails
		{
			get
			{
				if (fProfitShareShipmentDetails == null)
				{
					fProfitShareShipmentDetails = new ProfitShareShipmentDetailCollection(Factory);
				}

				return fProfitShareShipmentDetails;
			}
		}

		ProfitShareShipmentDetailCollection fProfitShareShipmentDetails;

		public bool HasProfitShare
		{
			get
			{
				foreach (ProfitShareShipmentDetail profitShareShipmentDetail in ProfitShareShipmentDetails)
				{
					foreach (ProfitShareCharge charge in profitShareShipmentDetail.ProfitShareCharges)
					{
						if (charge.ProfitShare.HasValue)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add { }
			remove { }
		}

		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg
		{
			get { return ProfitShareParty; }
		}

		#endregion

		#region ISourceIdentifierProvider members

		ZGuid ISourceIdentifierProvider.SourceIdentifier => ProfitShareParty?.PK ?? ZGuid.Empty;

		#endregion
	}
}
