using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.Business.Testing
{
	[TestedType(typeof(WorkflowConfigurationStepCollection))]
	sealed class WorkflowConfigurationStepCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<WorkflowConfigurationStepCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override WorkflowConfigurationStepCollection GetCollectionToTest()
		{
			return new WorkflowConfigurationStepCollection(DashDataRegistry.CommercialInvoiceWorkflowConfigurationPairListProvider);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WorkflowConfigurationStep();
		}

		public void TestEquals()
		{
			var a = new WorkflowConfigurationStepCollection(WorkflowConfigurationStepTest.GetCodesProviderForTesting());
			var b = new WorkflowConfigurationStepCollection(WorkflowConfigurationStepTest.GetCodesProviderForTesting());
			Assert(a.Equals(a));
			Assert(b.Equals(b));
			Assert(a.Equals(b));
			Assert(b.Equals(a));
			Assert(a == b);
			Assert(b == a);

			a.AddNew().Code = "DVL";
			Assert(a.Equals(a));
			Assert(b.Equals(b));
			Assert(!a.Equals(b));
			Assert(!b.Equals(a));
			Assert(a != b);
			Assert(b != a);

			var item = b.AddNew();
			item.Code = "XML";
			Assert(a.Equals(a));
			Assert(b.Equals(b));
			Assert(!a.Equals(b));
			Assert(!b.Equals(a));
			Assert(a != b);
			Assert(b != a);

			item.Code = "DVL";
			Assert(a.Equals(a));
			Assert(b.Equals(b));
			Assert(a.Equals(b));
			Assert(b.Equals(a));
			Assert(a == b);
			Assert(b == a);

			b.AddNew().Code = "DVL";
			Assert(a.Equals(a));
			Assert(b.Equals(b));
			Assert(!a.Equals(b));
			Assert(!b.Equals(a));
			Assert(a != b);
			Assert(b != a);

			WorkflowConfigurationStepCollection c = null;
			WorkflowConfigurationStepCollection d = null;
			Assert(c == d);
			Assert(c == null);
			Assert(null == c);
			Assert(a != c);
			Assert(a != null);
			Assert(null != a);

			Assert(!a.Equals(c));
			Assert(!a.Equals(null));
		}

		public void TestCollection_Has_Validation_Error_When_ProductCodeMatching_Step_Precedes_OrganizationMatching_Step()
		{
			var newItem1 = Collection.AddNew();
			newItem1.Code = SharedConstants.DataProcessingType.Code.ProductCodeMatching;

			var newItem2 = Collection.AddNew();
			newItem2.Code = SharedConstants.DataProcessingType.Code.OrganizationMatching;

			var newItem3 = Collection.AddNew();
			newItem3.Code = SharedConstants.DataProcessingType.Code.NotifyDownstreamServices;

			Collection.RunPreSaveValidation();
			
			AssertHasError(newItem1.CodeInfo, "The PCM step must be preceded by the ORM step.");

			newItem1.Code = SharedConstants.DataProcessingType.Code.OrganizationMatching;
			newItem2.Code = SharedConstants.DataProcessingType.Code.ProductCodeMatching;

			Collection.RunPreSaveValidation();

			AssertNoErrors(newItem1.CodeInfo);
			AssertNoErrors(newItem2.CodeInfo);
			AssertNoErrors(newItem3.CodeInfo);
		}

		public void TestCollection_Has_Validation_Error_When_NotifyDownstreamServices_Step_Is_Not_Last()
		{
			var newItem1 = Collection.AddNew();
			newItem1.Code = SharedConstants.DataProcessingType.Code.OrganizationMatching;

			var newItem2 = Collection.AddNew();
			newItem1.Code = SharedConstants.DataProcessingType.Code.NotifyDownstreamServices;

			var newItem3 = Collection.AddNew();
			newItem2.Code = SharedConstants.DataProcessingType.Code.ProductCodeMatching;

			Collection.RunPreSaveValidation();

			AssertHasError(newItem1.CodeInfo, "The NDS step must be last in the workflow.");

			newItem1.Code = SharedConstants.DataProcessingType.Code.OrganizationMatching;
			newItem2.Code = SharedConstants.DataProcessingType.Code.ProductCodeMatching;
			newItem3.Code = SharedConstants.DataProcessingType.Code.NotifyDownstreamServices;

			Collection.RunPreSaveValidation();

			AssertNoErrors(newItem1.CodeInfo);
			AssertNoErrors(newItem2.CodeInfo);
			AssertNoErrors(newItem3.CodeInfo);
		}

		public void TestCollection_Has_Validation_Error_When_It_Has_Repeated_Step()
		{
			var newItem1 = Collection.AddNew();
			newItem1.Code = SharedConstants.DataProcessingType.Code.OrganizationMatching;

			var newItem2 = Collection.AddNew();
			newItem2.Code = SharedConstants.DataProcessingType.Code.ProductCodeMatching;

			var newItem3 = Collection.AddNew();
			newItem3.Code = SharedConstants.DataProcessingType.Code.ProductCodeMatching;

			Collection.RunPreSaveValidation();

			AssertHasError(newItem2.CodeInfo, "Task PCM can only be added once in a workflow configuration.");
			AssertHasError(newItem3.CodeInfo, "Task PCM can only be added once in a workflow configuration.");

			newItem1.Code = SharedConstants.DataProcessingType.Code.OrganizationMatching;
			newItem2.Code = SharedConstants.DataProcessingType.Code.ProductCodeMatching;
			newItem3.Code = SharedConstants.DataProcessingType.Code.NotifyDownstreamServices;

			Collection.RunPreSaveValidation();

			AssertNoErrors(newItem1.CodeInfo);
			AssertNoErrors(newItem2.CodeInfo);
			AssertNoErrors(newItem3.CodeInfo);
		}
	}
}
