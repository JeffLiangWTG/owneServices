using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Common.Testing
{
	class LandedCostHeaderFilterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestWithDeclaration()
		{
			var jobDeclarationWithLC = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			var landedCostHeader = (BusinessObject)Factory.New<Integration.LandedCosting.ILandedCostHeader>();
			landedCostHeader[LandedCostHeaderSchema.LT_ParentID.Name] = jobDeclarationWithLC.PK;

			var jobDeclarationWithoutLC = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();

			ZQuery filter = new LandedCostHeaderFilter(jobDeclarationWithLC as ILandedCostHeader);
			NUnit.Framework.Assert.That(filter.FetchOnlyFromLocalCache, Is.EqualTo(true));
			var lCHeaders = (BusinessObject[])Factory.Load<Integration.LandedCosting.ILandedCostHeader>(filter);
			NUnit.Framework.Assert.That(lCHeaders.Length, Is.EqualTo(1), "Only one of LC Headers should retrived");

			filter = new LandedCostHeaderFilter(jobDeclarationWithoutLC as ILandedCostHeader);
			NUnit.Framework.Assert.That(filter.FetchOnlyFromLocalCache, Is.EqualTo(true));
			lCHeaders = (BusinessObject[])Factory.Load<Integration.LandedCosting.ILandedCostHeader>(filter);
			NUnit.Framework.Assert.That(lCHeaders.Length, Is.EqualTo(0), "This declaration does not have LC attached");
		}
	}
}
