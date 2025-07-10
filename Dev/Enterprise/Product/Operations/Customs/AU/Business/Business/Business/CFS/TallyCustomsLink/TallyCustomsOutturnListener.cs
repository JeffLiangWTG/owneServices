using System;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class TallyCustomsOutturnListener : IOutturn
	{
		public TallyCustomsOutturnListener(DepotCusOutturn outturn)
		{
			if (outturn == null)
			{
				throw new ArgumentNullException(nameof(outturn));
			}

			this.outturn = outturn;
		}

		public ZString MarksAndNumbers
		{
			get { return outturn.C5_MarksAndNumbers; }
		}

		public ZInt NumberOfPackages
		{
			get { return outturn.C5_OuterPacks; }
		}

		public void SetPackageType(ZString packageType)
		{
			outturn.C5_PackagesUnits = SeaCargoUtilities.ConvertPkgUnitToCMRPackageType(packageType);
		}

		public void SetPackagesOutturned(ZInt packagesOutturned)
		{
			outturn.C5_PackagesOutturned = packagesOutturned;
		}

#if DEBUG
		public ZInt GetPackagesOutturned()
		{
			return outturn.C5_PackagesOutturned;
		}
#endif

		public void SetDamaged(ZBool damaged)
		{
			outturn.C5_DamageIndicator = damaged;
		}

		public void SetPillaged(ZBool pillaged)
		{
			outturn.C5_PillageIndicator = pillaged;
		}

		public ZDateTime UnpackDate
		{
			get { return outturn.C5_CargoUnpackDate; }
			set
			{
				if (outturn.C5_CargoReceiptDate == outturn.C5_CargoUnpackDate)
				{
					outturn.C5_CargoReceiptDate = value;
				}
				outturn.C5_CargoUnpackDate = value;
			}
		}

		public ZBool IsDeleted
		{
			get { return outturn.IsDeleted; }
		}

		readonly DepotCusOutturn outturn;
	}
}
