using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class OTHRLine : MessageLine
	{
		public OTHRLine(ExportAWBHeader aWBHeader, ExportAWBOtherCharges aWBOtherCharges)
		{
			this.AWBHeader = aWBHeader;
			this.AWBOtherCharges = aWBOtherCharges;
		}

		#region Overrides

		protected override int FieldCount
		{
			get { return JXCConstants.OTHRFieldCount; }
		}

		protected override ZString LineType
		{
			get { return JXCConstants.LineTypes.OTHR; }
		}

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.OTHRFieldPositions.PrepaidOrCollect, PrepaidOrCollect);
			dataRow.SetField(JXCConstants.OTHRFieldPositions.IATAChargeCode1, IATACode);
			dataRow.SetField(JXCConstants.OTHRFieldPositions.ChargeAmount1, AWBOtherCharges.EO_Amount);
			dataRow.SetField(JXCConstants.OTHRFieldPositions.ChargeDescription1, AWBOtherCharges.EO_ChargeDescription, JXCConstants.OTHRFieldBoundaries.ChargeDescriptionMaxLength);
		}

		ZString IATACode
		{
			get
			{
				return (AWBHeader.EH_Table == JobShipmentSchema.Constants.TableName)
					? AWBOtherCharges.EO_ChargeCode + Core.Constants.AWB.EntitlementCode.Agent
					: AWBOtherCharges.EO_ChargeCode + AWBOtherCharges.EO_EntitlementCode;
			}
		}

		ZString PrepaidOrCollect
		{
			get
			{
				return (AWBHeader.EH_Table == JobShipmentSchema.Constants.TableName && AWBHeader.EH_OtherPPDCOL == "BTH")
					? AWBOtherCharges.EO_EntitlementCode
					: AWBHeader.EH_OtherPPDCOL.Left(1);
			}
		}

		#endregion

		readonly ExportAWBHeader AWBHeader;
		readonly ExportAWBOtherCharges AWBOtherCharges;
	}
}

#region Overrides
#endregion
#region Implementation
#endregion
