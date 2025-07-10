using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.Esterometro.Testing
{
	public class DatiFatturaHeaderTest : EsterometroTestCaseWithFactory
	{
		#region End to End Tests
		public void TestDatiFatturaHeader_BuildXml_BranchOrgProxy()
		{
			var branchOrgProxy = ObjectCreator.CreateEsterometroOrgProxyForBranch();
			var companyOrgProxy = ObjectCreator.CreateEsterometroOrgProxyForCompany();
			var company = ObjectCreator.CreateNewCompany("DIT", Core.Constants.CountryCodes.Italy, orgProxy: companyOrgProxy);
			var branch = ObjectCreator.CreateBranch("BIT", company, branchOrgProxy);
			Factory.Save();

			var expectedXml = @"<DatiFatturaHeader>
  <ProgressivoInvio>00001</ProgressivoInvio>
  <Dichiarante>
    <CodiceFiscale>   12457898</CodiceFiscale>
    <Carica>1</Carica>
  </Dichiarante>
</DatiFatturaHeader>";
			AssertXMLForBranch(branch.PK, expectedXml, new DatiFatturaHeader());
		}

		public void TestDatiFatturaHeader_BuildXml_ProgressivoInvio()
		{
			var branchOrgProxy = ObjectCreator.CreateEsterometroOrgProxyForBranch();
			var companyOrgProxy = ObjectCreator.CreateEsterometroOrgProxyForCompany();
			var company = ObjectCreator.CreateNewCompany("DIT", Core.Constants.CountryCodes.Italy, orgProxy: companyOrgProxy);
			var branch = ObjectCreator.CreateBranch("BIT", company, branchOrgProxy);
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.ComplianceReportFileNextSequenceNumber.SetTemporaryValue(branch.PK.ToGuid(), Guid.Empty, Guid.Empty, 2))
			{
				var expectedXml = @"<DatiFatturaHeader>
  <ProgressivoInvio>00002</ProgressivoInvio>
  <Dichiarante>
    <CodiceFiscale>   12457898</CodiceFiscale>
    <Carica>1</Carica>
  </Dichiarante>
</DatiFatturaHeader>";
				AssertXMLForBranch(branch.PK, expectedXml, new DatiFatturaHeader());
			}

			using (AccountingMasterFilesRegistry.Instance.ComplianceReportFileNextSequenceNumber.SetTemporaryValue(branch.PK.ToGuid(), Guid.Empty, Guid.Empty, 10))
			{
				var expectedXml = @"<DatiFatturaHeader>
  <ProgressivoInvio>0000A</ProgressivoInvio>
  <Dichiarante>
    <CodiceFiscale>   12457898</CodiceFiscale>
    <Carica>1</Carica>
  </Dichiarante>
</DatiFatturaHeader>";
				AssertXMLForBranch(branch.PK, expectedXml, new DatiFatturaHeader());
			}
		}

		public void TestDatiFatturaHeader_BuildXml_CompanyOrgProxy()
		{
			var companyOrgProxy = ObjectCreator.CreateEsterometroOrgProxyForCompany();
			var company = ObjectCreator.CreateNewCompany("DIT", Core.Constants.CountryCodes.Italy, orgProxy: companyOrgProxy);
			var branch = ObjectCreator.CreateBranch("BIT", company);
			branch.GB_OH_OrgProxy = ZGuid.Empty;
			Factory.Save();

			AssertEquals(ZGuid.Empty, branch.GB_OH_OrgProxy);

			var expectedXml = @"<DatiFatturaHeader>
  <ProgressivoInvio>00001</ProgressivoInvio>
  <Dichiarante>
    <CodiceFiscale>   98645152</CodiceFiscale>
    <Carica>1</Carica>
  </Dichiarante>
</DatiFatturaHeader>";
			AssertXMLForBranch(branch.PK, expectedXml, new DatiFatturaHeader());
		}

		public void TestDatiFatturaHeader_BuildXml_NoOrgProxy()
		{
			var company = ObjectCreator.CreateNewCompany("DIT", Core.Constants.CountryCodes.Italy);
			var branch = ObjectCreator.CreateBranch("BIT", company);
			Factory.Save();
			Factory.Save();

			AssertEquals(ZGuid.Empty, branch.GB_OH_OrgProxy);
			AssertEquals(ZGuid.Empty, company.GC_OH_OrgProxy);

			var expectedXml = @"<DatiFatturaHeader>
  <ProgressivoInvio>00001</ProgressivoInvio>
  <Dichiarante>
    <CodiceFiscale />
    <Carica>1</Carica>
  </Dichiarante>
</DatiFatturaHeader>";
			AssertXMLForBranch(branch.PK, expectedXml, new DatiFatturaHeader());
		}
		#endregion

		#region Unit Tests for individual elements
		public void TestCodiceFiscaleXsdCompliance()
		{
			var expectedXml = @"<DatiFatturaHeader>
  <ProgressivoInvio>00001</ProgressivoInvio>
  <Dichiarante>
    <CodiceFiscale>0123456789012345</CodiceFiscale>
    <Carica>1</Carica>
  </Dichiarante>
</DatiFatturaHeader>";
			this.AssertXMLEqualsByDiff("CodiceFiscale element truncates to 16 characters.", expectedXml,
				new DatiFatturaHeader().BuildBuildDatiFatturaHeaderXmlForUnitTest("012345678901234567890", 1).ToString()
			);

			expectedXml = @"<DatiFatturaHeader>
  <ProgressivoInvio>00001</ProgressivoInvio>
  <Dichiarante>
    <CodiceFiscale>0123 4567 8901 2</CodiceFiscale>
    <Carica>1</Carica>
  </Dichiarante>
</DatiFatturaHeader>";
			this.AssertXMLEqualsByDiff("CodiceFiscale element includes whitespace when considering 16 character limit.", expectedXml,
				new DatiFatturaHeader().BuildBuildDatiFatturaHeaderXmlForUnitTest("0123 4567 8901 2345", 1).ToString()
			);

			expectedXml = @"<DatiFatturaHeader>
  <ProgressivoInvio>00001</ProgressivoInvio>
  <Dichiarante>
    <CodiceFiscale>ABCD456789012345</CodiceFiscale>
    <Carica>1</Carica>
  </Dichiarante>
</DatiFatturaHeader>";
			this.AssertXMLEqualsByDiff("CodiceFiscale element converts lower case to upper.", expectedXml,
				new DatiFatturaHeader().BuildBuildDatiFatturaHeaderXmlForUnitTest("abCD456789012345", 1).ToString()
			);

			expectedXml = @"<DatiFatturaHeader>
  <ProgressivoInvio>00001</ProgressivoInvio>
  <Dichiarante>
    <CodiceFiscale>  23456789012345</CodiceFiscale>
    <Carica>1</Carica>
  </Dichiarante>
</DatiFatturaHeader>";
			this.AssertXMLEqualsByDiff("CodiceFiscale element replaces non-ASCII characters with whitespace.", expectedXml,
				new DatiFatturaHeader().BuildBuildDatiFatturaHeaderXmlForUnitTest("€€23456789012345", 1).ToString()
			);

			expectedXml = @"<DatiFatturaHeader>
  <ProgressivoInvio>00001</ProgressivoInvio>
  <Dichiarante>
    <CodiceFiscale />
    <Carica>1</Carica>
  </Dichiarante>
</DatiFatturaHeader>";
			this.AssertXMLEqualsByDiff("CodiceFiscale element is blank, and produces element with no value.", expectedXml,
				new DatiFatturaHeader().BuildBuildDatiFatturaHeaderXmlForUnitTest("", 1).ToString()
			);
		}

		public void TestProgressivoInvioFormatting()
		{
			var expectedXml = @"<DatiFatturaHeader>
  <ProgressivoInvio>00001</ProgressivoInvio>
  <Dichiarante>
    <CodiceFiscale />
    <Carica>1</Carica>
  </Dichiarante>
</DatiFatturaHeader>";
			this.AssertXMLEqualsByDiff("ProgressivoInvio element formats 1.", expectedXml,
				new DatiFatturaHeader().BuildBuildDatiFatturaHeaderXmlForUnitTest("", 1).ToString()
			);

			expectedXml = @"<DatiFatturaHeader>
  <ProgressivoInvio>0000A</ProgressivoInvio>
  <Dichiarante>
    <CodiceFiscale />
    <Carica>1</Carica>
  </Dichiarante>
</DatiFatturaHeader>";
			this.AssertXMLEqualsByDiff("ProgressivoInvio element formats 10 as A.", expectedXml,
				new DatiFatturaHeader().BuildBuildDatiFatturaHeaderXmlForUnitTest("", 10).ToString()
			);
		}

		public void TestReadNextProgressiveNumber()
		{
			AssertEquals("Progressive Number should start at one.", 1, DatiFatturaHeader.GetNextSequenceNumber());
			AssertEquals("Progressive Number should not increment.", 1, DatiFatturaHeader.GetNextSequenceNumber());
			using (AccountingMasterFilesRegistry.Instance.ComplianceReportFileNextSequenceNumber.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 2))
			{
				AssertEquals("Progressive Number should be read from registry.", 2, DatiFatturaHeader.GetNextSequenceNumber());
			}
		}

		public void TestProgressivoInvioReadsFromRegistry()
		{
			var expectedXml = @"<DatiFatturaHeader>
  <ProgressivoInvio>00001</ProgressivoInvio>
  <Dichiarante>
    <CodiceFiscale />
    <Carica>1</Carica>
  </Dichiarante>
</DatiFatturaHeader>";
			AssertXMLForBranch(Env.CurrentBranchPK, $"ProgressivoInvio element should default to 1, based on registry item {nameof(AccountingMasterFilesRegistry.ComplianceReportFileNextSequenceNumber)}.", expectedXml, new DatiFatturaHeader());

			using (AccountingMasterFilesRegistry.Instance.ComplianceReportFileNextSequenceNumber.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 2))
			{
				expectedXml = @"<DatiFatturaHeader>
  <ProgressivoInvio>00002</ProgressivoInvio>
  <Dichiarante>
    <CodiceFiscale />
    <Carica>1</Carica>
  </Dichiarante>
</DatiFatturaHeader>";
				AssertXMLForBranch(Env.CurrentBranchPK, $"ProgressivoInvio element is read from registry item {nameof(AccountingMasterFilesRegistry.ComplianceReportFileNextSequenceNumber)}.", expectedXml, new DatiFatturaHeader());
			}

			var company = ObjectCreator.CreateNewCompany("DIT", Core.Constants.CountryCodes.Italy);
			var branch = ObjectCreator.CreateBranch("BIT", company);
			using (AccountingMasterFilesRegistry.Instance.ComplianceReportFileNextSequenceNumber.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 3))
			using (AccountingMasterFilesRegistry.Instance.ComplianceReportFileNextSequenceNumber.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, 4))
			{
				expectedXml = @"<DatiFatturaHeader>
  <ProgressivoInvio>00003</ProgressivoInvio>
  <Dichiarante>
    <CodiceFiscale />
    <Carica>1</Carica>
  </Dichiarante>
</DatiFatturaHeader>";
				AssertXMLForBranch(Env.CurrentBranchPK, $"ProgressivoInvio element registry item is read from current company.", expectedXml, new DatiFatturaHeader());
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					expectedXml = @"<DatiFatturaHeader>
  <ProgressivoInvio>00004</ProgressivoInvio>
  <Dichiarante>
    <CodiceFiscale />
    <Carica>1</Carica>
  </Dichiarante>
</DatiFatturaHeader>";
					AssertXMLForBranch(Env.CurrentBranchPK, $"ProgressivoInvio element registry item is read from different company.", expectedXml, new DatiFatturaHeader());
				}
			}
		}

		public void TestIvaOrgHeaderFallbackLogic()
		{
			var branchOrgProxy = ObjectCreator.CreateEsterometroOrgProxyForBranch();
			var companyOrgProxy = ObjectCreator.CreateEsterometroOrgProxyForCompany();
			var company = ObjectCreator.CreateNewCompany("DIT", Core.Constants.CountryCodes.Italy, orgProxy: companyOrgProxy);
			var branch = ObjectCreator.CreateBranch("BIT", company, branchOrgProxy);
			Factory.Save();

			var actualOrgHeader = EsterometroDataHelper.SelectBestOrgProxyOrNull(branch, company);
			AssertEquals("Branch with Italian IVA number should use branch IVA.", branchOrgProxy.IvaNumberForItaly(), actualOrgHeader.IvaNumberForItaly());

			branchOrgProxy.CustomsCodes.RemoveAll();
			branchOrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.IVA, "66778899", Core.Constants.CountryCodes.Australia);
			AssertEquals("Precondition: one CustomCode", 1, branchOrgProxy.CustomsCodes.Count);
			actualOrgHeader = EsterometroDataHelper.SelectBestOrgProxyOrNull(branch, company);
			AssertEquals("Branch without Italian IVA number should be blank.", ZString.Empty, actualOrgHeader.IvaNumberForItaly());

			branchOrgProxy.CustomsCodes.RemoveAll();
			AssertEquals("Precondition: no CustomCodes", 0, branchOrgProxy.CustomsCodes.Count);
			actualOrgHeader = EsterometroDataHelper.SelectBestOrgProxyOrNull(branch, company);
			AssertEquals("Branch without IVA number should be blank.", ZString.Empty, actualOrgHeader.IvaNumberForItaly());

			branch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertNull("Precondition: null branch OrgProxy", branch.OrgProxy);
			actualOrgHeader = EsterometroDataHelper.SelectBestOrgProxyOrNull(branch, company);
			AssertEquals("Branch with null org proxy should use company IVA.", companyOrgProxy.IvaNumberForItaly(), actualOrgHeader.IvaNumberForItaly());

			companyOrgProxy.CustomsCodes.RemoveAll();
			companyOrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.IVA, "11223344", Core.Constants.CountryCodes.Australia);
			AssertNull("Precondition: null branch OrgProxy", branch.OrgProxy);
			AssertEquals("Precondition: one CustomCode", 1, companyOrgProxy.CustomsCodes.Count);
			actualOrgHeader = EsterometroDataHelper.SelectBestOrgProxyOrNull(branch, company);
			AssertEquals("Branch with null org proxy, and company without Italian IVA should give empty IVA.", ZString.Empty, actualOrgHeader.IvaNumberForItaly());

			companyOrgProxy.CustomsCodes.RemoveAll();
			AssertNull("Precondition: null branch OrgProxy", branch.OrgProxy);
			AssertEquals("Precondition: no CustomCodes", 0, companyOrgProxy.CustomsCodes.Count);
			actualOrgHeader = EsterometroDataHelper.SelectBestOrgProxyOrNull(branch, company);
			AssertEquals("Branch with null org proxy, and company without IVA should give empty IVA.", ZString.Empty, actualOrgHeader.IvaNumberForItaly());

			company.GC_OH_OrgProxy = ZGuid.Empty;
			AssertNull("Precondition: null branch OrgProxy", branch.OrgProxy);
			AssertNull("Precondition: null company OrgProxy", company.OrgProxy);
			actualOrgHeader = EsterometroDataHelper.SelectBestOrgProxyOrNull(branch, company);
			AssertEquals("Branch with null org proxy, and company null org proxy should give null org proxy.", ZString.Empty, actualOrgHeader.IvaNumberForItaly());

			branch.GB_OH_OrgProxy = branchOrgProxy.PK;
			branchOrgProxy.CustomsCodes.RemoveAll();
			branchOrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.IVA, "66778899", Core.Constants.CountryCodes.Italy);
			AssertNotNull("Precondition: not null branch OrgProxy", branch.OrgProxy);
			AssertNull("Precondition: null company OrgProxy", company.OrgProxy);
			AssertEquals("Precondition: one branch CustomCode", 1, branchOrgProxy.CustomsCodes.Count);
			actualOrgHeader = EsterometroDataHelper.SelectBestOrgProxyOrNull(branch, company);
			AssertEquals("Branch with Italian IVA number, and company null org proxy should use branch IVA.", branchOrgProxy.IvaNumberForItaly(), actualOrgHeader.IvaNumberForItaly());
		}
		#endregion
	}
}
