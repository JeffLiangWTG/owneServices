using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgPatternMatchOverride : OrgPatternMatchOverride
	{
		public EDIOrgPatternMatchOverride(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override OrgPatternMatchOverrideLookups GetNewLookups()
		{
			return new EDIOrgPatternMatchOverrideLookups(this);
		}

		public override bool IsNotCode => base.IsNotCode && OO_Relationship != EDIConstants.OrgPatternMatchOverrideRelationships.EHubClientID;
	}
}
