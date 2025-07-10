using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.CA.DIF.Business.Testing
{
	sealed class DIFDocumentValidationTest : TestCaseWithFactory
	{
		public void TestValidateRequiredDocumentPK()
		{
			var jobDeclaration = (IDISHost)JobDeclaration;

			var requiredDocument = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();

			var difDocument = HostWrapper.DISDocuments.AddNew();
			difDocument.RequiredDocumentPK = ZGuid.Empty;
			AssertHasErrorContaining(difDocument.RequiredDocumentPKInfo, MandatoryValidation.MustBeEntered);

			difDocument.RequiredDocumentPK = requiredDocument.PK;
			AssertNoErrorContaining(difDocument.RequiredDocumentPKInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckEDocsDocumentPK()
		{
			var declaration = (ICADIFHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			var eDocs = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);

			var hostWrapper = new DIFHostWrapper(declaration);

			var difDocument = hostWrapper.DISDocuments.AddNew();

			difDocument.EDocsDocumentPK = ZGuid.Empty;
			AssertHasMessageErrorContaining(difDocument.EDocsDocumentPKInfo, MandatoryValidation.YouHaveNotEntered);

			difDocument.EDocsDocumentPK = ZGuid.NewZGuid();
			AssertNoMessageErrorContaining(difDocument.EDocsDocumentPKInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(difDocument.EDocsDocumentPKInfo, DIFDocumentValidation.SelectValidEDocs);

			difDocument.EDocsDocumentPK = eDocs.UniqueKey;
			AssertNoMessageErrorContaining(difDocument.EDocsDocumentPKInfo, DIFDocumentValidation.SelectValidEDocs);
		}

		public void TestCheckEDocsDocumentPKWithInvalidCharaters()
		{
			var declaration = (ICADIFHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			var eDocs1 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);
			var eDocs2 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC×.pdf", "ABC", false);

			var hostWrapper = new DIFHostWrapper(declaration);

			var difDocument = hostWrapper.DISDocuments.AddNew();
			difDocument.EDocsDocumentPK = eDocs1.UniqueKey;
			AssertNoMessageErrorContaining(difDocument.EDocsDocumentPKInfo, DIFDocumentValidation.FileNameHasInvalidCharacters);

			difDocument.EDocsDocumentPK = eDocs2.UniqueKey;
			AssertHasMessageErrorContaining(difDocument.EDocsDocumentPKInfo, DIFDocumentValidation.FileNameHasInvalidCharacters);
		}

		public void TestCheckPGA()
		{
			var difDocument = HostWrapper.DISDocuments.AddNew();
			difDocument.Validation.ValidateAll();

			AssertHasMessageErrorContaining(difDocument.PGAInfo, MandatoryValidation.YouHaveNotEntered);

			difDocument.PGA = "XXX";
			AssertHasMessageErrorContaining(difDocument.PGAInfo, ListValidation.InvalidCodeMessageError);

			difDocument.PGA = "HC";
			AssertNoMessageErrorContaining(difDocument.PGAInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(difDocument.PGAInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBusinessNumber()
		{
			(HostWrapper.DISHost as BaseJobDeclaration).Importer.CustomsCodes.AddNew("BRC", "BRC001", "CA");
			Factory.Save();
			var difDocument = HostWrapper.DISDocuments.AddNew();
			difDocument.BusinessNumber = "";
			difDocument.Validation.ValidateAll();

			AssertHasMessageErrorContaining(difDocument.BusinessNumberInfo, MandatoryValidation.YouHaveNotEntered);

			difDocument.BusinessNumber = "XXXXX";
			AssertHasMessageErrorContaining(difDocument.BusinessNumberInfo, ListValidation.InvalidCodeMessageError);

			difDocument.Validation.ValidateAll();
			difDocument.BusinessNumber = "BRC001";
			AssertNoMessageErrorContaining(difDocument.BusinessNumberInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckDocumentType()
		{
			var startDate = ZDateTime.UtcToday.AddDays(-1);
			var endDate = ZDateTime.UtcToday.AddDays(1);

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIAAIRSRegistrationType, "CAART", dataGrouping: dataGrouping.ZZZ_DataGrouping);
			var calpcCodeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIALPCOType, "CALPC", dataGrouping: dataGrouping.ZZZ_DataGrouping);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, calpcCodeType.ZZK_CodeType, "C01", "LPCO 001", startDate, endDate);

			Factory.Save();

			var difDocument = HostWrapper.DISDocuments.AddNew();
			difDocument.Validation.ValidateAll();

			AssertHasMessageErrorContaining(difDocument.DocumentTypeInfo, MandatoryValidation.YouHaveNotEntered);

			difDocument.DocumentType = "5050";
			AssertHasMessageErrorContaining(difDocument.DocumentTypeInfo, ListValidation.InvalidCodeMessageError);

			difDocument.PGA = "CFIA";
			difDocument.DocumentType = "C01";
			difDocument.Validation.ValidateAll();
			AssertNoMessageErrorContaining(difDocument.DocumentTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(difDocument.DocumentTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckDocumentNumber()
		{
			var difDocument = HostWrapper.DISDocuments.AddNew();
			difDocument.Validation.ValidateAll();

			AssertHasMessageErrorContaining(difDocument.DocumentNumberInfo, MandatoryValidation.YouHaveNotEntered);

			difDocument.DocumentNumber = "10000001";
			AssertNoMessageErrorContaining(difDocument.DocumentNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckEffectiveDateAndCheckExpiryDate()
		{
			(HostWrapper.DISHost as BaseJobDeclaration).JE_DateOfArrival = new ZDate(2017, 12, 14);

			var difDocument = HostWrapper.DISDocuments.AddNew();
			difDocument.Status = "AOS";
			difDocument.EffectiveDate = new ZDateTime(2017, 12, 15);
			difDocument.ExpiryDate = new ZDateTime(2017, 12, 12);

			AssertHasMessageErrorContaining(difDocument.EffectiveDateInfo, DIFDocumentValidation.ExpiryDateCanNotBeLessThanEffectiveDate);
			AssertHasMessageErrorContaining(difDocument.EffectiveDateInfo, DIFDocumentValidation.EffectiveDateShouldBeEarlierThanOrSameDay);

			AssertHasMessageErrorContaining(difDocument.ExpiryDateInfo, DIFDocumentValidation.ExpiryDateCanNotBeLessThanEffectiveDate);
			AssertHasMessageErrorContaining(difDocument.ExpiryDateInfo, DIFDocumentValidation.ExpiryDateShouldBeLaterThanOrSameDay);

			difDocument.Status = "CAS";
			difDocument.Validation.ValidateAll();
			AssertNoMessageErrorContaining(difDocument.EffectiveDateInfo, DIFDocumentValidation.EffectiveDateShouldBeEarlierThanOrSameDay);
			AssertNoMessageErrorContaining(difDocument.ExpiryDateInfo, DIFDocumentValidation.ExpiryDateShouldBeLaterThanOrSameDay);

			difDocument.Status = "AOS";
			difDocument.Validation.ValidateAll();
			difDocument.EffectiveDate = new ZDateTime(2017, 12, 12);
			difDocument.ExpiryDate = new ZDateTime(2017, 12, 15);

			AssertNoMessageErrorContaining(difDocument.EffectiveDateInfo, DIFDocumentValidation.ExpiryDateCanNotBeLessThanEffectiveDate);
			AssertNoMessageErrorContaining(difDocument.EffectiveDateInfo, DIFDocumentValidation.EffectiveDateShouldBeEarlierThanOrSameDay);
			AssertNoMessageErrorContaining(difDocument.ExpiryDateInfo, DIFDocumentValidation.ExpiryDateCanNotBeLessThanEffectiveDate);
			AssertNoMessageErrorContaining(difDocument.ExpiryDateInfo, DIFDocumentValidation.ExpiryDateShouldBeLaterThanOrSameDay);

			var cusPermitHeader = (ICADIFHost)new TestHelper(Factory).GetCusPermitHeader();
			var hostWrapperPermit = new DIFHostWrapper(cusPermitHeader);
			var difDocumentPermit = hostWrapperPermit.DISDocuments.AddNew();
			AssertNoMessageErrorContaining(difDocumentPermit.EffectiveDateInfo, DIFDocumentValidation.ExpiryDateCanNotBeLessThanEffectiveDate);
			AssertNoMessageErrorContaining(difDocumentPermit.EffectiveDateInfo, DIFDocumentValidation.EffectiveDateShouldBeEarlierThanOrSameDay);
			AssertNoMessageErrorContaining(difDocumentPermit.ExpiryDateInfo, DIFDocumentValidation.ExpiryDateCanNotBeLessThanEffectiveDate);
			AssertNoMessageErrorContaining(difDocumentPermit.ExpiryDateInfo, DIFDocumentValidation.ExpiryDateShouldBeLaterThanOrSameDay);
		}

		public void TestComment_MaxLength()
		{
			var difDocument = HostWrapper.DISDocuments.AddNew();
			AssertNoExceptionThrown(() => difDocument.Comment = ZString.Replicate('X', 100));
			AssertExceptionThrown<MaxLengthExceededException>(() => difDocument.Comment += 'X');
			ErrorReporter.Clear();
		}

		[UseSnapshotProtection]
		public void TestCheckURN()
		{
			ObjectFactory.Get<Integration.Customs.CA.ICACustomsDataRegistry>().AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, string.Empty);
			var difDocument = HostWrapper.DISDocuments.AddNew();
			difDocument.RunPreSaveValidation();
			AssertHasErrorContaining(difDocument.URNInfo, "ASEC Number is not set for the current company.");

			ObjectFactory.Get<Integration.Customs.CA.ICACustomsDataRegistry>().AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");
			difDocument.RunPreSaveValidation();
			AssertNoErrorContaining(difDocument.URNInfo, "ASEC Number is not set for the current company.");

			var difFountain = Env.NumberFountains.GetCAEntryNumberGeneratorFountain("CADeclarationTransactionNumber12345DIF");
			difFountain.SetValues(Factory, minValue: 1, nextValue: 9_899, maxValue: 10_000);
			for (int i = 0; i < 101; i++)
			{
				difFountain.GetNext(Factory);
			}
			AssertEquals(10_000, difFountain.PeekPreliminaryOrDefault(Factory, 0));
			difDocument.RunPreSaveValidation();
			Assert(!difDocument.URNInfo.Notifications.Any(x => x.Message.Contains("There are no more available numbers in the number range for DIF.")));

			difFountain.GetNext(Factory);
			AssertEquals(10_001, difFountain.PeekPreliminaryOrDefault(Factory, 0));

			difDocument.RunPreSaveValidation();
			AssertHasErrorContaining(difDocument.URNInfo, "There are no more available numbers in the number range for DIF.");
		}

		DIFHostWrapper hostWrapper;
		DIFHostWrapper HostWrapper => hostWrapper ?? (hostWrapper = new DIFHostWrapper((ICADIFHost)JobDeclaration));

		BusinessObject jobDeclaration;
		BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());
	}
}
