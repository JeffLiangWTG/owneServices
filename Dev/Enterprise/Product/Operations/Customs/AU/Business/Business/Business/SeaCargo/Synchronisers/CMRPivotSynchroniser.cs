using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPivotSynchroniser : PivotSynchroniser
	{
		public CMRPivotSynchroniser(CMRHouseBillSynchroniser houseSynchroniser, CusSCAPivot destination, PackLine source, CommonShipment shipment)
			: base(houseSynchroniser, destination, source, shipment)
		{
		}

		protected override void PackTypeSynchroniser_Format(object sender, Customs.Business.FieldSynchroniser.ConvertEventArgs e)
		{
			if (e.Value is ZString)
			{
				ZString packType = (ZString)e.Value;
				bool multiplePackageTypesExist = false;
				foreach (PackLine additionalPackLine in AdditionalPackLinesToWatch)
				{
					if (packType != additionalPackLine.JL_F3_NKPackType)
					{
						multiplePackageTypesExist = true;
					}
				}
				if (multiplePackageTypesExist)
				{
					e.Value = new ZString(CMRPackageTypes.Codes.UnpackedOrPacked);
				}
				else
				{
					e.Value = SeaCargoUtilities.ConvertPkgUnitToCMRPackageType(packType);
				}
			}
		}

		protected override void PackageCountSynchroniser_Format(object sender, Customs.Business.FieldSynchroniser.ConvertEventArgs e)
		{
			if (e.Value is ZInt)
			{
				ZInt totalPackageCount = 0;
				if (Destination.CV_AssociatedContainer != CusSCAPivot.Bulk)
				{
					totalPackageCount = (ZInt)e.Value;
					foreach (PackLine additionalPackLine in AdditionalPackLinesToWatch)
					{
						totalPackageCount += additionalPackLine.JL_PackageCount;
					}
				}
				e.Value = totalPackageCount;
			}
		}
	}
}
