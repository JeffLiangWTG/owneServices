using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DeliveryMethods;

namespace Enterprise.DocumentVisualizer.Delivery
{
	[Serializable]
	sealed class DocumentDeliveryInstructions : DeliveryInstructions
	{
		public DocumentDeliveryInstructions(BusinessObjectFactory factory, DocumentPack pack, IReadOnlyCollection<IDocumentDelivery> documentDeliveries)
			: base(pack, new FactoryStrategy.PopulateButDoNotSave(factory))
		{
			Argument.NotNull(pack, nameof(pack));
			this.documentDeliveries = Argument.NotNull(documentDeliveries, nameof(documentDeliveries));
		}

		readonly IReadOnlyCollection<IDocumentDelivery> documentDeliveries;

		public override ZBool AllowModify => false;

		public override ZBool IsDeliveringFormDocument => true;

		protected override DeliverableCollectionView GetNewDeliverableCollectionView()
		{
			var deliverables = new DocumentDeliverableCollection();

			foreach (var documentDelivery in documentDeliveries)
			{
				deliverables.Add(new DocumentDeliverable(Factory, documentDelivery));
			}

			var view = new DeliverableCollectionView(deliverables, Recipients);
			view.Rebuild();
			return view;
		}
	}
}
