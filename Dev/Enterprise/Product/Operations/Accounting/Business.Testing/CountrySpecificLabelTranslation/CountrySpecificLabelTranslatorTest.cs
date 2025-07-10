using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public class CountrySpecificLabelTranslatorTest : TestCaseWithFactory
	{
		public void TestCountrySpecificLabelTranslator_Country_Implement_CountrySpecificLabelTranslator()
		{
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var labelTranslator = new CountrySpecificLabelTranslator() as ILabelTranslator;
			var expectedResult = "Authorization Data";
			var unexpectedLanguage = Core.SharedConstants.Languages.Bulgarian;

			mockIAccountingCountryFactory.As<ICountrySpecificLabelTranslator>().Setup(x => x.GetTranslation(It.IsAny<LabelsEnum>())).Returns(expectedResult);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			using (Res.TemporarilySwitchLanguage(unexpectedLanguage))
			{
				AssertEquals(expectedResult, labelTranslator.GetTranslation(LabelsEnum.InvoiceAuthorisationRecordAuthorisationDataLabel));
			}

			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), Times.Once);
			mockIAccountingCountryFactory.As<ICountrySpecificLabelTranslator>().Verify(x => x.GetTranslation(LabelsEnum.InvoiceAuthorisationRecordAuthorisationDataLabel), Times.Once);
		}

		public void TestCountrySpecificLabelTranslator_DefaultTranslation()
		{
			var expectedLabelTranslations = ExpectedLabelTranslations;
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			var labelTranslator = new CountrySpecificLabelTranslator() as ILabelTranslator;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				foreach (LabelsEnum label in Enum.GetValues(typeof(LabelsEnum)))
				{
					foreach (var language in UnexpectedLanguages)
					{
						using (Res.TemporarilySwitchLanguage(language))
						{
							expectedLabelTranslations.TryGetValue(label, out var expectedLabelTranslation);
							if (expectedLabelTranslation != null)
							{
								var expectedLabelTranslationFound = expectedLabelTranslation.FirstOrDefault(x => x.languages.Contains(language));
								var result = labelTranslator.GetTranslation(label, expectedLabelTranslationFound.parametersForTranslation);

								if (expectedLabelTranslationFound.translation != null)
								{
									AssertEquals($"The label {label} of {language} should be correct", expectedLabelTranslationFound.translation, result);
								}
								else
								{
									AssertNull($"the translated label {label} of language {language} should be null", result);
								}
							}
							else
							{
								var result = labelTranslator.GetTranslation(label, "XXX", "XXX");
								AssertNull($"the translated label {label} of language {language} should be null", result);
							}
						}
					}
				}
			}

			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(Core.Constants.CountryCodes.Brazil), Times.Once);
		}

		public void TestCountrySpecificLabelTranslator_GetDefaultLabelTranslation_ValidateException()
		{
			var mockIGlobalAccountingCountryFactory = SetupMocks();
			var labelTranslator = new CountrySpecificLabelTranslator();

			AssertExceptionThrown<ArgumentException>("Invalid parameter value", () => labelTranslator.GetDefaultLabelTranslation(null));

			AssertExceptionThrown<ArgumentException>("Invalid parameter value", () => labelTranslator.GetDefaultLabelTranslation((LabelsEnum)1000));
		}

		[ExpectNoExceptions]
		public void TestCountrySpecificLabelTranslator_IAccountingCountryFactoryCaching()
		{
			var unexpectedLanguage = Core.SharedConstants.Languages.Spanish;
			var label = LabelsEnum.InvoiceAuthorisationRecordAuthorisationDataLabel;
			var mockIGlobalAccountingCountryFactory = SetupMocks();
			var labelTranslator = new CountrySpecificLabelTranslator() as ILabelTranslator;

			using (Res.TemporarilySwitchLanguage(unexpectedLanguage))
			{
				labelTranslator.GetTranslation(label);

				mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), Times.Once);
				mockIGlobalAccountingCountryFactory.Reset();

				labelTranslator.GetTranslation(label);
				mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), Times.Never);
			}
		}

		readonly string[] UnexpectedLanguages = new string[] { Core.Constants.Languages.Afrikaans, Core.Constants.Languages.Spanish };

		Mock<IGlobalAccountingCountryFactory> SetupMocks()
		{
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIAccountingDependencyFactory = new Mock<IAccountingDependencyFactory>();
			var mockILabelTranslator = new Mock<ILabelTranslator>();

			mockILabelTranslator.Setup(x => x.GetTranslation(It.IsAny<LabelsEnum>())).Returns(It.IsAny<ZString>());
			mockIAccountingDependencyFactory.Setup(x => x.GetCountrySpecificLabelTranslator()).Returns(mockILabelTranslator.Object);
			mockIAccountingCountryFactory.As<ICountrySpecificLabelTranslator>().Setup(x => x.GetTranslation(It.IsAny<LabelsEnum>())).Returns(It.IsAny<ZString>());
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			return mockIGlobalAccountingCountryFactory;
		}

		protected Dictionary<LabelsEnum, (string[] languages, object[] parametersForTranslation, string translation)[]> ExpectedLabelTranslations =>
			new Dictionary<LabelsEnum, (string[] languages, object[] parametersForTranslation, string translation)[]>
			{
				{
					LabelsEnum.InvoiceAuthorisationRecordAuthorisationDataLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Authorization Data" )
					}
				},
				{
					LabelsEnum.InvoiceAuthorisationRecordDebtorNumberLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Debtor Number" )
					}
				},
				{
					LabelsEnum.InvoiceAuthorisationRecordIssuerAuthorizationDataLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Issuer Authorization Data" )
					}
				},
				{
					LabelsEnum.InvoiceAuthorisationRecordIssuerCertificateIdentifierLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Issuer Certificate Identifier" )
					}
				},
				{
					LabelsEnum.GovernmentCreditTermsLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Government Credit Terms" )
					}
				},
				{
					LabelsEnum.InvoiceAuthorisationRecordPlaceOfIssueLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Place of Issue" )
					}
				},
				{
					LabelsEnum.InvoiceAuthorisationRecordCounterLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Invoice Counter" )
					}
				},
				{
					LabelsEnum.InvoiceAuthorisationRecordTimeLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "SDC Time" )
					}
				},
				{
					LabelsEnum.ComplianceSubtypeLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Compliance Subtype" )
					}
				},
				{
					LabelsEnum.EInvoicingGovernmentAllocatedNumberLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Government Allocated Number" )
					}
				},
				{
					LabelsEnum.TaxRegimeInformationLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Tax Regime" )
					}
				},
				{
					LabelsEnum.GovernmentAgreedPaymentMethodLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Government Agreed Payment Method" )
					}
				},
				{
					LabelsEnum.InvoiceAuthorizationRecordVerificationURLLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Authorization Record Verification URL" )
					}
				},
				{
					LabelsEnum.SubjectToTaxLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Subject to Tax" )
					}
				},
				{
					LabelsEnum.MeasurementUnitLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Measurement unit" )
					}
				},
				{
					LabelsEnum.GovernmentReportingCodeLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Government charge code" )
					}
				},
				{
					LabelsEnum.TaxBaseAmountLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Tax Base" )
					}
				},
				{
					LabelsEnum.TaxLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Tax" )
					}
				},
				{
					LabelsEnum.TaxAmountLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Amount" )
					}
				},
				{
					LabelsEnum.IVATaxLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "IVA" )
					}
				},
				{
					LabelsEnum.RetentionTaxLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Retention" )
					}
				},
				{
					LabelsEnum.DebtorTaxRegimeLabel,
					new []
					{
						(UnexpectedLanguages, CountryLabelTranslatorTest.EmptyParametersForTranslation,  "Debtor's Tax Regime" )
					}
				},
				{
					LabelsEnum.RecipientConsumptionTaxRegimeHeading,
					new []
					{
						(UnexpectedLanguages,  new object[] { "XXX" },  "XXX REGIME" )
					}
				}
			};
	}
}
