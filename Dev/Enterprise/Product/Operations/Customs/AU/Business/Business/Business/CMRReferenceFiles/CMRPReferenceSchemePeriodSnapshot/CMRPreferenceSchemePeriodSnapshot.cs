
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceSchemePeriodSnapshot : AutoCMRPreferenceSchemePeriodSnapshot, ICodeDescription
	{
		public CMRPreferenceSchemePeriodSnapshot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRPreferenceSchemePeriodSnapshot New(BusinessObjectFactory factory)
		{
			return factory.New<CMRPreferenceSchemePeriodSnapshot>();
		}

		#region ICodeDescription Members

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		string ICodeDescription.Description
		{
			get { return PF_Description; }
		}

		string ICodeDescription.Code
		{
			get { return PF_SchemeType; }
		}

		#endregion
	}
}
