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
	public class LVXConsolidationOperationalActionMethod : OperationalActionMethod
	{
		public LVXConsolidationOperationalActionMethod()
			: base(new ZGuid("d1ace445-0195-452d-bbda-ba71299921ec"))
		{
		}

		public override string Description
		{
			get { return Res.GetString("4fef15c6-6fd6-412d-84ec-858d9d2ba661", "Courier LVS Declaration Jobs"); }
		}

		public override string Name
		{
			get { return Res.GetString("7500c3b6-b93e-4e23-b7fe-adfa48432606", "Consolidate"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new LVXConsolidationOperationalActionMethodApplicator(factory);
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
