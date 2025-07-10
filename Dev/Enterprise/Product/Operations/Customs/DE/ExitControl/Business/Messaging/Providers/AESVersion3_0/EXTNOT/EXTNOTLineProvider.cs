using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public class EXTNOTLineProvider : ExitTransportLineProvider, IEXTNOTLine
	{
		public EXTNOTLineProvider(CusExitReportItem reportItem, string shipmentType) : base(reportItem)
		{
			this.shipmentType = shipmentType;
		}
		readonly string shipmentType;

		public IReadOnlyCollection<IPackage> Packages
		{
			get
			{
				if (packaging == null)
				{
					packaging = shipmentType.In(A0131ATLASTypeOfShipment.Codes.AP, A0131ATLASTypeOfShipment.Codes.FP)
						? reportItem.Report.CusExitReportItems
							.Where(x => x.ERI_CCI_ConsignmentItem == reportItem.ERI_CCI_ConsignmentItem)
							.Select(x => new PackageProvider(x, shipmentType)).ToArray()
						: Array.Empty<IPackage>();
				}
				return packaging;
			}
		}
		IReadOnlyCollection<IPackage> packaging;
	}
}
