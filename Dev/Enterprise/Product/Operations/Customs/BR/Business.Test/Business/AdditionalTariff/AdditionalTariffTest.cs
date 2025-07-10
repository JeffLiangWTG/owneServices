using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(AdditionalTariff))]
	class AdditionalTariffTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetLegalActSubjectByTariffType()
		{
			CombineAssertions(() =>
			{
				var subject = AdditionalTariff.GetLegalActSubjectByTariffType(ChildTariffTypeList.Codes.LEBIT);
				AssertEquals("subject must be DutyTariff", AdditionalTaxTypeList.Codes.ExDutyTariff, subject);

				subject = AdditionalTariff.GetLegalActSubjectByTariffType(ChildTariffTypeList.Codes.LETEC);
				AssertEquals("subject must be DutyTariff", AdditionalTaxTypeList.Codes.ExDutyTariff, subject);

				subject = AdditionalTariff.GetLegalActSubjectByTariffType(ChildTariffTypeList.Codes.COVID19);
				AssertEquals("subject must be DutyTariff", AdditionalTaxTypeList.Codes.ExDutyTariff, subject);

				subject = AdditionalTariff.GetLegalActSubjectByTariffType(ChildTariffTypeList.Codes.BIT);
				AssertEquals("subject must be DutyTariff", AdditionalTaxTypeList.Codes.ExDutyTariff, subject);

				subject = AdditionalTariff.GetLegalActSubjectByTariffType(ChildTariffTypeList.Codes.BK);
				AssertEquals("subject must be DutyTariff", AdditionalTaxTypeList.Codes.ExDutyTariff, subject);

				subject = AdditionalTariff.GetLegalActSubjectByTariffType(ChildTariffTypeList.Codes.IPI);
				AssertEquals("subject must be IPITariff", AdditionalTaxTypeList.Codes.ExIPITariff, subject);

				subject = AdditionalTariff.GetLegalActSubjectByTariffType("XXX");
				AssertEquals("subject must be empty", ZString.Empty, subject);
			});
		}

		public void TestProperites()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var additionTariff = invoiceLine.AdditionalTariffs.AddNew();
			additionTariff.TariffCode = "09022000_001";
			additionTariff.TariffType = ChildTariffTypeList.Codes.COVID19;
			additionTariff.LegalActType = "1";
			additionTariff.LegalActIssuingBody = "2";
			additionTariff.LegalActNumber = "3";
			additionTariff.LegalActYear = "2021";
			CombineAssertions(() =>
			{
				AssertEquals("BZ_Tariff", "09022000_001", additionTariff.TariffDetail.BZ_Tariff);
				AssertEquals("BZ_Type", "COVID", additionTariff.TariffDetail.BZ_Type);
				AssertEquals("BZ_LegalActSubject", additionTariff.LegalActSubject, additionTariff.TariffDetail.BZ_LegalActSubject);
				AssertEquals("CSI_SubType", "1", additionTariff.LegalAct.CSI_SubType);
				AssertEquals("CSI_Code", "1", additionTariff.LegalAct.CSI_Code);
				AssertEquals("CSI_IssuerType", "2", additionTariff.LegalAct.CSI_IssuerType);
				AssertEquals("CSI_ReferenceNumber", "3", additionTariff.LegalAct.CSI_ReferenceNumber);
				AssertEquals("CSI_YearOfIssue", "2021", additionTariff.LegalAct.CSI_YearOfIssue);
				AssertEquals("TariffTypeDescription", "COVID19", additionTariff.TariffTypeDescription);
			});
		}

		public void TestExNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "09022000";

			var additionTariff = invoiceLine.AdditionalTariffs.AddNew();
			AssertEquals("ExNumber", ZString.Empty, additionTariff.ExNumber);

			additionTariff.TariffCode = "09022000_001";
			AssertEquals("ExNumber", "001", additionTariff.ExNumber);

			additionTariff.ExNumber = "002";
			AssertEquals("ExNumber", "002", additionTariff.ExNumber);
			AssertEquals("TariffCode", "09022000_002", additionTariff.TariffCode);

			additionTariff.ExNumber = ZString.Empty;
			AssertEquals("ExNumber", ZString.Empty, additionTariff.ExNumber);
			AssertEquals("TariffCode", ZString.Empty, additionTariff.TariffCode);
		}

		public void TestDefaultLegalActSubjectOnTariffTypeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var additionTariff = invoiceLine.AdditionalTariffs.AddNew();
			additionTariff.TariffCode = "09022000_001";
			additionTariff.TariffType = ChildTariffTypeList.Codes.COVID19;
			AssertEquals("subject must be 1", AdditionalTaxTypeList.Codes.ExDutyTariff, additionTariff.LegalActSubject);
			additionTariff.TariffType = ChildTariffTypeList.Codes.IPI;
			AssertEquals("subject must be 2", AdditionalTaxTypeList.Codes.ExIPITariff, additionTariff.LegalActSubject);
			additionTariff.TariffType = "XX";
			AssertEquals("subject should not changed", AdditionalTaxTypeList.Codes.ExIPITariff, additionTariff.LegalActSubject);
		}

		public void TestGetLegalActSubjects()
		{
			var codesList = AdditionalTariff.GetLegalActSubjects(Factory.New<JobComInvoiceLine>());
			CombineAssertions(() =>
			{
				AssertEquals("The count should be", 3, codesList.Count);
				AssertEquals("The codes should be", "1, 2, 3", codesList.CodesAsString);
			});

			codesList = AdditionalTariff.GetLegalActSubjects(Factory.New<CusClassPartPivot>());
			CombineAssertions(() =>
			{
				AssertEquals("The count should be", 2, codesList.Count);
				AssertEquals("The codes should be", "1, 2", codesList.CodesAsString);
			});
		}

		public void TestGetLegalActSubjectDescriptionByTariffType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var additionTariff = invoiceLine.AdditionalTariffs.AddNew();
			additionTariff.TariffCode = "09022000_001";
			additionTariff.TariffType = ChildTariffTypeList.Codes.COVID19;
			additionTariff.LegalActType = "1";
			additionTariff.LegalActIssuingBody = "2";
			additionTariff.LegalActNumber = "3";
			additionTariff.LegalActYear = "2021";

			CombineAssertions(() =>
			{
				AssertEquals("subject must be DutyTariff", AdditionalTaxTypeList.Descriptions.ExDutyTariff, additionTariff.LegalActSubjectDescription);

				additionTariff.LegalActSubject = AdditionalTariff.GetLegalActSubjectByTariffType(ChildTariffTypeList.Codes.LETEC);
				AssertEquals("subject must be DutyTariff", AdditionalTaxTypeList.Descriptions.ExDutyTariff, additionTariff.LegalActSubjectDescription);

				additionTariff.LegalActSubject = AdditionalTariff.GetLegalActSubjectByTariffType(ChildTariffTypeList.Codes.COVID19);
				AssertEquals("subject must be DutyTariff", AdditionalTaxTypeList.Descriptions.ExDutyTariff, additionTariff.LegalActSubjectDescription);

				additionTariff.LegalActSubject = AdditionalTariff.GetLegalActSubjectByTariffType(ChildTariffTypeList.Codes.BIT);
				AssertEquals("subject must be DutyTariff", AdditionalTaxTypeList.Descriptions.ExDutyTariff, additionTariff.LegalActSubjectDescription);

				additionTariff.LegalActSubject = AdditionalTariff.GetLegalActSubjectByTariffType(ChildTariffTypeList.Codes.BK);
				AssertEquals("subject must be DutyTariff", AdditionalTaxTypeList.Descriptions.ExDutyTariff, additionTariff.LegalActSubjectDescription);

				additionTariff.LegalActSubject = AdditionalTariff.GetLegalActSubjectByTariffType(ChildTariffTypeList.Codes.IPI);
				AssertEquals("subject must be IPITariff", AdditionalTaxTypeList.Descriptions.ExIPITariff, additionTariff.LegalActSubjectDescription);

				additionTariff.LegalActSubject = AdditionalTariff.GetLegalActSubjectByTariffType("XXX");
				AssertEquals("subject must be empty", ZString.Empty, additionTariff.LegalActSubjectDescription);
			});
		}

		[TestDate(2023, 1, 1)]
		public void TestGetTariffTypeListByLegalActSubject()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);

			CombineAssertions(() =>
			{
				var tariffTypeList = AdditionalTariff.GetTariffTypeListByLegalActSubject(Factory, ZDateTime.Now, AdditionalTaxTypeList.Codes.ExDutyTariff);
				AssertSame("The list cached", tariffTypeList, AdditionalTariff.GetTariffTypeListByLegalActSubject(Factory, ZDateTime.Now, AdditionalTaxTypeList.Codes.ExDutyTariff));
				AssertEquals("The count should be", 5, tariffTypeList.Count);
				AssertEquals("The elements should be", "LEBIT - LEBIT\r\nLETEC - LETEC\r\nCOVID - COVID19\r\nBIT - BIT\r\nBK - BK", tariffTypeList.ElementsAsString);

				tariffTypeList = AdditionalTariff.GetTariffTypeListByLegalActSubject(Factory, ZDateTime.Now, AdditionalTaxTypeList.Codes.ExIPITariff);
				AssertSame("The list cached", tariffTypeList, AdditionalTariff.GetTariffTypeListByLegalActSubject(Factory, ZDateTime.Now, AdditionalTaxTypeList.Codes.ExIPITariff));
				AssertEquals("The count should be", 1, tariffTypeList.Count);
				AssertEquals("The elements should be", "IPI - IPI", tariffTypeList.ElementsAsString);

				tariffTypeList = AdditionalTariff.GetTariffTypeListByLegalActSubject(Factory, ZDateTime.Now, AdditionalTaxTypeList.Codes.TariffAgreement);
				AssertSame("The list cached", tariffTypeList, AdditionalTariff.GetTariffTypeListByLegalActSubject(Factory, ZDateTime.Now, AdditionalTaxTypeList.Codes.TariffAgreement));
				AssertEquals("The count should be", 4, tariffTypeList.Count);
				AssertEquals("The elements should be", "AR99, ASGPC, CO99, MX99", tariffTypeList.CodesAsString);

				tariffTypeList = AdditionalTariff.GetTariffTypeListByLegalActSubject(Factory, ZDateTime.Now, ZString.Empty);
				AssertSame("The list cached", tariffTypeList, AdditionalTariff.GetTariffTypeListByLegalActSubject(Factory, ZDateTime.Now, ZString.Empty));
				AssertType<ChildTariffTypeList>(tariffTypeList);
				AssertEquals("The count should be", 6, tariffTypeList.Count);
			});
		}

		public void TestTariffAgreementCode()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var additionTariff = invoiceLine.AdditionalTariffs.AddNew();
			AssertNull(additionTariff.TariffAgreementCode);
			additionTariff.LegalActSubject = AdditionalTaxTypeList.Codes.TariffAgreement;
			AssertNull(additionTariff.TariffAgreementCode);
			additionTariff.TariffType = "AR99";
			AssertEquals("AR99", additionTariff.TariffAgreementCode.ZZD_Code);
			additionTariff.LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;
			AssertNull(additionTariff.TariffAgreementCode);
		}

		public void TestDefaultLegalActInformationAndReadOnly()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var additionTariff = invoiceLine.AdditionalTariffs.AddNew();

			additionTariff.LegalActSubject = AdditionalTaxTypeList.Codes.TariffAgreement;
			additionTariff.TariffType = "ASGPC";
			AssertLegalActProperties("DEC", "EXEC", "5106", "2004", true);

			additionTariff.TariffType = "MX99";
			AssertLegalActProperties("Proto", "Aladi", "5902", ZString.Empty, true);

			additionTariff.LegalActType = ZString.Empty;
			additionTariff.LegalActIssuingBody = ZString.Empty;
			additionTariff.LegalActNumber = ZString.Empty;
			additionTariff.LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;
			additionTariff.TariffType = "ASGPC";
			AssertLegalActProperties(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, false);

			additionTariff.LegalActSubject = AdditionalTaxTypeList.Codes.TariffAgreement;
			AssertLegalActProperties("DEC", "EXEC", "5106", "2004", true);

			additionTariff.LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;
			AssertLegalActProperties("DEC", "EXEC", "5106", "2004", false);

			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			AssertLegalActProperties(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, true);

			additionTariff.LegalActType = "DEC";
			additionTariff.LegalActIssuingBody = "EXEC";
			additionTariff.LegalActNumber = "5106";
			additionTariff.LegalActYear = "2004";
			invoiceLine.DutyRateIsOverridden = true;
			AssertLegalActProperties("DEC", "EXEC", "5106", "2004", false);

			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.FreeTradeAgreement;
			additionTariff = invoiceLine.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.TariffAgreement);
			invoiceLine.DutyRateIsOverridden = false;
			AssertLegalActProperties(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, true);
			invoiceLine.DutyRateIsOverridden = true;
			AssertLegalActProperties(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, true);

			additionTariff.LegalActType = "DEC";
			additionTariff.LegalActIssuingBody = "EXEC";
			additionTariff.LegalActNumber = "5106";
			additionTariff.LegalActYear = "2004";
			additionTariff.LegalActSubject = AdditionalTaxTypeList.Codes.ExIPITariff;
			AssertLegalActProperties("DEC", "EXEC", "5106", "2004", false);

			void AssertLegalActProperties(ZString expectedType, ZString expectedIssuingBody, ZString expectedNumber, ZString expectedYear, bool readOnly)
			{
				CombineAssertions($"Subject={additionTariff.LegalActSubject} TariffType={additionTariff.TariffType}", () =>
				{
					AssertEquals("LegalActType", expectedType, additionTariff.LegalActType);
					AssertEquals("LegalActType.ReadOnly", readOnly, additionTariff.LegalActTypeInfo.ReadOnly);

					AssertEquals("LegalActIssuingBody", expectedIssuingBody, additionTariff.LegalActIssuingBody);
					AssertEquals("LegalActIssuingBody.ReadOnly", readOnly, additionTariff.LegalActIssuingBodyInfo.ReadOnly);

					AssertEquals("LegalActNumber", expectedNumber, additionTariff.LegalActNumber);
					AssertEquals("LegalActNumber.ReadOnly", readOnly, additionTariff.LegalActNumberInfo.ReadOnly);

					AssertEquals("LegalActYear", expectedYear, additionTariff.LegalActYear);
					AssertEquals("LegalActYear.ReadOnly", readOnly, additionTariff.LegalActYearInfo.ReadOnly);
				});
			}
		}

		public void TestDelete()
		{
			var additionalTariff = GetNewBusinessObject() as AdditionalTariff;
			var tariffDetail = additionalTariff.TariffDetail;
			additionalTariff.Delete();
			Assert("IsDeleted", additionalTariff.IsDeleted);
			Assert("TariffDetail.IsDeleted", tariffDetail.IsDeleted);
			Assert("LegalAct.IsDeleted", additionalTariff.LegalAct.IsDeleted);
		}

		public void TestTariffDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var legalActInfo = invoiceLine.LegalActInfos.AddNew();
			legalActInfo.CSI_SubType = AdditionalTaxTypeList.Codes.ExDutyTariff;
			var additional = new AdditionalTariff(legalActInfo, null);
			AssertNotNull("A new TariffDetail created", additional.TariffDetail);
			AssertEquals("TariffDetail.BZ_LegalActSubject", AdditionalTaxTypeList.Codes.ExDutyTariff, additional.TariffDetail.BZ_LegalActSubject);
			AssertEquals("TariffDetail.HasChanges", false, additional.TariffDetail.HasChanges);
		}

		public void TestPropertiesReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var additional = invoiceLine.AdditionalTariffs.AddNew(AdditionalTaxTypeList.Codes.TariffAgreement) as AdditionalTariff;
			CombineAssertions("Non-cloned", () =>
			{
				Assert("ExNumber should NOT be readonly", !additional.ExNumberInfo.ReadOnly);
				Assert("TariffType should NOT be readonly", !additional.TariffTypeInfo.ReadOnly);
				Assert("LegalActSubject should NOT be readonly", !additional.LegalActSubjectInfo.ReadOnly);
				Assert("LegalActSubjectDescription should NOT be readonly", !additional.LegalActSubjectDescriptionInfo.ReadOnly);
				Assert("LegalActType should NOT be readonly", !additional.LegalActTypeInfo.ReadOnly);
				Assert("LegalActNumber should NOT be readonly", !additional.LegalActNumberInfo.ReadOnly);
				Assert("LegalActYear should NOT be readonly", !additional.LegalActYearInfo.ReadOnly);
			});

			var licDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var licEntryInstruction = licDeclaration.CustomsEntryInstructions.AddNew();
			licEntryInstruction.CEI_JE = licDeclaration.PK;
			licEntryInstruction.CEI_Description = "TEST1";

			var licInvLine = licDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			licInvLine.JI_CEI = licEntryInstruction.PK;

			var clonedInvLine = declaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(licEntryInstruction) }).Single();
			AssertEquals(0, clonedInvLine.AdditionalTariffs.Count);
			additional = clonedInvLine.AdditionalTariffs.AddNew(AdditionalTaxTypeList.Codes.TariffAgreement) as AdditionalTariff;
			CombineAssertions("Non-cloned", () =>
			{
				Assert("ExNumber should NOT be readonly", !additional.ExNumberInfo.ReadOnly);
				Assert("TariffType should NOT be readonly", !additional.TariffTypeInfo.ReadOnly);
				Assert("LegalActSubject should NOT be readonly", !additional.LegalActSubjectInfo.ReadOnly);
				Assert("LegalActSubjectDescription should NOT be readonly", !additional.LegalActSubjectDescriptionInfo.ReadOnly);
				Assert("LegalActType should NOT be readonly", !additional.LegalActTypeInfo.ReadOnly);
				Assert("LegalActNumber should NOT be readonly", !additional.LegalActNumberInfo.ReadOnly);
				Assert("LegalActYear should NOT be readonly", !additional.LegalActYearInfo.ReadOnly);
			});
			declaration.DetachImportLicense(new[] { licEntryInstruction });

			licInvLine.JI_SecondaryPreference = "OMC";
			clonedInvLine = declaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(licEntryInstruction) }).Single();
			AssertEquals(1, clonedInvLine.AdditionalTariffs.Count);
			additional = clonedInvLine.AdditionalTariffs.AddNew(AdditionalTaxTypeList.Codes.TariffAgreement) as AdditionalTariff;
			CombineAssertions("Cloned", () =>
			{
				Assert("ExNumber should NOT be readonly", !additional.ExNumberInfo.ReadOnly);
				Assert("TariffType should be readonly", additional.TariffTypeInfo.ReadOnly);
				Assert("LegalActSubject should be readonly", additional.LegalActSubjectInfo.ReadOnly);
				Assert("LegalActSubjectDescription should be readonly", additional.LegalActSubjectDescriptionInfo.ReadOnly);
				Assert("LegalActType should be readonly", additional.LegalActTypeInfo.ReadOnly);
				Assert("LegalActNumber should be readonly", additional.LegalActNumberInfo.ReadOnly);
				Assert("LegalActYear should be readonly", additional.LegalActYearInfo.ReadOnly);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var cusLineTariffDetails = invoiceLine.CusLineTariffDetails.AddNew();
			var legalActInfo = invoiceLine.LegalActInfos.AddNew();
			return new AdditionalTariff(legalActInfo, cusLineTariffDetails);
		}
	}
}
