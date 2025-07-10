
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceSchemeRule : AutoCMRPreferenceSchemeRule, ICodeDescription
	{
		public CMRPreferenceSchemeRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRPreferenceSchemeRule New(BusinessObjectFactory factory)
		{
			return factory.New<CMRPreferenceSchemeRule>();
		}

		#region ICodeDescription Members

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		public string Description
		{
			get
			{
				var snapshot = Factory.LoadTop1<CMRPreferenceRulePeriodSnapshot>(new ZQuery(CMRPreferenceRulePeriodSnapshotSchema.PU_RuleType, PR_RuleType));
				return snapshot != null ? snapshot.PU_Description.ToString() : "";
			}
		}

		public string Code
		{
			get { return PR_RuleType; }
		}

		#endregion
	}
}
