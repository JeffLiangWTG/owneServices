using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsTransfer : DocWhsDocket
	{
		#region Static

		public static DocWhsTransfer New(WhsTransfer whsTransfer, BusinessObjectFactory factoryToWrap)
		{
			return (whsTransfer == null) ? null : new DocWhsTransfer(whsTransfer, factoryToWrap);
		}

		public static DocWhsTransfer New(WhsDocketLabelControl docketLabel, BusinessObjectFactory factoryToWrap)
		{
			return (docketLabel == null) ? null : ((docketLabel.Docket == null) ? null : new DocWhsTransfer(docketLabel, factoryToWrap));
		}

		#endregion

		#region Contructors

		DocWhsTransfer(WhsTransfer whsTransfer, BusinessObjectFactory factoryToWrap)
			: base(whsTransfer, factoryToWrap)
		{
		}

		DocWhsTransfer(WhsDocketLabelControl docketLabel, BusinessObjectFactory factoryToWrap)
			: base(docketLabel, factoryToWrap)
		{
		}

		#endregion

		#region Related Business Objects

		protected override DocOrganisation TransportCoCore => null;

		protected override DocDocAddress TransportCoAddressCore => null;

		WhsTransfer WhsTransfer
		{
			get { return (WhsTransfer)WrappedObject; }
		}

		#endregion

		#region Business Objects Overrides

		protected override DocWhsDocketLineCollection GetDocketLines()
		{
			return new DocWhsTransferLineCollection(WhsTransfer.Lines, Factory);
		}

		#endregion

		#region Properties

		#region ZString Fields

		protected override ZString CarrierNameCore => ZString.Empty;

		public ZString TransferConfimationDocumentTitle
		{
			get
			{
				ZString result = Res.GetString("f367ff97-8676-45fe-b9b4-8d473b92a040", "Transfer Confirmation");
				if (((WhsTransfer)WrappedObject).IsInterWarehouseTransfer)
				{
					result = Res.GetString("5c1678a8-098d-480e-834f-9e9a1bc08a27", "Inter-Warehouse Transfer Confirmation");
				}

				return result;
			}
		}

		#endregion

		#endregion
	}
}
