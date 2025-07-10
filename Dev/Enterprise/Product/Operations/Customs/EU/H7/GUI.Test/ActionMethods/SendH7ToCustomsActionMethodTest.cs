using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing;

[TestedType(typeof(SendH7ToCustomsActionMethod))]
sealed class SendH7ToCustomsActionMethodTest : OperationalActionMethodTest<SendH7ToCustomsActionMethod>
{
	public void TestSendH7ToCustomsActionMethod_OverridenProperties()
	{
		var actionMethod = new SendH7ToCustomsActionMethod();
		var bizOFactory = new BusinessObjectFactory();
		var applicator = actionMethod.NewApplicator(bizOFactory, null);
		CombineAssertions("Test overriden properties", () =>
		{
			AssertNotNull(applicator);
			Assert(applicator is SendH7ToCustomsApplicator);
			AssertEquals("Send H7 Jobs to Customs", actionMethod.Name);
			AssertEquals("Send H7 Jobs to Customs in Bulk", actionMethod.Description);
		});
	}

	public void TestGetApplicator_Spain()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			var actionMethod = new SendH7ToCustomsActionMethod();
			var bizOFactory = new BusinessObjectFactory();
			var applicator = actionMethod.NewApplicator(bizOFactory, null);
			AssertEquals("Enterprise.Customs.ES.Manifest.H7.GUI.ESSendH7ToCustomsApplicator", applicator.GetType().FullName);
		}
	}

	public void TestGetApplicator_GenericEUCountry()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
		{
			var actionMethod = new SendH7ToCustomsActionMethod();
			var bizOFactory = new BusinessObjectFactory();
			var applicator = actionMethod.NewApplicator(bizOFactory, null);
			AssertType<SendH7ToCustomsApplicator>(applicator);
		}
	}

	protected override SendH7ToCustomsActionMethod NewMethod()
	{
		return new SendH7ToCustomsActionMethod();
	}
}
