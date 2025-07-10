using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	public class CommissionFinalizerResultCountMessageTest : TestCaseWithFactory
	{
		public void TestCommissionFinalizerTooManyResultsErrorMessage()
		{
			OrganisationsDataRegistry.Instance.CommissionFinalizerMaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			using (var filterControl = new CommissionFinalizerFilterControl(new CommissionFinalizer(), new CommissionFinalizerFilterBusinessObject()))
			{
				var commissionFinalizerResultCountMessage = new CommissionFinalizerResultCountMessageForTest(filterControl);

				AssertMultilineASCIIEquals(@"This search returns more than the maximum number of records to display.
The number of search records to display can be defined in the registry up to a maximum value of 40,000 records.

See: Registry -> Sales & Marketing -> Commission -> Commission Finalizer Max. No. of Records to Show
The current value is set to 100.", commissionFinalizerResultCountMessage.CommissionFinalizerTooManyResultsErrorMessage);
			}
		}
	}

	public class CommissionFinalizerResultCountMessageForTest : CommissionFinalizerResultCountMessage
	{
		public CommissionFinalizerResultCountMessageForTest(IResultCountHandler resultCountHandler) : base(resultCountHandler)
		{
		}

		internal string CommissionFinalizerTooManyResultsErrorMessage => GetTooManyResultsErrorMessage(0);
	}
}
