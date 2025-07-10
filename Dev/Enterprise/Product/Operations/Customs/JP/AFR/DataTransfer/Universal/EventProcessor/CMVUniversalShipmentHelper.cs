using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class CMVUniversalShipmentHelper
	{
		public CMVUniversalShipmentHelper(IXmlImportLogger logger, JPAFRHeader header, EDIMessage cmvMessage)
		{
			this.header = Argument.NotNull(header, nameof(header));
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.cmvMessage = Argument.NotNull(cmvMessage, nameof(cmvMessage));
		}

		public List<JPAFRBills> BillsSent
		{
			get
			{
				if (billsSent == null)
				{
					var universalShipment = cmvMessage.GetEM_MessageTextReader().Parse<UniversalShipment>();
					var blanketChange = universalShipment.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.BlanketChange) ?? ZString.Empty;
					var billNoesSent = universalShipment.SubShipmentCollection?.Select(x => x.WayBillNumber ?? ZString.Empty).Where(x => !x.IsEmpty).ToArray() ?? Array.Empty<ZString>();
					billsSent = billNoesSent.Select(billNo =>
					{
						var bill = header.Bills.FirstOrDefault(x => x.JPB_BillNumber == billNo);
						if (bill == null)
						{
							logger.Log(Integration.LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Bill '{0}' could not be found on AFR Job '{1}'.", billNo, header.JPH_JobReference));
						}
						return bill;
					}).ToList();

					if (blanketChange == AddInfoConstants.True && !billsSent.Any())
					{
						billsSent = header.Bills.ToList();
					}
				}
				return billsSent;
			}
		}
		List<JPAFRBills> billsSent;

		public List<JPAFRBills> BillsUnsent => billsUnsent ?? (billsUnsent = header.Bills.Except(BillsSent).ToList());
		List<JPAFRBills> billsUnsent;

		readonly JPAFRHeader header;
		readonly IXmlImportLogger logger;
		readonly EDIMessage cmvMessage;
	}
}
