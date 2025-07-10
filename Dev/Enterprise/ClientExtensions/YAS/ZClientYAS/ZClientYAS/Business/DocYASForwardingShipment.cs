using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.YAS.Business
{
	public class DocYASForwardingShipment : DocForwardingShipment
	{
		#region Constructors and Type Overriding

		public DocYASForwardingShipment(ForwardingShipment shipment, BusinessObjectFactory factoryToWrap)
			: base(shipment, factoryToWrap)
		{
		}

		public new static DocYASForwardingShipment New(ForwardingShipment shipment, BusinessObjectFactory factoryToWrap)
		{
			return (shipment != null) ? new DocYASForwardingShipment(shipment, factoryToWrap) : null;
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNewMethod);
		}

		static DocYASForwardingShipment OverriddenNewMethod(ForwardingShipment shipment, BusinessObjectFactory factoryToWrap)
		{
			return DocYASForwardingShipment.New(shipment, factoryToWrap);
		}

		#endregion

		public new DocYASBillOfLading BillOfLading
		{
			get
			{
				if (fBillOfLading == null)
				{
					fBillOfLading = new DocYASBillOfLading(this);
				}

				return (DocYASBillOfLading)fBillOfLading;
			}
		}

		public ZBool IsExportingYASBillOfLading
		{
			get
			{
				var typecastedobj = CommonShipment as YASForwardingShipment;
				if (typecastedobj != null)
				{
					return typecastedobj.IsExportingYASBillOfLading;
				}
				else
				{
					return ZBool.False;
				}
			}
		}

		public ZString EFreightIndicator
		{
			get
			{
				return (!CommonShipment.IsDeleted &&
					((JobDocsAndCartage)DocsAndCartage.WrappedObject).JP_CustomFlag1) ? EFreightIndicatorStr : ZString.Empty;
			}
		}

		public DocYASForwardingShipment YASColoadMasterShipment
		{
			get
			{
				return (ColoadMasterShipment != null) ?
					DocYASForwardingShipment.New(((ForwardingShipment)ColoadMasterShipment.WrappedObject), Factory) :
						null;
			}
		}

		public static ZString EFreightIndicatorStr = @"///E-FRT///";
	}
}

