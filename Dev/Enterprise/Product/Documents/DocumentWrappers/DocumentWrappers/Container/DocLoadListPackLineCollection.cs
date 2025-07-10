using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public class DocLoadListPackLineCollection : DocumentWrapperCollection
	{
		public DocLoadListPackLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocLoadListPackLineCollection(DocBaseConsol consol, DocPackLinesCollection packLines, BusinessObjectFactory factory)
			: base(factory)
		{
			GetPackLines(consol, packLines, true);
		}

		public DocLoadListPackLineCollection(DocBaseConsol consol, DocPackLinesCollection packLines, bool mergeSimilarPackLines, BusinessObjectFactory factory)
			: base(factory)
		{
			GetPackLines(consol, packLines, mergeSimilarPackLines);
		}

		public new DocLoadListPackLine this[int index]
		{
			get { return (DocLoadListPackLine)base[index]; }
		}

		public void SortOnShipmentInterimReceipt()
		{
			this.Sort(new PackingOrderShipmentInterimReceiptComparer(false));
		}

		public void SortOnPackingOrderShipmentInterimReceipt()
		{
			this.Sort(new PackingOrderShipmentInterimReceiptComparer(true));
		}

		#region Implementation

		protected void GetPackLines(DocBaseConsol consol, DocPackLinesCollection packLines, bool mergeSimilarPackLines)
		{
			LoadListPackLineCollection loadListPackColl = new LoadListPackLineCollection(Factory);

			if (packLines != null)
			{
				Dictionary<ZString, LoadListPackLine> containerNumTable = new Dictionary<ZString, LoadListPackLine>();

				foreach (DocPackLines line in packLines)
				{
					if (line != null)
					{
						DocContainer container = line.ContainerForLoadList;
						ZString contShipNum = GetContainerAndShipmentNumber(line, container);
						if (IsContainerOnThisConsol(consol, container))
						{
							LoadListPackLine existingLine;

							if (!mergeSimilarPackLines || !containerNumTable.TryGetValue(contShipNum, out existingLine))
							{
								existingLine = new LoadListPackLine(line, container);
								loadListPackColl.Add(existingLine);
								if (mergeSimilarPackLines)
								{
									containerNumTable.Add(contShipNum, existingLine);
								}
							}
							else
							{
								if (existingLine.Shipment != null && line.Shipment != null &&
									existingLine.Shipment.ShipmentNumber == line.Shipment.ShipmentNumber)
								{
									existingLine.AmendExistingPackLine(line);
								}
								else
								{
									loadListPackColl.AddRange(line, container);
								}
							}
						}
					}
				}

				foreach (LoadListPackLine loadListLines in loadListPackColl)
				{
					this.Add(DocLoadListPackLine.New(loadListLines, Factory));
				}

				this.SortOnShipmentInterimReceipt();
			}
		}

		ZBool IsContainerOnThisConsol(DocBaseConsol consol, DocContainer container)
		{
			ZBool result = true; //Container != null;
			if (result && consol != null)
			{
				if (container != null && container.Consol != null && container.Consol.ConsolNumber != consol.ConsolNumber)
				{
					result = ZBool.False;
				}
			}
			return result;
		}

		ZString GetContainerAndShipmentNumber(DocPackLines line, DocContainer container)
		{
			ZString contShipNum = ZString.Empty;
			if (container != null)
			{
				contShipNum = container.ContainerNumber;
			}

			if (contShipNum.IsEmpty)
			{
				contShipNum = (NoResString)"NOT ASSIGNED";
			}

			if (line.Shipment != null)
			{
				contShipNum += line.Shipment.ShipmentNumber;
			}

			return contShipNum;
		}

		#endregion

		#region Packing Order & Shipment Interim Receipt Comparer

		public class PackingOrderShipmentInterimReceiptComparer : System.Collections.IComparer
		{
			public PackingOrderShipmentInterimReceiptComparer(bool sortOnPackingOrder)
			{
				this.sortOnPackingOrder = sortOnPackingOrder;
			}
			readonly bool sortOnPackingOrder;

			public int Compare(object x, object y)
			{
				int result = 0;
				if (x is DocLoadListPackLine && y is DocLoadListPackLine)
				{
					DocLoadListPackLine packA = (DocLoadListPackLine)x;
					DocLoadListPackLine packB = (DocLoadListPackLine)y;
					if (packA != null && packB != null)
					{
						if (sortOnPackingOrder)
						{
							result = packA.ContainerPackingOrder.CompareTo(packB.ContainerPackingOrder);
						}

						if (result == 0 && packA.Shipment != null && packB.Shipment != null)
						{
							result = packA.Shipment.InterimReceipt.CompareTo(packB.Shipment.InterimReceipt);
							if (result == 0)
							{
								result = packA.Shipment.ShipmentNumber.CompareTo(packB.Shipment.ShipmentNumber);
							}
						}
					}
				}

				return result;
			}
		}

		#endregion

	}
}
