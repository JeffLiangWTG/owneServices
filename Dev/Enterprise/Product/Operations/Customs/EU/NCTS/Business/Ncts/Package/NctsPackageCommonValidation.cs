using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public abstract class NctsPackageCommonValidation : Customs.Business.CusInvPackValidation
	{
		public NctsPackageCommonValidation(NctsPackage parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			CheckRuleC0670();
		}

		protected internal virtual void CheckRuleC0670() { }

		protected override void CheckB5_UnitType()
		{
			base.CheckB5_UnitType();
			ListValidation.MessageErrorIfInvalidCode(Parent.B5_UnitTypeInfo);
		}

		protected override void CheckB5_MarksAndNumbers()
		{
			base.CheckB5_MarksAndNumbers();

			CheckB5_MarksAndNumbers_Mandatory();
		}

		protected virtual void CheckB5_MarksAndNumbers_Mandatory()
		{
			if (ShouldCheckMandatoryMarksAndNumbers)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.B5_MarksAndNumbersInfo);
			}
		}

		protected virtual bool ShouldCheckMandatoryMarksAndNumbers => !Parent.B5_UnitType.IsEmpty && !Parent.IsUnpacked && !Parent.IsBulk;

		protected override void CheckB5_UnitCount()
		{
			base.CheckB5_UnitCount();

			CheckB5_UnitCount_Mandatory();
		}

		protected virtual void CheckB5_UnitCount_Mandatory()
		{
			if (!Parent.B5_UnitType.IsEmpty && Parent.IsUnpacked)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.B5_UnitCountInfo);
			}
		}

		protected new NctsPackage Parent => (NctsPackage)base.Parent;
	}
}
