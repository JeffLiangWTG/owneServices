using Enterprise.Accounting.Business.Registry;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluationRegistryItem))]
	class IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluationRegistryItemTest : StronglyTypedRegistryItemTestCase<bool>
	{
		protected override StronglyTypedRegistryItem<bool, bool> GetNewRegistryItem()
		{
			return new IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluationRegistryItem(
				new IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluationRegistryItemImpl(
					new BooleanRegistryItem("", null, null, null, RegistryStorageFlags.System, false),
					"",
					null,
					null,
					null,
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport
					));
		}
	}
}
