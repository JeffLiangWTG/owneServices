using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(JPJobDocAddress))]
	public class JPJobDocAddressTest : JobDocAddressTest
	{
		public void TestE2_GovRegNumType()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(DocAddress.E2_GovRegNumTypeInfo);

			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Code", captionResourceString.Caption);
				AssertEquals("Max length", 3, DocAddress.E2_GovRegNumTypeInfo.MaxLength);
				AssertNullOrEmpty("Should not be 'DEF'", DocAddress.E2_GovRegNumType);
			});
		}

		public void TestDefualE2_GovRegNumWhenOverride()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			AssertDefualE2_GovRegNumWhenOverride(declaration.AttorneyForCustomsProceduresAddress, new Dictionary<string, string>
						{
							{ OrgCusCode.JapanCodeTypes.JAS, "ABC123456789" },
							{ OrgCusCode.JapanCodeTypes.CIE, "C000012345679" },
							{ OrgCusCode.JapanCodeTypes.LPC, "1234567890123" },
						});
			AssertDefualE2_GovRegNumWhenOverride(declaration.SupplierDocumentaryAddress, new Dictionary<string, string>
						{
							{ OrgCusCode.JapanCodeTypes.JAS, "ABC123456789" },
							{ OrgCusCode.JapanCodeTypes.CIE, "C000012345679" },
							{ OrgCusCode.JapanCodeTypes.LPC, "1234567890123" },
						});
			AssertDefualE2_GovRegNumWhenOverride(declaration.ImporterDocumentaryAddress, new Dictionary<string, string>
						{
							{ OrgCusCode.JapanCodeTypes.CIE, "C000012345679" },
							{ OrgCusCode.JapanCodeTypes.FSB, "FBC123456789" },
						});
			AssertDefualE2_GovRegNumWhenOverride(declaration.DeclarationConsignorAddress, new Dictionary<string, string>
						{
							{ OrgCusCode.JapanCodeTypes.JAS, "ABC123456789" },
							{ OrgCusCode.JapanCodeTypes.CIE, "C000012345679" },
							{ OrgCusCode.JapanCodeTypes.LPC, "1234567890123" },
						});
			AssertDefualE2_GovRegNumWhenOverride(declaration.DeclarationConsigneeAddress, new Dictionary<string, string>
						{
							{ OrgCusCode.JapanCodeTypes.CIE, "C000012345679" },
							{ OrgCusCode.JapanCodeTypes.FSB, "FBC123456789" },
						});
			AssertDefualE2_GovRegNumWhenOverride(declaration.AirCargoAgent, new Dictionary<string, string>
						{
							{ OrgCusCode.JapanCodeTypes.NUC, "A1234" },
						});
			AssertDefualE2_GovRegNumWhenOverride(declaration.ExternalBrokerAddress, new Dictionary<string, string>
						{
							{ OrgCusCode.JapanCodeTypes.NUC, "A1234" },
						});
			AssertDefualE2_GovRegNumWhenOverride(declaration.InspectionWitness, new Dictionary<string, string>
						{
							{ OrgCusCode.JapanCodeTypes.NUC, "A1234" },
						});

			declaration.JE_MessageType = "IMP";
			AssertDefualE2_GovRegNumWhenOverride(declaration.AttorneyForCustomsProceduresAddress, new Dictionary<string, string>
						{
							{ OrgCusCode.JapanCodeTypes.JAS, "ABC123456789" },
							{ OrgCusCode.JapanCodeTypes.CIE, "C000012345679" },
							{ OrgCusCode.JapanCodeTypes.LPC, "1234567890123" },
						});
			AssertDefualE2_GovRegNumWhenOverride(declaration.SupplierDocumentaryAddress, new Dictionary<string, string>
						{
							{ OrgCusCode.JapanCodeTypes.CIE, "C000012345679" },
							{ OrgCusCode.JapanCodeTypes.FSB, "FBC123456789" },
						});
			AssertDefualE2_GovRegNumWhenOverride(declaration.ImporterDocumentaryAddress, new Dictionary<string, string>
						{
							{ OrgCusCode.JapanCodeTypes.JAS, "ABC123456789" },
							{ OrgCusCode.JapanCodeTypes.CIE, "C000012345679" },
							{ OrgCusCode.JapanCodeTypes.LPC, "1234567890123" },
						});
			AssertDefualE2_GovRegNumWhenOverride(declaration.DeclarationConsignorAddress, new Dictionary<string, string>
						{
							{ OrgCusCode.JapanCodeTypes.CIE, "C000012345679" },
							{ OrgCusCode.JapanCodeTypes.FSB, "FBC123456789" },
						});
			AssertDefualE2_GovRegNumWhenOverride(declaration.DeclarationConsigneeAddress, new Dictionary<string, string>
						{
							{ OrgCusCode.JapanCodeTypes.JAS, "ABC123456789" },
							{ OrgCusCode.JapanCodeTypes.CIE, "C000012345679" },
							{ OrgCusCode.JapanCodeTypes.LPC, "1234567890123" },
						});
		}

		void AssertDefualE2_GovRegNumWhenOverride(JobDocAddress jobDocAddress, Dictionary<string, string> codeTypePairs)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var newAddress = org.Addresses.AddNew();
			jobDocAddress.E2_OA_Address = newAddress.PK;
			jobDocAddress.E2_AddressOverride = true;
			AssertEquals(ZString.Empty, jobDocAddress.E2_GovRegNum);
			AssertEquals(codeTypePairs.LastOrDefault().Key, jobDocAddress.E2_GovRegNumType);
			foreach (var codeTypePair in codeTypePairs)
			{
				newAddress.CustomsCodes.AddNew(codeTypePair.Key, codeTypePair.Value, Core.Constants.CountryCodes.Japan);
				jobDocAddress.E2_AddressOverride = false;
				jobDocAddress.E2_AddressOverride = true;
				AssertEquals(codeTypePair.Key, jobDocAddress.E2_GovRegNumType);
				AssertEquals(codeTypePair.Value, jobDocAddress.E2_GovRegNum);
			}
		}

		public void TestRequiredCusCodeTypes()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			AssertContainsExactElementsInAnyOrder([OrgCusCode.JapanCodeTypes.LPC, OrgCusCode.JapanCodeTypes.CIE, OrgCusCode.JapanCodeTypes.JAS], declaration.AttorneyForCustomsProceduresAddress.RequiredCusCodeTypes);
			AssertContainsExactElementsInAnyOrder([OrgCusCode.JapanCodeTypes.LPC, OrgCusCode.JapanCodeTypes.CIE, OrgCusCode.JapanCodeTypes.JAS], declaration.SupplierDocumentaryAddress.RequiredCusCodeTypes);
			AssertContainsExactElementsInAnyOrder([OrgCusCode.JapanCodeTypes.FSB, OrgCusCode.JapanCodeTypes.CIE], declaration.ImporterDocumentaryAddress.RequiredCusCodeTypes);
			AssertContainsExactElementsInAnyOrder([OrgCusCode.JapanCodeTypes.LPC, OrgCusCode.JapanCodeTypes.CIE, OrgCusCode.JapanCodeTypes.JAS], declaration.DeclarationConsignorAddress.RequiredCusCodeTypes);
			AssertContainsExactElementsInAnyOrder([OrgCusCode.JapanCodeTypes.FSB, OrgCusCode.JapanCodeTypes.CIE], declaration.DeclarationConsigneeAddress.RequiredCusCodeTypes);
			AssertContainsExactElementsInAnyOrder([OrgCusCode.JapanCodeTypes.NUC], declaration.AirCargoAgent.RequiredCusCodeTypes);
			AssertContainsExactElementsInAnyOrder([OrgCusCode.JapanCodeTypes.NUC], declaration.ExternalBrokerAddress.RequiredCusCodeTypes);
			AssertContainsExactElementsInAnyOrder([OrgCusCode.JapanCodeTypes.NUC], declaration.InspectionWitness.RequiredCusCodeTypes);
		}

		public void TestAirCargoAgentOverride()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "OrgTest2";
			header.MainAddress.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.AAL, "A45", Core.Constants.CountryCodes.Japan);

			var airCargoAgent = declaration.AirCargoAgent;
			airCargoAgent.E2_OA_Address = header.MainAddress.PK;
			AssertEquals("A45", airCargoAgent.LocationCode);
			AssertEquals(OrgCusCode.JapanCodeTypes.AAL, airCargoAgent.LocationCodeType);
			AssertEquals(0, airCargoAgent.DocAddressNumbers.Count);

			airCargoAgent.E2_AddressOverride = true;
			AssertEquals("A45", airCargoAgent.LocationCode);
			AssertEquals(OrgCusCode.JapanCodeTypes.AAL, airCargoAgent.LocationCodeType);
			AssertEquals(1, airCargoAgent.DocAddressNumbers.Count);

			airCargoAgent.LocationCode = ZString.Empty;
			AssertEquals(ZString.Empty, airCargoAgent.LocationCode);
			AssertEquals(OrgCusCode.JapanCodeTypes.AAL, airCargoAgent.LocationCodeType);
			AssertEquals(0, airCargoAgent.DocAddressNumbers.Count);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var airCargoAgentAddress = declaration.AirCargoAgent;
			airCargoAgentAddress.E2_AddressOverride = true;
			return airCargoAgentAddress;
		}

		JPJobDocAddress DocAddress => docAddress ??= Factory.New<JPJobDocAddress>();
		JPJobDocAddress docAddress;
	}
}
