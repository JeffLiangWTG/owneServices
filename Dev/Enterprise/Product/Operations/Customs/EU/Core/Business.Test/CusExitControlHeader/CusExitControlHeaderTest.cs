using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusExitControlHeader))]
	public class CusExitControlHeaderTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			return exitHeader;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		[ExpectNoExceptions]
		public void TestCusExitDetailsChildEditable()
		{
			var exitHeader = Factory.New<CusExitControlHeader>();
			NUnit.Framework.Assert.That(exitHeader.IsRegisteredEditableChildObject(exitHeader.CusExitDetails), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDelete()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			var exitDetail = exitHeader.CusExitDetails.AddNew();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(exitHeader.IsDeleted, NUnit.Framework.Is.EqualTo(false), "CusExitControlHeader not deleted");
				NUnit.Framework.Assert.That(exitDetail.IsDeleted, NUnit.Framework.Is.EqualTo(false), "CusExitDetail not deleted");
				exitHeader.Delete();
				NUnit.Framework.Assert.That(exitHeader.IsDeleted, NUnit.Framework.Is.EqualTo(true), "CusExitControlHeader deleted");
				NUnit.Framework.Assert.That(exitDetail.IsDeleted, NUnit.Framework.Is.EqualTo(true), "CusExitDetail deleted");
			});
		}

		[ExpectNoExceptions]
		public void TestParent()
		{
			var exitHeader = Factory.New<CusExitControlHeader>();
			var jobDeclaration1 = Factory.New<JobDeclaration>();
			NUnit.Framework.Assert.That(exitHeader.CEH_Parent, NUnit.Framework.Is.EqualTo(default(BusinessObject)));

			exitHeader.CEH_ParentID = jobDeclaration1.PK;
			exitHeader.CEH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			NUnit.Framework.Assert.That(exitHeader.CEH_Parent, NUnit.Framework.Is.SameAs(jobDeclaration1));

			var jobDeclaration2 = Factory.New<JobDeclaration>();
			exitHeader.CEH_Parent = jobDeclaration2;
			NUnit.Framework.Assert.That(exitHeader.CEH_ParentID, NUnit.Framework.Is.EqualTo(jobDeclaration2.PK));
			NUnit.Framework.Assert.That(exitHeader.CEH_ParentTableCode, NUnit.Framework.Is.EqualTo(JobDeclarationSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDeclaration()
		{
			var exitHeader = Factory.New<CusExitControlHeader>();
			NUnit.Framework.Assert.That(exitHeader.Declaration, NUnit.Framework.Is.EqualTo(default(JobDeclaration)), "Declaration - should be [null]");

			var declaration = Factory.New<JobDeclaration>();
			exitHeader.CEH_ParentTableCode = declaration.TablePrefix;
			exitHeader.CEH_ParentID = declaration.PK;
			NUnit.Framework.Assert.That(exitHeader.Declaration, NUnit.Framework.Is.EqualTo(declaration), "Declaration");

			var shipment = Factory.New<ForwardingShipment>();
			exitHeader.CEH_ParentTableCode = shipment.TablePrefix;
			exitHeader.CEH_ParentID = shipment.PK;
			NUnit.Framework.Assert.That(exitHeader.Declaration, NUnit.Framework.Is.EqualTo(default(JobDeclaration)), "Declaration - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestDataGrouping()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;

			var exitHeader = Factory.New<CusExitControlHeader>();
			var expectedCountry = NoVariableDefaultDataGroupingCodeCountry ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			NUnit.Framework.Assert.That(exitHeader.DataGrouping, NUnit.Framework.Is.EqualTo(expectedCountry), "DataGrouping when Parent is not JobDeclaration");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			exitHeader.CEH_ParentTableCode = declaration.TablePrefix;
			exitHeader.CEH_ParentID = declaration.PK;
			expectedCountry = NoVariableDefaultDataGroupingCodeCountry ?? Core.Constants.CountryCodes.France;
			NUnit.Framework.Assert.That(exitHeader.DataGrouping, NUnit.Framework.Is.EqualTo(expectedCountry), "DataGrouping when Parent is JobDeclaration");

			var shipment = Factory.New<ForwardingShipment>();
			exitHeader.CEH_ParentTableCode = shipment.TablePrefix;
			exitHeader.CEH_ParentID = shipment.PK;
			expectedCountry = NoVariableDefaultDataGroupingCodeCountry ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			NUnit.Framework.Assert.That(exitHeader.DataGrouping, NUnit.Framework.Is.EqualTo(expectedCountry), "DataGrouping when Parent is not JobDeclaration");
		}

		protected virtual ZString? NoVariableDefaultDataGroupingCodeCountry => null;

		[ExpectNoExceptions]
		public void TestCountryCode()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;

			var exitHeader = Factory.New<CusExitControlHeader>();
			NUnit.Framework.Assert.That(exitHeader.CountryCode, NUnit.Framework.Is.EqualTo(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), "CountryCode when Parent is not JobDeclaration");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			exitHeader.CEH_ParentTableCode = declaration.TablePrefix;
			exitHeader.CEH_ParentID = declaration.PK;
			NUnit.Framework.Assert.That(exitHeader.CountryCode, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.France).Using(CustomComparers.TypeComparison), "CountryCode when Parent is JobDeclaration");

			var shipment = Factory.New<ForwardingShipment>();
			exitHeader.CEH_ParentTableCode = shipment.TablePrefix;
			exitHeader.CEH_ParentID = shipment.PK;
			NUnit.Framework.Assert.That(exitHeader.CountryCode, NUnit.Framework.Is.EqualTo(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), "CountryCode when Parent is not JobDeclaration");
		}
	}
}
