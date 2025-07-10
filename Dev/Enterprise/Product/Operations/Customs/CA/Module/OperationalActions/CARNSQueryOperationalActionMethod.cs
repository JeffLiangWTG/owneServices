using System.ComponentModel;
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
	public class CARNSQueryOperationalActionMethod : OperationalActionMethod
	{
		public CARNSQueryOperationalActionMethod()
			: base(new ZGuid("863EF97E-FA84-48B0-9775-977CAD048F7F"))
		{
		}
		public override string Description
		{
			get { return Res.GetString("8740EAF9-A535-4202-9A45-02F921A9DF56", "Send Release Status Query for Import Declarations"); }
		}

		public override string Name
		{
			get { return Res.GetString("E1FBCCA6-C89E-4A07-A2AE-9B91015C808B", "Send Release Status Query"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new CARNSQueryOperationalActionMethodApplicator();
		}

		public override bool IsRunAgainDisabled
		{
			get { return true; }
		}

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new CARNSQueryOperationalActionControl();
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
