using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaManifestHeaderProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			object header = parentID.IsValid && parentTablePrefix == AsycudaManifestHeaderSchema.Constants.Prefix ? factory.Load<Integration.Customs.ManifestBase.IAsycudaManifestHeader>(parentID) : null;
			return MapAsycudaManifestHeaderToProcessTaskType(header);
		}

		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			switch (workflowDescriptor.Code)
			{
				case WorkflowDescriptors.AsycudaManifestHeaderWorkflowDescriptorCode:
					subQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, ApplicationCode_ZAOutturnGateInOut);
					subQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, CountryCodes.SouthAfrica);
					break;
				case WorkflowDescriptors.AsycudaManifestWorkflowDescriptorCode:
					subQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, SQLComparisonOperator.NotEqual, ApplicationCode_ZAOutturnGateInOut);
					break;
				case WorkflowDescriptors.EUH7AsycudaManifestHeaderWorkflowDescriptorCode:
					subQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, new[] { ApplicationCodeTypeList.Codes.EuH7, ApplicationCodeTypeList.Codes.EuH7V1, ApplicationCodeTypeList.Codes.EuH7V2 });
					break;
			}
		}

		public const string ApplicationCode_ZAOutturnGateInOut = ApplicationCodeTypeList.Codes.ZAOutturnAndGateInOrOut;

		public const string ApplicationCode_EuH7LowValue = ApplicationCodeTypeList.Codes.EuH7;

		public const string ApplicationCode_EuH7LowValueV1 = ApplicationCodeTypeList.Codes.EuH7V1;

		public const string ApplicationCode_EuH7LowValueV2 = ApplicationCodeTypeList.Codes.EuH7V2;

		Type MapAsycudaManifestHeaderToProcessTaskType(object header)
		{
			Type result = null;
			if (header != null)
			{
				if (header is Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeader)
				{
					result = ObjectFactory.GetType<Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeaderProcessTask>();
				}
				else if (header is Integration.Customs.ASYCUDA.IAsycudaManifestHeader)
				{
					result = ObjectFactory.GetType<Integration.Customs.ASYCUDA.IAsycudaManifestHeaderProcessTask>();
				}
				else if (header is Integration.Customs.ZA.IAsycudaManifestHeader)
				{
					result = ObjectFactory.GetType<Integration.Customs.ZA.IAsycudaManifestHeaderProcessTask>();
				}
				else if (header is Integration.Customs.EU.ITemporaryStorageHeader)
				{
					result = ObjectFactory.GetType<Integration.Customs.EU.ITemporaryStorageHeaderProcessTask>();
				}
			}
			return result;
		}
	}
}
