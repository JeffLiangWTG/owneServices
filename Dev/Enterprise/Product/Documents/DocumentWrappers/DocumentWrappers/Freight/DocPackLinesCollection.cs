using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocPackLinesCollection : DocumentWrapperCollection
	{
		#region Construction

		public DocPackLinesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocPackLinesCollection(Enterprise.Freight.Business.PackLineCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public static DocPackLinesCollection New(BusinessObjectFactory factory)
		{
			DocPackLinesCollection result = null;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(factory);
			}
			else
			{
				result = new DocPackLinesCollection(factory);
			}
			return result;
		}

		protected delegate DocPackLinesCollection NewDelegate(BusinessObjectFactory factory);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		public new DocPackLines this[int index]
		{
			get { return (DocPackLines)base[index]; }
		}

		public void SortOnShipmentNumber()
		{
			Sort(new ShipmentPackLineComparer());
		}

		public void SortByHazardous()
		{
			Sort(new HazardousPackLineComparer());
		}
	}

	public class ShipmentPackLineComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			DocPackLines packLineA = (DocPackLines)x;
			DocPackLines packLineB = (DocPackLines)y;
			return packLineA.ShipmentNumber.CompareTo(packLineB.ShipmentNumber);
		}
	}

	public class HazardousPackLineComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			DocPackLines packLineA = (DocPackLines)x;
			DocPackLines packLineB = (DocPackLines)y;
			return packLineB.UNDGs.Length.CompareTo(packLineA.UNDGs.Length);
		}
	}
}


