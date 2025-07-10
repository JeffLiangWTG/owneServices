using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	abstract class AdditionalActionsXmlDataImporterTest<TBusinessObject, TDataAdapter> : TestCaseWithFactory
				where TBusinessObject : BusinessObject
				where TDataAdapter : IValueObjectDataAdapter
	{
		public void TestAdapter()
		{
			AdditionalActionsXmlDataImporter importer = GetImporter();
			AssertEquals("Adapter", typeof(TDataAdapter), importer.Adapter.GetType());
		}

		public void TestImport()
		{
			NotificationBuffer notify = new NotificationBuffer();
			int objectCount = Factory.GetDatabaseCount(typeof(TBusinessObject));

			using (StreamReader reader = new StreamReader(PathToXmlFile))
			{
				Importer.ImportData(reader, "", notify, SourceInfo.EmptySourceInfo);
			}
			AssertEquals("There should be no errors", true, !notify.HasErrors);
			AssertEquals("One BizObj Should have been imported.", objectCount + 1, Factory.GetDatabaseCount(typeof(TBusinessObject)));
		}

		public void TestAdditionalActionsNotAddedWhenErrorImportingData()
		{
			NotificationBuffer notify = new NotificationBuffer();
			AdditionalActionsXmlDataImporter importerWithError = GetImporterWithError();
			using (StreamReader reader = new StreamReader(PathToXmlFile))
			{
				importerWithError.ImportData(reader, "", notify, SourceInfo.EmptySourceInfo);
			}
			AssertNull(importerWithError.ExtraActionsForTest);
		}

		protected AdditionalActionsXmlDataImporter Importer
		{
			get
			{
				if (fImporter == null)
				{
					fImporter = GetImporter();
				}
				return fImporter;
			}
		}
		AdditionalActionsXmlDataImporter fImporter;

		protected readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		protected abstract AdditionalActionsXmlDataImporter GetImporter();

		protected abstract AdditionalActionsXmlDataImporter GetImporterWithError();

		protected abstract string PathToXmlFile { get; }

		protected override void SetUp()
		{
			base.SetUp();
			GlbGroup postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			postMasterGroup.Staff[0].GS_EmailAddress = "a@b.c";
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}
	}
}
