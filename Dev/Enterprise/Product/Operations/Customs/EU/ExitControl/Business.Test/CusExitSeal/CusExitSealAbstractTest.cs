using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestsSubclassesOf(typeof(CusExitSeal))]
	public abstract class CusExitSealAbstractTest<TSeal, TContainer, THeader> : EnterpriseBusinessObjectTestCase
		where TSeal : CusExitSeal
		where TContainer : CusExitContainer
		where THeader : CusExitHeader
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).seal;
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		public static (TSeal seal, TContainer container, THeader header) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = CusExitHeaderAbstractTest<THeader>.GetNewBusinessObject(factory);
			var container = (TContainer)header.CusExitContainers.AddNew();
			var seal = (TSeal)container.AllSealNumbers.AddNew();
			seal.BK_SealNumber = seal.PK.ToString().Substring(0, CusExitSeal.Schema.BK_SealNumberMaxLength);
			return (seal, container, header);
		}
	}
}
