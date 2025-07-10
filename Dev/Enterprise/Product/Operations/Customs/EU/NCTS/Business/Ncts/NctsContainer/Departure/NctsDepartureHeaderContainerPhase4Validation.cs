using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureHeaderContainerPhase4Validation : NctsDepartureHeaderContainerValidation
	{
		public NctsDepartureHeaderContainerPhase4Validation(NctsDepartureHeaderContainer parent) : base(parent)
		{
		}

		protected override void CheckBC_ContainerNum()
		{
			base.CheckBC_ContainerNum();

			ContainerNumberValidation.WarnIfInvalid(Parent.BC_ContainerNumInfo);
		}

		protected override void CheckBC_Seal1()
		{
			base.CheckBC_Seal1();

			var parent = Parent;
			if (parent.AdditionalSeals.Count > 0)
			{
				MandatoryValidation.CheckEntered(parent.BC_Seal1Info);
			}
			if (!parent.Seal1.IsEmpty && parent.Header?.HeaderContainersSeals.Where(x => x.Equals(parent.Seal1)).Skip(1).Any() == true)
			{
				parent.BC_Seal1Info.AddWarning(Res.GetString("4E95AD47-E61B-4AE9-9816-AB24FB0FD4ED", "Duplicate Seal 1 Number entered."));
			}
		}

		protected override void CheckBC_Seal2()
		{
			base.CheckBC_Seal2();

			var parent = Parent;
			if (parent.AdditionalSeals.Count > 0)
			{
				MandatoryValidation.CheckEntered(parent.BC_Seal2Info);
			}
			if (!parent.Seal2.IsEmpty && (parent.Header?.HeaderContainersSeals.Where(x => x.Equals(parent.Seal2)).Skip(1).Any() ?? false))
			{
				parent.BC_Seal2Info.AddWarning(Res.GetString("BF188921-2739-4A9E-BD5A-176DF7BC12B1", "Duplicate Seal 2 Number entered."));
			}
		}
	}
}
