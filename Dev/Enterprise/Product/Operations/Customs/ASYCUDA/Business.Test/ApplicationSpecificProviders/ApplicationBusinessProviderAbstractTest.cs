using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestsSubclassesOf(typeof(ApplicationBusinessProvider))]
	public abstract class ApplicationBusinessProviderAbstractTest<T, THeader> : TestCaseWithFactory
			where T : ApplicationBusinessProvider
			where THeader : AsycudaManifestHeader
	{
		public void TestConstructor()
		{
			var providerType = TestedTypeHelper.GetTestedType(GetType());
			AssertNotNull("Should have a default ctor for ObjectFactory - " + providerType.FullName, providerType.GetConstructor(Array.Empty<Type>()));
		}

		public virtual void TestAsycudaManifestHeaderType()
		{
			var header = CreateNewManifest();
			var provider = header.ApplicationBusinessProvider;
			AssertEquals(typeof(THeader), provider.AsycudaManifestHeaderType);
		}

		public virtual void TestManifestTypes()
		{
			var header = CreateNewManifest();
			var provider = header.ApplicationBusinessProvider;
			AssertContainsExactElementsInAnyOrder(new ManifestType.EqualityComparer(), ExpectedManifestTypes, provider.ManifestTypes);
			ResetManifestTypes(provider);
		}

		public void TestMessagingProvider()
		{
			var header = CreateNewManifest();
			AssertEquals(ExpectedMessagingProviderType, header.ApplicationBusinessProvider.MessagingProvider?.GetType());
		}

		public void TestFeatureProvider()
		{
			var header = CreateNewManifest();
			AssertEquals(ExpectedFeatureProviderType, header.ApplicationBusinessProvider.FeatureProvider.GetType());
		}

		public void TestGetCustomsDeclarationDataObjectWriter()
		{
			var header = CreateNewManifest();
			var bill = header.Bills[0];
			AssertEquals(ExpectedGetCustomsDeclarationDataObjectWriterType, header.ApplicationBusinessProvider.GetCustomsDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, bill)), header).GetType());
		}

		public void TestGetAsycudaManifestHeaderDataObjectWriter()
		{
			var header = CreateNewManifest();
			AssertEquals(ExpectedGetAsycudaManifestHeaderDataObjectWriterType, header.ApplicationBusinessProvider.GetAsycudaManifestHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, header))).GetType());
		}

		public void TestGetUniversalMessagingHelper()
		{
			var header = CreateNewManifest();
			AssertEquals(ExpectedGetUniversalMessagingHelperType, header.ApplicationBusinessProvider.GetUniversalMessagingHelper(new NotificationsForTest()).GetType());
		}

		public void TestGetAsycudaManifestDataObjectReaderHelper()
		{
			var header = CreateNewManifest();
			AssertEquals(ExpectedGetAsycudaManifestDataObjectReaderHelperType, header.ApplicationBusinessProvider.GetAsycudaManifestDataObjectReaderHelper(header.AMA_RN_NKCountry).GetType());
		}

		public virtual void TestGetNewAsycudaUniversalEventMessageProcessor()
		{
			var header = CreateNewManifest();
			var eventXML = new Event();
			var dataContext = DataContextFactory.New();
			var actionPurpose = new CodeDescriptionPair() { Code = AsycudaEventMessageConstants.ActionPurpose.ERR };
			var workflowInfo = new WorkflowInfo { ActionPurpose = actionPurpose };
			dataContext.SetWorkflowInfo(workflowInfo);
			eventXML.DataContext = dataContext;
			var ediMessage = Factory.New<AsycudaEDIMessage>();
			var provider = header.ApplicationBusinessProvider;
			AssertEquals("ERR", typeof(AsycudaUniversalEventMessageFailureProcessor), provider.GetNewAsycudaUniversalEventMessageProcessor(new XmlSessionTracker(new ServiceTaskLogForTesting()), eventXML, ediMessage, header).GetType());
			actionPurpose.Code = "AEP";
			AssertNull("AEP", provider.GetNewAsycudaUniversalEventMessageProcessor(new XmlSessionTracker(new ServiceTaskLogForTesting()), eventXML, ediMessage, header));
			actionPurpose.Code = null;
			AssertNull("Null code", provider.GetNewAsycudaUniversalEventMessageProcessor(new XmlSessionTracker(new ServiceTaskLogForTesting()), eventXML, ediMessage, header));
			eventXML.DataContext = null;
			AssertNull("Null DataContext", provider.GetNewAsycudaUniversalEventMessageProcessor(new XmlSessionTracker(new ServiceTaskLogForTesting()), eventXML, ediMessage, header));
		}

		public void TestGetAsycudaManifestHeaderDataObjectWriterHelper()
		{
			var header = CreateNewManifest();
			AssertEquals(ExpectedGetAsycudaManifestHeaderDataObjectWriterHelperType, header.ApplicationBusinessProvider.GetAsycudaManifestHeaderDataObjectWriterHelper(header).GetType());
		}

		public void TestGetAsycudaBillEventContextReader()
		{
			var header = CreateNewManifest();
			var bill = header.Bills[0];
			AssertEquals(ExpectedAsycudaBillEventContextReaderType, header.ApplicationBusinessProvider.GetAsycudaBillEventContextReader(bill).GetType());
		}

		protected abstract IEnumerable<IManifestType> ExpectedManifestTypes { get; }

		protected abstract Type ExpectedMessagingProviderType { get; }

		protected abstract Type ExpectedFeatureProviderType { get; }

		protected abstract Type ExpectedGetCustomsDeclarationDataObjectWriterType { get; }

		protected abstract Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType { get; }

		protected virtual Type ExpectedGetAsycudaManifestHeaderDataObjectWriterHelperType => typeof(AsycudaManifestHeaderDataObjectWriterHelper);

		protected virtual Type ExpectedGetUniversalMessagingHelperType => typeof(AsycudaManifestUniversalMessagingHelper);

		protected virtual Type ExpectedGetAsycudaManifestDataObjectReaderHelperType => typeof(AsycudaManifestDataObjectReaderHelper);

		protected virtual Type ExpectedAsycudaBillEventContextReaderType => typeof(AsycudaBillEventContextReader);

		protected void ResetManifestTypes(ApplicationBusinessProvider provider) => provider.ResetManifestTypes();

		protected virtual THeader CreateNewManifest()
		{
			var header = Factory.NewWithValidTestData<THeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BolType = AsycudaBill.ChildBolCode;
			var pack = bill.Packs.AddNew();
			pack.APA_GoodsDescription = "HELLO";
			_ = pack.PackedItemForTesting();
			return header;
		}
	}
}
