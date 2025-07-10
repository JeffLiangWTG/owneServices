using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsPackagePhase4Validation : EU.NCTS.Business.NctsPackagePhase4Validation
	{
		public NctsPackagePhase4Validation(NctsPackage parent) : base(parent)
		{
		}

		public new NctsPackage Parent => (NctsPackage)base.Parent;

		protected override void CheckB5_UnitType()
		{
			base.CheckB5_UnitType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.B5_UnitTypeInfo);
		}

		protected override void CheckB5_UnitCount()
		{
			if (!Parent.IsBulk && !Parent.IsUnpacked && !Parent.B5_UnitType.IsEmpty)
			{
				MandatoryValidation.CheckNotNegative(Parent.B5_UnitCountInfo);
			}
		}
	}
}
