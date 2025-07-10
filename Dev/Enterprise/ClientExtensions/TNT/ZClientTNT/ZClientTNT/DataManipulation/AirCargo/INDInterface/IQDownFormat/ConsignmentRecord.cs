using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.TNT
{
	public class ConsignmentRecord : IQDownBaseRecord
	{
		public ConsignmentRecord(ZString line)
			: base(line, 57)
		{
		}

		#region Schema

		public static class Schema
		{
			public static readonly FixedWidthFlatFileFieldProperty RecordType = new FixedWidthFlatFileFieldProperty(0, 0, 2);       //	Record Type Specifier must be '03'
			public static readonly FixedWidthFlatFileFieldProperty HouseBill = new FixedWidthFlatFileFieldProperty(1, 2, 10);   //	House Bill Number
			public static readonly FixedWidthFlatFileFieldProperty Origin = new FixedWidthFlatFileFieldProperty(2, 12, 3);  //	Port of Origin
			public static readonly FixedWidthFlatFileFieldProperty Destination = new FixedWidthFlatFileFieldProperty(3, 15, 3); //	Port of Destination
			public static readonly FixedWidthFlatFileFieldProperty ConsignorLegacyCode = new FixedWidthFlatFileFieldProperty(4, 18, 8); //	Consignor Account
			public static readonly FixedWidthFlatFileFieldProperty ConsignorName = new FixedWidthFlatFileFieldProperty(5, 26, 31);  //	Consignor Name
			public static readonly FixedWidthFlatFileFieldProperty ConsignorAddress1 = new FixedWidthFlatFileFieldProperty(6, 57, 31);  //	Consignor Addr1
			public static readonly FixedWidthFlatFileFieldProperty ConsignorAddress2 = new FixedWidthFlatFileFieldProperty(7, 88, 31);  //	Consignor Addr2
			public static readonly FixedWidthFlatFileFieldProperty ConsignorCity = new FixedWidthFlatFileFieldProperty(8, 119, 31); //	Consignor City
			public static readonly FixedWidthFlatFileFieldProperty ConsignorState = new FixedWidthFlatFileFieldProperty(9, 150, 31);    //	Consignor State
			public static readonly FixedWidthFlatFileFieldProperty ConsignorCountry = new FixedWidthFlatFileFieldProperty(10, 181, 3);  //	Consignor Country
			public static readonly FixedWidthFlatFileFieldProperty ConsignorPostCode = new FixedWidthFlatFileFieldProperty(11, 184, 9); //	Consignor ZIP/Post
			public static readonly FixedWidthFlatFileFieldProperty ConsignorPhone = new FixedWidthFlatFileFieldProperty(12, 193, 12);   //	Consignor Telephone
			public static readonly FixedWidthFlatFileFieldProperty ConsignorContactPhone = new FixedWidthFlatFileFieldProperty(13, 205, 12);    //	Sender Contact Tel
			public static readonly FixedWidthFlatFileFieldProperty ConsignorContactName = new FixedWidthFlatFileFieldProperty(14, 217, 22); //	Sender Contact name
			public static readonly FixedWidthFlatFileFieldProperty PickupName = new FixedWidthFlatFileFieldProperty(15, 239, 31);   //	Pickup Name
			public static readonly FixedWidthFlatFileFieldProperty PickupAddress1 = new FixedWidthFlatFileFieldProperty(16, 270, 31);   //	Pickup Addr1
			public static readonly FixedWidthFlatFileFieldProperty PickupAddress2 = new FixedWidthFlatFileFieldProperty(17, 301, 31);   //	Pickup Addr2
			public static readonly FixedWidthFlatFileFieldProperty PickupCity = new FixedWidthFlatFileFieldProperty(18, 332, 31);   //	Pickup City
			public static readonly FixedWidthFlatFileFieldProperty PickupState = new FixedWidthFlatFileFieldProperty(19, 363, 31);  //	Pickup State
			public static readonly FixedWidthFlatFileFieldProperty PickupCountry = new FixedWidthFlatFileFieldProperty(20, 394, 3); //	Pickup Country
			public static readonly FixedWidthFlatFileFieldProperty PickupPostCode = new FixedWidthFlatFileFieldProperty(21, 397, 9);    //	Pickup ZIP/Post
			public static readonly FixedWidthFlatFileFieldProperty PickupPhone = new FixedWidthFlatFileFieldProperty(22, 406, 12);  //	Pickup Telephone
			public static readonly FixedWidthFlatFileFieldProperty PickupContactPhone = new FixedWidthFlatFileFieldProperty(23, 418, 12);   //	Pickup Contact Tel
			public static readonly FixedWidthFlatFileFieldProperty PickupContactName = new FixedWidthFlatFileFieldProperty(24, 430, 22);    //	Pickup Contact name
			public static readonly FixedWidthFlatFileFieldProperty ConsigneeName = new FixedWidthFlatFileFieldProperty(25, 452, 31);    //	Consignee Name
			public static readonly FixedWidthFlatFileFieldProperty ConsigneeAddress1 = new FixedWidthFlatFileFieldProperty(26, 483, 31);    //	Consignee Addr1
			public static readonly FixedWidthFlatFileFieldProperty ConsigneeAddress2 = new FixedWidthFlatFileFieldProperty(27, 514, 31);    //	Consignee Addr2
			public static readonly FixedWidthFlatFileFieldProperty ConsigneeCity = new FixedWidthFlatFileFieldProperty(28, 545, 31);    //	Consignee City
			public static readonly FixedWidthFlatFileFieldProperty ConsigneeState = new FixedWidthFlatFileFieldProperty(29, 576, 31);   //	Consignee State
			public static readonly FixedWidthFlatFileFieldProperty ConsigneeCountry = new FixedWidthFlatFileFieldProperty(30, 607, 3);  //	Consignee Country
			public static readonly FixedWidthFlatFileFieldProperty ConsigneePostCode = new FixedWidthFlatFileFieldProperty(31, 610, 9); //	Consignee ZIP/Post
			public static readonly FixedWidthFlatFileFieldProperty ConsigneePhone = new FixedWidthFlatFileFieldProperty(32, 619, 12);   //	Consignee Telephone
			public static readonly FixedWidthFlatFileFieldProperty ConsigneeContactPhone = new FixedWidthFlatFileFieldProperty(33, 631, 12);    //	Consignee Contact Tel
			public static readonly FixedWidthFlatFileFieldProperty ConsigneeContactName = new FixedWidthFlatFileFieldProperty(34, 643, 22); //	Consignee Contact name
			public static readonly FixedWidthFlatFileFieldProperty DeliveryName = new FixedWidthFlatFileFieldProperty(35, 665, 31); //	Delivery Name
			public static readonly FixedWidthFlatFileFieldProperty DeliveryAddress1 = new FixedWidthFlatFileFieldProperty(36, 696, 31); //	Delivery Addr1
			public static readonly FixedWidthFlatFileFieldProperty DeliveryAddress2 = new FixedWidthFlatFileFieldProperty(37, 727, 31); //	Delivery Addr2
			public static readonly FixedWidthFlatFileFieldProperty DeliveryCity = new FixedWidthFlatFileFieldProperty(38, 758, 31); //	Delivery City
			public static readonly FixedWidthFlatFileFieldProperty DeliveryState = new FixedWidthFlatFileFieldProperty(39, 789, 31);    //	Delivery State
			public static readonly FixedWidthFlatFileFieldProperty DeliveryCountry = new FixedWidthFlatFileFieldProperty(40, 820, 3);   //	Delivery Country
			public static readonly FixedWidthFlatFileFieldProperty DeliveryPostCode = new FixedWidthFlatFileFieldProperty(41, 823, 9);  //	Delivery ZIP/Post
			public static readonly FixedWidthFlatFileFieldProperty DeliveryPhone = new FixedWidthFlatFileFieldProperty(42, 832, 12);    //	Delivery Telephone
			public static readonly FixedWidthFlatFileFieldProperty DeliveryContactPhone = new FixedWidthFlatFileFieldProperty(43, 844, 12); //	Delivery Contact Tel
			public static readonly FixedWidthFlatFileFieldProperty DeliveryContactName = new FixedWidthFlatFileFieldProperty(44, 856, 22);  //	Delivery Contact name
			public static readonly FixedWidthFlatFileFieldProperty DocumentIndicator = new FixedWidthFlatFileFieldProperty(45, 878, 1); //	Document Indicator ('N'on-docs,	'D'ocs)
			public static readonly FixedWidthFlatFileFieldProperty TermsOfPayment = new FixedWidthFlatFileFieldProperty(46, 879, 1);    //	terms of payment ('S'=Prepaid or 'R'=Collect)
			public static readonly FixedWidthFlatFileFieldProperty GoodsValue = new FixedWidthFlatFileFieldProperty(47, 880, 13);   //	Goods Value 10.2
			public static readonly FixedWidthFlatFileFieldProperty GoodsCurrency = new FixedWidthFlatFileFieldProperty(48, 893, 3); //	Goods Currency
			public static readonly FixedWidthFlatFileFieldProperty PackageCount = new FixedWidthFlatFileFieldProperty(49, 896, 6);  //	Total Package Count
			public static readonly FixedWidthFlatFileFieldProperty Weight = new FixedWidthFlatFileFieldProperty(50, 902, 10);   //	Weight 6.3
			public static readonly FixedWidthFlatFileFieldProperty TDoc = new FixedWidthFlatFileFieldProperty(51, 912, 9);  //	T-DOC
			public static readonly FixedWidthFlatFileFieldProperty Remarks = new FixedWidthFlatFileFieldProperty(52, 921, 40);  //	Remarks
			public static readonly FixedWidthFlatFileFieldProperty ConsigneeTelex = new FixedWidthFlatFileFieldProperty(53, 961, 12);   //	Consignee Telex
			public static readonly FixedWidthFlatFileFieldProperty ConsigneeFax = new FixedWidthFlatFileFieldProperty(54, 973, 11); //	Consignee Fax
			public static readonly FixedWidthFlatFileFieldProperty space = new FixedWidthFlatFileFieldProperty(55, 984, 15);    //	space
			public static readonly FixedWidthFlatFileFieldProperty RecordDelimiter = new FixedWidthFlatFileFieldProperty(56, 999, 1);   //	Record delimiter must be '.'
		}

		#endregion

		#region Field Property

		public ZString RecordType
		{
			get { return this[Schema.RecordType.Name]; }
		}

		public virtual ZString HouseBill
		{
			get { return this[Schema.HouseBill.Name]; }
			set { this[Schema.HouseBill.Name] = value; }
		}

		public virtual ZString Origin
		{
			get { return this[Schema.Origin.Name]; }
			set { this[Schema.Origin.Name] = value; }
		}

		public virtual ZString Destination
		{
			get { return this[Schema.Destination.Name]; }
			set { this[Schema.Destination.Name] = value; }
		}

		public virtual ZString ConsignorLegacyCode
		{
			get { return this[Schema.ConsignorLegacyCode.Name]; }
			set { this[Schema.ConsignorLegacyCode.Name] = value; }
		}

		public virtual ZString ConsignorName
		{
			get { return this[Schema.ConsignorName.Name]; }
			set { this[Schema.ConsignorName.Name] = value; }
		}

		public virtual ZString ConsignorAddress1
		{
			get { return this[Schema.ConsignorAddress1.Name]; }
			set { this[Schema.ConsignorAddress1.Name] = value; }
		}

		public virtual ZString ConsignorAddress2
		{
			get { return this[Schema.ConsignorAddress2.Name]; }
			set { this[Schema.ConsignorAddress2.Name] = value; }
		}

		public virtual ZString ConsignorCity
		{
			get { return this[Schema.ConsignorCity.Name]; }
			set { this[Schema.ConsignorCity.Name] = value; }
		}

		public virtual ZString ConsignorState
		{
			get { return this[Schema.ConsignorState.Name]; }
			set { this[Schema.ConsignorState.Name] = value; }
		}

		public virtual ZString ConsignorCountry
		{
			get { return this[Schema.ConsignorCountry.Name]; }
			set { this[Schema.ConsignorCountry.Name] = value; }
		}

		public virtual ZString ConsignorPostCode
		{
			get { return this[Schema.ConsignorPostCode.Name]; }
			set { this[Schema.ConsignorPostCode.Name] = value; }
		}

		public virtual ZString ConsignorPhone
		{
			get { return this[Schema.ConsignorPhone.Name]; }
			set { this[Schema.ConsignorPhone.Name] = value; }
		}

		public virtual ZString ConsignorContactPhone
		{
			get { return this[Schema.ConsignorContactPhone.Name]; }
			set { this[Schema.ConsignorContactPhone.Name] = value; }
		}

		public virtual ZString ConsignorContactName
		{
			get { return this[Schema.ConsignorContactName.Name]; }
			set { this[Schema.ConsignorContactName.Name] = value; }
		}

		public virtual ZString PickupName
		{
			get { return this[Schema.PickupName.Name]; }
			set { this[Schema.PickupName.Name] = value; }
		}

		public virtual ZString PickupAddress1
		{
			get { return this[Schema.PickupAddress1.Name]; }
			set { this[Schema.PickupAddress1.Name] = value; }
		}

		public virtual ZString PickupAddress2
		{
			get { return this[Schema.PickupAddress2.Name]; }
			set { this[Schema.PickupAddress2.Name] = value; }
		}

		public virtual ZString PickupCity
		{
			get { return this[Schema.PickupCity.Name]; }
			set { this[Schema.PickupCity.Name] = value; }
		}

		public virtual ZString PickupState
		{
			get { return this[Schema.PickupState.Name]; }
			set { this[Schema.PickupState.Name] = value; }
		}

		public virtual ZString PickupCountry
		{
			get { return this[Schema.PickupCountry.Name]; }
			set { this[Schema.PickupCountry.Name] = value; }
		}

		public virtual ZString PickupPostCode
		{
			get { return this[Schema.PickupPostCode.Name]; }
			set { this[Schema.PickupPostCode.Name] = value; }
		}

		public virtual ZString PickupPhone
		{
			get { return this[Schema.PickupPhone.Name]; }
			set { this[Schema.PickupPhone.Name] = value; }
		}

		public virtual ZString PickupContactPhone
		{
			get { return this[Schema.PickupContactPhone.Name]; }
			set { this[Schema.PickupContactPhone.Name] = value; }
		}

		public virtual ZString PickupContactName
		{
			get { return this[Schema.PickupContactName.Name]; }
			set { this[Schema.PickupContactName.Name] = value; }
		}

		public virtual ZString ConsigneeName
		{
			get { return this[Schema.ConsigneeName.Name]; }
			set { this[Schema.ConsigneeName.Name] = value; }
		}

		public virtual ZString ConsigneeAddress1
		{
			get { return this[Schema.ConsigneeAddress1.Name]; }
			set { this[Schema.ConsigneeAddress1.Name] = value; }
		}

		public virtual ZString ConsigneeAddress2
		{
			get { return this[Schema.ConsigneeAddress2.Name]; }
			set { this[Schema.ConsigneeAddress2.Name] = value; }
		}

		public virtual ZString ConsigneeCity
		{
			get { return this[Schema.ConsigneeCity.Name]; }
			set { this[Schema.ConsigneeCity.Name] = value; }
		}

		public virtual ZString ConsigneeState
		{
			get { return this[Schema.ConsigneeState.Name]; }
			set { this[Schema.ConsigneeState.Name] = value; }
		}

		public virtual ZString ConsigneeCountry
		{
			get { return this[Schema.ConsigneeCountry.Name]; }
			set { this[Schema.ConsigneeCountry.Name] = value; }
		}

		public virtual ZString ConsigneePostCode
		{
			get { return this[Schema.ConsigneePostCode.Name]; }
			set { this[Schema.ConsigneePostCode.Name] = value; }
		}

		public virtual ZString ConsigneePhone
		{
			get { return this[Schema.ConsigneePhone.Name]; }
			set { this[Schema.ConsigneePhone.Name] = value; }
		}

		public virtual ZString ConsigneeContactPhone
		{
			get { return this[Schema.ConsigneeContactPhone.Name]; }
			set { this[Schema.ConsigneeContactPhone.Name] = value; }
		}

		public virtual ZString ConsigneeContactName
		{
			get { return this[Schema.ConsigneeContactName.Name]; }
			set { this[Schema.ConsigneeContactName.Name] = value; }
		}

		public virtual ZString DeliveryName
		{
			get { return this[Schema.DeliveryName.Name]; }
			set { this[Schema.DeliveryName.Name] = value; }
		}

		public virtual ZString DeliveryAddress1
		{
			get { return this[Schema.DeliveryAddress1.Name]; }
			set { this[Schema.DeliveryAddress1.Name] = value; }
		}

		public virtual ZString DeliveryAddress2
		{
			get { return this[Schema.DeliveryAddress2.Name]; }
			set { this[Schema.DeliveryAddress2.Name] = value; }
		}

		public virtual ZString DeliveryCity
		{
			get { return this[Schema.DeliveryCity.Name]; }
			set { this[Schema.DeliveryCity.Name] = value; }
		}

		public virtual ZString DeliveryState
		{
			get { return this[Schema.DeliveryState.Name]; }
			set { this[Schema.DeliveryState.Name] = value; }
		}

		public virtual ZString DeliveryCountry
		{
			get { return this[Schema.DeliveryCountry.Name]; }
			set { this[Schema.DeliveryCountry.Name] = value; }
		}

		public virtual ZString DeliveryPostCode
		{
			get { return this[Schema.DeliveryPostCode.Name]; }
			set { this[Schema.DeliveryPostCode.Name] = value; }
		}

		public virtual ZString DeliveryPhone
		{
			get { return this[Schema.DeliveryPhone.Name]; }
			set { this[Schema.DeliveryPhone.Name] = value; }
		}

		public virtual ZString DeliveryContactPhone
		{
			get { return this[Schema.DeliveryContactPhone.Name]; }
			set { this[Schema.DeliveryContactPhone.Name] = value; }
		}

		public virtual ZString DeliveryContactName
		{
			get { return this[Schema.DeliveryContactName.Name]; }
			set { this[Schema.DeliveryContactName.Name] = value; }
		}

		public virtual ZString DocumentIndicator
		{
			get { return this[Schema.DocumentIndicator.Name]; }
			set { this[Schema.DocumentIndicator.Name] = value; }
		}

		public virtual ZString TermsOfPayment
		{
			get { return this[Schema.TermsOfPayment.Name]; }
			set { this[Schema.TermsOfPayment.Name] = value; }
		}

		public virtual ZDecimal GoodsValue
		{
			get { return GetFieldAsZDecimal(Schema.GoodsValue.Name, 2); }
			set { this[Schema.GoodsValue.Name] = value.ToString("##########.##"); }
		}

		public virtual ZString GoodsCurrency
		{
			get { return this[Schema.GoodsCurrency.Name]; }
			set { this[Schema.GoodsCurrency.Name] = value; }
		}

		public virtual ZShort PackageCount
		{
			get { return GetFieldAsZShort(Schema.PackageCount.Name); }
			set { this[Schema.PackageCount.Name] = value.ToString(); }
		}

		public virtual ZDecimal Weight
		{
			get { return RoundWeightTo2Decimal(GetFieldAsZDecimal(Schema.Weight.Name, 3)); }
			set { this[Schema.Weight.Name] = RoundWeightTo2Decimal(value).ToString("######.##"); }
		}

		ZDecimal RoundWeightTo2Decimal(ZDecimal value)
		{
			ZDecimal weightShowing3Decimal = ZArchitecture.Core.Utilities.Round(value, 3);
			ZDecimal weightShowing2Decimal = ZArchitecture.Core.Utilities.Round(value, 2);
			weightShowing3Decimal = (weightShowing2Decimal < weightShowing3Decimal) ? new ZDecimal(weightShowing2Decimal + 0.01m) : weightShowing2Decimal; // round up
			return weightShowing3Decimal;
		}

		public virtual ZString TDoc
		{
			get { return this[Schema.TDoc.Name]; }
			set { this[Schema.TDoc.Name] = value; }
		}

		public virtual ZString Remarks
		{
			get { return this[Schema.Remarks.Name]; }
			set { this[Schema.Remarks.Name] = value; }
		}

		public virtual ZString ConsigneeTelex
		{
			get { return this[Schema.ConsigneeTelex.Name]; }
			set { this[Schema.ConsigneeTelex.Name] = value; }
		}

		public virtual ZString ConsigneeFax
		{
			get { return this[Schema.ConsigneeFax.Name]; }
			set { this[Schema.ConsigneeFax.Name] = value; }
		}

		public ZString spaces
		{
			get { return this[Schema.space.Name]; }
		}

		public override ZString RecordDelimiter
		{
			get { return this[Schema.RecordDelimiter.Name]; }
		}

		public override ZString HumanReadable
		{
			get { return ZString.Format("Record {0} (Con Number={1})", RecordType, HouseBill); }
		}

		#endregion

		public void Merge(ConsignmentRecord record)
		{
			PackageCount += record.PackageCount;
			Weight += record.Weight;
			GoodsValue += record.GoodsValue;
		}

		#region Implementation

		protected override void FillFromRawData(ZString rawData)
		{
			SetProperty(Schema.RecordType, rawData);
			SetProperty(Schema.HouseBill, rawData);
			SetProperty(Schema.Origin, rawData);
			SetProperty(Schema.Destination, rawData);
			SetProperty(Schema.ConsignorLegacyCode, rawData);
			SetProperty(Schema.ConsignorName, rawData);
			SetProperty(Schema.ConsignorAddress1, rawData);
			SetProperty(Schema.ConsignorAddress2, rawData);
			SetProperty(Schema.ConsignorCity, rawData);
			SetProperty(Schema.ConsignorState, rawData);
			SetProperty(Schema.ConsignorCountry, rawData);
			SetProperty(Schema.ConsignorPostCode, rawData);
			SetProperty(Schema.ConsignorPhone, rawData);
			SetProperty(Schema.ConsignorContactPhone, rawData);
			SetProperty(Schema.ConsignorContactName, rawData);
			SetProperty(Schema.PickupName, rawData);
			SetProperty(Schema.PickupAddress1, rawData);
			SetProperty(Schema.PickupAddress2, rawData);
			SetProperty(Schema.PickupCity, rawData);
			SetProperty(Schema.PickupState, rawData);
			SetProperty(Schema.PickupCountry, rawData);
			SetProperty(Schema.PickupPostCode, rawData);
			SetProperty(Schema.PickupPhone, rawData);
			SetProperty(Schema.PickupContactPhone, rawData);
			SetProperty(Schema.PickupContactName, rawData);
			SetProperty(Schema.ConsigneeName, rawData);
			SetProperty(Schema.ConsigneeAddress1, rawData);
			SetProperty(Schema.ConsigneeAddress2, rawData);
			SetProperty(Schema.ConsigneeCity, rawData);
			SetProperty(Schema.ConsigneeState, rawData);
			SetProperty(Schema.ConsigneeCountry, rawData);
			SetProperty(Schema.ConsigneePostCode, rawData);
			SetProperty(Schema.ConsigneePhone, rawData);
			SetProperty(Schema.ConsigneeContactPhone, rawData);
			SetProperty(Schema.ConsigneeContactName, rawData);
			SetProperty(Schema.DeliveryName, rawData);
			SetProperty(Schema.DeliveryAddress1, rawData);
			SetProperty(Schema.DeliveryAddress2, rawData);
			SetProperty(Schema.DeliveryCity, rawData);
			SetProperty(Schema.DeliveryState, rawData);
			SetProperty(Schema.DeliveryCountry, rawData);
			SetProperty(Schema.DeliveryPostCode, rawData);
			SetProperty(Schema.DeliveryPhone, rawData);
			SetProperty(Schema.DeliveryContactPhone, rawData);
			SetProperty(Schema.DeliveryContactName, rawData);
			SetProperty(Schema.DocumentIndicator, rawData);
			SetProperty(Schema.TermsOfPayment, rawData);
			SetProperty(Schema.GoodsValue, rawData);
			SetProperty(Schema.GoodsCurrency, rawData);
			SetProperty(Schema.PackageCount, rawData);
			SetProperty(Schema.Weight, rawData);
			SetProperty(Schema.TDoc, rawData);
			SetProperty(Schema.Remarks, rawData);
			SetProperty(Schema.ConsigneeTelex, rawData);
			SetProperty(Schema.ConsigneeFax, rawData);
			SetProperty(Schema.space, rawData);
			SetProperty(Schema.RecordDelimiter, rawData);
		}

		public const string DateFormat = "ddMMyy";

		#endregion

	}
}
