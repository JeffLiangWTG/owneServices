using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	class AgreementCancelledCommissionsControlTest : TestCaseWithFactory
	{
		public void TestGetCommissionLinesToNotReinstate()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			var lines = new List<ViewCommissionLine>();

			Enumerable.Range(1, 5).ForEach(x => lines.Add(Factory.New<ViewCommissionLine>()));

			var cancelledLinesCollection = new CancelledLinesCollection(Factory);
			cancelledLinesCollection.AddLines(lines);

			using (var control = new AgreementCancelledCommissionsControlForTest(cancelledLinesCollection, agreement))
			{
				control.CancelledLinesCollection_ExposedForTest[0].IsExcluded = false;
				control.CancelledLinesCollection_ExposedForTest[2].IsExcluded = false;

				var commissionLinesToNotReinstate = control.GetCommissionLinesToNotReinstate();
				AssertEquals("Only 3 lines should be excluded", 3, commissionLinesToNotReinstate.Count());
			}
		}
	}

	#region Implementation

	class AgreementCancelledCommissionsControlForTest : AgreementCancelledCommissionsControl
	{
		public AgreementCancelledCommissionsControlForTest(CancelledLinesCollection collection, OrgCommissionAgreement agreement)
			: base(collection, agreement) { }

		public CancelledLinesCollection CancelledLinesCollection_ExposedForTest => CancelledLines;
	}

	#endregion
}
