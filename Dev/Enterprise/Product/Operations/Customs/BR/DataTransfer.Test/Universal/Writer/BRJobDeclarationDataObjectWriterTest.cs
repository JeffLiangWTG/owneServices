using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.BR.DataTransfer.Universal.Testing
{
	public class BRJobDeclarationDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestDataObjectWriter()
		{
			var declaration = Factory.New<JobDeclaration>();

			var writer = new BRJobDeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())));
			writer.GetDataObject(declaration);

			AssertType<BRDataObjectWriterHelper>(writer.GetCreateNewUniversalDataObjectWriterHelper(declaration));
		}

		public void TestExportSpecialTransport()
		{
			const string specialTransport = "SPE";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SpecialTransport = specialTransport;

			var writer = new BRJobDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())));
			var output = writer.GetDataObject(declaration);

			var outAddInfo = output.AddInfoCollection.Find(x => x.Key.GetValueOrDefault() == "SpecialTransport");
			AssertEquals("Special Tranport Value:", specialTransport, outAddInfo.Value);
		}

		class BRJobDeclarationDataObjectWriterForTest : BRJobDeclarationDataObjectWriter
		{
			internal BRJobDeclarationDataObjectWriterForTest(IDataWritingManager manager) : base(manager)
			{
				ShouldPopulateAttachedDocumentCollection = true;
			}

			public UniversalDataObjectWriterHelper GetCreateNewUniversalDataObjectWriterHelper(BaseJobDeclaration declarationBO)
			{
				return CreateNewUniversalDataObjectWriterHelper(declarationBO);
			}
		}

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			base.SetUp();
		}

		IDisposable setupCreator;

		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
		}
	}
}
