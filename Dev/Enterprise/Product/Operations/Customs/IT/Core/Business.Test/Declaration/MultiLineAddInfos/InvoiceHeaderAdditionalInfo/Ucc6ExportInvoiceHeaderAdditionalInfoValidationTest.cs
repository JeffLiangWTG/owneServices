using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportInvoiceHeaderAdditionalInfoValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
{
	public void TestCheckCSI_ReferenceNumber_TransitionPeriod()
	{
		var expectedMessage = "[E1104] during the transition period, which is active now, Invoice Header>Additional Documents>Reference cannot be longer than 35 characters.";
		invoiceHeaderAdditionalInfo.CSI_SubType = "TRA";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("In TransitionPeriod and Kind = TRA", () =>
			{
				using (TemporarilySetAESTransitionPeriod(isInAESTransitionPeriod: true))
				{
					invoiceHeaderAdditionalInfo.CSI_ReferenceNumber = new ZString('0', 36);
					AssertHasMessageError("CSI_ReferenceNumber length more than 35 chars", invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo, expectedMessage);

					invoiceHeaderAdditionalInfo.CSI_ReferenceNumber = new ZString('0', 35);
					AssertNoMessageError("CSI_ReferenceNumber length equal to 35 chars", invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo, expectedMessage);
				}
			});

			CombineAssertions("Not in TransitionPeriod and Kind = TRA", () =>
			{
				using (TemporarilySetAESTransitionPeriod(isInAESTransitionPeriod: false))
				{
					invoiceHeaderAdditionalInfo.CSI_ReferenceNumber = new ZString('0', 36);
					AssertNoMessageError("CSI_ReferenceNumber length more than 35 chars", invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo, expectedMessage);

					invoiceHeaderAdditionalInfo.CSI_ReferenceNumber = new ZString('0', 35);
					AssertNoMessageError("CSI_ReferenceNumber length equal to 35 chars", invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo, expectedMessage);
				}
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
				invoiceHeaderAdditionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
				invoiceHeaderAdditionalInfo.CSI_Code = "AB01C";
				invoiceHeaderAdditionalInfo.CSI_ReferenceNumber = ZString.Empty;
				AssertHasMessageErrorContaining("CSI_ReferenceNumber is empty, Kind: REF, Code requires CSI_ReferenceNumber", invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceHeaderAdditionalInfo.CSI_ReferenceNumber = "12345";
				AssertNoMessageErrorContaining("CSI_ReferenceNumber is filled, Kind: REF, Code requires CSI_ReferenceNumber", invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceHeaderAdditionalInfo.CSI_Code = "XY01Z";
				invoiceHeaderAdditionalInfo.CSI_ReferenceNumber = ZString.Empty;
				AssertNoMessageErrorContaining("CSI_ReferenceNumber is empty, Kind: REF, Code does not require CSI_ReferenceNumber", invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceHeaderAdditionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
				invoiceHeaderAdditionalInfo.CSI_Code = "AB01C";
				invoiceHeaderAdditionalInfo.Validation.ValidateCSI_ReferenceNumber();
				AssertHasMessageErrorContaining("CSI_ReferenceNumber is empty, Kind: TRA, Code does not matter", invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceHeaderAdditionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				invoiceHeaderAdditionalInfo.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageError("CSI_ReferenceNumber empty, Kind: INF, Code requires CSI_ReferenceNumber", invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}

	public void TestCheckCSI_Description_TransitionPeriod()
	{
		var expectedMessage = "[E1106] during the transition period, which is active now, Invoice Header>Additional Documents>Description cannot be longer than 70 characters.";
		invoiceHeaderAdditionalInfo.CSI_SubType = "INF";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("In TransitionPeriod and Kind = INF", () =>
			{
				using (TemporarilySetAESTransitionPeriod(isInAESTransitionPeriod: true))
				{
					invoiceHeaderAdditionalInfo.CSI_Description = new ZString('0', 71);
					AssertHasMessageError("CSI_Description length more than 70 chars", invoiceHeaderAdditionalInfo.CSI_DescriptionInfo, expectedMessage);

					invoiceHeaderAdditionalInfo.CSI_Description = new ZString('0', 70);
					AssertNoMessageError("CSI_Description length equal to 70 chars", invoiceHeaderAdditionalInfo.CSI_DescriptionInfo, expectedMessage);
				}
			});

			CombineAssertions("Not in TransitionPeriod and Kind = INF", () =>
			{
				using (TemporarilySetAESTransitionPeriod(isInAESTransitionPeriod: false))
				{
					invoiceHeaderAdditionalInfo.CSI_Description = new ZString('0', 71);
					AssertNoMessageError("CSI_Description length more than 70 chars", invoiceHeaderAdditionalInfo.CSI_DescriptionInfo, expectedMessage);

					invoiceHeaderAdditionalInfo.CSI_Description = new ZString('0', 70);
					AssertNoMessageError("CSI_Description length equal to 70 chars", invoiceHeaderAdditionalInfo.CSI_DescriptionInfo, expectedMessage);
				}
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		var invoice = declaration.Invoices.AddNew();
		invoiceHeaderAdditionalInfo = invoice.AdditionalInfos.AddNew();
	}

	IDisposable TemporarilySetAESTransitionPeriod(bool isInAESTransitionPeriod)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, isInAESTransitionPeriod);

	JobDeclaration declaration;
	InvoiceHeaderAdditionalInfo invoiceHeaderAdditionalInfo;
}
