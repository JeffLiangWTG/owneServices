
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class GsumShipmentWrapper : JXCExportHeaderWrapper, IObsoleteValidation
	{
		public GsumShipmentWrapper(JASForwardingShipment shipment)
			: base(shipment)
		{
		}

		public JASOrgHeader OriginOfficeOrg
		{
			get
			{
				JASOrgHeader result = null;

				if (Shipment.DepartureConsol != null && Shipment.DepartureConsol.SendingForwarder != null)
				{
					result = (JASOrgHeader)Shipment.DepartureConsol.SendingForwarder;
				}

				return result;
			}
		}

		protected override JASOrgHeader SendingForwarderCore
		{
			get
			{
				return (CurrentOrgProxy != null && !CurrentOrgProxy.OfficeCode.IsEmpty)
					? CurrentOrgProxy
					: JASDataRegistry.Instance.GetJASWWOrganisation(Factory);
			}
		}

		protected override JASOrgHeader ReceivingForwarderCore
		{
			get
			{
				JASOrgHeader result = null;

				JASForwardingConsol relevantCounterpartyConsol = GetRelevantCounterpartyConsol();
				if (relevantCounterpartyConsol != null)
				{
					result = (IsCurrentOrgProxy(relevantCounterpartyConsol.ReceivingForwarder))
						? relevantCounterpartyConsol.SendingForwarder
						: relevantCounterpartyConsol.ReceivingForwarder;
				}

				if (result == null || result.OfficeCode.IsEmpty)
				{
					result = JASDataRegistry.Instance.GetJASWWOrganisation(Factory);
				}

				return result;
			}
		}

		protected override ZString FreightDestCore
		{
			get { return Shipment.JS_RL_NKDestination; }
		}

		bool IsCurrentOrgProxy(JASOrgHeader orgHeader)
		{
			return orgHeader != null && CurrentOrgProxy != null && orgHeader.PK == CurrentOrgProxy.PK;
		}

		JASForwardingConsol GetRelevantCounterpartyConsol()
		{
			JASForwardingConsol result = null;

			if (Shipment.IsExport())
			{
				result = (JASForwardingConsol)Shipment.ArrivalConsol;
			}
			else if (Shipment.IsImport())
			{
				result = (JASForwardingConsol)Shipment.DepartureConsol;
			}

			if (result == null && Shipment.Consols.Count > 0)
			{
				result = (JASForwardingConsol)Shipment.Consols[0];
			}

			return result;
		}

		JASOrgHeader CurrentOrgProxy
		{
			get { return GlbCompany.CurrentCompany.OrgProxy as JASOrgHeader; }
		}

		public JASForwardingShipment Shipment
		{
			get { return (JASForwardingShipment)WrappedBizO; }
		}
	}
}
