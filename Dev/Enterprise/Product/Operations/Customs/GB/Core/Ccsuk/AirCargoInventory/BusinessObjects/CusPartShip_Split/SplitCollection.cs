using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class SplitCollection : BusinessObjectCollection<SplitConsignment>
	{
		public SplitCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public SplitCollection(ICcsukCusAwb awb)
			: base(awb.Factory)
		{
			this.awb = awb;
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			if (awb is CusHAWB)
			{
				return typeof(SplitHouse);
			}
			else if (awb is CusMAWB)
			{
				return typeof(SplitBasic);
			}
			else
			{
				return base.GetTypeOfElementsFromPK(pK);
			}
		}

		public SplitConsignment this[ZString splitReference]
		{
			get
			{
				return (from SplitConsignment split in Elements where split.SplitReference == splitReference select split).FirstOrDefault();
			}
		}

		protected override BusinessObject AddNewCore()
		{
			if (awb != null)
			{
				var split = base.AddNewCore(GetTypeOfElementsFromPK(ZGuid.Empty));
				var schemaColumn = awb is CusHAWB ? CusPartShipSchema.CG_CS : awb is CusMAWB ? CusPartShipSchema.CG_CM_LinkToPartMaster : null;
				split[schemaColumn.Name] = awb.PK;
				((SplitConsignment)split).AgentBadge = awb.AgentBadge;
				return split;
			}
			else
			{
				throw new NotSupportedException("You can't call AddNew if your collection was initialised with the SplitCollection(BusinessObjectFactory factory) ctor, only if you use the SplitCollection(ICcsukCusAwb awb) can you call AddNew().");
			}
		}

		protected override BusinessObject AddNewCore(Type bizOType)
		{
			if (awb != null)
			{
				var split = base.AddNewCore();
				var schemaColumn = awb is CusHAWB ? CusPartShipSchema.CG_CS : awb is CusMAWB ? CusPartShipSchema.CG_CM_LinkToPartMaster : null;
				split[schemaColumn.Name] = awb.PK;
				((SplitConsignment)split).AgentBadge = awb.AgentBadge;
				return split;
			}
			else
			{
				throw new NotSupportedException("You can't call AddNew if your collection was initialised with the SplitCollection(BusinessObjectFactory factory) ctor, only if you use the SplitCollection(ICcsukCusAwb awb) can you call AddNew().");
			}
		}

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);
			if (awb is CusMAWB)
			{
				if (businessObject is SplitBasic)
				{
					businessObject[CusPartShipSchema.CG_CM_LinkToPartMaster] = awb.PK;
				}
				else
				{
					throw new NotSupportedException("Can only add SplitBasic objects to a mawb/basic's Splits collection");
				}
			}
			else if (awb is CusHAWB)
			{
				if (businessObject is SplitHouse)
				{
					businessObject[CusPartShipSchema.CG_CS] = awb.PK;
				}
				else
				{
					throw new NotSupportedException("Can only add SplitHouse objects to a hawb's Splits collection");
				}
			}
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = base.CreateRelationshipFilter();
			SchemaGuidColumn col = awb is CusHAWB ? CusPartShipSchema.CG_CS : awb is CusMAWB ? CusPartShipSchema.CG_CM_LinkToPartMaster : null;
			if (col != null)
			{
				filter.AddToFilter(col, awb.PK);
			}
			else
			{
				throw new NotSupportedException("CusPartShipCollection only supports CusHAWB and CusMAWB, yours was " + awb == null ? "(null)" : awb.GetType().Name);
			}
			return filter;
		}

		readonly ICcsukCusAwb awb;
	}
}
