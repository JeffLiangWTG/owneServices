using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusInBondContainerValidation : Customs.Business.CusInBondContainerValidation
	{
		public CusInBondContainerValidation(CusInBondContainer parent)
			: base(parent)
		{
		}

		protected new CusInBondContainer Parent => (CusInBondContainer)base.Parent;

		protected override void CheckBC_ContainerNum()
		{
			base.CheckBC_ContainerNum();
			var info = Parent.BC_ContainerNumInfo;
			var containerNumber = Parent.BC_ContainerNum;
			if (containerNumber.IsEmpty)
			{
				info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(info.HumanReadableName));
			}
			else
			{
				ListValidation.WarnIfInvalidCode(info);
				if (Parent.MoveDetail is CusInBondMoveDetail moveDetail)
				{
					var pk = Parent.PK;
					if (moveDetail.Containers.Any(x => x.BC_ContainerNum == containerNumber && x.PK != pk))
					{
						info.AddMessageError(Res.GetString("ad06f0fd-c104-41fa-ab71-12b6cbc42b3b", "Same number has been added multiple times."));
					}
				}
			}
		}
	}
}
