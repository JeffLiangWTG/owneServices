using CargoWise.Types;
using Enterprise.Customs.Module;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.FR.Module
{
	public abstract class FrOperationActionMethod : OperationalActionMethod
	{
		protected FrOperationActionMethod(ZGuid guid)
		: base(guid)
		{ }

		public override FilterRequirementList GetFilterRequirements()
		{
			var result = new FilterRequirementList();
			result.Add(new FilterIsUnderFrenchCustomsJurisdictionConstraint().Name, new string[1] { "Y" });
			return result;
		}
	}
}
