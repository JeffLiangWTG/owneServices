using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PutawaySequence : RegistryBusinessObjectTemplate, IPutawaySequence, IPutawaySequenceValidatorConsumer
	{
		PutawaySequenceValidator validator;

		public PutawaySequence()
		{
		}

		PutawaySequenceValidator Validator
		{
			get
			{
				if (validator == null)
				{
					validator = new PutawaySequenceValidator(this);
				}
				return validator;
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PutawaySequence();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ClientArea = new ZByte(reader.ReadElementString(Schema.ClientArea));
			Location = new ZByte(reader.ReadElementString(Schema.Location));
			PickFace = new ZByte(reader.ReadElementString(Schema.PickFace));
			ProductArea = new ZByte(reader.ReadElementString(Schema.ProductArea));

			Column = new ZByte(reader.ReadElementString(Schema.Column));
			Level = new ZByte(reader.ReadElementString(Schema.Level));
			Row = new ZByte(reader.ReadElementString(Schema.Row));
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateLocation();
			ValidatePickFace();
			ValidateClientArea();
			ValidateProductArea();

			ValidateRow();
			ValidateColumn();
			ValidateLevel();
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			Location = 1;
			PickFace = 0;
			ClientArea = 0;
			ProductArea = 0;

			Row = 1;
			Column = 2;
			Level = 3;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ClientArea, ClientArea.ToString());
			writer.WriteElementString(Schema.Location, Location.ToString());
			writer.WriteElementString(Schema.PickFace, PickFace.ToString());
			writer.WriteElementString(Schema.ProductArea, ProductArea.ToString());

			writer.WriteElementString(Schema.Column, Column.ToString());
			writer.WriteElementString(Schema.Level, Level.ToString());
			writer.WriteElementString(Schema.Row, Row.ToString());
		}

		#region Location Sort Order

		#region Row

		ZByte row;

		public ZByte Row
		{
			get { return row; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(RowInfo, ref row, value);
				if (!IsValidationSuspended)
				{
					ValidateRow();
				}
			}
		}

		public ZPropertyInfo RowInfo
		{
			get { return GetZPropertyInfo(Schema.Row); }
		}

		public void ValidateRow()
		{
			RowInfo.ClearAllNotifications();
			Validator.ValidateLocationSortOrder(RowInfo);
		}

		#endregion

		#region Column

		ZByte column;

		public ZByte Column
		{
			get { return column; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(ColumnInfo, ref column, value);
				if (!IsValidationSuspended)
				{
					ValidateColumn();
				}
			}
		}

		public ZPropertyInfo ColumnInfo
		{
			get { return GetZPropertyInfo(Schema.Column); }
		}

		public void ValidateColumn()
		{
			ColumnInfo.ClearAllNotifications();
			Validator.ValidateLocationSortOrder(ColumnInfo);
		}

		#endregion

		#region Level

		ZByte level;

		public ZByte Level
		{
			get { return level; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(LevelInfo, ref level, value);
				if (!IsValidationSuspended)
				{
					ValidateLevel();
				}
			}
		}

		public ZPropertyInfo LevelInfo
		{
			get { return GetZPropertyInfo(Schema.Level); }
		}

		public void ValidateLevel()
		{
			LevelInfo.ClearAllNotifications();
			Validator.ValidateLocationSortOrder(LevelInfo);
		}

		#endregion

		#endregion

		#region Putaway Algorithm Sequence

		#region Location

		ZByte location;

		public ZByte Location
		{
			get { return location; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(LocationInfo, ref location, value);
				if (!IsValidationSuspended)
				{
					ValidateLocation();
				}
			}
		}

		public ZPropertyInfo LocationInfo
		{
			get { return GetZPropertyInfo(Schema.Location); }
		}

		public void ValidateLocation()
		{
			LocationInfo.ClearAllNotifications();
			Validator.ValidatePutawayAlgorithmSequence(LocationInfo);
		}

		#endregion

		#region Pick Face

		ZByte pickFace;

		public ZByte PickFace
		{
			get { return pickFace; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(PickFaceInfo, ref pickFace, value);
				if (!IsValidationSuspended)
				{
					ValidatePickFace();
				}
			}
		}

		public ZPropertyInfo PickFaceInfo
		{
			get { return GetZPropertyInfo(Schema.PickFace); }
		}

		public void ValidatePickFace()
		{
			PickFaceInfo.ClearAllNotifications();
			Validator.ValidatePutawayAlgorithmSequence(PickFaceInfo);
		}

		#endregion

		#region Client Area

		ZByte clientArea;

		public ZByte ClientArea
		{
			get { return clientArea; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(ClientAreaInfo, ref clientArea, value);
				if (!IsValidationSuspended)
				{
					ValidateClientArea();
				}
			}
		}

		public ZPropertyInfo ClientAreaInfo
		{
			get { return GetZPropertyInfo(Schema.ClientArea); }
		}

		public void ValidateClientArea()
		{
			ClientAreaInfo.ClearAllNotifications();
			Validator.ValidatePutawayAlgorithmSequence(ClientAreaInfo);
		}

		#endregion

		#region Product Area

		ZByte productArea;

		public ZByte ProductArea
		{
			get { return productArea; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(ProductAreaInfo, ref productArea, value);
				if (!IsValidationSuspended)
				{
					ValidateProductArea();
				}
			}
		}

		public ZPropertyInfo ProductAreaInfo
		{
			get { return GetZPropertyInfo(Schema.ProductArea); }
		}

		public void ValidateProductArea()
		{
			ProductAreaInfo.ClearAllNotifications();
			Validator.ValidatePutawayAlgorithmSequence(ProductAreaInfo);
		}

		#endregion

		#endregion

		public static class Schema
		{
			public const string ClientArea = "ClientArea";
			public const string Location = "Location";
			public const string PickFace = "PickFace";
			public const string ProductArea = "ProductArea";

			public const string Column = "Column";
			public const string Level = "Level";
			public const string Row = "Row";
		}
	}
}
