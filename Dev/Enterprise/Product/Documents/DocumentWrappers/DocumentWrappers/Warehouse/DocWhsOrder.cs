using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsOrder : DocWhsPickableDocket
	{
		#region Constructors

		protected DocWhsOrder(WhsOrder order, BusinessObjectFactory factoryToWrap)
			: base(order, factoryToWrap)
		{
		}

		protected DocWhsOrder(WhsDocketLabelControl docketLabel, BusinessObjectFactory factoryToWrap)
			: base(docketLabel, factoryToWrap)
		{
		}

		#endregion

		#region New

		public static DocWhsOrder New(WhsOrder order, BusinessObjectFactory factoryToWrap)
		{
			DocWhsOrder result = null;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(order, factoryToWrap);
			}
			else if (order != null)
			{
				result = new DocWhsOrder(order, factoryToWrap);
			}
			return result;
		}

		public static new DocWhsOrder New(WhsDocketLabelControl docketLabel, BusinessObjectFactory factoryToWrap)
		{
			return (docketLabel == null) ? null : ((docketLabel.Docket == null) ? null : new DocWhsOrder(docketLabel, factoryToWrap));
		}

		protected new delegate DocWhsOrder NewDelegate(WhsOrder order, BusinessObjectFactory factoryToWrap);

		protected new static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		#region Related Business Objects

		#region Order

		WhsOrder Order
		{
			get { return (WhsOrder)WrappedObject; }
		}

		#endregion

		#region RolledUpLinesForOrderCopy

		public DocWhsOrderLineCollection RolledUpLinesForOrderCopy
		{
			get
			{
				if (rolledUpLinesForOrderCopy == null)
				{
					rolledUpLinesForOrderCopy = GetRolledUpLinesForOrderCopy(Order.ParentLines);
					ZString propertyToSortBy = GetPropertyToSortBy();
					if (!propertyToSortBy.IsEmpty)
					{
						rolledUpLinesForOrderCopy.Sort(propertyToSortBy);
					}
				}
				return rolledUpLinesForOrderCopy;
			}
		}

		DocWhsOrderLineCollection rolledUpLinesForOrderCopy;

		#region GetRolledUpLinesForOrderCopy

		DocWhsOrderLineCollection GetRolledUpLinesForOrderCopy(WhsOrderLineCollection whsOrderLineCollection)
		{
			var result = new DocWhsOrderLineCollection(Factory);
			var docPickableDocketLinesDictionary = new Dictionary<string, DocWhsPickableDocketLine>();

			foreach (WhsOrderLine orderLine in whsOrderLineCollection)
			{
				string key = GetDictionaryKey(orderLine);
				DocWhsPickableDocketLine docLine;
				if (docPickableDocketLinesDictionary.TryGetValue(key, out docLine))
				{
					docLine.AddLineForRollUp(orderLine);
				}
				else
				{
					docLine = DocWhsPickableDocketLine.New(orderLine, Factory);
					result.Add(docLine);
					docPickableDocketLinesDictionary.Add(key, docLine);
				}
			}
			return result;
		}

		string GetDictionaryKey(WhsOrderLine line)
		{
			string result = "";
			string separator = "^";

			OrgHeader client = line.Docket.Client;
			if (client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(line.SupplierPart))
			{
				result += line.WE_OP;
				result += line.WE_PartAttrib1 + separator;
				result += line.WE_PartAttrib2 + separator;
				result += line.WE_PartAttrib3 + separator;
				result += line.WE_SerialNumber + separator;
				result += line.WE_ExpiryDate + separator;
				result += line.WE_PackingDate + separator;
				result += line.WE_LineComment;
			}
			else
			{
				result += line.PK; // do not roll up at all.
			}

			return result;
		}

		#endregion

		#endregion

		#endregion

		#region CartageDropMode

		public ZString CartageDropMode
		{
			get { return PickableDocketHelper.GetCartageDropModeFromWhsOrder(); }
		}

		WhsPickableDocketHelper PickableDocketHelper
		{
			get { return pickableDocketHelper ?? (pickableDocketHelper = new WhsPickableDocketHelper(WhsOrder)); }
		}

		WhsPickableDocketHelper pickableDocketHelper;

		#endregion

		#region Properties

		#region AutoPrintOrderCopyForMOP

		public ZBool AutoPrintOrderCopyForMOP
		{
			get { return Order.Warehouse != null ? Order.Warehouse.WW_AutoPrintOrderCopyForMOPOnPick : ZBool.False; }
		}

		#endregion

		#region IDocServicesParent Members Overrides

		protected override ZString GetPackages()
		{
			return Res.GetString("c96bd099-0d0d-481f-8284-3b2e6b228d67", "{0} Sent", Order.WD_PackagesSent);
		}

		protected override ZString GetWeight()
		{
			return Order.WD_WeightSent.ToString();
		}

		protected override ZString GetVolume()
		{
			return Order.WD_CubicSent.ToString();
		}

		#endregion

		#endregion
	}
}
