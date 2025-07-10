using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DepotCusOutturnOutturnReportLineInformation : SeaCusOutturnOutturnReportLineInformation, ISeaOutturnReportLineInformation
	{
		public DepotCusOutturnOutturnReportLineInformation(DepotCusOutturn outturn)
			: base(outturn)
		{
			this.outturn = outturn;
		}

		#region ISeaOutturnReportLineInformation Members

		protected override ZString GetContainerNumber()
		{
			return outturn.C5_ContainerNumber;
		}

		protected override ZString GetHouseBillOfLading()
		{
			return outturn.C5_HouseBill;
		}

		protected override ZString GetImportCargoType()
		{
			return outturn.C5_CargoType;
		}

		protected override ZString GetMarksAndNumbers()
		{
			return outturn.C5_MarksAndNumbers;
		}

		protected override ZString GetOceanBillOfLading()
		{
			return outturn.C5_MasterBill;
		}

		protected override ZString GetPackageType()
		{
			return outturn.IsBulk ? ZString.Empty : outturn.C5_PackagesUnits;
		}

		protected override ZString GetSealNumber()
		{
			return outturn.C5_ContainerSeal;
		}

		protected override ZString GetOutturnStatus()
		{
			return outturn.OutturnStatus;
		}

		#endregion

		protected override ZDateTime DateTimeOfCargoReceiptUnloadCore()
		{
			return outturn.C5_CargoReceiptDate;
		}

		protected override ZDateTime DateTimeOfOutturnCore()
		{
			return outturn.C5_CargoUnpackDate;
		}

		protected override MasterFiles.Business.OrgAddress GetDestinationAddress()
		{
			return outturn.Header.OutturningPremise;
		}

		protected override ZInt NumberOfPackagesCore()
		{
			return outturn.IsBulk ? ZInt.Zero : base.NumberOfPackagesCore();
		}

		protected override ZInt QuantityCore()
		{
			return outturn.IsBulk ? outturn.C5_PackagesOutturned : ZInt.Zero;
		}

		protected override ZString QuantityUnitCore()
		{
			return outturn.IsBulk ? outturn.C5_PackagesUnits : ZString.Empty;
		}

		protected override bool UnpackIndicatorCore()
		{
			return !outturn.C5_CargoUnpackDate.IsEmpty;
		}

		#region Implementation

		readonly DepotCusOutturn outturn;

		#endregion
	}
}
