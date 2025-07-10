using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class OutturnLineCollection : NonPersistentBusinessObjectCollection<OutturnLine>
	{
		readonly Dictionary<string, OutturnLine> consignmentRefIndex;

		public OutturnLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			consignmentRefIndex = new Dictionary<string, OutturnLine>();
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (!(bizOAdded is OutturnLine))
			{
				throw new InvalidOperationException();
			}

			var line = (OutturnLine)bizOAdded;
			if (!consignmentRefIndex.ContainsKey(line.ConsignmentRef))
			{
				consignmentRefIndex.Add(line.ConsignmentRef, line);
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			if (!(bizO is OutturnLine))
			{
				throw new InvalidOperationException();
			}
			var line = (OutturnLine)bizO;
			if (consignmentRefIndex.ContainsKey(line.ConsignmentRef))
			{
				consignmentRefIndex.Remove(line.ConsignmentRef);
			}
		}

		public bool ContainsConsignmentRef(string consignmentRef)
		{
			return consignmentRefIndex.ContainsKey(consignmentRef);
		}

		public OutturnLine FindByConsignmentRef(string consignmentRef)
		{
			if (ContainsConsignmentRef(consignmentRef))
			{
				return consignmentRefIndex[consignmentRef];
			}
			else
			{
				return null;
			}
		}

		public OutturnLineCollection DeepCopy()
		{
			var newCollection = CreateNewOutturnLineCollection();

			foreach (var element in this)
			{
				newCollection.Add(((OutturnLine)element).DeepCopy());
			}

			return newCollection;
		}

		protected abstract OutturnLineCollection CreateNewOutturnLineCollection();

		public void ResetOutturnLineCountField()
		{
			foreach (var element in this)
			{
				((OutturnLine)element).Count = 0;
			}
		}
	}
}
