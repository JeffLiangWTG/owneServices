using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class JobDeclarationTransportValidation : EU.Business.Declaration.JobDeclarationTransportValidation
	{
		public JobDeclarationTransportValidation(Transport transport)
			: base(transport)
		{
		}

		protected override void CheckJW_LegOrder()
		{
			base.CheckJW_LegOrder();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JW_LegOrderInfo);
			var legOrder = Parent.JW_LegOrder;
			if (legOrder > ZByte.Zero)
			{
				var parentPK = Parent.PK;
				if (Parent.OtherParentTransports.Any(x => x.JW_LegOrder == legOrder && x.PK != parentPK))
				{
					Parent.JW_LegOrderInfo.AddMessageError(Res.GetString("5a8cfa2d-d06c-4211-af11-40bc5385b5ad", "This Leg No. has already been entered."));
				}
			}
		}
	}
}
