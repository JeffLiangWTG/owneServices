using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing;

[TestedType(typeof(JobDeclaration.EUDeclarationDocManagerInfo))]
public class EUDeclarationDocManagerInfoTest : DeclarationDocManagerInfoTest
{
	public override BusinessObject GetEmptyParentBusinessObject() => Factory.New(typeof(JobDeclaration));

	protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

	public void TestRelatedObjectsCusExitDetailRetrieved()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
		exitHeader.CEH_Parent = jobDeclaration;
		var exitDetail = exitHeader.CusExitDetails.AddNew();
		Factory.Save();
		var relatedObjects = jobDeclaration.DocManagerInfo.RelatedObjects;
		CombineAssertions(() =>
		{
			Assert("exitDetail added", relatedObjects.Contains(exitDetail));
			AssertNotNull("DocManagerInfo of exitDetail", ((IDocManagerSupport)exitDetail).DocManagerInfo);
		});
	}

	public void TestRelatedObjectsCusExitReportRetrieved()
	{
		var declaration = Factory.New<JobDeclaration>();
		Factory.Save();
		var header = ExitControlTestHelper.CreateCusExitHeader(declaration);
		var consignment = ExitControlTestHelper.CreateCusExitConsignment(Factory, header.PK, header.CXH_ClusterKey, "AAAAA");
		var report = (BusinessObject)ExitControlTestHelper.CreateCusExitReport(Factory, header.PK, header.CXH_ClusterKey, consignment.PK, ExitReportTypeList.Codes.Presentation, "IEDUB100");
		Factory.Save();
		var relatedObjects = declaration.DocManagerInfo.RelatedObjects;
		CombineAssertions(() =>
		{
			Assert("exitDetail added", relatedObjects.Contains(report));
			AssertNotNull("DocManagerInfo of exitDetail", ((IDocManagerSupport)report).DocManagerInfo);
		});
	}

	public void TestIsInEuropeanCustomsUnionOrInheritsFromEU()
	{
		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			AssertIsInEuropeanCustomsUnionOrInheritsFromEU(Constants.CountryCodes.Belgium, true);
			AssertIsInEuropeanCustomsUnionOrInheritsFromEU(Constants.CountryCodes.Denmark, true);
			AssertIsInEuropeanCustomsUnionOrInheritsFromEU(Constants.CountryCodes.France, true);
			AssertIsInEuropeanCustomsUnionOrInheritsFromEU(Constants.CountryCodes.Reunion, true);

			AssertIsInEuropeanCustomsUnionOrInheritsFromEU(Constants.CountryCodes.Germany, true);
			AssertIsInEuropeanCustomsUnionOrInheritsFromEU(Constants.CountryCodes.Ireland, true);
			AssertIsInEuropeanCustomsUnionOrInheritsFromEU(Constants.CountryCodes.Italy, true);
			AssertIsInEuropeanCustomsUnionOrInheritsFromEU(Constants.CountryCodes.Netherlands, true);
			AssertIsInEuropeanCustomsUnionOrInheritsFromEU(Constants.CountryCodes.Poland, true);
			AssertIsInEuropeanCustomsUnionOrInheritsFromEU(Constants.CountryCodes.Spain, true);
			AssertIsInEuropeanCustomsUnionOrInheritsFromEU(Constants.CountryCodes.Sweden, true);
			AssertIsInEuropeanCustomsUnionOrInheritsFromEU(Constants.CountryCodes.Turkey, true);
			AssertIsInEuropeanCustomsUnionOrInheritsFromEU(Constants.CountryCodes.UnitedKingdom, true);

			AssertIsInEuropeanCustomsUnionOrInheritsFromEU(Constants.CountryCodes.Australia, false);
			AssertIsInEuropeanCustomsUnionOrInheritsFromEU(Constants.CountryCodes.UnitedStates, false);
		});

		void AssertIsInEuropeanCustomsUnionOrInheritsFromEU(string countryCode, bool expected)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				AssertEquals($"IsInEuropeanCustomsUnionOrInheritsFromEU for {countryCode}", expected, declaration.IsInEuropeanCustomsUnionOrInheritsFromEU);
			}
		}
	}
}
