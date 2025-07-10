
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceSchemePeriodCountry : AutoCMRPreferenceSchemePeriodCountry, ICodeDescription
	{
		public CMRPreferenceSchemePeriodCountry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRPreferenceSchemePeriodCountry New(BusinessObjectFactory factory)
		{
			return factory.New<CMRPreferenceSchemePeriodCountry>();
		}

		#region ICodeDescription Members

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		string ICodeDescription.Description
		{
			get
			{
				var snapshot = Factory.LoadTop1<CMRPreferenceSchemePeriodSnapshot>(new ZQuery(CMRPreferenceSchemePeriodSnapshotSchema.PF_SchemeType, PC_PreferenceSchemePeriodSnapshotSchemeType));
				return snapshot != null ? snapshot.PF_Description.ToString() : "";
			}
		}

		string ICodeDescription.Code
		{
			get { return PC_PreferenceSchemePeriodSnapshotSchemeType; }
		}

		#endregion
	}
}
