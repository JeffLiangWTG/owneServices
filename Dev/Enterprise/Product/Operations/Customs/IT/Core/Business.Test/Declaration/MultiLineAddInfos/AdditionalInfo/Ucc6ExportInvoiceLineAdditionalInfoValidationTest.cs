using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportInvoiceLineAdditionalInfoValidationTest : BusinessObjectValidationTestCase
{
	public void TestCSI_SubStyle()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions(() =>
			{
				additionalInfo.CSI_SubType = ZString.Empty;
				AssertHasMessageErrorContaining("CSI_SubType Empty", additionalInfo.CSI_SubTypeInfo, MandatoryValidation.YouHaveNotEntered);

				additionalInfo.CSI_SubType = "PQRS";
				AssertHasMessageErrorContaining("Invalid code is not allow", additionalInfo.CSI_SubTypeInfo, ListValidation.InvalidCodeMessageError.ToString());

				additionalInfo.CSI_SubType = "INF";
				AssertNoMessageErrorContaining("CSI_SubType is mandatory", additionalInfo.CSI_SubTypeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}

	public void TestCSI_ReferenceNumber_MandatoryValidation()
	{
		AdditionalInfoTestHelper.SetUpRefCusCodesForAttributeName(Factory);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			CombineAssertions(() =>
			{
				additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
				additionalInfo.CSI_Code = "AB01C";
				additionalInfo.CSI_ReferenceNumber = ZString.Empty;
				AssertHasMessageErrorContaining("CSI_ReferenceNumber is empty, Kind: REF, Code requires CSI_ReferenceNumber", additionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				additionalInfo.CSI_ReferenceNumber = "12345";
				AssertNoMessageErrorContaining("CSI_ReferenceNumber is filled, Kind: REF, Code requires CSI_ReferenceNumber", additionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				additionalInfo.CSI_Code = "XY01Z";
				additionalInfo.CSI_ReferenceNumber = ZString.Empty;
				AssertNoMessageErrorContaining("CSI_ReferenceNumber is empty, Kind: REF, Code does not require CSI_ReferenceNumber", additionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalInfo.CSI_Code = "AB01C";
				additionalInfo.Validation.ValidateCSI_ReferenceNumber();
				AssertHasMessageErrorContaining("CSI_ReferenceNumber is empty, Kind: TRA, Code does not matter", additionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageError("CSI_ReferenceNumber empty, Kind: INF, Code requires CSI_ReferenceNumber", additionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}

	public void TestCheckCSI_ReferenceNumber_TransitionPeriod()
	{
		var expectedMessage = "[E1104] during the transition period, which is active now, Invoice Line>Additional Documents>Reference cannot be longer than 35 characters.";
		additionalInfo.CSI_SubType = "TRA";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("In TransitionPeriod and Kind = TRA", () =>
			{
				using (TemporarilySetAESTransitionPeriod(isInAESTransitionPeriod: true))
				{
					additionalInfo.CSI_ReferenceNumber = new ZString('0', 36);
					AssertHasMessageError("CSI_ReferenceNumber length more than 35 chars", additionalInfo.CSI_ReferenceNumberInfo, expectedMessage);

					additionalInfo.CSI_ReferenceNumber = new ZString('0', 35);
					AssertNoMessageError("CSI_ReferenceNumber length equal to 35 chars", additionalInfo.CSI_ReferenceNumberInfo, expectedMessage);
				}
			});

			CombineAssertions("Not in TransitionPeriod and Kind = TRA", () =>
			{
				using (TemporarilySetAESTransitionPeriod(isInAESTransitionPeriod: false))
				{
					additionalInfo.CSI_ReferenceNumber = new ZString('0', 36);
					AssertNoMessageError("CSI_ReferenceNumber length more than 35 chars", additionalInfo.CSI_ReferenceNumberInfo, expectedMessage);

					additionalInfo.CSI_ReferenceNumber = new ZString('0', 35);
					AssertNoMessageError("CSI_ReferenceNumber length equal to 35 chars", additionalInfo.CSI_ReferenceNumberInfo, expectedMessage);
				}
			});
		}
	}

	public void TestCheckCSI_Description_TransitionPeriod()
	{
		var expectedMessage = "[E1106] during the transition period, which is active now, Invoice Line>Additional Documents>Description cannot be longer than 70 characters.";
		additionalInfo.CSI_SubType = "INF";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("In TransitionPeriod and Kind = INF", () =>
			{
				using (TemporarilySetAESTransitionPeriod(isInAESTransitionPeriod: true))
				{
					additionalInfo.CSI_Description = new ZString('0', 71);
					AssertHasMessageError("CSI_Description length more than 70 chars", additionalInfo.CSI_DescriptionInfo, expectedMessage);

					additionalInfo.CSI_Description = new ZString('0', 70);
					AssertNoMessageError("CSI_Description length equal to 70 chars", additionalInfo.CSI_DescriptionInfo, expectedMessage);
				}
			});

			CombineAssertions("Not in TransitionPeriod and Kind = INF", () =>
			{
				using (TemporarilySetAESTransitionPeriod(isInAESTransitionPeriod: false))
				{
					additionalInfo.CSI_Description = new ZString('0', 71);
					AssertNoMessageError("CSI_Description length more than 70 chars", additionalInfo.CSI_DescriptionInfo, expectedMessage);

					additionalInfo.CSI_Description = new ZString('0', 70);
					AssertNoMessageError("CSI_Description length equal to 70 chars", additionalInfo.CSI_DescriptionInfo, expectedMessage);
				}
			});
		}
	}

	IDisposable TemporarilySetAESTransitionPeriod(bool isInAESTransitionPeriod)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, isInAESTransitionPeriod);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		var invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		additionalInfo = invoiceLine.AdditionalInfos.AddNew();
	}

	JobDeclaration declaration;
	AdditionalInfo additionalInfo;
	JobComInvoiceLine invoiceLine;
}
