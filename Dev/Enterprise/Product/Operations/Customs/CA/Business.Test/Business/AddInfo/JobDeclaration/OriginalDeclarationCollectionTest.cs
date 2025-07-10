using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(OriginalDeclarationCollection))]
	sealed class OriginalDeclarationCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OriginalDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK, JobMessageTypeList.Codes.B2Adjustments);
		}
	}
}
