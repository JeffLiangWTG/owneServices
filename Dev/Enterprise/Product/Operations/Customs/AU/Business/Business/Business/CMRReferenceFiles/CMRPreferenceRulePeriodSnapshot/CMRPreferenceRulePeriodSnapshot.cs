
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceRulePeriodSnapshot : AutoCMRPreferenceRulePeriodSnapshot, ICodeDescription
	{
		public CMRPreferenceRulePeriodSnapshot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRPreferenceRulePeriodSnapshot New(BusinessObjectFactory factory)
		{
			return factory.New<CMRPreferenceRulePeriodSnapshot>();
		}

		#region ICodeDescription Members

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		public string Description
		{
			get { return PU_Description; }
		}

		public string Code
		{
			get { return PU_RuleType; }
		}

		#endregion
	}
}
