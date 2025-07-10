using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.Business.Testing
{
	[TestedType(typeof(WorkflowConfigurationStep))]
	public sealed class WorkflowConfigurationStepTest : RegistryBusinessObjectTemplateTestCase<WorkflowConfigurationStep>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override WorkflowConfigurationStep GetBusinessObjectToClone()
		{
			BizObj.Code = SharedConstants.DataProcessingType.Code.OrganizationMatching;
			return BizObj;
		}

		protected override WorkflowConfigurationStep GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var collection = new WorkflowConfigurationStepCollection(GetCodesProviderForTesting());
			return collection.AddNew();
		}

		public static CodeDescriptionPairListProvider GetCodesProviderForTesting()
		{
			return new CodeDescriptionPairListProvider(() =>
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(SharedConstants.DataProcessingType.Code.OrganizationMatching, SharedConstants.DataProcessingType.Description.OrganizationMatching);
				list.AddPair(SharedConstants.DataProcessingType.Code.ProductCodeMatching, SharedConstants.DataProcessingType.Description.ProductCodeMatching);
				list.AddPair(SharedConstants.DataProcessingType.Code.NotifyDownstreamServices, SharedConstants.DataProcessingType.Description.NotifyDownstreamServices);
				return list;
			});
		}

		public void TestDescription()
		{
			BizObj.Code = "";
			AssertEquals("Description", "", BizObj.Description);

			BizObj.Code = SharedConstants.DataProcessingType.Code.OrganizationMatching;
			AssertEquals("Description", SharedConstants.DataProcessingType.Description.OrganizationMatching, BizObj.Description);

			BizObj.Code = SharedConstants.DataProcessingType.Code.ProductCodeMatching;
			AssertEquals("Description", SharedConstants.DataProcessingType.Description.ProductCodeMatching, BizObj.Description);

			BizObj.Code = SharedConstants.DataProcessingType.Code.NotifyDownstreamServices;
			AssertEquals("Description", SharedConstants.DataProcessingType.Description.NotifyDownstreamServices, BizObj.Description);
		}

		public void TestValidation()
		{
			BizObj.Code = "";
			AssertHasError(BizObj.CodeInfo, "Please enter a value.");

			BizObj.Code = "ZZZ";
			AssertHasError(BizObj.CodeInfo, "Enter a valid selection.");

			BizObj.Code = SharedConstants.DataProcessingType.Code.OrganizationMatching;
			AssertNoErrors(BizObj.CodeInfo);

			BizObj.Code = "";
			BizObj.ClearAllNotifications();
			BizObj.RunPreSaveValidation();
			AssertHasError(BizObj.CodeInfo, "Please enter a value.");
		}
	}
}
