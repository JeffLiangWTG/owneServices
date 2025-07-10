using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(JobDeclarationCollection))]
public class JobDeclarationCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<JobDeclaration>();
	protected override BusinessObjectCollection GetCollectionToTest() => new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
}
