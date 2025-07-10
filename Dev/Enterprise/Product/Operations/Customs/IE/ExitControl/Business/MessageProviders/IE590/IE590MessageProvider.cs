using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.ExitControl.Business.AES
{
	public class IE590MessageProvider : ExitReportMessageProvider, IIE590Header, IIE590ExportOperation, IIE590GoodsShipment, IIE590PersonConfirmingExit
	{
		public IE590MessageProvider(CusExitReport exitReport)
			: base(exitReport)
		{
			Consignment = new IE590ConsignmentProvider(exitReport, exitHeader);
		}

		#region IIE590Header Members

		public IIE590ExportOperation ExportOperation => this;
		IIE590ExportOperation IIE590Header.ExportOperation => this;

		public string CustomsOfficeOfExitActual => exitReport.CER_OfficeOfExit;
		string IIE590Header.CustomsOfficeOfExitActual => CustomsOfficeOfExitActual;

		public DateTime PassageExitDate => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(exitReport.CER_DateTime.ToZDateTime());
		DateTime IIE590Header.PassageExitDate => PassageExitDate;

		public IIE590PersonConfirmingExit PersonConfirmingExit => this;
		IIE590PersonConfirmingExit IIE590Header.PersonConfirmingExit => PersonConfirmingExit;

		public IIE590GoodsShipment GoodsShipment => this;
		IIE590GoodsShipment IIE590Header.GoodsShipment => GoodsShipment;

		#endregion

		#region IIE590ExportOperation Members
		public string AdditionalDeclarationType => exitReport.CER_AdditionalDeclarationType;
		string IIE590ExportOperation.AdditionalDeclarationType => AdditionalDeclarationType;
		public string ManifestNumber => null;
		string IIE590ExportOperation.ManifestNumber => ManifestNumber;

		public bool DiscrepanciesExistAtExit => exitReport.CER_Calc_Discrepancies;
		bool IIE590ExportOperation.DiscrepanciesExistAtExit => DiscrepanciesExistAtExit;

		public string MRN => exitReport.Consignment?.CXC_MovementReference ?? ZString.Empty;
		string IMRN.MRN => MRN;

		#endregion

		#region IIE590GoodsShipment Members

		public IIE590Consignment Consignment { get; }
		IIE590Consignment IIE590GoodsShipment.Consignment => Consignment;

		public IReadOnlyCollection<IIE507And590CommonGoodsItem> GoodsItems => goodsItems ?? (goodsItems = GetGoodsItemDetails().Select(x => new IE507And590CommonGoodsItemProvider(x.consignmentItem, x.grossMass, x.netMass, x.packageData)).ToArray());
		IIE507And590CommonGoodsItem[] goodsItems;
		IReadOnlyCollection<IIE507And590CommonGoodsItem> IIE590GoodsShipment.GoodsItems => GoodsItems;
		#endregion

		#region IIE590PersonConfirmingExit Members

		public string Role => "1";
		string IIE590PersonConfirmingExit.Role => Role;

		public string IdentificationNumber => exitHeader.Carrier != null ?
			EU.Business.EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(exitHeader.Carrier.Header, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori) : ZString.Empty;
		string IIE590PersonConfirmingExit.IdentificationNumber => IdentificationNumber;

		public string Reference => null;
		string IIE590PersonConfirmingExit.Reference => Reference;

		#endregion
	}
}
