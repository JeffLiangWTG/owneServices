using System.Collections;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.JXC;
using Enterprise.Client.JAS.Business.JXC.Import;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.AWB
{
	public class JASShipmentExportAWBHeader : ShipmentExportAWBHeader, IJASExportAWBHeader
	{
		public JASShipmentExportAWBHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString DeclaredValueCurrency
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency.RX_Code; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public IJobChargeData[] CreateChargesData(GlbBranch branch)
		{
			ArrayList list = new ArrayList();

			if (branch != null)
			{
				if (EH_WeightCOL)
				{
					ZGuid freightChargeCodeGuid = new ZGuid(Env.Registry.GetFreightChargeCode(branch.GB_GC.ToGuid()));
					AccChargeCode freightChargeCode = (AccChargeCode)Factory.Load(typeof(AccChargeCode), freightChargeCodeGuid);
					list.Add(new JobChargeData(freightChargeCode.AC_Code, freightChargeCode.AC_Desc, EH_Currency, EH_TotalLineTotals));
				}

				if (EH_OtherPPDCOL != ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid)
				{
					bool includeAllOtherCharges = EH_OtherPPDCOL == ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
					foreach (ShipmentExportAWBOtherCharges otherCharge in AWBOtherCharges)
					{
						if (includeAllOtherCharges || otherCharge.EO_PPDCLT == ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect)
						{
							ZQuery chargeCodeFilter = new ZQuery(AccChargeCodeSchema.AC_IATA_ChargeCodeMap, otherCharge.EO_ChargeCode);
							chargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_GC, branch.GB_GC);
							AccChargeCode accChargeCode = (AccChargeCode)Factory.LoadTop1(typeof(AccChargeCode), chargeCodeFilter);
							ZString chargeCode = (accChargeCode != null) ? accChargeCode.AC_Code : otherCharge.EO_ChargeCode;
							JobChargeData chargeData = new JobChargeData(chargeCode, otherCharge.EO_ChargeDescription, EH_Currency, otherCharge.EO_Amount);
							list.Add(chargeData);
						}
					}
				}
			}

			return (IJobChargeData[])list.ToArray(typeof(IJobChargeData));
		}

		#region IJASExportAWBHeader Members

		JASOrgHeader IJASExportAWBHeader.Shipper
		{
			get { return (JASOrgHeader)base.ShipperDocumentaryAddress.Organisation; }
		}

		JASOrgHeader IJASExportAWBHeader.Consignee
		{
			get { return (JASOrgHeader)base.ConsigneeDocumentaryAddress.Organisation; }
		}

		ZString IJASExportAWBHeader.ShipperAccountForJXC
		{
			get
			{
				ZString result = EH_ShipperAccount;

				if (result.IsEmpty)
				{
					result = (ShipperDocumentaryAddress.Organisation != null) ? ShipperDocumentaryAddress.Organisation.OH_Code : (ZString)JXCConstants.NotAvailable;
				}

				return result;
			}
		}

		ZString IJASExportAWBHeader.ConsigneeAccountForJXC
		{
			get
			{
				ZString result = EH_ConsigneeAccount;

				if (result.IsEmpty)
				{
					result = (ConsigneeDocumentaryAddress.Organisation != null) ? ConsigneeDocumentaryAddress.Organisation.OH_Code : (ZString)JXCConstants.NotAvailable;
				}

				return result;
			}
		}

		#endregion

		protected override Freight.Forwarding.AWB.Business.ExportAWBRateLineCollection GetNewAWBRateLines()
		{
			return new JASShipmentExportAWBRateLineCollection(this);
		}
	}
}

#region Implementation
#endregion
