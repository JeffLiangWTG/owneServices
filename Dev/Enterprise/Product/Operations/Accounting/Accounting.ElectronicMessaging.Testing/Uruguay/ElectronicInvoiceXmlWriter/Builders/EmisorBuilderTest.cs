using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.UruguayOrgCusCodeInfo;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay.Testing
{
	class EmisorBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<EmisorBuilder>(new EncabezadoBuilder().EmisorBuilder_ExposedForTestOnly);
		}

		public void TestEmisorBuilder_TransactionInfoHelperCreatorType()
		{
			var adjustPostedInvoiceHelper = new EmisorBuilder();
			AssertType<TransactionInfoHelper>(adjustPostedInvoiceHelper.TransactionInfoHelper_ExposedForTestOnly);
		}

		[ExpectNoExceptions]
		public void TestEmisorBuilder_AddCdgDGISucur()
		{
			var creator = new EmisorBuilder();

			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
			creator.SubstituteTransactionInfoHelper_ForTestOnly(transactionInfoHelperMock.Object);

			var emisor = (creator as IEmisorBuilder).BuildEmisorInfo(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { BranchAddress = new OrganizationAddress() }, Factory);
			transactionInfoHelperMock.Verify(x => x.GetRegistrationCode(It.IsAny<OrganizationAddress>(), It.IsAny<ZString>(), It.IsAny<ZString>()), Times.AtLeastOnce);
		}

		public void TestBuildEmisorCdgDGISucurValid()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
			{
				new RegistrationNumber()
				{
					CountryOfIssue = new Country { Code = "UY" },
					Type = new RegistrationNumberType() { Code = "BRC" },
					Value = "0123"
				}
			});

			var builder = new EmisorBuilder() as IEmisorBuilder;
			var emisor = builder.BuildEmisorInfo(transaction, Factory);
			AssertEquals(emisor.CdgDGISucur, "0123");
		}

		public void TestBuildEmisorAddressValuesValid()
		{
			var address = GetOrganizationAddress_ForTestOnly();

			address.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
			{
				new RegistrationNumber()
				{
					CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.Uruguay },
					Type =  new RegistrationNumberType() { Code = OrgCusCodes.RUT },
					Value = "999999999999"
				}
			});

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BranchAddress = address
			};

			var builder = new EmisorBuilder();

			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
			builder.SubstituteTransactionInfoHelper_ForTestOnly(transactionInfoHelperMock.Object);

			transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(It.IsAny<OrganizationAddress>(), It.IsAny<ZString>(), It.IsAny<ZString>())).Returns("999999999999");

			var emisor = (builder as IEmisorBuilder).BuildEmisorInfo(transaction, Factory);

			AssertEquals(nameof(Emisor.RUCEmisor), "999999999999", emisor.RUCEmisor);
			AssertEquals(nameof(Emisor.RznSoc), "URUGUAYAN COMPANY S.A.", emisor.RznSoc);
			AssertEquals(nameof(Emisor.DomFiscal), "FERNANDEZ 1148", emisor.DomFiscal);
			AssertEquals(nameof(Emisor.Ciudad), "JOSE IGNACIO", emisor.Ciudad);
		}

		public void TestBuildEmisorInfo_NullChecks()
		{
			var builder = new EmisorBuilder() as IEmisorBuilder;

			TransactionInfo transaction = null;
			AssertBuildEmisorInfo_NullChecks();

			transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			AssertBuildEmisorInfo_NullChecks();

			transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { BranchAddress = new OrganizationAddress() };
			AssertBuildEmisorInfo_NullChecks();

			void AssertBuildEmisorInfo_NullChecks()
			{
				var emisor = builder.BuildEmisorInfo(transaction, Factory);

				AssertNullOrEmpty(nameof(Emisor.RUCEmisor), emisor.RUCEmisor);
				AssertNullOrEmpty(nameof(Emisor.RznSoc), emisor.RznSoc);
				AssertNullOrEmpty(nameof(Emisor.DomFiscal), emisor.DomFiscal);
				AssertNullOrEmpty(nameof(Emisor.Ciudad), emisor.Ciudad);
				AssertNullOrEmpty(nameof(Emisor.Departamento), emisor.Departamento);
			}
		}

		public void TestBuildEmisorInfoAdicional_DescriptionIsValid()
		{
			var emisor = CreateEmisorBuilderWithRegistrationCode("Test Code Description");

			AssertEquals(nameof(Emisor.InfoAdicionalEmisor), "{ Test Code Description\n Contribuyente Amparado a la Ley N° 15.921 }", emisor.InfoAdicionalEmisor);
		}

		public void TestBuildEmisorInfoAdicional_DescriptionIsEmpty()
		{
			var emisor = CreateEmisorBuilderWithRegistrationCode("");

			AssertEquals(nameof(Emisor.InfoAdicionalEmisor), null, emisor.InfoAdicionalEmisor);
		}

		public void TestBuildEmisorInfoAdicional_DescriptionIsNull()
		{
			var emisor = CreateEmisorBuilderWithRegistrationCode(null);

			AssertEquals(nameof(Emisor.InfoAdicionalEmisor), null, emisor.InfoAdicionalEmisor);
		}

		public void TestBuildEmisorInfoAdicional_TransactionIsNull()
		{
			var emisorBuilder = new EmisorBuilder();
			var emisor = (emisorBuilder as IEmisorBuilder).BuildEmisorInfo(null, Factory);
			AssertEquals(nameof(Emisor.InfoAdicionalEmisor), null, emisor.InfoAdicionalEmisor);
		}

		public void TestBuildEmisorInfoAdicional_OrganizationAddressIsNull()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { BranchAddress = null };
			var emisorBuilder = new EmisorBuilder();
			var emisor = (emisorBuilder as IEmisorBuilder).BuildEmisorInfo(transactionInfo, Factory);
			AssertEquals(nameof(Emisor.InfoAdicionalEmisor), null, emisor.InfoAdicionalEmisor);
		}

		[ExpectNoExceptions]
		public void TestBuildEmisorInfoAdicional_GetRegistrationCodeParameters()
		{
			var organizationAddress = new OrganizationAddress();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { BranchAddress = organizationAddress };

			var emisorBuilder = new EmisorBuilder();
			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
			emisorBuilder.SubstituteTransactionInfoHelper_ForTestOnly(transactionInfoHelperMock.Object);

			var emisor = (emisorBuilder as IEmisorBuilder).BuildEmisorInfo(transactionInfo, Factory);

			transactionInfoHelperMock.Verify(x => x.GetRegistrationCode(organizationAddress, CountryCodes.Uruguay, UruguayOrgCusCodeInfo.OrgCusCodes.FZU));
		}

		public void TestBuildEmisorAddressValidLenght()
		{
			var creator = new EmisorBuilder();
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BranchAddress = GetOrganizationAddress_ForTestOnly()
			};

			transaction.BranchAddress.Address1 = "AVENIDA CRUZ ALTA 2592ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss";
			transaction.BranchAddress.City = "PUNTA DEL ESTEaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

			var emisor = (creator as IEmisorBuilder).BuildEmisorInfo(transaction, Factory);

			Assert("It must be less than or equal to 70", emisor.DomFiscal.Length <= 70);
			Assert("It must be less than or equal to 30", emisor.Ciudad.Length <= 30);
		}

		public void TestBuildEmisorInfo_BadRegistrationNumber()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BranchAddress = GetOrganizationAddress_ForTestOnly()
			};

			transaction.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
			{
				new RegistrationNumber()
				{
					CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.Argentina },
					Type =  new RegistrationNumberType() { Code = OrgCusCodes.RUT },
					Value = "999999999999"
				}
			});

			var builder = new EmisorBuilder() as IEmisorBuilder;
			var emisor = builder.BuildEmisorInfo(transaction, Factory);
			AssertNullOrEmpty(nameof(Emisor.RUCEmisor), emisor.RUCEmisor);

			transaction.BranchAddress.RegistrationNumberCollection.Clear();
			transaction.BranchAddress.RegistrationNumberCollection.Add(new RegistrationNumber()
			{
				CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.Uruguay },
				Type = new RegistrationNumberType() { Code = OrgCusCodes.CID },
				Value = "999999999999"
			});

			emisor = builder.BuildEmisorInfo(transaction, Factory);
			AssertNullOrEmpty(nameof(Emisor.RUCEmisor), emisor.RUCEmisor);
		}

		public void TestBuildEmisorInfo_StateDescriptionByCountry()
		{
			var creator = new EmisorBuilder();
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BranchAddress = GetOrganizationAddress_ForTestOnly()
			};
			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
			transactionInfoHelperMock.Setup(x => x.GetStateDescriptionByCountry("UY", "MO", Factory)).Returns("Montevideo");
			creator.SubstituteTransactionInfoHelper_ForTestOnly(transactionInfoHelperMock.Object);

			var emisor = (creator as IEmisorBuilder).BuildEmisorInfo(transaction, Factory);
			transactionInfoHelperMock.Verify(x => x.GetStateDescriptionByCountry(transaction.BranchAddress.Country.Code, transaction.BranchAddress.State, Factory), Times.Once);
			AssertEquals(emisor.Departamento, "Montevideo");
		}

		public void TestBuildEmisorInfo_StateDescriptionByCountry_HasValidLength()
		{
			var creator = new EmisorBuilder();
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BranchAddress = GetOrganizationAddress_ForTestOnly()
			};

			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
			transactionInfoHelperMock.Setup(x => x.GetStateDescriptionByCountry("UY", "MO", Factory)).Returns("CIUDAD_DE_MONTEVIDEO_CIUDAD_DE_MONTEVIDEO_44");
			creator.SubstituteTransactionInfoHelper_ForTestOnly(transactionInfoHelperMock.Object);

			var emisor = (creator as IEmisorBuilder).BuildEmisorInfo(transaction, Factory);
			Assert(nameof(Emisor.Departamento) + " must be less than or equal to 30", emisor.Departamento.Length <= 30);
			AssertEquals(nameof(Emisor.Departamento) + " content must have exacly 30 characters.", "CIUDAD_DE_MONTEVIDEO_CIUDAD_DE", emisor.Departamento);
		}

		public void TestBuildEmisorInfo_StateDescriptionByCountry_HasCorrectParameters()
		{
			var returnCorrectValue = "XXXDepartamentoXXX";
			var inputContent = new string[] { "XX", "YY" };

			var mockObject = new Mock<ITransactionInfoHelper>();
			mockObject
				.Setup(x => x.GetStateDescriptionByCountry(inputContent[0], inputContent[1], It.IsAny<BusinessObjectFactory>()))
				.Callback<string, string, BusinessObjectFactory>((t1, t2, t3) =>
				{
					inputContent[0] = t1;
					inputContent[1] = t2;
				})
				.Returns(returnCorrectValue);

			var creator = new EmisorBuilder();
			creator.SubstituteTransactionInfoHelper_ForTestOnly(mockObject.Object);

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			};

			var emisor = (creator as IEmisorBuilder).BuildEmisorInfo(transaction, Factory);
			AssertEquals(nameof(Emisor.Departamento) + " should be null'.", null, emisor.Departamento);

			transaction.BranchAddress.Country = null;
			transaction.BranchAddress.State = null;
			emisor = (creator as IEmisorBuilder).BuildEmisorInfo(transaction, Factory);
			AssertEquals(nameof(Emisor.Departamento) + " should be null'.", null, emisor.Departamento);

			transaction.BranchAddress.Country = new Country() { Code = null };

			AssertBranchAddressCorrectParams(null, null, null);
			AssertBranchAddressCorrectParams("", null, null);
			AssertBranchAddressCorrectParams("UY", null, null);
			AssertBranchAddressCorrectParams("UY", "", null);
			AssertBranchAddressCorrectParams("AU", "ACT", null);
			AssertBranchAddressCorrectParams(inputContent[0], "MO", null);

			AssertBranchAddressCorrectParams(inputContent[0], inputContent[1], returnCorrectValue);

			void AssertBranchAddressCorrectParams(string countryCode, string stateCode, string expectedValue)
			{
				transaction.BranchAddress.Country.Code = countryCode;
				transaction.BranchAddress.State = stateCode;
				emisor = (creator as IEmisorBuilder).BuildEmisorInfo(transaction, Factory);
				AssertEquals(nameof(Emisor.Departamento), expectedValue, emisor.Departamento);
			}
		}

		#region Implementations

		public OrganizationAddress GetOrganizationAddress_ForTestOnly()
		{
			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Country = new Country() { Code = Core.Constants.CountryCodes.Uruguay },
				CompanyName = "URUGUAYAN COMPANY S.A.",
				Address1 = "FERNANDEZ 1148",
				City = "JOSE IGNACIO",
				State = "MO"
			};

			return address;
		}

		#endregion Implementations

		Emisor CreateEmisorBuilderWithRegistrationCode(string description)
		{
			var emisorBuilder = new EmisorBuilder();

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { BranchAddress = new OrganizationAddress() };
			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
			emisorBuilder.SubstituteTransactionInfoHelper_ForTestOnly(transactionInfoHelperMock.Object);

			transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(It.IsAny<OrganizationAddress>(), CountryCodes.Uruguay, UruguayOrgCusCodeInfo.OrgCusCodes.FZU)).Returns(description);

			var emisor = (emisorBuilder as IEmisorBuilder).BuildEmisorInfo(transactionInfo, Factory);

			return emisor;
		}
	}
}
