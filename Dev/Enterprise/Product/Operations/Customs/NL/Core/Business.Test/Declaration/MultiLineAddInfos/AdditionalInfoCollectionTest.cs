using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(AdditionalInfoCollection))]
public class AdditionalInfoCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest() => new AdditionalInfoCollection(Factory.New<JobComInvoiceLine>());
}
