using System;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Business.Common;

namespace Enterprise.Accounting.RulesEngine.Facts.Testing
{
	public class EnvironmentFactTest : TestCase
	{
		public void TestCurrentBranch()
		{
			var companyMock = new Mock<ICompany>();
			var branchMock = new Mock<IBranchFact>();
			var departmentMock = new Mock<IDepartmentFact>();

			var countryMock = new Mock<ICountry>();
			companyMock.Setup(x => x.Country).Returns(countryMock.Object);

			var branchPK = Guid.NewGuid();
			var branchCode = "AAA";
			branchMock.Setup(x => x.PK).Returns(branchPK);
			branchMock.Setup(x => x.Code).Returns(branchCode);

			var environmentFact = new EnvironmentFact(companyMock.Object, branchMock.Object, departmentMock.Object);

			AssertNotNull(environmentFact.CurrentBranch);
			AssertEquals(branchPK, environmentFact.CurrentBranch.Fact.PK);
			AssertEquals(branchCode, environmentFact.CurrentBranch.Fact.Code);
		}

		public void TestCurrentDepartment()
		{
			var companyMock = new Mock<ICompany>();
			var branchMock = new Mock<IBranchFact>();
			var departmentMock = new Mock<IDepartmentFact>();

			var countryMock = new Mock<ICountry>();
			companyMock.Setup(x => x.Country).Returns(countryMock.Object);

			var departmentPK = Guid.NewGuid();
			var departmentCode = "BBB";
			departmentMock.Setup(x => x.PK).Returns(departmentPK);
			departmentMock.Setup(x => x.Code).Returns(departmentCode);

			var environmentFact = new EnvironmentFact(companyMock.Object, branchMock.Object, departmentMock.Object);

			AssertNotNull(environmentFact.CurrentDepartment);
			AssertEquals(departmentPK, environmentFact.CurrentDepartment.Fact.PK);
			AssertEquals(departmentCode, environmentFact.CurrentDepartment.Fact.Code);
		}

		public void TestCurrentCompanyCountry()
		{
			var companyMock = new Mock<ICompany>();
			var branchMock = new Mock<IBranchFact>();
			var departmentMock = new Mock<IDepartmentFact>();

			var companyCountryCode = "XY";
			var countryMock = new Mock<ICountry>();
			countryMock.Setup(x => x.Code).Returns(companyCountryCode);

			companyMock.Setup(x => x.Country).Returns(countryMock.Object);

			var environmentFact = new EnvironmentFact(companyMock.Object, branchMock.Object, departmentMock.Object);

			AssertEquals(companyCountryCode, environmentFact.CurrentCompanyCountry);
		}
	}
}
