using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Accounting.GUI
{
	public class ProfitShareActionMethod : OperationalActionMethod
	{
		public ProfitShareActionMethod(Type actionSupporterType)
			: base(new ZGuid("17602048-2a4b-41de-af95-3069d1943810"))
		{
			this.actionSupporterType = actionSupporterType;
		}

		readonly Type actionSupporterType;

		public override string Description
		{
			get { return Res.GetString("cc57e28a-a8c4-47b9-911a-bc126c9e7797", "Creates and posts Profit Share Charges based on Profit Share Agreement."); }
		}

		public override string Name
		{
			get { return Res.GetString("aa875cc5-d9e8-42fa-a0f8-53a4c05a4924", "Create Profit Share Charges"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new ProfitShareActionMethodApplicator(actionSupporterType);
		}
	}
}
