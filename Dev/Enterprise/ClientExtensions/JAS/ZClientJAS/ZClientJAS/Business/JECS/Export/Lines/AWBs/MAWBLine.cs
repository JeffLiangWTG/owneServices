using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class MAWBLine : AWBLine
	{
		public MAWBLine(ConsolExportAWBHeader awbHeader)
			: base(awbHeader)
		{
		}

		#region Overrides

		protected override int FieldCount
		{
			get { return JXCConstants.MAWBFieldCount; }
		}

		protected override ZString LineType
		{
			get { return (Consol != null && Consol.IsDirect) ? JXCConstants.LineTypes.DAWB : JXCConstants.LineTypes.MAWB; }
		}

		protected override JXCConstants.AWBFieldPositions NewFieldPositions()
		{
			return new JXCConstants.MAWBFieldPositions();
		}

		#endregion

		#region Set Field Values

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			base.SetFieldValues(dataRow);
			SetJASCommodityCodeField(dataRow);
		}

		protected override ZString DeclaredValueCurrency
		{
			get { return AWBHeader.EH_Currency; }
		}

		protected override ZString CustomsValueCurrency
		{
			get { return AWBHeader.EH_Currency; }
		}

		#region Header Fields

		protected override void SetHeaderFields(JXCFlatFileDataRow dataRow)
		{
			base.SetHeaderFields(dataRow);
			dataRow.SetField(JXCConstants.MAWBFieldPositions.BookedStatus, "Y");
		}

		#endregion

		#region Shipper Fields

		protected override void SetShipperFields(JXCFlatFileDataRow dataRow)
		{
			base.SetShipperFields(dataRow);
			dataRow.SetField(JXCConstants.MAWBFieldPositions.ShipperStreetAddress, AWBHeader.EH_ShipperAddress, ShipperAddressMaxLength);
			dataRow.SetField(JXCConstants.MAWBFieldPositions.ShipperAddress4, AWBHeader.EH_ShipperPlace);
		}

		protected override int ShipperAddressMaxLength
		{
			get { return JXCConstants.MAWBFieldBoundaries.ShipperAddressMaxLength; }
		}

		#endregion

		#region Consignee Fields

		protected override void SetConsigneeFields(JXCFlatFileDataRow dataRow)
		{
			base.SetConsigneeFields(dataRow);
			dataRow.SetField(JXCConstants.MAWBFieldPositions.ConsigneeStreetAddress, AWBHeader.EH_ConsigneeAddress, ConsigneeAddressMaxLength);
			dataRow.SetField(JXCConstants.MAWBFieldPositions.ConsigneeAddress4, AWBHeader.EH_ConsigneePlace);
		}

		protected override int ConsigneeAddressMaxLength
		{
			get { return JXCConstants.MAWBFieldBoundaries.ConsigneeAddressMaxLength; }
		}

		#endregion

		#region JAS Commodity Code Field

		void SetJASCommodityCodeField(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.MAWBFieldPositions.JASCommodityCode, "ZZ");
		}

		#endregion

		protected override int CarrierAddressMaxLength
		{
			get { return JXCConstants.MAWBFieldBoundaries.CarrierAddressMaxLength; }
		}

		#endregion

		#region Data Source

		JASForwardingConsol Consol
		{
			get { return (JASForwardingConsol)AWBHeader.Consol; }
		}

		new ConsolExportAWBHeader AWBHeader
		{
			get { return (ConsolExportAWBHeader)base.AWBHeader; }
		}

		#endregion
	}
}

#region Overrides
#endregion
#region Implementation
#region Consol
#endregion
#endregion
