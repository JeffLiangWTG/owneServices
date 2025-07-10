using System;
using System.Globalization;
using System.Xml;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	class LicenceCompanyDetailsTest : TestCase
	{
		public void TestAddToLicenceNode()
		{
			string[] expectedAttributes = new string[] {
				"OrgPKThatGeneratedThisLicence", "EnterpriseCode", "PhysicalServerID", "Code", "Name", "Address1", "Address2", "City", "PostCode", "State", "CountryPK",
				"BusinessRegNo", "BusinessRegNo2", "LocalCurrencyCode", "LocalCurrencyPK", "IsReciprocal", "IsGSTRegistered", "IsGSTCashBasis", "IsWHTRegistered", "IsWHTCashBasis",
				"Phone", "Fax", "Email", "WebAddress", "SystemId", "CompanyId"
			};

			XmlNode testNode = NodeOfCompanyDetails; // This calls AddToLicenceNode
			for (int i = 0; i < testNode.Attributes.Count; i++)
			{
				AssertEquals(string.Format(CultureInfo.CurrentCulture, "Node {0} should match the expected item in order", i), expectedAttributes[i], testNode.Attributes[i].Name);
			}
		}

		public void TestLoadFromLicenceNode()
		{
			LicenceCompanyDetails details = new LicenceCompanyDetails(EnvProxy.Instance.CurrentCompany);
			details.LoadFromLicenceNode(NodeOfCompanyDetails);
			var currentCompany = EnvProxy.Instance.CurrentCompany;
			AssertEquals(Guid.Empty, details.OrgPKThatGeneratedThisLicence);
			AssertEquals("TST", details.EnterpriseCode);
			AssertEquals(EnvProxy.Instance.Registry.PhysicalServerID, details.PhysicalServerID);
			AssertEquals(currentCompany.Code, details.Code);
			AssertEquals(currentCompany.Name, details.Name);
			AssertEquals(currentCompany.Address1, details.Address1);
			AssertEquals(currentCompany.Address2, details.Address2);
			AssertEquals(currentCompany.City, details.City);
			AssertEquals(currentCompany.Postcode, details.PostCode);
			AssertEquals(currentCompany.State, details.State);
			AssertEquals(currentCompany.Country.PK, details.CountryPK);
			AssertEquals(currentCompany.BusinessRegNo1, details.BusinessRegNo);
			AssertEquals(currentCompany.BusinessRegNo2, details.BusinessRegNo2);
			AssertEquals(currentCompany.LocalCurrency.Code, details.LocalCurrencyCode);
			AssertEquals(currentCompany.IsReciprocal, details.IsReciprocal);
			AssertEquals(currentCompany.IsGSTRegistered, details.IsGSTRegistered);
			AssertEquals(currentCompany.IsGSTCashBasis, details.IsGSTCashBasis);
			AssertEquals(currentCompany.IsWHTRegistered, details.IsWHTRegistered);
			AssertEquals(currentCompany.IsWHTCashBasis, details.IsWHTCashBasis);
			AssertEquals("11111111", details.Phone);
			AssertEquals("22222222", details.Fax);
			AssertEquals("test@cargowise.com", details.Email);
			AssertEquals("www.cargowise.com", details.WebAddress);
		}

		public void TestGSTandReciprocalSettingDoesntValidateAgainstLicenceKey()
		{
			string currentUser = EnvProxy.Instance.CurrentUser.LoginName;
			Guid currentBranch = EnvProxy.Instance.CurrentBranch.PK;
			Guid currentDepartment = EnvProxy.Instance.CurrentDepartment.PK;

			using (EnvProxy.Instance.SetTemporaryUserContext(currentUser, currentBranch, currentDepartment))
			{
				LicenceCompanyDetails testCompanyDetails = new LicenceCompanyDetails(EnvProxy.Instance.CurrentCompany);
				testCompanyDetails.Set(Guid.Empty, "TST", EnvProxy.Instance.Registry.PhysicalServerID,
					EnvProxy.Instance.CurrentCompany.Code,
					EnvProxy.Instance.CurrentCompany.Name,
					EnvProxy.Instance.CurrentCompany.Address1,
					EnvProxy.Instance.CurrentCompany.Address2,
					EnvProxy.Instance.CurrentCompany.City,
					EnvProxy.Instance.CurrentCompany.Postcode,
					EnvProxy.Instance.CurrentCompany.State,
					EnvProxy.Instance.CurrentCompany.Country.PK,
					EnvProxy.Instance.CurrentCompany.BusinessRegNo1,
					EnvProxy.Instance.CurrentCompany.BusinessRegNo2,
					EnvProxy.Instance.CurrentCompany.LocalCurrency.Code,
					Guid.Empty,
					!EnvProxy.Instance.CurrentCompany.IsReciprocal,
					!EnvProxy.Instance.CurrentCompany.IsGSTRegistered,
					!EnvProxy.Instance.CurrentCompany.IsGSTCashBasis,
					EnvProxy.Instance.CurrentCompany.IsWHTRegistered,
					EnvProxy.Instance.CurrentCompany.IsWHTCashBasis);
				Assert("Should not validate GC_IsReciprocal, GC_IsGSTRegistered, GC_IsGSTCashBasis when detecting tampering", testCompanyDetails.IsCompanyDetailsCorrect);
			}
		}

		public void TestSettableProperties()
		{
			LicenceCompanyDetails details = new LicenceCompanyDetails(EnvProxy.Instance.CurrentCompany);
			details.EnterpriseCode = "EnterpriseCode";
			AssertEquals("EnterpriseCode", details.EnterpriseCode);

			details.Code = "Code";
			AssertEquals("Code", details.Code);

			details.PhysicalServerID = "Physical server ID";
			AssertEquals("Physical server ID", details.PhysicalServerID);
		}

		public void TestLoadFromLicenceVersionWithoutPhone()
		{
			// Licence without the phone, etc fields that were added May 2013
			const string xml = @"<TestLoadFromLicenceNode OrgPKThatGeneratedThisLicence=""00000000-0000-0000-0000-000000000000"" EnterpriseCode=""TST"" PhysicalServerID=""DAT"" Code=""EDI""
				Name=""Eagle Datamation International"" Address1=""184 Bourke Road"" Address2="""" City=""Alexandria"" PostCode=""2015"" State=""NSW""
				CountryPK=""e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e"" BusinessRegNo=""41 065 894 724"" BusinessRegNo2=""C065894724"" 
				LocalCurrencyCode=""AUD"" LocalCurrencyPK=""53feb23f-a7b0-4e0d-af5d-f6f6e5417399"" IsReciprocal=""False""
				IsGSTRegistered=""True"" IsGSTCashBasis=""False"" IsWHTRegistered=""False"" IsWHTCashBasis=""True"" />";

			LicenceCompanyDetails details = new LicenceCompanyDetails(EnvProxy.Instance.CurrentCompany);
			var doc = new XmlDocument();
			doc.LoadXml(xml);
			details.LoadFromLicenceNode(doc.FirstChild);
			AssertEquals(Guid.Empty, details.OrgPKThatGeneratedThisLicence);
			AssertEquals("TST", details.EnterpriseCode);
			AssertEquals("DAT", details.PhysicalServerID);
			AssertEquals("EDI", details.Code);
			AssertEquals("Eagle Datamation International", details.Name);
			AssertEquals("184 Bourke Road", details.Address1);
			AssertEquals("", details.Address2);
			AssertEquals("Alexandria", details.City);
			AssertEquals("2015", details.PostCode);
			AssertEquals("NSW", details.State);
			AssertEquals(new Guid("e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e"), details.CountryPK);
			AssertEquals("41 065 894 724", details.BusinessRegNo);
			AssertEquals("C065894724", details.BusinessRegNo2);
			AssertEquals("AUD", details.LocalCurrencyCode);
			AssertEquals(false, details.IsReciprocal);
			AssertEquals(true, details.IsGSTRegistered);
			AssertEquals(false, details.IsGSTCashBasis);
			AssertEquals(false, details.IsWHTRegistered);
			AssertEquals(true, details.IsWHTCashBasis);
		}

		XmlNode NodeOfCompanyDetails
		{
			get
			{
				LicenceCompanyDetails companyDetails = new LicenceCompanyDetails(EnvProxy.Instance.CurrentCompany);
				var currentCompany = EnvProxy.Instance.CurrentCompany;
				companyDetails.Set(Guid.Empty, "TST",
						EnvProxy.Instance.Registry.PhysicalServerID,
						currentCompany.Code,
						currentCompany.Name,
						currentCompany.Address1,
						currentCompany.Address2,
						currentCompany.City,
						currentCompany.Postcode,
						currentCompany.State,
						currentCompany.Country.PK,
						currentCompany.BusinessRegNo1,
						currentCompany.BusinessRegNo2,
						currentCompany.LocalCurrency.Code,
						Guid.Empty,
						currentCompany.IsReciprocal,
						currentCompany.IsGSTRegistered,
						currentCompany.IsGSTCashBasis,
						currentCompany.IsWHTRegistered,
						currentCompany.IsWHTCashBasis,
						"11111111",
						"22222222",
						"test@cargowise.com",
						"www.cargowise.com");

				XmlDocument licenceKeyXml = new XmlDocument();
				XmlNode companyNode = licenceKeyXml.CreateNode(XmlNodeType.Element, Name, "");
				companyDetails.AddToLicenceNode(companyNode);

				return companyNode;
			}
		}
	}
}