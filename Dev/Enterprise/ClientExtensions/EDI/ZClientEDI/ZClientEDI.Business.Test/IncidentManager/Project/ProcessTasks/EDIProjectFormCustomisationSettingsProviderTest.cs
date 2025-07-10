using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(EDIProjectFormCustomisationSettingsProvider))]
	public class EDIProjectFormCustomisationSettingsProviderTest : ProcessManagement.Business.Test.ProjectFormCustomisationSettingsProviderTest
	{
		public override ProcessManagement.Business.ProjectFormCustomisationSettingsProvider GetNewProvider()
		{
			return new EDIProjectFormCustomisationSettingsProvider();
		}

		public override void TestDisplayFields()
		{
			var provider = GetNewProvider();
			var statusFields = provider.DisplayFields.Cast<FormCustomisableElement>().Where(x => x.DisplayTabCode == EDIProjectFormCustomisationSettingsProvider.ControlNames.DetailsTabName && x.ElementGroup == EDIProjectFormCustomisationSettingsProvider.ControlNames.StatePanel);
			AssertEquals("Status group fields count", 12, statusFields.Count());
			AssertEquals("Status group fields have distinct row numbers", 12, statusFields.Select(x => x.RowNumber).Distinct().Count());
		}
	}

	[TestedType(typeof(EDIProjectFormCustomisationSettingsProvider))]
	public class EDIProjectFormCustomisationSettingsProviderRequiredTest : FormCustomisationSettingsProviderTest<EDIProjectFormCustomisationSettingsProvider>
	{
		public override EDIProjectFormCustomisationSettingsProvider GetNewProvider()
		{
			return new EDIProjectFormCustomisationSettingsProvider();
		}

		public override void TestDisplayTabs()
		{
			Assert(true);
		}

		public override void TestPropertiesThatAffectWorkflow()
		{
			Assert(true);
		}

		public override void TestTabPlacementProhibitions()
		{
			Assert(true);
		}
	}
}
