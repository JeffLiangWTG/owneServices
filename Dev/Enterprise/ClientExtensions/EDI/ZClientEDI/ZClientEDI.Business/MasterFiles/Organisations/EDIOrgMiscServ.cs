using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Organisations.OrgHeader.FindBoxCollections;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgMiscServ : OrgMiscServ
	{
		public EDIOrgMiscServ(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		#region Header

		EDIOrgHeader fHeader;
		public new EDIOrgHeader Header
		{
			get
			{
				if (fHeader == null)
				{
					fHeader = Factory.Load<EDIOrgHeader>(OM_OH);
				}
				return fHeader;
			}
		}

		#endregion

		#region LocalTransports

		public new EDILocalTransportCollection LocalTransports
		{
			get
			{
				if (fLocalTransports == null)
				{
					fLocalTransports = new EDILocalTransportCollection(Factory);
				}
				return fLocalTransports;
			}
		}

		EDILocalTransportCollection fLocalTransports;

		#endregion

		#region Brokers
		public new EDIBrokerCollection Brokers
		{
			get
			{
				if (fBrokers == null)
				{
					fBrokers = new EDIBrokerCollection(Factory);
				}
				return fBrokers;
			}
		}

		EDIBrokerCollection fBrokers;

		#endregion

		protected override ZQuery WarehouseFilterQuery()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsCompetitor, ZBool.True);
			filter.AddToFilter(base.WarehouseFilterQuery(), JoinCondition.Or);
			return filter;
		}

		#region Validation

		protected override OrgMiscServValidation GetNewValidation()
		{
			return new EDIOrgMiscServValidation(this);
		}

		public new EDIOrgMiscServValidation Validation
		{
			get { return (EDIOrgMiscServValidation)base.Validation; }
		}

		#endregion

		#endregion

	}
}

