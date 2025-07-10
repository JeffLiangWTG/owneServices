using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using CoreConstants = Enterprise.Core.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module.OperationalActions
{
	public class LVXDeConsolidationOperationalActionMethod : OperationalActionMethod
	{
		public LVXDeConsolidationOperationalActionMethod()
			: base(new ZGuid("b2551611-e041-4711-b907-1d1ae7c44a68"))
		{
		}

		public override string Description
		{
			get { return Res.GetString("cd817d4a-3719-4584-b16d-5f7338d1508b", "De-Consolidate Courier LVS Declaration Jobs"); }
		}

		public override string Name
		{
			get { return Res.GetString("a1f542db-fa4e-4bec-8197-9cb313f3a471", "De-Consolidate"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new LVXDeConsolidationOperationalActionMethodApplicator(factory);
		}

		public override bool IsRunAgainDisabled
		{
			get { return true; }
		}

		public override bool HasControl
		{
			get { return false; }
		}

		public override bool HasSettings
		{
			get { return false; }
		}

		public override FilterRequirementList GetFilterRequirements()
		{
			var result = base.GetFilterRequirements();

			result.Add(FilterConstants.Country, new string[] { CoreConstants.CountryCodes.Canada });

			return result;
		}
	}
}
