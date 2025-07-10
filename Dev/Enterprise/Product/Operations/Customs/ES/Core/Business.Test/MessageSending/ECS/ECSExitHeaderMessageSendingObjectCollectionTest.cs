using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageSending;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(ECSExitHeaderMessageSendingObjectCollection<ECSExitHeaderMessageSendingObject>))]
	public class ECSExitHeaderMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ECSExitHeaderMessageSendingObjectCollection<ECSExitHeaderMessageSendingObject>>
	{
		public virtual void TestAllowNewCore()
		{
			var testItem = new ECSExitHeaderMessageSendingObjectCollection<ECSExitHeaderMessageSendingObject>(Factory);
			Assert(!testItem.AllowNew);
		}

		#region Overrides of BusinessObjectCollectionBaseTestCase<BusinessObjectCollection>

		protected override ECSExitHeaderMessageSendingObjectCollection<ECSExitHeaderMessageSendingObject> GetCollectionToTest() => new ECSExitHeaderMessageSendingObjectCollection<ECSExitHeaderMessageSendingObject>(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ECSExitHeaderMessageSendingObject(Factory.New<CusExitDetail>());

		#endregion
	}
}
