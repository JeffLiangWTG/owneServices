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
	public class CAB3OperationalActionMethod : OperationalActionMethod
	{
		public CAB3OperationalActionMethod()
			: base(new ZGuid("76F064C7-170D-4788-9713-5EFE1E5FB115"))
		{
		}

		public override string Description
		{
			get { return Res.GetString("2BB6DC9D-02D5-4C3A-B24C-31DE884E73A9", "Send CAD Message for Import Declarations"); }
		}

		public override string Name
		{
			get { return Res.GetString("8DA0AF0A-118B-4215-8BD8-E8A7C750F13E", "Send CAD Message"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new CAB3OperationalActionMethodApplicator();
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
			return new CAB3OperationalActionControl();
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
