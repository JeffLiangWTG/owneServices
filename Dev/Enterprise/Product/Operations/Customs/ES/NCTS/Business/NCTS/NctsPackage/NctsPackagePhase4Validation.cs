using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsPackagePhase4Validation : EU.NCTS.Business.NctsPackagePhase4Validation
	{
		public NctsPackagePhase4Validation(NctsPackage parent) : base(parent)
		{
		}

		public new NctsPackage Parent => (NctsPackage)base.Parent;

		protected override void CheckB5_PackageID()
		{
			base.CheckB5_PackageID();

			if (!Parent.IsArrivalMovement && ((NctsDepartureCargoDesc)Parent.Parent).IsVehicles)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.B5_PackageIDInfo, Res.GetString("1E088561-3F3E-4C0F-BE40-413C53BEFC9A", "VIN"));
			}
		}
		protected override void CheckB5_Brand()
		{
			base.CheckB5_Brand();

			if (!Parent.IsArrivalMovement && ((NctsDepartureCargoDesc)Parent.Parent).IsVehicles)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.B5_BrandInfo, Res.GetString("5A76FE3C-07F7-4264-AF2F-09625477C65D", "Brand"));
			}
		}

		protected override void CheckB5_Model()
		{
			base.CheckB5_Model();

			if (!Parent.IsArrivalMovement && ((NctsDepartureCargoDesc)Parent.Parent).IsVehicles)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.B5_ModelInfo, Res.GetString("779B542D-FEA9-4C9E-B884-BF9E60FC4C63", "Model"));
			}
		}

		protected override void CheckB5_UnitType()
		{
			base.CheckB5_UnitType();
			if (!Parent.IsArrivalMovement && !((NctsDepartureCargoDesc)Parent.Parent).IsVehicles)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.B5_UnitTypeInfo);
			}
		}

		protected override void CheckB5_UnitCount()
		{
			var currentPackage = Parent;
			var unitCountInfo = currentPackage.B5_UnitCountInfo;
			var unitCount = currentPackage.B5_UnitCount;

			MandatoryValidation.CheckNotNegative(unitCountInfo);
			if (!currentPackage.B5_UnitType.IsEmpty)
			{
				CheckUnitCountRelatedToUnitType(currentPackage, unitCountInfo, unitCount);
			}
		}

		#region Implementation

		void CheckUnitCountRelatedToUnitType(NctsPackage currentPackage, ZPropertyInfo unitCountInfo, ZLong unitCount)
		{
			if (currentPackage.IsBulk)
			{
				CheckUnitCountMustBeZero(currentPackage, unitCountInfo, unitCount);
			}
			else if (unitCount.IsEmpty)
			{
				CheckUnitCountWhenIsEmpty(currentPackage, unitCountInfo);
			}
		}

		void CheckUnitCountWhenIsEmpty(NctsPackage currentPackage, ZPropertyInfo unitCountInfo)
		{
			if (currentPackage.IsUnpacked)
			{
				MandatoryValidation.MessageErrorIfIsZero(unitCountInfo);
			}
			else
			{
				new UnitCountRelatedToUnitTypeChecker(currentPackage).Check();
			}
		}

		void CheckUnitCountMustBeZero(NctsPackage currentPackage, ZPropertyInfo unitCountInfo, ZLong unitCount)
		{
			if (unitCount > 0)
			{
				unitCountInfo.AddMessageError(Res.GetString("BE7F2892-E210-4712-9CD0-3ED80D6B0888", "For the selected unit type, Unit count must be zero."));
			}
		}

		#endregion
	}
}
