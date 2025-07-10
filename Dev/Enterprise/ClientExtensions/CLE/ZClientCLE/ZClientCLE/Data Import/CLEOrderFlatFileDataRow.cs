using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.CLE.OrdersDataImport
{
	public class CLEOrderFlatFileDataRow : FlatFileDataRow
	{
		public CLEOrderFlatFileDataRow(FlatFileDataRow row, BusinessObjectFactory factory)
			: base(row)
		{
			this.factory = factory;
			Buyer = GetBuyer();
		}

		readonly OrgHeader Buyer;

		class Schema
		{
			//Purchase Order Header Details
			public const int Source = 0;
			public const int OrderNumber = 1;
			public const int RequiredExWorksDate = 2;
			public const int TransportMode = 3;
			public const int ContainerMode = 4;
			public const int OrderDate = 5;
			public const int Currency = 6;
			public const int TotalOrderAmount = 7;
			public const int INCOterm = 8;
			public const int SupplierCode = 9;
			public const int SupplierCompanyName = 10;
			public const int SupplierAddress1 = 11;
			public const int SupplierAddress2 = 12;
			public const int SupplierCity = 13;
			public const int SupplierState = 14;
			public const int SupplierPostCode = 15;
			public const int SupplierISOCountryCode = 16;
			public const int SupplierCountryName = 17;
			public const int SupplierUNLOCO = 18;
			public const int BuyerCode = 19;
			public const int BuyerCompanyName = 20;
			public const int BuyerAddress1 = 21;
			public const int BuyerAddress2 = 22;
			public const int BuyerCity = 23;
			public const int BuyerState = 24;
			public const int BuyerPostCode = 25;
			public const int BuyerISOCountryCode = 26;
			public const int BuyerCountryName = 27;
			public const int BuyerUNLOCO = 28;
			public const int LoadPort = 29;
			public const int DischargePort = 30;
			public const int ControllingPartyCode = 31;
			public const int Contact2 = 32;

			//Purchase Order Line Details
			public const int OrderLineNumber = 33;
			public const int ProductCode = 34;
			public const int ProductDescription = 35;
			public const int Quantity = 36;
			public const int UQ = 37;
			public const int TotalLinePrice = 38;
			public const int RequiredIntoStoreDate = 39;
			public const int DeliveryPoint = 40;
			public const int SpecialInstractions = 41;
			public const int CustomDate1 = 42;
			public const int CustomDate5 = 43;
			public const int CustomAttrib5 = 44;
			public const int CustomDecimal5 = 45;
			public const int CustomFlag5 = 46;
			public const int CustomText1 = 47;
			public const int EstimatedExFactoryDate = 48;
		}

		public Order LastOrderSplit
		{
			get
			{
				Order result = null;

				if (Buyer != null)
				{
					var orderHeaderFilter = new ZDBOnlyQuery(typeof(Order));
					orderHeaderFilter.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, OrderNumber);
					orderHeaderFilter.OrderBy = JobOrderHeaderSchema.JD_OrderNumberSplit.Name + " DESC";

					var buyerSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobOrderHeaderSchema.JD_OA_BuyerAddress);
					buyerSubQuery.AddToFilter(OrgAddressSchema.OA_OH, Buyer.PK);
					orderHeaderFilter.AddSubQuery(buyerSubQuery, JoinCondition.And);

					result = factory.LoadTop1<Order>(orderHeaderFilter);
				}

				return result;
			}
		}

		public OrderLine OrderLineFromDB
		{
			get
			{
				OrderLine result = null;

				if (Buyer != null )
				{
					var orderLineFilter = new ZDBOnlyQuery(typeof(OrderLine));

					var orderHeaderFilter = new ZDBOnlySubQuery(typeof(Order), JobOrderLineSchema.JO_JD);
					orderHeaderFilter.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, OrderNumber);

					var buyerAddressFilter = new ZDBOnlySubQuery(typeof(OrgAddress), JobOrderHeaderSchema.JD_OA_BuyerAddress);
					buyerAddressFilter.AddToFilter(OrgAddressSchema.OA_OH, Buyer.PK);
					orderHeaderFilter.AddSubQuery(buyerAddressFilter, JoinCondition.And);

					orderLineFilter.AddToFilter(JobOrderLineSchema.JO_LineNo, OrderLineNumber);
					orderLineFilter.AddSubQuery(orderHeaderFilter, JoinCondition.And);
					result = factory.LoadTop1<OrderLine>(orderLineFilter);
				}

				return result;
			}
		}

		public ZString DeliveryPoint
		{
			get { return GetField(Schema.DeliveryPoint);	}
		}

		OrgHeader GetBuyer()
		{
			OrgHeader result = null;
			OrgHeader orgForCodeMapping = OrgHeader.LoadFromForeignCode(factory, Source, GlbCompany.CurrentCompany.OrgProxy);

			if (orgForCodeMapping != null)
			{
				result = OrgHeader.LoadFromForeignCode(factory, BuyerCode, orgForCodeMapping);
			}
			return result;
		}

		public ZString DeliveryPointUNLOCO
		{
			get
			{
				if (fDeliveryPointUNLOCO.IsEmpty)
				{
					//Delivery point UNLOCO:
					//1. get importer through code-mapping from the file
					//2. on the importer get the UNLOCO by matching the DeliveryPoint field on the file to the Address Short Code on the Address tab. E.g. in the file is 20SYD.
					//3. if the UNLOCO is not defined on the address OR the DeliveryPoint is not defined in the file then use the Importer's UNLOCO.

					if (Buyer != null)
					{
						ZQuery addressFilter = new ZQuery(OrgAddressSchema.OA_Code, DeliveryPoint);
						OrgAddress[] codeMappedAddresses = (OrgAddress[])Buyer.Addresses.Find(addressFilter);
						if (codeMappedAddresses.Length > 0)
						{
							fDeliveryPointUNLOCO = codeMappedAddresses[0].OA_RL_NKRelatedPortCode;
						}

						if (fDeliveryPointUNLOCO.IsEmpty)
						{
							fDeliveryPointUNLOCO = Buyer.OH_RL_NKClosestPort;
						}
					}
				}

				return fDeliveryPointUNLOCO;
			}
		}
		ZString fDeliveryPointUNLOCO;

		readonly BusinessObjectFactory factory;

		public ZString Source
		{
			get { return GetField(Schema.Source); }
		}

		public ZString OrderNumber
		{
			get { return GetField(Schema.OrderNumber); }
		}

		public ZDateTime RequiredExWorksDate
		{
			get { return GetFieldAsZDateTime(Schema.RequiredExWorksDate, "yyyyMMdd"); }
		}

		public ZString TransportMode
		{
			get { return GetField(Schema.TransportMode); }
		}

		public ZString ContainerMode
		{
			get { return GetField(Schema.ContainerMode); }
		}

		public ZDateTime OrderDate
		{
			get { return GetFieldAsZDateTime(Schema.OrderDate, "yyyyMMdd"); }
		}

		public ZString Currency
		{
			get { return GetField(Schema.Currency); }
		}

		public ZDecimal TotalOrderAmount
		{
			get { return GetFieldAsZDecimal(Schema.TotalOrderAmount); }
		}

		public ZString INCOterm
		{
			get { return GetField(Schema.INCOterm); }
		}

		public ZString SupplierCode
		{
			get { return GetField(Schema.SupplierCode); }
		}

		public ZString SupplierCompanyName
		{
			get { return GetField(Schema.SupplierCompanyName); }
		}

		public ZString SupplierAddress1
		{
			get { return GetField(Schema.SupplierAddress1); }
		}

		public ZString SupplierAddress2
		{
			get { return GetField(Schema.SupplierAddress2); }
		}

		public ZString SupplierCity
		{
			get { return GetField(Schema.SupplierCity); }
		}

		public ZString SupplierState
		{
			get { return GetField(Schema.SupplierState); }
		}

		public ZString SupplierPostCode
		{
			get { return GetField(Schema.SupplierPostCode); }
		}

		public ZString SupplierISOCountryCode
		{
			get { return GetField(Schema.SupplierISOCountryCode); }
		}

		public ZString SupplierCountryName
		{
			get { return GetField(Schema.SupplierCountryName); }
		}

		public ZString SupplierUNLOCO
		{
			get { return GetField(Schema.SupplierUNLOCO); }
		}

		public ZString BuyerCode
		{
			get { return GetField(Schema.BuyerCode); }
		}

		public ZString BuyerCompanyName
		{
			get { return GetField(Schema.BuyerCompanyName); }
		}

		public ZString BuyerAddress1
		{
			get { return GetField(Schema.BuyerAddress1); }
		}

		public ZString BuyerAddress2
		{
			get { return GetField(Schema.BuyerAddress2); }
		}

		public ZString BuyerCity
		{
			get { return GetField(Schema.BuyerCity); }
		}

		public ZString BuyerState
		{
			get { return GetField(Schema.BuyerState); }
		}

		public ZString BuyerPostCode
		{
			get { return GetField(Schema.BuyerPostCode); }
		}

		public ZString BuyerISOCountryCode
		{
			get { return GetField(Schema.BuyerISOCountryCode); }
		}

		public ZString BuyerCountryName
		{
			get { return GetField(Schema.BuyerCountryName); }
		}

		public ZString BuyerUNLOCO
		{
			get { return GetField(Schema.BuyerUNLOCO); }
		}

		public ZString LoadPort
		{
			get { return GetField(Schema.LoadPort); }
		}

		public ZString DischargePort
		{
			get { return GetField(Schema.DischargePort); }
		}

		public ZString ControllingPartyCode
		{
			get { return GetField(Schema.ControllingPartyCode); }
		}

		public ZInt OrderLineNumber
		{
			get { return GetFieldAsZDecimal(Schema.OrderLineNumber).ToZInt(); }
		}

		public ZString ProductCode
		{
			get { return GetField(Schema.ProductCode); }
		}

		public ZString ProductDescription
		{
			get { return GetField(Schema.ProductDescription); }
		}

		public ZDecimal Quantity
		{
			get { return GetFieldAsZDecimal(Schema.Quantity); }
		}

		public ZString UQ
		{
			get { return GetField(Schema.UQ); }
		}

		public ZDecimal TotalLinePrice
		{
			get { return GetFieldAsZDecimal(Schema.TotalLinePrice); }
		}

		public ZDateTime RequiredIntoStoreDate
		{
			get { return GetFieldAsZDateTime(Schema.RequiredIntoStoreDate, "yyyyMMdd"); }
		}

		public ZString SpecialInstractions
		{
			get { return GetField(Schema.SpecialInstractions); }
		}

		public ZString Contact2
		{
			get { return GetField(Schema.Contact2); }
		}

		public ZDateTime CustomDate1
		{
			get { return GetFieldAsZDateTime(Schema.CustomDate1, "yyyyMMdd"); }
		}

		public ZDateTime CustomDate5
		{
			get { return GetFieldAsZDateTime(Schema.CustomDate5, "yyyyMMdd"); }
		}

		public ZString CustomAttrib5
		{
			get { return GetField(Schema.CustomAttrib5); }
		}

		public ZDecimal CustomDecimal5
		{
			get { return GetFieldAsZDecimal(Schema.CustomDecimal5); }
		}

		public ZString CustomFlag5
		{
			get { return GetField(Schema.CustomFlag5); }
		}

		public ZString CustomText1
		{
			get { return GetField(Schema.CustomText1); }
		}

		public ZDateTime EstimatedExFactoryDate
		{
			get { return GetFieldAsZDateTime(Schema.EstimatedExFactoryDate, "yyyyMMdd"); }
		}
	}
}
