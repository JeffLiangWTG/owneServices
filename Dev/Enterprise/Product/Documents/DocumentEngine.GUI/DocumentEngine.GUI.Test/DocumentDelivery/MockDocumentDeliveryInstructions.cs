using System;
using CargoWise.Types;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	[Serializable]
	class MockDocumentDeliveryInstructions : DeliveryInstructions
	{
		public MockDocumentDeliveryInstructions() { }

		public MockDocumentDeliveryInstructions(DocumentPack docPack)
			: base(docPack) { }

		public override ZBool IsDeliveringFormDocument => true;

		DeliverableCollectionView deliverables;
		protected override DeliverableCollectionView GetNewDeliverableCollectionView()
		{
			if (deliverables == null)
			{
				var dummy = Factory.New(typeof(DummyDocManagerTestBizO)) as DummyDocManagerTestBizO;
				dummy.SetupDocManagerObjects();
				var eDoc = (IDeliverable)((IDocManagerSupport)dummy).DocManagerInfo.Documents[0];
				var pack = new DocumentPack();
				pack.Add(eDoc);

				deliverables = new DeliverableCollectionView(pack, new DocDeliveryContactCollection(Factory));
			}
			return deliverables;
		}
	}
}
