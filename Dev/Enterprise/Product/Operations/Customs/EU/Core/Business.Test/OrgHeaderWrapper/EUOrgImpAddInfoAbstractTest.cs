using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(EUOrgImpAddInfo))]
	public abstract class EUOrgImpAddInfoAbstractTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new EUOrgImpAddInfo(Factory);
	}
}
