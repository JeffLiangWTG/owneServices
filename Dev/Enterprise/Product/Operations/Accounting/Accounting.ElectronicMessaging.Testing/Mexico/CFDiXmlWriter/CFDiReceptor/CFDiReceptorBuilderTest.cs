using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business.CountryCompliance.Mexico;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.MexicoOrgCusCodeInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	public class CFDiReceptorBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<CFDiReceptorBuilder>(new CFDiComprobanteBuilder().CFDiReceptorBuilder_ExposedForTestOnly);
		}

		public void TestReceptorWithCompanyName()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { CompanyName = "TERNIUM MEXICO SA DE CV" }
			};

			var builder = (ICFDiReceptorBuilder)new CFDiReceptorBuilder();
			var receptor = builder.BuildReceptorInfo(transaction);

			AssertEquals("TERNIUM MEXICO SA DE CV", receptor.Nombre);
		}

		public void TestReceptorWithValidRegistrationNumberRFC()
		{
			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			address.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
			{
				new RegistrationNumber()
				{
					CountryOfIssue = new Country() { Code = "MX" },
					Type =  new RegistrationNumberType() { Code = "RFC" },
					Value = "TES030201001"
				}
			});

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrganizationAddress = address
			};

			var builder = (ICFDiReceptorBuilder)new CFDiReceptorBuilder();
			var receptor = builder.BuildReceptorInfo(transaction);

			AssertEquals("TES030201001", receptor.Rfc);
		}

		public void TestReceptorWithValidRegistrationNumberRFG()
		{
			var address1 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			address1.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
			{
				new RegistrationNumber()
				{
					CountryOfIssue = new Country() { Code = "MX" },
					Type =  new RegistrationNumberType() { Code = "RFG" },
					Value = "TES030201002"
				}
			});
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrganizationAddress = address1
			};

			var builder = (ICFDiReceptorBuilder)new CFDiReceptorBuilder();
			var receptor = builder.BuildReceptorInfo(transaction);

			AssertEquals("TES030201002", receptor.Rfc);
		}

		public void TestReceptorInvalidRegistrationNumber()
		{
			var address1 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CompanyName = "TERNIUM MEXICO SA DE CV",
			};
			address1.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
					{
						new RegistrationNumber()
						{
							CountryOfIssue = new Country() { Code = "MX" },
							Type =  new RegistrationNumberType() { Code = "VAT" },
							Value = "TES030201001"
						}
					});
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrganizationAddress = address1
			};

			var builder = (ICFDiReceptorBuilder)new CFDiReceptorBuilder();
			var receptor = builder.BuildReceptorInfo(transaction);

			AssertNullOrEmpty("RFC should be null because VAT Code doesn't exists for Mexico.", receptor.Rfc);
		}

		public void TestReceptorWithMultipleRegistrationNumber()
		{
			var address1 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CompanyName = "TERNIUM MEXICO SA DE CV",
			};
			address1.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
			{
				new RegistrationNumber()
				{
					CountryOfIssue = new Country() { Code = "MX" },
					Type =  new RegistrationNumberType() { Code = "VAT" },
					Value = "TES030201001"
				},
				new RegistrationNumber()
				{
					CountryOfIssue = new Country() { Code = "MX" },
					Type =  new RegistrationNumberType() { Code = "RFC" },
					Value = "TES030201002"
				}
			});
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrganizationAddress = address1
			};

			var builder = (ICFDiReceptorBuilder)new CFDiReceptorBuilder();
			var receptor = builder.BuildReceptorInfo(transaction);

			AssertEquals("TES030201002", receptor.Rfc);
		}

		public void TestReceptorBuilder_TransactionInfoHelperCreatorType()
		{
			var adjustPostedInvoiceHelper = new CFDiReceptorBuilder();
			AssertType<TransactionInfoHelper>(adjustPostedInvoiceHelper.TransactionInfoHelper_ExposedForTestOnly);
		}

		[ExpectNoExceptions]
		public void TestReceptorBuilder_GetRegistrationCodes()
		{
			var expectedCodeRFC = "RFC030201001";
			var transaction = baseTransationInfoWithOrganizationAddress_ForTestOnly("RFC", expectedCodeRFC);
			AssertCode(transaction, "RFC");

			var expectedCodeRFG = "RFG030201002";
			transaction = baseTransationInfoWithOrganizationAddress_ForTestOnly("RFG", expectedCodeRFG);
			AssertCode(transaction, "RFG");

			void AssertCode(TransactionInfo transactionInfo, string registrationTypeCode)
			{
				var creator = new CFDiReceptorBuilder();
				var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
				creator.SubstituteTransactionInfoHelper_ForTestOnly(transactionInfoHelperMock.Object);

				var receptor = (creator as ICFDiReceptorBuilder).BuildReceptorInfo(transactionInfo);
				transactionInfoHelperMock.Verify(x => x.GetRegistrationCode(transactionInfo.OrganizationAddress, "MX", registrationTypeCode), Times.Once);
			}
		}

		public void TestBuildEmisorCodeCFDValid()
		{
			var usosCFDI = ((new MexicoEInvoicingExtension() as IMexicoEInvoicingExtension).GetUsosCFDI()).GetAllCodesZString();
			foreach (var item in usosCFDI)
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				};
				transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
				{
					new RegistrationNumber()
					{
						CountryOfIssue = new Country { Code = "MX" },
						Type = new RegistrationNumberType() { Code = "CFD" },
						Value = item
					}
				});
				var builder = new CFDiReceptorBuilder() as ICFDiReceptorBuilder;
				var receptor = builder.BuildReceptorInfo(transaction);
				var usoCFDiAllowedEnumValues = Enum.GetValues(typeof(c_UsoCFDI));

				AssertCollectionContains(receptor.UsoCFDI, usoCFDiAllowedEnumValues);
			}
		}

		public void TestMexicoEinvoicingExtension_ValidGetUsosCFDI()
		{
			var expectedValues = (new MexicoEInvoicingExtension() as IMexicoEInvoicingExtension).GetUsosCFDI().GetAllCodesZString();
			var usoCFDiAllowedEnumValues = Enum.GetValues(typeof(c_UsoCFDI)).OfType<object>().Select(o => o.ToString()).ToArray();

			AssertContainsExactElementsInAnyOrder("Elements in GetExpectedvalidCodes is the same as in c_UsoCFDI enum", expectedValues, usoCFDiAllowedEnumValues);
		}

		public void TestEmptyCFDCode()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var builder = new CFDiReceptorBuilder() as ICFDiReceptorBuilder;
			var receptor = builder.BuildReceptorInfo(transaction);

			AssertEquals(receptor.UsoCFDI, c_UsoCFDI.G03);
		}

		public void TestGetNotValidCFDCode()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					CompanyName = "TERNIUM MEXICO SA DE CV",
				}
			};
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
			{
				new RegistrationNumber()
				{
					CountryOfIssue = new Country { Code = "MX" },
					Type = new RegistrationNumberType() { Code = "CFD" },
					Value = "A0X45"
				}
			});
			var builder = (ICFDiReceptorBuilder)new CFDiReceptorBuilder();
			var receptor = builder.BuildReceptorInfo(transaction);

			AssertEquals(receptor.UsoCFDI, c_UsoCFDI.G03);
		}

		public void TestReceptorBuilder_DomiciliofiscalReceptor_OrganizationAddress()
		{
			var builder = (ICFDiReceptorBuilder)new CFDiReceptorBuilder();

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var receptor = builder.BuildReceptorInfo(transaction);
			AssertNull(nameof(receptor.DomicilioFiscalReceptor), receptor.DomicilioFiscalReceptor);

			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);

			receptor = builder.BuildReceptorInfo(transaction);
			AssertNull(nameof(receptor.DomicilioFiscalReceptor), receptor.DomicilioFiscalReceptor);

			transaction.OrganizationAddress.Postcode = "00109";
			receptor = builder.BuildReceptorInfo(transaction);
			AssertEquals(nameof(receptor.DomicilioFiscalReceptor), "00109", receptor.DomicilioFiscalReceptor);
		}

		public void TestReceptorBuilder_DomiciliofiscalReceptor_BranchAddressIsNulL()
		{
			var builder = (ICFDiReceptorBuilder)new CFDiReceptorBuilder();

			var transaction = baseTransationInfoWithOrganizationAddress_ForTestOnly("RFG", "XXX");
			var receptor = builder.BuildReceptorInfo(transaction);

			AssertNull(nameof(receptor.DomicilioFiscalReceptor), receptor.DomicilioFiscalReceptor);
		}

		public void TestReceptorBuilder_DomiciliofiscalReceptor_BranchAddress_PostCodeIsNull()
		{
			var builder = (ICFDiReceptorBuilder)new CFDiReceptorBuilder();

			var transaction = baseTransationInfoWithOrganizationAddress_ForTestOnly("RFG", "XXX");
			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			var receptor = builder.BuildReceptorInfo(transaction);

			AssertNull(nameof(receptor.DomicilioFiscalReceptor), receptor.DomicilioFiscalReceptor);
		}

		public void TestReceptorBuilder_DomiciliofiscalReceptor_BranchAddress_PostCodeIsEmpty()
		{
			var builder = (ICFDiReceptorBuilder)new CFDiReceptorBuilder();

			var transaction = baseTransationInfoWithOrganizationAddress_ForTestOnly("RFG", "XXX");
			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.Postcode = string.Empty;
			var receptor = builder.BuildReceptorInfo(transaction);

			AssertEquals(nameof(receptor.DomicilioFiscalReceptor), string.Empty, receptor.DomicilioFiscalReceptor);
		}

		public void TestReceptorBuilder_DomiciliofiscalReceptor_BranchAddress_RegistrationNumberRFG()
		{
			var builder = (ICFDiReceptorBuilder)new CFDiReceptorBuilder();

			var transaction = baseTransationInfoWithOrganizationAddress_ForTestOnly("RFG", "XXX");
			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.Postcode = "1234";
			var receptor = builder.BuildReceptorInfo(transaction);

			AssertEquals(nameof(receptor.DomicilioFiscalReceptor), "1234", receptor.DomicilioFiscalReceptor);
		}

		public void TestReceptorBuilder_DomiciliofiscalReceptor_BranchAddress_RegistrationNumber_Any()
		{
			var builder = (ICFDiReceptorBuilder)new CFDiReceptorBuilder();

			var transaction = baseTransationInfoWithOrganizationAddress_ForTestOnly("XXX", "XXX");
			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.Postcode = "1234";
			var receptor = builder.BuildReceptorInfo(transaction);

			AssertNull(nameof(receptor.DomicilioFiscalReceptor), receptor.DomicilioFiscalReceptor);
		}

		#region Regimen Fiscal Receptor

		public void TestReceptorBuilder_RegimenFiscal_ValidREGRegistrationCode()
		{
			var builder = (ICFDiReceptorBuilder)new CFDiReceptorBuilder();

			foreach (var taxRegime in Enum.GetValues(typeof(c_RegimenFiscal)))
			{
				var regNumberValidValue = taxRegime.ToString().Replace("Item", "");
				var transaction = baseTransationInfoWithOrganizationAddress_ForTestOnly(OrgCusCodes.REG, regNumberValidValue);
				var receptor = builder.BuildReceptorInfo(transaction);
				AssertEquals(nameof(receptor.RegimenFiscalReceptor), taxRegime, receptor.RegimenFiscalReceptor);
			}
		}

		public void TestReceptorBuilder_RegimenFiscal_InvalidREGRegistrationCode()
		{
			var builder = (ICFDiReceptorBuilder)new CFDiReceptorBuilder();

			var regNumberInValidValues = new string[] { null, string.Empty, "123", "abc", "@A-X", "Item999" };
			foreach (var regNumber in regNumberInValidValues)
			{
				var transaction = baseTransationInfoWithOrganizationAddress_ForTestOnly(OrgCusCodes.REG, regNumber);
				var receptor = builder.BuildReceptorInfo(transaction);
				AssertEquals(nameof(receptor.RegimenFiscalReceptor), c_RegimenFiscal.Item601, receptor.RegimenFiscalReceptor);
			}
		}

		public void TestReceptorBuilder_RegimenFiscal_MissingREGRegistrationCode()
		{
			var builder = (ICFDiReceptorBuilder)new CFDiReceptorBuilder();
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var receptor = builder.BuildReceptorInfo(transaction);
			AssertEquals(nameof(receptor.RegimenFiscalReceptor), c_RegimenFiscal.Item601, receptor.RegimenFiscalReceptor);
		}

		#endregion

		#region Implementation

		TransactionInfo baseTransationInfoWithOrganizationAddress_ForTestOnly(string expectedRegistrationNumberType, string expectedCode = null, string expectedCountryOfIssueCode = "MX")
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.CompanyName = "TERNIUM MEXICO SA DE CV";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
			  {
				new RegistrationNumber()
				{
				  CountryOfIssue = new Country { Code = expectedCountryOfIssueCode },
				  Type = new RegistrationNumberType() { Code = expectedRegistrationNumberType },
				  Value = expectedCode
				}
			  });

			return transaction;
		}

		#endregion
	}
}
