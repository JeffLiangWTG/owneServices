using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	public class FeatureControlRuleLicenceDatabasePivot : AutoFeatureControlRuleLicenceDatabasePivot
	{
		public FeatureControlRuleLicenceDatabasePivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("ControlRule")]
		public override ZGuid FCD_FCR_FeatureControlRule { get => base.FCD_FCR_FeatureControlRule; set => base.FCD_FCR_FeatureControlRule = value; }

		[RelatedBusinessObject("Database")]
		public override ZGuid FCD_LD_LicenceDatabase { get => base.FCD_LD_LicenceDatabase; set => base.FCD_LD_LicenceDatabase = value; }

		public FeatureControlRule ControlRule => Factory.Load<FeatureControlRule>(FCD_FCR_FeatureControlRule);

		public LicenceDatabase Database => Factory.Load<LicenceDatabase>(FCD_LD_LicenceDatabase);

		public override void Delete()
		{
			if (ControlRule != null)
			{
				ControlRule.FCR_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			}
			base.Delete();
		}

		protected override AutologState AutoLoggingState => AutologState.NotLogged;
	}
}
