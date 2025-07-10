using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	public class CFDiEmisorBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<CFDiEmisorBuilder>(new CFDiComprobanteBuilder().CFDiEmisorBuilder_ExposedForTestOnly);
		}

		public void TestReceptorWithRFC()
		{
			var address1 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			address1.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
					{
						new RegistrationNumber
						{
							CountryOfIssue = new Country { Code = "MX" },
							Type =  new RegistrationNumberType { Code = "RFC" },
							Value = "TES030201001"
						}
					});
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BranchAddress = address1
			};

			var builder = (ICFDiEmisorBuilder)new CFDiEmisorBuilder();
			var comprobanteEmisor = builder.BuildEmisorInfo(transaction);

			AssertEquals("TES030201001", comprobanteEmisor.Rfc);
		}

		public void TestReceptorWithNotInformedRFC()
		{
			var address1 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			address1.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
					{
						new RegistrationNumber
						{
							CountryOfIssue = new Country { Code = "MX" },
							Type =  new RegistrationNumberType { Code = "RFC" },
						}
					});
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BranchAddress = address1
			};

			var builder = (ICFDiEmisorBuilder)new CFDiEmisorBuilder();
			var comprobanteEmisor = builder.BuildEmisorInfo(transaction);

			AssertNullOrEmpty("RFC should not be empty for Mexico.", comprobanteEmisor.Rfc);
		}
		public void TestReceptorWiNulltBranchAddress()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var builder = (ICFDiEmisorBuilder)new CFDiEmisorBuilder();
			var comprobanteEmisor = builder.BuildEmisorInfo(transaction);

			AssertNullOrEmpty("Organization Address should not be null or empty.", comprobanteEmisor.Rfc);
		}
		public void TestReceptorWithNullRegistrationNumberCollection()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			};

			var builder = (ICFDiEmisorBuilder)new CFDiEmisorBuilder();
			var comprobanteEmisor = builder.BuildEmisorInfo(transaction);

			AssertNullOrEmpty("Organization Address should not be empty null or empty.", comprobanteEmisor.Rfc);
		}
		public void TestReceptorWithNotMexicoCountry()
		{
			var address1 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			address1.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
					{
						new RegistrationNumber
						{
							CountryOfIssue = new Country { Code = "AR" },
							Type =  new RegistrationNumberType { Code = "RFC" },
							Value = "TES030201001"
						}
					});
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BranchAddress = address1
			};

			var builder = (ICFDiEmisorBuilder)new CFDiEmisorBuilder();
			var comprobanteEmisor = builder.BuildEmisorInfo(transaction);

			AssertNullOrEmpty("RFC should be null because RFC Code does not exists for Argentina.", comprobanteEmisor.Rfc);
		}
		public void TestReceptorWithNotInformedCountry()
		{
			var address1 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			address1.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
					{
						new RegistrationNumber
						{
							CountryOfIssue = new Country(),
							Type =  new RegistrationNumberType { Code = "RFC" },
							Value = "TES030201001"
						}
					});
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BranchAddress = address1
			};

			var builder = (ICFDiEmisorBuilder)new CFDiEmisorBuilder();
			var comprobanteEmisor = builder.BuildEmisorInfo(transaction);

			AssertNullOrEmpty("RFC should be null because only is valid for Mexico.", comprobanteEmisor.Rfc);
		}
		public void TestReceptorWithWrongRfcCodeType()
		{
			var address1 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			address1.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
					{
						new RegistrationNumber
						{
							CountryOfIssue = new Country { Code = "MX" },
							Type =  new RegistrationNumberType { Code = "RUC" },
							Value = "TES030201001"
						}
					});
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BranchAddress = address1
			};

			var builder = (ICFDiEmisorBuilder)new CFDiEmisorBuilder();
			var comprobanteEmisor = builder.BuildEmisorInfo(transaction);

			AssertNullOrEmpty("RFC should be null because RFC Type is valid only for Mexico.", comprobanteEmisor.Rfc);
		}
		public void TestReceptorWithNoRfcCodeType()
		{
			var address1 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			address1.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
					{
						new RegistrationNumber
						{
							CountryOfIssue = new Country { Code = "MX" },
							Type =  new RegistrationNumberType(),
							Value = "TES030201001"
						}
					});
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BranchAddress = address1
			};

			var builder = (ICFDiEmisorBuilder)new CFDiEmisorBuilder();
			var comprobanteEmisor = builder.BuildEmisorInfo(transaction);

			AssertNullOrEmpty("RFC should be null because it´s needed a RFC Type for Mexico.", comprobanteEmisor.Rfc);
		}

		public void TestBuildEmisorInfo_GettingRegimentFiscalFromRegistry()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BranchAddress = new OrganizationAddress { Country = new Country { Code = CountryCodes.Mexico } }
			};

			var builder = (ICFDiEmisorBuilder)new CFDiEmisorBuilder();
			var comprobanteEmisor = builder.BuildEmisorInfo(transaction);
			AssertEquals(nameof(ComprobanteEmisor.RegimenFiscal), c_RegimenFiscal.Item601, comprobanteEmisor.RegimenFiscal);

			var mexicoTaxRegimeIdTypesList = ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>().GetFeatureInterface<IDebtorTaxRegime>(CountryCodes.Mexico).GetTaxRegimeIdTypes();
			var registryValues = mexicoTaxRegimeIdTypesList.CodesAsString.Split(',').Select(s => "Item" + s.Trim()).ToArray();
			var govAllowedEnumValues = Enum.GetValues(typeof(c_RegimenFiscal)).OfType<object>().Select(o => o.ToString()).ToArray();

			Comparison<string> comparision = new Comparison<string>((x, y) => x.CompareTo(y));
			Array.Sort(registryValues, comparision);
			Array.Sort(govAllowedEnumValues, comparision);

			AssertEquals("Number of elements in MexicoTaxRegimeIdTypes is the same as in c_RegimenFiscal enum", registryValues.Length, govAllowedEnumValues.Length);

			foreach (var taxRegimeId in registryValues.Select((value, idx) => new { idx, value }))
			{
				AssertEquals("Should have the same value.", taxRegimeId.value, govAllowedEnumValues[taxRegimeId.idx]);
				AssertEquals("Should have the same index.", taxRegimeId.idx, Array.FindIndex(govAllowedEnumValues, item => item == taxRegimeId.value));
			}

			AccountingElectronicMessagingRegistry.Instance.MexicoTaxRegimeID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "624");
			var emisorCoordinados = builder.BuildEmisorInfo(transaction);
			AssertEquals(nameof(ComprobanteEmisor.RegimenFiscal), c_RegimenFiscal.Item624, emisorCoordinados.RegimenFiscal);
		}

		public void TestEmisorBuilder_TransactionInfoHelperType()
		{
			var adjustPostedInvoiceHelper = new CFDiEmisorBuilder();
			AssertType<TransactionInfoHelper>(adjustPostedInvoiceHelper.TransactionInfoHelper_ExposedForTestOnly);
		}

		[ExpectNoExceptions]
		public void TestEmisorBuilder_GetRegistrationCode()
		{
			var expectedCode = "TES030201001";
			var transaction = baseTransationInfoWithBranchAdress_ForTestOnly("RFC", expectedCode);

			var creator = new CFDiEmisorBuilder();
			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
			creator.SubstituteTransactionInfoHelper_ForTestOnly(transactionInfoHelperMock.Object);

			var emisor = (creator as ICFDiEmisorBuilder).BuildEmisorInfo(transaction);
			transactionInfoHelperMock.Verify(x => x.GetRegistrationCode(transaction.BranchAddress, "MX", "RFC"), Times.Once);
		}

		public void TestEmisorBuilder_CompanyName()
		{
			var builder = (ICFDiEmisorBuilder)new CFDiEmisorBuilder();

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var comprobanteEmisor = builder.BuildEmisorInfo(transaction);
			AssertNull(comprobanteEmisor.Nombre);

			transaction.BranchAddress = new OrganizationAddress { CompanyName = "YOUR MEXICO CORP" };
			comprobanteEmisor = builder.BuildEmisorInfo(transaction);
			AssertEquals(nameof(ComprobanteEmisor.Nombre), "YOUR MEXICO CORP", comprobanteEmisor.Nombre);
		}

		public void TestEmisorBuilder_CompanyName_CallTransactionInfoHelper()
		{
			var creator = new CFDiEmisorBuilder();
			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
			creator.SubstituteTransactionInfoHelper_ForTestOnly(transactionInfoHelperMock.Object);

			var originalCompanyName = "MEXICO CORP NAME";
			var alternativeCompanyName = "MEXICO CORP ALTERNATIVE NAME";

			var transaction = baseTransationInfoWithBranchAdress_ForTestOnly("ELN", alternativeCompanyName);
			transaction.BranchAddress.CompanyName = originalCompanyName;

			transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(transaction.BranchAddress, "MX", "ELN")).Returns(alternativeCompanyName);
			var emisor = (creator as ICFDiEmisorBuilder).BuildEmisorInfo(transaction);

			AssertEquals(nameof(ComprobanteEmisor.Nombre), alternativeCompanyName, emisor.Nombre);
		}

		#region Implementation

		TransactionInfo baseTransationInfoWithBranchAdress_ForTestOnly(string expectedRegistrationNumberType, string expectedCode = null, string expectedCountryOfIssueCode = "MX")
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
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
