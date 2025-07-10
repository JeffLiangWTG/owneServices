using System.Collections.Generic;
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
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay.Testing
{
	class ReceptorBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<ReceptorBuilder>(new EncabezadoBuilder().ReceptorBuilder_ExposedForTestOnly);
		}

		public void TestBuildReceptorInfo_eFactura()
		{
			var builder = new ReceptorBuilder() as IReceptorBuilder;
			var transaction = CreateTransactionInfo(CountryCodes.Uruguay, new List<RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumber(UruguayOrgCusCodeInfo.OrgCusCodes.RUT, "123456789", CountryCodes.Uruguay));
			AssertBuildReceptorInfo_eFactura(ZString.Empty, ZString.Empty, ZString.Empty);

			var companyName = "SOFTCARGO";
			var city = "MONTEVIDEO";
			var address = "MONTANA 1148";
			transaction.OrganizationAddress.CompanyName = companyName;
			transaction.OrganizationAddress.City = city;
			transaction.OrganizationAddress.Address1 = address;
			AssertBuildReceptorInfo_eFactura(companyName, city, address);

			void AssertBuildReceptorInfo_eFactura(string rznSocRecep, string ciudadRecep, string dirRecep)
			{
				var receptor = builder.BuildEFacReceptor(transaction);

				AssertEquals(nameof(Receptor_Fact.TipoDocRecep), DocType.Item2, receptor.TipoDocRecep);
				AssertEquals(nameof(Receptor_Fact.RznSocRecep), rznSocRecep, receptor.RznSocRecep);
				AssertEquals(nameof(Receptor_Fact.CiudadRecep), ciudadRecep, receptor.CiudadRecep);
				AssertEquals(nameof(Receptor_Fact.DirRecep), dirRecep, receptor.DirRecep);
			}
		}

		public void TestBuildReceptorInfo_eTicket()
		{
			var companyName = "SOFTCARGO";
			var city = "MONTEVIDEO";
			var address = "MONTANA 1148";

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrganizationAddress = new OrganizationAddress()
				{
					CompanyName = companyName,
					City = city,
					Address1 = address
				}
			};

			var builder = new ReceptorBuilder() as IReceptorBuilder;
			var receptor = builder.BuildETicketReceptor(transaction);
			AssertEquals(nameof(Receptor_Tck.TipoDocRecep), DocType.Item1, receptor.TipoDocRecep);
			AssertEquals(nameof(Receptor_Tck.RznSocRecep), companyName, receptor.RznSocRecep);
			AssertEquals(nameof(Receptor_Tck.CiudadRecep), city, receptor.CiudadRecep);
			AssertEquals(nameof(Receptor_Tck.DirRecep), address, receptor.DirRecep);
		}

		public void TestBuildReceptorInfo_NullChecks()
		{
			var builder = new ReceptorBuilder() as IReceptorBuilder;

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			AssertBuildReceptorInfo_NullChecks_eFactura();
			AssertBuildReceptorInfo_NullChecks_eTicket();

			transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { OrganizationAddress = new OrganizationAddress() };
			AssertBuildReceptorInfo_NullChecks_eFactura();
			AssertBuildReceptorInfo_NullChecks_eTicket();

			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			AssertBuildReceptorInfo_NullChecks_eFactura();
			AssertBuildReceptorInfo_NullChecks_eTicket();

			void AssertBuildReceptorInfo_NullChecks_eFactura()
			{
				var receptor = builder.BuildEFacReceptor(transaction);
				AssertNull(receptor);
			}

			void AssertBuildReceptorInfo_NullChecks_eTicket()
			{
				var receptor = builder.BuildETicketReceptor(transaction);

				AssertEquals(nameof(Receptor_Tck.TipoDocRecep), DocType.Item1, receptor.TipoDocRecep);
				AssertEquals(nameof(Receptor_Tck.RznSocRecep), ZString.Empty, receptor.RznSocRecep);
				AssertEquals(nameof(Receptor_Tck.CiudadRecep), ZString.Empty, receptor.CiudadRecep);
				AssertEquals(nameof(Receptor_Tck.DirRecep), ZString.Empty, receptor.DirRecep);

				AssertEquals(nameof(Receptor_Tck.ItemElementName), ItemChoiceType.DocRecep, receptor.ItemElementName);
				AssertEquals(nameof(Receptor_Tck.TipoDocRecepSpecified), false, receptor.TipoDocRecepSpecified);
				AssertNullOrEmpty(nameof(Receptor_Tck.Item), receptor.Item);
				AssertEquals(nameof(Receptor_Tck.CodPaisRecep), CodPaisType.AD, receptor.CodPaisRecep);
				AssertEquals(nameof(Receptor_Tck.CodPaisRecepSpecified), false, receptor.CodPaisRecepSpecified);
			}
		}

		public void TestBuildReceptor_InfoAdicional_ValidCodeWithDescription()
		{
			var receptorFacTicket = CreateEFacReceptorBuilderWithRegistrationCode("Test Code Description");
			var receptorETicket = CreateETicketReceptorBuilderWithRegistrationCode("Test Code Description");

			AssertEquals(nameof(Receptor_Fact.InfoAdicional), "{ Test Code Description }", receptorFacTicket.InfoAdicional);
			AssertEquals(nameof(Receptor_Tck.InfoAdicional), "{ Test Code Description }", receptorETicket.InfoAdicional);
		}

		public void TestBuildReceptor_InfoAdicional_ValidCodeWithEmptyDescription()
		{
			var receptorFacTicket = CreateEFacReceptorBuilderWithRegistrationCode("");
			var receptorETicket = CreateETicketReceptorBuilderWithRegistrationCode("");

			AssertEquals(nameof(Receptor_Fact.InfoAdicional), null, receptorFacTicket.InfoAdicional);
			AssertEquals(nameof(Receptor_Tck.InfoAdicional), null, receptorETicket.InfoAdicional);
		}

		public void TestBuildReceptor_InfoAdicional_NullDescription()
		{
			var receptorFacTicket = CreateEFacReceptorBuilderWithRegistrationCode(null);
			var receptorETicket = CreateETicketReceptorBuilderWithRegistrationCode(null);

			AssertEquals(nameof(Receptor_Fact.InfoAdicional), null, receptorFacTicket.InfoAdicional);
			AssertEquals(nameof(Receptor_Tck.InfoAdicional), null, receptorETicket.InfoAdicional);
		}

		[ExpectNoExceptions]
		public void TestBuildReceptor_InfoAdicional_ETicket_GetRegistrationCodeParameters()
		{
			AssertBuildReceptorInfoAdicional_GetRegistrationCodeParameters(null, null);
			AssertBuildReceptorInfoAdicional_GetRegistrationCodeParameters(new TransactionInfo(), null);

			var organizationAddress = new OrganizationAddress();
			var transactionInfo = new TransactionInfo() { OrganizationAddress = organizationAddress };

			AssertBuildReceptorInfoAdicional_GetRegistrationCodeParameters(transactionInfo, organizationAddress);

			void AssertBuildReceptorInfoAdicional_GetRegistrationCodeParameters(TransactionInfo transactionInfo, OrganizationAddress expectedorganizationAddress)
			{
				var receptorBuilder = new ReceptorBuilder();

				var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
				receptorBuilder.SubstituteTransactionInfoHelper_ForTestOnly(transactionInfoHelperMock.Object);

				var recreceptorETicketeptor = (receptorBuilder as IReceptorBuilder).BuildETicketReceptor(transactionInfo);
				transactionInfoHelperMock.Verify(x => x.GetRegistrationCode(expectedorganizationAddress, CountryCodes.Uruguay, UruguayOrgCusCodeInfo.OrgCusCodes.FZU));
			}
		}

		[ExpectNoExceptions]
		public void TestBuildReceptor_InfoAdicional_EFactura_GetRegistrationCodeParameters()
		{
			var receptorBuilder = new ReceptorBuilder();
			var transactionInfo = CreateTransactionInfo(CountryCodes.Uruguay, null);
			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();

			receptorBuilder.SubstituteTransactionInfoHelper_ForTestOnly(transactionInfoHelperMock.Object);
			transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(It.IsAny<OrganizationAddress>(), CountryCodes.Uruguay, UruguayOrgCusCodeInfo.OrgCusCodes.RUT)).Returns("123456789");
			transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(It.IsAny<OrganizationAddress>(), CountryCodes.Uruguay, UruguayOrgCusCodeInfo.OrgCusCodes.FZU)).Returns("");

			var receptorFacTicket = (receptorBuilder as IReceptorBuilder).BuildEFacReceptor(transactionInfo);

			transactionInfoHelperMock.Verify(x => x.GetRegistrationCode(transactionInfo.OrganizationAddress, CountryCodes.Uruguay, UruguayOrgCusCodeInfo.OrgCusCodes.FZU));
		}

		public void TestBuildReceptorInfo_Lenghts()
		{
			var transaction = CreateTransactionInfo(CountryCodes.Uruguay, new List<RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumber(UruguayOrgCusCodeInfo.OrgCusCodes.RUT, "123456789", CountryCodes.Uruguay));
			transaction.OrganizationAddress.CompanyName = "SOFTCARGO INTERNATIONAL SOCIEDAD ANONIMA, NOW IS WISETECH GLOBAL INTERNATIONAL. This is a company name with most of 150 characters then i needed add more words.";
			transaction.OrganizationAddress.City = "CIUDAD DE TREINTA Y TRES EN URUGUAY";
			transaction.OrganizationAddress.Address1 = "AVENIDA CRUZ ALTA 2592 BETWEEN CONCEPCION DEL URUGUAY AND BULEVAR GENERAL ARTIGAS";

			var builder = new ReceptorBuilder() as IReceptorBuilder;
			var receptor = builder.BuildEFacReceptor(transaction);

			CombineAssertions(() =>
			{
				AssertEquals("It must be equal to 150", "SOFTCARGO INTERNATIONAL SOCIEDAD ANONIMA, NOW IS WISETECH GLOBAL INTERNATIONAL. This is a company name with most of 150 characters then i needed add m", receptor.RznSocRecep);
				AssertEquals("It must be equal to 30", "CIUDAD DE TREINTA Y TRES EN UR", receptor.CiudadRecep);
				AssertEquals("It must be equal to 70", "AVENIDA CRUZ ALTA 2592 BETWEEN CONCEPCION DEL URUGUAY AND BULEVAR GENE", receptor.DirRecep);
			});
		}

		public void TestBuildReceptor_Country_eFactura()
		{
			var builder = (IReceptorBuilder)new ReceptorBuilder();
			var transaction = CreateTransactionInfo(CountryCodes.Argentina, new List<RegistrationNumber>());
			AssertBuildReceptor_Country_BadSettings();

			transaction.OrganizationAddress.Country.Code = CountryCodes.Uruguay;
			AssertBuildReceptor_Country_BadSettings();

			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumber(UruguayOrgCusCodeInfo.OrgCusCodes.CID, "123456789", CountryCodes.Uruguay));
			AssertBuildReceptor_Country_BadSettings();

			transaction.OrganizationAddress.RegistrationNumberCollection[0].Type.Code = UruguayOrgCusCodeInfo.OrgCusCodes.RUT;
			var receptor = builder.BuildEFacReceptor(transaction);
			AssertEquals(nameof(receptor.CodPaisRecep), CodPaisType.UY, receptor.CodPaisRecep);

			void AssertBuildReceptor_Country_BadSettings()
			{
				receptor = builder.BuildEFacReceptor(transaction);
				AssertNull(receptor);
			}
		}

		public void TestBuildReceptor_Country_eTicket()
		{
			var builder = (IReceptorBuilder)new ReceptorBuilder();
			var transaction = CreateTransactionInfo(CountryCodes.Uruguay, new List<RegistrationNumber>());
			AssertBuildReceptor_Country(CodPaisType.UY);

			transaction.OrganizationAddress.Country.Code = CountryCodes.Argentina;
			AssertBuildReceptor_Country(CodPaisType.AR);

			var specialCountries = new List<string>()
			{
				CountryCodes.NetherlandsAntilles,
				CountryCodes.Kosovo,
				"CS",
				"XZ"
			};

			foreach (var code in specialCountries)
			{
				transaction.OrganizationAddress.Country.Code = code;
				AssertBuildReceptor_Country(CodPaisType.Item99);
			}

			void AssertBuildReceptor_Country(CodPaisType country)
			{
				var receptorETicket = builder.BuildETicketReceptor(transaction);
				AssertEquals(nameof(receptorETicket.CodPaisRecep), country, receptorETicket.CodPaisRecep);
				AssertEquals(nameof(receptorETicket.CodPaisRecepSpecified), true, receptorETicket.CodPaisRecepSpecified);
			}
		}

		public void TestBuildReceptor_ETicket_TipoDocRecep()
		{
			var builder = (IReceptorBuilder)new ReceptorBuilder();
			var transaction = CreateTransactionInfo(CountryCodes.Uruguay, new List<RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumber(OrgCusCode.CodeTypes.MedicareID, "40012345678", CountryCodes.Uruguay));
			var receptorETicket = builder.BuildETicketReceptor(transaction);
			AssertEquals(nameof(receptorETicket.TipoDocRecep), DocType.Item4, receptorETicket.TipoDocRecep);

			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumber(OrgCusCode.CodeTypes.PassportID, "ABC12345678", CountryCodes.Uruguay));
			receptorETicket = builder.BuildETicketReceptor(transaction);
			AssertEquals(nameof(receptorETicket.TipoDocRecep), DocType.Item5, receptorETicket.TipoDocRecep);

			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumber(UruguayOrgCusCodeInfo.OrgCusCodes.CID, "9.999.999-9", CountryCodes.Uruguay));
			receptorETicket = builder.BuildETicketReceptor(transaction);
			AssertEquals(nameof(receptorETicket.TipoDocRecep), DocType.Item3, receptorETicket.TipoDocRecep);
		}

		public void TestBuildReceptor_ETicket_ItemElementName()
		{
			var builder = (IReceptorBuilder)new ReceptorBuilder();
			var transaction = CreateTransactionInfo(null, null);

			transaction.OrganizationAddress.Country = new Country();
			AssertBuildReceptor_ItemElementName_DocRecep_ETicket();
			transaction.OrganizationAddress.Country.Code = ZString.Empty;
			AssertBuildReceptor_ItemElementName_DocRecep_ETicket();
			transaction.OrganizationAddress.Country.Code = CountryCodes.Uruguay;
			AssertBuildReceptor_ItemElementName_DocRecep_ETicket();
			transaction.OrganizationAddress.Country.Code = CountryCodes.Argentina;
			AssertBuildReceptor_ItemElementName_DocRecepExt_ETicket();

			void AssertBuildReceptor_ItemElementName_DocRecep_ETicket()
			{
				var receptorETicket = builder.BuildETicketReceptor(transaction);
				AssertEquals(nameof(receptorETicket.ItemElementName), ItemChoiceType.DocRecep, receptorETicket.ItemElementName);
			}
			void AssertBuildReceptor_ItemElementName_DocRecepExt_ETicket()
			{
				var receptorETicket = builder.BuildETicketReceptor(transaction);
				AssertEquals(nameof(receptorETicket.ItemElementName), ItemChoiceType.DocRecepExt, receptorETicket.ItemElementName);
			}
		}

		public void TestBuildReceptor_EFactura_TipoDocRecep_DocRecep()
		{
			var builder = (IReceptorBuilder)new ReceptorBuilder();
			var transaction = CreateTransactionInfo(CountryCodes.Uruguay, new List<RegistrationNumber>());

			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var receptorEFact = builder.BuildEFacReceptor(transaction);
			AssertNull(receptorEFact);

			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumber(UruguayOrgCusCodeInfo.OrgCusCodes.CID, null, null));
			receptorEFact = builder.BuildEFacReceptor(transaction);
			AssertNull(receptorEFact);

			transaction.OrganizationAddress.RegistrationNumberCollection[0].Type.Code = UruguayOrgCusCodeInfo.OrgCusCodes.RUT;
			transaction.OrganizationAddress.RegistrationNumberCollection[0].CountryOfIssue = new Country() { Code = CountryCodes.Uruguay };
			transaction.OrganizationAddress.RegistrationNumberCollection[0].Value = ZString.Empty;
			receptorEFact = builder.BuildEFacReceptor(transaction);
			AssertNull(receptorEFact);

			transaction.OrganizationAddress.RegistrationNumberCollection[0].Value = "21-25340600-19";
			receptorEFact = builder.BuildEFacReceptor(transaction);
			AssertEquals(nameof(receptorEFact.DocRecep), "21-25340600-19", receptorEFact.DocRecep);
			AssertEquals(nameof(receptorEFact.TipoDocRecep), DocType.Item2, receptorEFact.TipoDocRecep);

			transaction.OrganizationAddress.RegistrationNumberCollection[0].Type.Code = UruguayOrgCusCodeInfo.OrgCusCodes.RUT;
			transaction.OrganizationAddress.RegistrationNumberCollection[0].CountryOfIssue = new Country() { Code = CountryCodes.Chile };
			receptorEFact = builder.BuildEFacReceptor(transaction);
			AssertNull(receptorEFact);
		}

		public void TestBuilReceptor_ETicket_DocRecep_CID_NonNumericCharacters()
		{
			var builder = new ReceptorBuilder() as IReceptorBuilder;
			var transaction = CreateTransactionInfo(CountryCodes.Uruguay, new List<RegistrationNumber>());

			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumber(UruguayOrgCusCodeInfo.OrgCusCodes.CID, "1.235.678-9", CountryCodes.Uruguay));
			AssertBuildReceptor_ETicket_CIDCharacters("12356789");

			transaction.OrganizationAddress.RegistrationNumberCollection[0].Value = "12356789";
			AssertBuildReceptor_ETicket_CIDCharacters("12356789");

			transaction.OrganizationAddress.RegistrationNumberCollection[0].Value = "ABC.236 - 8@4";
			AssertBuildReceptor_ETicket_CIDCharacters("23684");

			void AssertBuildReceptor_ETicket_CIDCharacters(string expectedValue)
			{
				var receptor = builder.BuildETicketReceptor(transaction);

				AssertEquals(nameof(receptor.Item), expectedValue, receptor.Item);
			}
		}

		TransactionInfo CreateTransactionInfo(string country, List<RegistrationNumber> registrationNumbers)
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Country = new Country { Code = country }
				}
			};
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => registrationNumbers);

			return transaction;
		}

		RegistrationNumber CreateOrgRegistrationNumber(string orgCusCode, string regNumber, string country)
		{
			return new RegistrationNumber()
			{
				Type = new RegistrationNumberType() { Code = orgCusCode },
				Value = regNumber,
				CountryOfIssue = new Country() { Code = country }
			};
		}

		Receptor_Tck CreateETicketReceptorBuilderWithRegistrationCode(string description)
		{
			var receptorBuilder = new ReceptorBuilder();

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
			receptorBuilder.SubstituteTransactionInfoHelper_ForTestOnly(transactionInfoHelperMock.Object);

			transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(It.IsAny<OrganizationAddress>(), CountryCodes.Uruguay, UruguayOrgCusCodeInfo.OrgCusCodes.FZU)).Returns(description);

			var receptor = (receptorBuilder as IReceptorBuilder).BuildETicketReceptor(transactionInfo);

			return receptor;
		}

		Receptor_Fact CreateEFacReceptorBuilderWithRegistrationCode(string description)
		{
			var receptorBuilder = new ReceptorBuilder();

			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
			var transactionInfo = CreateTransactionInfo(CountryCodes.Uruguay, null);

			receptorBuilder.SubstituteTransactionInfoHelper_ForTestOnly(transactionInfoHelperMock.Object);

			transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(It.IsAny<OrganizationAddress>(), CountryCodes.Uruguay, UruguayOrgCusCodeInfo.OrgCusCodes.RUT)).Returns("123456789");
			transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(It.IsAny<OrganizationAddress>(), CountryCodes.Uruguay, UruguayOrgCusCodeInfo.OrgCusCodes.FZU)).Returns(description);

			var receptor = (receptorBuilder as IReceptorBuilder).BuildEFacReceptor(transactionInfo);

			return receptor;
		}
	}
}
