using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.HR.Test
{
	[TestedType(typeof(EdiStaffChange))]
	public class EdiStaffChangeTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var bizo = Factory.NewWithValidTestData<EdiStaffChange>();
			bizo.ES9_GS = Env.CurrentUserPK;
			return bizo;
		}

		#endregion
	}
}
