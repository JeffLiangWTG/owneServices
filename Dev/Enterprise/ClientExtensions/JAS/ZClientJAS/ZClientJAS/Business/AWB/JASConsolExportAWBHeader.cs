using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.JXC;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.JAS.Business.AWB
{
	public class JASConsolExportAWBHeader : ConsolExportAWBHeader, IJASExportAWBHeader
	{
		public JASConsolExportAWBHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
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
			return new JASConsolExportAWBRateLineCollection(this);
		}
	}
}
