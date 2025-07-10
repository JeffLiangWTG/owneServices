using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.ExitControl.Business.AES
{
	public class IE507MessageProvider : ExitReportMessageProvider, IIE507Header, IIE507ExportOperation, IIE507GoodsShipment
	{
		public IE507MessageProvider(CusExitReport exitReport)
			: base(exitReport)
		{
			Consignment = new IE507ConsignmentProvider(exitReport, exitHeader);
		}

		#region IIE507Header Members
		public IIE507ExportOperation ExportOperation => this;
		IIE507ExportOperation IIE507Header.ExportOperation => ExportOperation;

		public string CustomsOfficeOfExitActual => exitReport.CER_OfficeOfExit;
		string IIE507Header.CustomsOfficeOfExitActual => CustomsOfficeOfExitActual;

		public IReadOnlyCollection<IAuthorisation> Authorisations => authorisations ?? (authorisations = exitConsignment.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Select(x => new AuthorisationProvider(x)).ToArray());
		IReadOnlyCollection<IAuthorisation> authorisations;
		IReadOnlyCollection<IAuthorisation> IIE507Header.Authorisations => Authorisations;

		public IIE507GoodsShipment GoodsShipment => this;
		IIE507GoodsShipment IIE507Header.GoodsShipment => GoodsShipment;
		#endregion

		#region IIE507ExportOperation Members

		public DateTime ArrivalNotificationDateAndTime => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(exitReport.CER_DateTime.ToZDateTime());

		public string ArrivalNotificationPlace => exitReport.CER_Location;

		public bool DiscrepanciesExist => exitReport.CER_Calc_Discrepancies;

		public string MRN => exitConsignment.CXC_MovementReference;

		public string StoringFlag => "0";

		#endregion

		#region IIE507GoodsShipment Members

		public IIE507Consignment Consignment { get; }
		IIE507Consignment IIE507GoodsShipment.Consignment => Consignment;

		public IReadOnlyCollection<IIE507GoodsItem> GoodsItems => goodsItems ?? (goodsItems = GetGoodsItemDetails().Select(x => new IE507GoodsItemProvider(x.consignmentItem, x.grossMass, x.netMass, x.packageData)).ToArray());
		IIE507GoodsItem[] goodsItems;
		IReadOnlyCollection<IIE507GoodsItem> IIE507GoodsShipment.GoodsItems => GoodsItems;

		#endregion
	}
}
