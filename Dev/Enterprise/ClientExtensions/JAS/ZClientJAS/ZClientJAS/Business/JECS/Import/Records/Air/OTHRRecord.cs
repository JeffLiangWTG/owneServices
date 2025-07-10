
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class OTHRRecord : JXCRecord
	{
		public OTHRRecord(ZString lineType, ZString lineContent)
			: base(lineType, lineContent)
		{
		}

		public void UpdateAWBHeader(ExportAWBHeader aWBHeader)
		{
			if (aWBHeader != null)
			{
				bool isHAWB = aWBHeader.EH_Table == JobShipmentSchema.Constants.TableName;
				if (!IsOtherCharges1Empty)
				{
					AddExportAWBOtherCharges(aWBHeader, IATAChargeCode1.Left(2), (isHAWB) ? ZString.Empty : IATAChargeCode1.Right(1), ChargeAmount1, ChargeDescription1, (isHAWB) ? PrepaidOrCollect : ZString.Empty);
				}

				if (!IsOtherCharges2Empty)
				{
					AddExportAWBOtherCharges(aWBHeader, IATAChargeCode2.Left(2), (isHAWB) ? ZString.Empty : IATAChargeCode2.Right(1), ChargeAmount2, ChargeDescription2, (isHAWB) ? PrepaidOrCollect : ZString.Empty);
				}

				if (!IsOtherCharges3Empty)
				{
					AddExportAWBOtherCharges(aWBHeader, IATAChargeCode3.Left(2), (isHAWB) ? ZString.Empty : IATAChargeCode3.Right(1), ChargeAmount3, ChargeDescription3, (isHAWB) ? PrepaidOrCollect : ZString.Empty);
				}
			}
		}

		void AddExportAWBOtherCharges(ExportAWBHeader aWBHeader, ZString chargeCode, ZString entitlementCode, ZDecimal amount, ZString description, ZString pPDCLT)
		{
			ExportAWBOtherCharges otherCharges = aWBHeader.AWBOtherCharges.AddNew();
			otherCharges.EO_ChargeCode = chargeCode;
			otherCharges.EO_EntitlementCode = entitlementCode.Left(otherCharges.EO_EntitlementCodeInfo.MaxLength).ToUpper();
			otherCharges.EO_Amount = amount;
			otherCharges.EO_ChargeDescription = description.Left(ExportAWBOtherCharges.Schema.EO_ChargeDescriptionMaxLength);
			pPDCLT = pPDCLT.Left(1).ToUpper();
			otherCharges.EO_PPDCLT = (pPDCLT == "P") ? ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid : (pPDCLT == "C" ? ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect : "");
		}

		#region Implementation

		bool IsOtherCharges1Empty
		{
			get { return IATAChargeCode1.IsEmpty && ChargeDescription1.IsEmpty && ChargeAmount1.IsEmpty; }
		}

		bool IsOtherCharges2Empty
		{
			get { return IATAChargeCode2.IsEmpty && ChargeDescription2.IsEmpty && ChargeAmount2.IsEmpty; }
		}

		bool IsOtherCharges3Empty
		{
			get { return IATAChargeCode3.IsEmpty && ChargeDescription3.IsEmpty && ChargeAmount3.IsEmpty; }
		}

		ZString PrepaidOrCollect
		{
			get { return Fields.GetFieldValue(JXCConstants.OTHRFieldPositions.PrepaidOrCollect); }
		}

		ZString IATAChargeCode1
		{
			get { return Fields.GetFieldValue(JXCConstants.OTHRFieldPositions.IATAChargeCode1); }
		}

		ZString ChargeDescription1
		{
			get { return Fields.GetFieldValue(JXCConstants.OTHRFieldPositions.ChargeDescription1); }
		}

		ZDecimal ChargeAmount1
		{
			get { return Fields.GetDecimalFieldValue(JXCConstants.OTHRFieldPositions.ChargeAmount1); }
		}

		ZString IATAChargeCode2
		{
			get { return Fields.GetFieldValue(JXCConstants.OTHRFieldPositions.IATAChargeCode2); }
		}

		ZString ChargeDescription2
		{
			get { return Fields.GetFieldValue(JXCConstants.OTHRFieldPositions.ChargeDescription2); }
		}

		ZDecimal ChargeAmount2
		{
			get { return Fields.GetDecimalFieldValue(JXCConstants.OTHRFieldPositions.ChargeAmount2); }
		}

		ZString IATAChargeCode3
		{
			get { return Fields.GetFieldValue(JXCConstants.OTHRFieldPositions.IATAChargeCode3); }
		}

		ZString ChargeDescription3
		{
			get { return Fields.GetFieldValue(JXCConstants.OTHRFieldPositions.ChargeDescription3); }
		}

		ZDecimal ChargeAmount3
		{
			get { return Fields.GetDecimalFieldValue(JXCConstants.OTHRFieldPositions.ChargeAmount3); }
		}

		#endregion
	}
}
