using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ScanCusSCAOceanBill : ScanMasterBill
	{
		public ScanCusSCAOceanBill(CusSCAOceanBill oceanBill)
			: base(oceanBill)
		{ }

		public new CusSCAOceanBill MasterBill
		{
			get { return (CusSCAOceanBill)base.MasterBill; }
		}

		public CusOutturnHeader OutturnHeader
		{
			get { return SelectedUnderbond.OutturnHeader; }
		}

		public override string ValidateSelectedUnderbond()
		{
			var errorBuilder = new ZStringBuilder(base.ValidateSelectedUnderbond());
			if (errorBuilder.IsEmpty)
			{
				var containerNo = SelectedUnderbond.ContainerNumber;
				if (SelectedUnderbond.C4_DestinationPremiseID.IsEmpty)
				{
					errorBuilder.AppendLine(DestinationPremiseIDMissing);
				}
				if (containerNo.IsEmpty)
				{
					errorBuilder.AppendLine(GetContainerNumberIsMissingError(SelectedUnderbond.C4_SendersMessageReference));
				}
				if (MasterBill.CB_LloydsIMO.IsEmpty)
				{
					errorBuilder.AppendLine(LloydMissing);
				}
				if (MasterBill.CB_Voyage.IsEmpty)
				{
					errorBuilder.AppendLine(VoyageMissing);
				}
				if (MasterBill.CB_OceanBill.IsEmpty)
				{
					errorBuilder.AppendLine(OceanBillMissing);
				}
			}
			if (errorBuilder.IsEmpty)
			{
				var containerNo = SelectedUnderbond.ContainerNumber;
				var hasArrivalOutturn = false;
				var outturnHeader = OutturnHeader;
				if (outturnHeader != null)
				{
					hasArrivalOutturn = outturnHeader.Outturns.Cast<CusOutturn>()
						.Any(x => (x.C5_CargoType == CMRImportCargoTypes.Codes.FullContainerLoad || x.C5_CargoType == CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills)
						&& (x.C5_HouseBill.IsEmpty || x.C5_MasterBill.IsEmpty) && !x.C5_CargoReceiptDate.IsEmpty && x.C5_ContainerNumber == containerNo);
				}
				if (!hasArrivalOutturn)
				{
					errorBuilder.AppendLine(GetContainerArrivalHasNotBeenPostedError(containerNo, MasterBill.CB_LloydsIMO, MasterBill.CB_Voyage, SelectedUnderbond.C4_DestinationPremiseID));
				}
			}
			return errorBuilder.ToString();
		}

		internal static string DestinationPremiseIDMissing
		{
			get { return Res.GetString("5B353D34-F937-4AC2-9F70-F477750C9CDE", "The selected underbond does not have Destination Premise ID."); }
		}

		internal static string GetContainerNumberIsMissingError(ZString underbondRef)
		{
			return Res.GetString("8655C2E7-6213-4099-AC68-B6142375C665", "Container number for Underbond {0} is empty. Please enter a container number.", underbondRef);
		}

		internal static string LloydMissing
		{
			get { return Res.GetString("ED7946C0-C0F9-4BC5-8187-21DB5013BE0C", "Please enter Vessel Lloyds numb"); }
		}

		internal static string VoyageMissing
		{
			get { return Res.GetString("A34E531B-E3FE-4C39-AB01-318B57F7299A", "Please enter Voyage Number."); }
		}

		internal static string GetContainerArrivalHasNotBeenPostedError(ZString containerNo, ZString lloyd, ZString voyage, ZString premise)
		{
			return Res.GetString("794BA436-8C91-463D-A5DC-6141D3CD9608", "The DCL container arrival outturn for container {0}, vessel Lloyds {1}, voyage {2}, and destination premise {3} has not been posted."
						, containerNo, lloyd, voyage, premise);
		}

		internal static string OceanBillMissing
		{
			get { return Res.GetString("8A8737F4-9F64-4A37-9618-BE0E19873911", "Please enter Ocean bill."); }
		}
	}
}
