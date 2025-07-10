using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Integration.Management;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public abstract class TopLevelDataObjectWriter<TBusinessObject, TDataObject> : DataObjectWriter<TBusinessObject, TDataObject>, ITopLevelDataObjectWriter
		where TBusinessObject : BusinessObject
		where TDataObject : ITopLevelDataObject, new()
	{
		protected TopLevelDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected abstract ZString GetEDIMessageSubType();
		protected abstract DataContextType GetTopLevelDataContextType();

		#region ITopLevelDataObjectWriter Members

		ITopLevelDataObject ITopLevelDataObjectWriter.GetDataObject(BusinessObject sourceBO)
		{
			return GetDataObject(GetTypedBusinessObject(sourceBO));
		}

		protected virtual TBusinessObject GetTypedBusinessObject(BusinessObject sourceBO)
		{
			return (TBusinessObject)sourceBO;
		}

		protected sealed override TDataObject PopulateDataObject(TBusinessObject sourceBO)
		{
			var dataObject = new TDataObject();
			dataObject.SetWriterStrategy(writeManager.WriterStrategy);
			var dataContextManager = GetDataContextManager(sourceBO) ?? throw new InvalidOperationException("The sourceBO passed into GetDataObject() must have a UniversalDataContextAttribute so that an DataContextManager can be loaded. Alternatively you can override GetDataContextManager(), but this is not the common option.");
			dataObject.DataContext = DataContextFactory.New(dataContextManager, writeManager.Schema.Namespace);

			IExternalFetchHintSupporter externalFetchHintSupporter = sourceBO == null ? null : sourceBO.Factory;
			if (externalFetchHintSupporter != null)
			{
				AddTableFetchHintCreators(externalFetchHintSupporter, sourceBO);
			}
			PopulateDataObject(sourceBO, dataObject);

			PopulateWorkflowRelatedProperties(sourceBO, dataObject);
			PopulateCosts(sourceBO, dataObject);

			return dataObject;
		}

		protected virtual void PopulateWorkflowRelatedProperties(TBusinessObject sourceBO, TDataObject dataObject)
		{
			if (!writeManager.IsPublishingInternally)
			{
				ObjectFactory.Get<IUniversalMilestoneWriter>().PopulateMilestones(sourceBO, dataObject, writeManager.ShouldPopulateInternalMilestones);
			}

			ObjectFactory.Get<IUniversalExceptionWriter>().PopulateExceptions(sourceBO, dataObject);
			ObjectFactory.Get<IUniversalTaskSetWriter>().PopulateTaskSets(writeManager.WriterStrategy, sourceBO, dataObject);
		}

		protected virtual void PopulateCosts(TBusinessObject sourceBO, TDataObject dataObject)
		{
			PopulateJobCostingDataWhereApplicable(sourceBO as IJobHeaderParentCore, dataObject as IJobCostingData);
			PopulateConsolCostsDataWhereApplicable(sourceBO as IGenericJobCostPlugInBase, dataObject as IConsolCostsData);
		}

		protected virtual void AddTableFetchHintCreators(IExternalFetchHintSupporter externalFetchHintSupporter, TBusinessObject sourceBO)
		{
		}

		void PopulateJobCostingDataWhereApplicable(IJobHeaderParentCore sourceJobCostingParent, IJobCostingData jobCostingParentData)
		{
			var parent = sourceJobCostingParent as TBusinessObject;
			if (parent != null && jobCostingParentData != null && (ShouldSendJobCostingData(parent) || writeManager.OverrideSendCostingData))
			{
				jobCostingParentData.JobCosting = GenerateJobCostingData(sourceJobCostingParent);
				AddImportMetaDataOnJobCosting(jobCostingParentData.JobCosting, parent);
			}
		}

		protected virtual JobCosting GenerateJobCostingData(IJobHeaderParentCore sourceJobCostingParent)
		{
			var jobCostingAdapter = ObjectFactory.Get<IJobCostingAdapter>();
			return jobCostingAdapter.Generate(sourceJobCostingParent, writeManager.WriterStrategy);
		}

		protected virtual void AddImportMetaDataOnJobCosting(JobCosting jobCosting, TBusinessObject sourceBO)
		{
		}

		protected virtual bool ShouldSendJobCostingData(TBusinessObject sourceBO)
		{
			return IsOrgProxyOnlySelected() || eAdaptorRegistry.Instance.UniversalXMLAlwaysIncludeJobCostingInUniversalShipment.Value;
		}

		protected bool IsOrgProxyOnlySelected()
		{
			var recipientRoleDetails = writeManager.Action.RecipientRoleDetails;
			return recipientRoleDetails != null && recipientRoleDetails.Length == 1 && recipientRoleDetails[0].Type == RecipientRoleType.ORP;
		}

		void PopulateConsolCostsDataWhereApplicable(IGenericJobCostPlugInBase sourceConsolCostsParent, IConsolCostsData consolCostsParentData)
		{
			if (sourceConsolCostsParent != null && consolCostsParentData != null && (ShouldSendConsolCostsData(sourceConsolCostsParent as TBusinessObject) || writeManager.OverrideSendCostingData))
			{
				var consolCostsAdapter = ObjectFactory.Get<IConsolCostsAdapter>();
				consolCostsParentData.ConsolCosts = consolCostsAdapter.Generate(sourceConsolCostsParent, writeManager.WriterStrategy);
			}
		}

		protected virtual bool ShouldSendConsolCostsData(TBusinessObject sourceBO)
		{
			return false;
		}

		protected virtual IDataContextManager GetDataContextManager(BusinessObject sourceBO)
		{
			return sourceBO.GetUniversalDataContextManager();
		}

		protected abstract void PopulateDataObject(TBusinessObject sourceBO, TDataObject dataObject);

		DataContextType ITopLevelDataObjectWriter.TopLevelDataContextType
		{
			get { return GetTopLevelDataContextType(); }
		}

		ZString ITopLevelDataObjectWriter.EDIMessageSubType
		{
			get { return GetEDIMessageSubType(); }
		}

		ZString ITopLevelDataObjectWriter.RootElementName
		{
			get { return typeof(TDataObject).GetAttribute<RootElementAttribute>().RootElementName; }
		}

		#endregion
	}
}
