using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.YAS.Business.ProofOfDeliveryInterface
{
	public class PODDataRow : SharedFlatFileDataRow
	{
		public PODDataRow() : base(Schema.FieldCapacity) { }

		public PODDataRow(FlatFileDataRow dataRow) : base(dataRow) { }

		public abstract class Schema
		{
			public static readonly FlatFileFieldProperty AirwayBillNumber = new FlatFileFieldProperty(0, 20);
			public static readonly FlatFileFieldProperty SignedBy = new FlatFileFieldProperty(1, 80);
			public static readonly FlatFileFieldProperty Status = new FlatFileFieldProperty(2, 20);
			public static readonly FlatFileFieldProperty QuantityDelivered = new FlatFileFieldProperty(3, 10);
			public static readonly FlatFileFieldProperty Remarks = new FlatFileFieldProperty(4, 500);
			public static readonly FlatFileFieldProperty ContactName = new FlatFileFieldProperty(5, 80);
			public static readonly FlatFileFieldProperty ContactPhoneNumber = new FlatFileFieldProperty(6, 80);
			public static readonly FlatFileFieldProperty VehicleNumber = new FlatFileFieldProperty(7, 20);
			public static readonly FlatFileFieldProperty UserId = new FlatFileFieldProperty(8, 30);
			public static readonly FlatFileFieldProperty ActualDeliveryDate = new FlatFileFieldProperty(9, 16);

			internal const int FieldCapacity = 18;
		}

		protected override void AddFieldProperties()
		{
			FieldProperties.Add(Schema.AirwayBillNumber);
			FieldProperties.Add(Schema.SignedBy);
			FieldProperties.Add(Schema.Status);
			FieldProperties.Add(Schema.QuantityDelivered);
			FieldProperties.Add(Schema.Remarks);
			FieldProperties.Add(Schema.ContactName);
			FieldProperties.Add(Schema.ContactPhoneNumber);
			FieldProperties.Add(Schema.VehicleNumber);
			FieldProperties.Add(Schema.UserId);
			FieldProperties.Add(Schema.ActualDeliveryDate);
		}

		public ZString AirwayBillNumber
		{
			get { return GetField(Schema.AirwayBillNumber); }
			set { SetField(Schema.AirwayBillNumber, value); }
		}

		public ZString SignedBy
		{
			get { return GetField(Schema.SignedBy); }
			set { SetField(Schema.SignedBy, value); }
		}

		public ZString Status
		{
			get { return GetField(Schema.Status); }
			set { SetField(Schema.Status, value); }
		}

		public ZInt QuantityDelivered
		{
			get { return GetFieldAsZInt(Schema.QuantityDelivered); }
			set { SetField(Schema.QuantityDelivered, value); }
		}

		public ZString Remarks
		{
			get { return GetField(Schema.Remarks); }
			set { SetField(Schema.Remarks, value); }
		}

		public ZString ContactName
		{
			get { return GetField(Schema.ContactName); }
			set { SetField(Schema.ContactName, value); }
		}

		public ZString ContactPhoneNumber
		{
			get { return GetField(Schema.ContactPhoneNumber); }
			set { SetField(Schema.ContactPhoneNumber, value); }
		}

		public ZString VehicleNumber
		{
			get { return GetField(Schema.VehicleNumber); }
			set { SetField(Schema.VehicleNumber, value); }
		}

		public ZString UserId
		{
			get { return GetField(Schema.UserId); }
			set { SetField(Schema.UserId, value); }
		}

		public ZDateTime ActualDeliveryDate
		{
			get
			{
				ZDateTime result = GetFieldAsZDateTime(Schema.ActualDeliveryDate);
				if (!result.IsValid)
				{
					result = GetFieldAsZDateTime(PODDataRow.Schema.ActualDeliveryDate, dataDateFormat2);
				}
				if (!result.IsValid)
				{
					result = GetFieldAsZDateTime(PODDataRow.Schema.ActualDeliveryDate, dataDateFormat3);
				}
				return result;
			}
			set { SetField(Schema.ActualDeliveryDate, value); }
		}

		public override string DataDateFormat
		{
			get { return dataDateFormat; }
		}
		const string dataDateFormat = "yyyy/MM/dd HH:mm";
		const string dataDateFormat2 = "yyyy-MM-dd HH:mm:ss.ffff";
		const string dataDateFormat3 = "yyyy-MM-dd HHmmss.ffff";

		public override bool IsValid()
		{
			return this.FieldCount == Schema.FieldCapacity && !AirwayBillNumber.IsEmpty && Status.ToUpper() == "COMPLETED";
		}
	}
}
