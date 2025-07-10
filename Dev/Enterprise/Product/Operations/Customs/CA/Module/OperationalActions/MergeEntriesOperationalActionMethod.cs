using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using CoreConstants = Enterprise.Core.Constants;

namespace Enterprise.Customs.CA.Module.OperationalActions
{
	public class MergeEntriesOperationalActionMethod : OperationalActionMethod
	{
		public MergeEntriesOperationalActionMethod()
			: base(new ZGuid("0E25F7C4-48E3-45A6-BECF-82859D2A0755"))
		{
		}

		public override string Name => Res.GetString("5339901C-A3F7-4349-A231-B82D075F52B7", "Merge Entries");

		public override string Description => Res.GetString("F45B2AF5-BF69-4B23-9EA7-7FFFF8283E8F", "Merge Entries for Declarations");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new MergeEntriesOperationalActionMethodApplicator(factory);
		}

		public override FilterRequirementList GetFilterRequirements()
		{
			var result = base.GetFilterRequirements();

			result.Add(FilterConstants.Country, new string[] { CoreConstants.CountryCodes.Canada });

			return result;
		}
	}
}
