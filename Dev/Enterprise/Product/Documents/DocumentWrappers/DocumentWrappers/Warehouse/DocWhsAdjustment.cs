using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsAdjustment : DocWhsDocket
	{
		#region Static

		public static DocWhsAdjustment New(WhsAdjustment whsAdjustment, BusinessObjectFactory factoryToWrap)
		{
			return (whsAdjustment == null) ? null : new DocWhsAdjustment(whsAdjustment, factoryToWrap);
		}

		public static DocWhsAdjustment New(WhsDocketLabelControl docketLabel, BusinessObjectFactory factoryToWrap)
		{
			return (docketLabel == null) ? null : ((docketLabel.Docket == null) ? null : new DocWhsAdjustment(docketLabel, factoryToWrap));
		}

		#endregion

		#region Contructors

		DocWhsAdjustment(WhsAdjustment whsAdjustment, BusinessObjectFactory factoryToWrap)
			: base(whsAdjustment, factoryToWrap)
		{
		}

		DocWhsAdjustment(WhsDocketLabelControl docketLabel, BusinessObjectFactory factoryToWrap)
			: base(docketLabel, factoryToWrap)
		{
		}

		#endregion

		#region Related Business Objects

		protected override DocOrganisation TransportCoCore => null;

		protected override DocDocAddress TransportCoAddressCore => null;

		WhsAdjustment WhsAdjustment
		{
			get { return (WhsAdjustment)WrappedObject; }
		}

		#endregion

		#region Business Objects Overrides

		protected override DocWhsDocketLineCollection GetDocketLines()
		{
			return new DocWhsAdjustmentLineCollection(WhsAdjustment.Lines, Factory);
		}

		#endregion

		#region Properties

		protected override ZString CarrierNameCore => ZString.Empty;

		#endregion
	}
}
