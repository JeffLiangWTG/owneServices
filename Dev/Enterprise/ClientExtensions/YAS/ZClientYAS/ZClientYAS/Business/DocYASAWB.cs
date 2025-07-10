using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.YAS.Business
{
	public class DocYASAWB : DocAWB
	{
		#region Construction

		protected DocYASAWB(ExportAWBHeader exportAWBHeader, BusinessObjectFactory factoryToWrap)
			: base(exportAWBHeader, factoryToWrap)
		{
		}

		public new static DocAWB New(ExportAWBHeader exportAWBHeader, BusinessObjectFactory factoryToWrap)
		{
			return (exportAWBHeader == null) ? null : new DocYASAWB(exportAWBHeader, factoryToWrap);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		public ZString EFreightIndicator
		{
			get
			{
				return (DocShipment != null && DocShipment is DocYASForwardingShipment) ?
							((DocYASForwardingShipment)DocShipment).EFreightIndicator : ZString.Empty;
			}
		}
	}
}
