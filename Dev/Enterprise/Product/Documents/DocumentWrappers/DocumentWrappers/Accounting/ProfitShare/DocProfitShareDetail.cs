using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocProfitShareDetail : DocBaseWrapper, IDocTypeCode
	{
		protected DocProfitShareDetail(ProfitShareDetail profitShareDetail, BusinessObjectFactory factoryToWrap)
			: base(profitShareDetail, factoryToWrap)
		{
			DocTypeCode = "PRS";
		}

		public static DocProfitShareDetail New(ProfitShareDetail profitShareDetail, BusinessObjectFactory factoryToWrap)
		{
			return profitShareDetail != null ? new DocProfitShareDetail(profitShareDetail, factoryToWrap) : null;
		}

		ProfitShareDetail ProfitShareDetail
		{
			get { return (ProfitShareDetail)WrappedObject; }
		}

		public override BusinessObject DeliveryContact
		{
			get
			{
				if (deliveryContact == null)
				{
					OrgContact contact = new DefaultContactFinder(ProfitShareDetail.ProfitShareParty).DefaultContact(ContactType.Receivables, "");
					deliveryContact = new DocAutoDelivery().GetDeliveryDetailsForContact(contact);
				}
				return deliveryContact;
			}
		}
		DocDeliveryContact deliveryContact;

		#region Consol

		public DocForwardingConsol Consol
		{
			get { return DocForwardingConsol.New((ForwardingConsol)ProfitShareDetail.Consol, Factory); }
		}

		#endregion

		#region Organisation

		public DocOrganisation OrgToCredit
		{
			get { return DocOrganisation.New(ProfitShareDetail.ProfitShareParty, Factory); }
		}

		#endregion

		#region Shipment and Charges Details

		public DocProfitShareShipmentDetailCollection ShipmentDetails
		{
			get
			{
				if (fShipmentDetails == null)
				{
					fShipmentDetails = new DocProfitShareShipmentDetailCollection(ProfitShareDetail.ProfitShareShipmentDetails, Factory);
				}

				return fShipmentDetails;
			}
		}

		// Flattended here due to limitations of documentengine - no nested sections
		public DocJobChargeCollection ShipmentChargesDetails =>
			DocJobChargeCollection.GetCollection(this, nameof(ShipmentChargesDetails),
				(collection) =>
					{
						foreach (var shipmentDetail in ShipmentDetails.Cast<DocProfitShareShipmentDetail>())
						{
							var charges = shipmentDetail.Charges.Cast<DocJobCharge>().OrderBy(c => c.JobCharge.JR_DisplaySequence);
							foreach (var charge in charges)
							{
								charge.ProfitShareShipmentDetail = shipmentDetail;
								collection.Add(charge);
							}
						}
					});

		DocProfitShareShipmentDetailCollection fShipmentDetails;

		#endregion

		#region IDocTypeCode

		ZString DocTypeCode;

		ZString IDocTypeCode.DocTypeCode
		{
			get { return DocTypeCode; }
			set { DocTypeCode = value; }
		}

		#endregion
	}
}
