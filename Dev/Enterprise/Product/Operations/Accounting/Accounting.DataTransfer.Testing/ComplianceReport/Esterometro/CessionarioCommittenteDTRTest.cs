using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.Esterometro.Testing
{
	public class CessionarioCommittenteDTRTest : EsterometroTestCaseWithFactory
	{
		public void TestCessionarioCommittenteDTR_BranchOrgProxy()
		{
			var branchOrgProxy = CreateOrgProxyForBranch();
			var companyOrgProxy = CreateOrgProxyForCompany();
			var company = ObjectCreator.CreateNewCompany("DIT", Core.Constants.CountryCodes.Italy, orgProxy: companyOrgProxy);
			var branch = ObjectCreator.CreateBranch("BIT", company, branchOrgProxy);
			Factory.Save();

			var expectedXML = @"<CessionarioCommittenteDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>12457898</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
</CessionarioCommittenteDTR>
";

			AssertXMLForBranch(branch.PK, expectedXML, new CessionarioCommittenteDTR());
		}

		public void TestCessionarioCommittenteDTR_BranchHasNoOrgProxyAndCompanyHasOrgProxy()
		{
			var companyOrgProxy = CreateOrgProxyForCompany();
			var company = ObjectCreator.CreateNewCompany("DIT", Core.Constants.CountryCodes.Italy, orgProxy: companyOrgProxy);
			var branch = ObjectCreator.CreateBranch("BIT", company);
			branch.GB_OH_OrgProxy = ZGuid.Empty;
			Factory.Save();

			AssertEquals(ZGuid.Empty, branch.GB_OH_OrgProxy);

			var expectedXML = @"<CessionarioCommittenteDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>98645152</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Fratelli Salvadori Srl</Denominazione>
    <Sede>
      <Indirizzo>Viale Peitro Pietramellara 11</Indirizzo>
      <CAP>40121</CAP>
      <Comune>Bologna</Comune>
      <Provincia>BO</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
</CessionarioCommittenteDTR>
";

			AssertXMLForBranch(branch.PK, expectedXML, new CessionarioCommittenteDTR());
		}

		public void TestCessionarioCommittenteDTR_BothBranchAndCompanyHasNoOrgProxy()
		{
			var company = ObjectCreator.CreateNewCompany("DIT", Core.Constants.CountryCodes.Italy);
			var branch = ObjectCreator.CreateBranch("BIT", company);
			Factory.Save();

			AssertEquals(ZGuid.Empty, branch.GB_OH_OrgProxy);
			AssertEquals(ZGuid.Empty, company.GC_OH_OrgProxy);

			var expectedXML = @"<CessionarioCommittenteDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice></IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione />
    <Sede>
      <Indirizzo />
      <CAP>00000</CAP>
      <Comune />
      <Nazione></Nazione>
    </Sede>
  </AltriDatiIdentificativi>
</CessionarioCommittenteDTR>
";

			AssertXMLForBranch(branch.PK, expectedXML, new CessionarioCommittenteDTR());
		}

		public void TestCessionarioCommittenteDTR_OrgProxyFullnameIsLongerThan80Characters()
		{
			var branchOrgProxy = CreateOrgProxyForBranch();
			branchOrgProxy.OH_FullName = "This is my really really really really really really really long organization name";
			var company = ObjectCreator.CreateNewCompany("DIT", Core.Constants.CountryCodes.Italy);
			var branch = ObjectCreator.CreateBranch("BIT", company, branchOrgProxy);
			Factory.Save();

			var expectedXML = @"<CessionarioCommittenteDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>12457898</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>This is my really really really really really really really long organization na</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
</CessionarioCommittenteDTR>
";

			AssertXMLForBranch(branch.PK, expectedXML, new CessionarioCommittenteDTR());
		}

		public void TestCessionarioCommittenteDTR_OrgProxyDoesNotHaveIVARegistrationNumber()
		{
			var branchOrgProxy = CreateOrgProxyForBranch();
			branchOrgProxy.CustomsCodes.RemoveAndDeleteAll();
			var company = ObjectCreator.CreateNewCompany("DIT", Core.Constants.CountryCodes.Italy);
			var branch = ObjectCreator.CreateBranch("BIT", company, branchOrgProxy);
			Factory.Save();

			var expectedXML = @"<CessionarioCommittenteDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice></IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
</CessionarioCommittenteDTR>
";

			AssertXMLForBranch(branch.PK, expectedXML, new CessionarioCommittenteDTR());
		}

		public void TestCessionarioCommittenteDTR_OrgProxyHaveIVARegistrationNumberIssuedByNonItalyCountry()
		{
			var branchOrgProxy = CreateOrgProxyForBranch();
			branchOrgProxy.CustomsCodes.RemoveAndDeleteAll();
			branchOrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.IVA, "45454545", Core.Constants.CountryCodes.Australia);
			var company = ObjectCreator.CreateNewCompany("DIT", Core.Constants.CountryCodes.Italy);
			var branch = ObjectCreator.CreateBranch("BIT", company, branchOrgProxy);
			Factory.Save();

			var expectedXML = @"<CessionarioCommittenteDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice></IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
</CessionarioCommittenteDTR>
";

			AssertXMLForBranch(branch.PK, expectedXML, new CessionarioCommittenteDTR());
		}

		public void TestCessionarioCommittenteDTR_PostCodeIsLongerThanFiveDigits()
		{
			var branchOrgProxy = CreateOrgProxyForBranch();
			branchOrgProxy.MainAddress.OA_PostCode = "203568";
			var company = ObjectCreator.CreateNewCompany("DIT", Core.Constants.CountryCodes.Italy);
			var branch = ObjectCreator.CreateBranch("BIT", company, branchOrgProxy);
			Factory.Save();

			var expectedXML = @"<CessionarioCommittenteDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>12457898</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20356</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
</CessionarioCommittenteDTR>
";

			AssertXMLForBranch(branch.PK, expectedXML, new CessionarioCommittenteDTR());
		}

		public void TestCessionarioCommittenteDTR_PostCodeIsShorterThanFiveDigits()
		{
			var branchOrgProxy = CreateOrgProxyForBranch();
			branchOrgProxy.MainAddress.OA_PostCode = "2035";
			var company = ObjectCreator.CreateNewCompany("DIT", Core.Constants.CountryCodes.Italy);
			var branch = ObjectCreator.CreateBranch("BIT", company, branchOrgProxy);
			Factory.Save();

			var expectedXML = @"<CessionarioCommittenteDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>12457898</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>02035</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
</CessionarioCommittenteDTR>
";

			AssertXMLForBranch(branch.PK, expectedXML, new CessionarioCommittenteDTR());
		}

		public void TestCessionarioCommittenteDTR_PostCodeContainsAlphabeticalCharacters()
		{
			var branchOrgProxy = CreateOrgProxyForBranch();
			branchOrgProxy.MainAddress.OA_PostCode = "LS158AH";
			var company = ObjectCreator.CreateNewCompany("DIT", Core.Constants.CountryCodes.Italy);
			var branch = ObjectCreator.CreateBranch("BIT", company, branchOrgProxy);
			Factory.Save();

			var expectedXML = @"<CessionarioCommittenteDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>12457898</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a socio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Cassanese 224 Palazzo Caravaggio</Indirizzo>
      <CAP>00000</CAP>
      <Comune>Milan</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
</CessionarioCommittenteDTR>
";

			AssertXMLForBranch(branch.PK, expectedXML, new CessionarioCommittenteDTR());
		}

		public void TestCessionarioCommittenteDTR_OrgProxyNameAndAddressHaveSpecialCharacters()
		{
			var branchOrgProxy = CreateOrgProxyForBranch();
			branchOrgProxy.OH_FullName = "Italy SRL a €ocio unico";
			branchOrgProxy.MainAddress.OA_Address1 = "Via Ca€€ane€e";
			branchOrgProxy.MainAddress.OA_City = "Bre€cia";
			var company = ObjectCreator.CreateNewCompany("DIT", Core.Constants.CountryCodes.Italy);
			var branch = ObjectCreator.CreateBranch("BIT", company, branchOrgProxy);
			Factory.Save();

			var expectedXML = @"<CessionarioCommittenteDTR>
  <IdentificativiFiscali>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>12457898</IdCodice>
    </IdFiscaleIVA>
  </IdentificativiFiscali>
  <AltriDatiIdentificativi>
    <Denominazione>Italy SRL a  ocio unico</Denominazione>
    <Sede>
      <Indirizzo>Via Ca  ane e 224 Palazzo Caravaggio</Indirizzo>
      <CAP>20090</CAP>
      <Comune>Bre cia</Comune>
      <Provincia>MI</Provincia>
      <Nazione>IT</Nazione>
    </Sede>
  </AltriDatiIdentificativi>
</CessionarioCommittenteDTR>
";

			AssertXMLForBranch(branch.PK, expectedXML, new CessionarioCommittenteDTR());
		}

		#region Helpers

		OrgHeader CreateOrgProxyForCompany() => ObjectCreator.CreateEsterometroOrgProxyForCompany();

		OrgHeader CreateOrgProxyForBranch() => ObjectCreator.CreateEsterometroOrgProxyForBranch();

		#endregion
	}
}
