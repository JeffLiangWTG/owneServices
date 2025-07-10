using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class JPManifestDataObjectWriterHelper : UniversalCommonHelper
	{
		public JPManifestDataObjectWriterHelper(JPManifestDataObjectWriterHelper list)
			: this(list.factory)
		{
			this.list = list;
		}

		public JPManifestDataObjectWriterHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public JPManifestDataObjectWriterHelper(JPAFRHeader header)
			: this(header.Factory)
		{
			this.header = header;
		}

		public JPManifestDataObjectWriterHelper(JPAFRBills bill, JPManifestDataObjectWriterHelper list)
			: this(list)
		{
			this.bill = bill;
			this.header = bill.Header;
		}

		public JPManifestDataObjectWriterHelper(JPAFRBills bill)
			: this(bill.Factory)
		{
			this.bill = bill;
			this.header = bill.Header;
		}

		JPAFRHeader Header
		{
			get
			{
				var result = header;
				if (result == null && list != null)
				{
					result = list.Header;
				}
				return result;
			}
		}

		readonly JPAFRHeader header;

		JPAFRBills Bill
		{
			get
			{
				var result = bill;
				if (result == null && list != null)
				{
					result = list.Bill;
				}
				return result;
			}
		}

		readonly JPAFRBills bill;

		readonly protected JPManifestDataObjectWriterHelper list;

		#region List

		public ICodeDescriptionPairList ForwardingTransportTypeList
		{
			get { return factory.GetCachedCodeDescriptionPairList(OLookUpEditType.TransportType); }
		}

		public CustomsChargeTypeList CustomsChargeTypeList
		{
			get { return factory.GetCachedValue<CustomsChargeTypeList>(); }
		}

		public WayBillTypeList WayBillTypeList
		{
			get { return factory.GetCachedValue<WayBillTypeList>(); }
		}

		public Freight.Common.Business.BindToLists BindToLists
		{
			get { return Freight.Common.Business.BindToLists.GetCachedLists(factory); }
		}

		#endregion
	}
}
