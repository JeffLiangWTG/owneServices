using System;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	public class XsdSequenceIncrementHelper
	{
		public XsdSequenceIncrementHelper(ISequencedValueObjectCollection collection)
		{
			this.Collection = collection;
		}

		public void AssignNewIncrementedSequence(ISequencedValueObject item)
		{
			if (item.Sequence == 0)
			{
				int maxSequence = 0;

				foreach (Xsd.ISequencedValueObject existingItem in Collection)
				{
					if (existingItem.SequenceSpecified)
					{
						maxSequence = Math.Max(maxSequence, existingItem.Sequence);
					}
				}

				item.Sequence = maxSequence + 1;
				item.SequenceSpecified = true;
			}
		}

		#region Implementation

		readonly ISequencedValueObjectCollection Collection;

		#endregion
	}
}
