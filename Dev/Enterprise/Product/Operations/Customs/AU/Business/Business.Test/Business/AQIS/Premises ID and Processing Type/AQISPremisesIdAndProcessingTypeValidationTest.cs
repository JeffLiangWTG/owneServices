using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AQISPremisesIdAndProcessingTypeValidationTest : TestCaseWithFactory
	{
		public void TestProcessingTypeWarningValidation()
		{
			var endsInND = CMRAqisProcessingType.New(Factory);
			endsInND.QT_AQISProcessingType = "EndsInND";
			endsInND.QT_AQISProcessingCargoType = Core.Constants.ContainerModes.FCL;
			endsInND.QT_AQISProcessingDescription = "EndsInND";

			var endsInD = CMRAqisProcessingType.New(Factory);
			endsInD.QT_AQISProcessingType = "EndsInD";
			endsInD.QT_AQISProcessingCargoType = Core.Constants.ContainerModes.FCL;
			endsInD.QT_AQISProcessingDescription = "EndsInD";

			var endsInP = CMRAqisProcessingType.New(Factory);
			endsInP.QT_AQISProcessingType = "EndsInP";
			endsInP.QT_AQISProcessingCargoType = Core.Constants.ContainerModes.FCL;
			endsInP.QT_AQISProcessingDescription = "EndsInP";

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var processingType = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			processingType.ProcessingType = endsInND.QT_AQISProcessingType;
			AssertHasWarnings("Processing Type has a warning", processingType.ProcessingTypeInfo);

			processingType.ProcessingType = endsInD.QT_AQISProcessingType;
			AssertHasWarnings("Processing Type has a warning", processingType.ProcessingTypeInfo);

			processingType.ProcessingType = endsInP.QT_AQISProcessingType;
			AssertNoWarnings("Processing Type has a warning", processingType.ProcessingTypeInfo);
		}

		public void TestProcessingTypeListValidation()
		{
			var fCLProcessingType = CMRAqisProcessingType.New(Factory);
			fCLProcessingType.QT_AQISProcessingType = "FCLType";
			fCLProcessingType.QT_AQISProcessingCargoType = Core.Constants.ContainerModes.FCL;
			fCLProcessingType.QT_AQISProcessingDescription = "FCL Description";
			Factory.Save();

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var processingType = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			AssertNoMessageErrors("Type has no message error", processingType.ProcessingTypeInfo);

			processingType.ProcessingType = "TTT";
			AssertHasMessageErrors("Type has a message error", processingType.ProcessingTypeInfo);

			processingType.ProcessingType = "FCLType";
			AssertNoMessageErrors("Type has no message error", processingType.ProcessingTypeInfo);
		}

		public void TestPremisesIdListValidation()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var premises = CMRAqisPremises.New(Factory);
				premises.QP_AQISPremisesName = "Name";
				premises.QP_AQISPremisesIdentifier = "Id";
				premises.QP_AQISPremisesPortCode = "AUSYD";
				Factory.Save();

				var declaration = Factory.New<JobDeclaration>();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var premisesIdAndProcessingType = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
				ValidationTestHelper.AssertInvalidCodeMessageError(premisesIdAndProcessingType.PremisesIdInfo, "TTT", "Id");
			}

			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				var universalHelper = new UniversalReferenceTestDataHelper(newFactory);
				universalHelper.CreateNewOrGetExistingCusCodeType(AUConstants.RefCusCodeTypeCodes.CMRAP, "AQIS Premises Id", Core.Constants.CountryCodes.Australia);
				var premisesId1 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRAP, "Code", "Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				newFactory.Save();

				var declaration = newFactory.New<JobDeclaration>();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var premisesIdAndProcessingType = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
				ValidationTestHelper.AssertInvalidCodeMessageError(premisesIdAndProcessingType.PremisesIdInfo, "TTT", "Code");
			}
		}

		public void TestAQISPremisesIDAndProcessingTypeAreNotDependent()
		{
			var premises = CMRAqisPremises.New(Factory);
			premises.QP_AQISPremisesName = "Name";
			premises.QP_AQISPremisesIdentifier = "Id";
			premises.QP_AQISPremisesPortCode = "AUSYD";

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			var premisesId = new AQISPremisesIdAndProcessingType(Factory, declaration);
			premisesId.PremisesId = "Id";
			premisesId.Validation.ValidateProcessingType();
			AssertNoErrors("Premises ID has no error", premisesId.ProcessingTypeInfo);

			var processingType = CMRAqisProcessingType.New(Factory);
			processingType.QT_AQISProcessingCargoType = "AIR";
			processingType.QT_AQISProcessingType = "Type";
			processingType.QT_AQISProcessingDescription = "Desc";

			premisesId.ProcessingType = "Type";
			premisesId.Validation.ValidateAll();
			AssertNoErrors("Premises ID has no error", premisesId.PremisesIdInfo);
			AssertNoErrors("Processing Line", premisesId.ProcessingTypeInfo);

			premisesId.PremisesId = "Id";
			premisesId.Validation.ValidateAll();
			AssertNoErrors("Premises ID has no error", premisesId.PremisesIdInfo);
			AssertNoErrors("Processing Line", premisesId.ProcessingTypeInfo);
		}

		public void TestPremisesIdOfPremisesIdsAndProcessingProcessingTypesEntered()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var premisesIdAndProcessingType1 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType1.PremisesId = "123";
			premisesIdAndProcessingType1.ProcessingType = "One";

			var premisesIdAndProcessingType2 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType2.PremisesId = "456";
			premisesIdAndProcessingType2.ProcessingType = "Two";

			var premisesIdAndProcessingType3 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType3.PremisesId = "789";
			premisesIdAndProcessingType3.ProcessingType = "Thr";

			var premisesIdAndProcessingType4 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType2.PremisesId = "321";
			premisesIdAndProcessingType2.ProcessingType = "Four";

			var premisesIdAndProcessingType5 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType5.PremisesId = "654";
			premisesIdAndProcessingType5.ProcessingType = "Five";

			var premisesIdAndProcessingType6 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType6.PremisesId = "987";
			premisesIdAndProcessingType6.ProcessingType = "Six";

			var premisesIdAndProcessingType7 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType7.PremisesId = "987";
			premisesIdAndProcessingType7.ProcessingType = "Sev";

			var premisesIdAndProcessingType8 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType8.PremisesId = "987";
			premisesIdAndProcessingType8.ProcessingType = "Eig";

			var premisesIdAndProcessingType9 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType9.PremisesId = "987";
			premisesIdAndProcessingType9.ProcessingType = "Nine";

			var premisesIdAndProcessingType10 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType10.PremisesId = "987";
			premisesIdAndProcessingType10.ProcessingType = "Ten";

			premisesIdAndProcessingType10.Validation.ValidateAll();

			AssertEquals("PremisesIdAndProcessingType 1 has no errors", false, premisesIdAndProcessingType1.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 2 has no errors", false, premisesIdAndProcessingType2.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 3 has no errors", false, premisesIdAndProcessingType3.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 4 has no errors", false, premisesIdAndProcessingType4.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 5 has no errors", false, premisesIdAndProcessingType5.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 6 has no errors", false, premisesIdAndProcessingType6.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 7 has no errors", false, premisesIdAndProcessingType7.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 8 has no errors", false, premisesIdAndProcessingType8.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 9 has no errors", false, premisesIdAndProcessingType9.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 10 has no errors", false, premisesIdAndProcessingType10.HasRowErrors);

			var premisesIdAndProcessingType11 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType11.PremisesId = "987";
			premisesIdAndProcessingType11.ProcessingType = "Ele";
			Factory.Save();

			premisesIdAndProcessingType11.Validation.ValidateAll();
			AssertEquals("PremisesIdAndProcessingType 1 has no errors", false, premisesIdAndProcessingType1.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 2 has no errors", false, premisesIdAndProcessingType2.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 3 has no errors", false, premisesIdAndProcessingType3.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 4 has no errors", false, premisesIdAndProcessingType4.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 5 has no errors", false, premisesIdAndProcessingType5.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 6 has no errors", false, premisesIdAndProcessingType6.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 7 has no errors", false, premisesIdAndProcessingType7.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 8 has no errors", false, premisesIdAndProcessingType8.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 9 has no errors", false, premisesIdAndProcessingType9.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 10 has no errors", false, premisesIdAndProcessingType10.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 11 has errors", true, premisesIdAndProcessingType11.HasRowErrors);

			invoiceLine.AQISPremisesIdAndProcessingTypes.RemoveAndDelete(premisesIdAndProcessingType2);
			premisesIdAndProcessingType11.Validation.ValidateAll();
			AssertEquals("PremisesIdAndProcessingType 1 has no errors", false, premisesIdAndProcessingType1.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 3 has no errors", false, premisesIdAndProcessingType3.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 4 has no errors", false, premisesIdAndProcessingType4.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 5 has no errors", false, premisesIdAndProcessingType5.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 6 has no errors", false, premisesIdAndProcessingType6.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 7 has no errors", false, premisesIdAndProcessingType7.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 8 has no errors", false, premisesIdAndProcessingType8.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 9 has no errors", false, premisesIdAndProcessingType9.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 10 has no errors", false, premisesIdAndProcessingType10.HasRowErrors);
			AssertEquals("PremisesIdAndProcessingType 11 has no errors", false, premisesIdAndProcessingType11.HasRowErrors);
		}

		public void TestAQISPremisesIDAndProcessingTypeAreAllEmpty()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			const string message = "This field contains a blank value so will not be sent in the message. If this is not correct, delete this AEP record and save before resubmitting.";
			var premisesIdAndProcessingType1 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType1.PremisesId = "1";
			AssertNoRowMessageError("PremisesId not empty", premisesIdAndProcessingType1, message);
			premisesIdAndProcessingType1.PremisesId = "";
			AssertHasRowMessageError("PremisesId and ProcessingType are empty", premisesIdAndProcessingType1, message);
			premisesIdAndProcessingType1.ProcessingType = "1";
			AssertNoRowMessageError("ProcessingType not empty", premisesIdAndProcessingType1, message);
			premisesIdAndProcessingType1.ProcessingType = "";
			AssertHasRowMessageError("PremisesId and ProcessingType are empty", premisesIdAndProcessingType1, message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
		}
	}
}
