using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PickingSequence : RegistryBusinessObjectTemplate, IPickingSequence
	{
		#region Schema

		public static class Schema
		{
			public const string BrokenPallets = "BrokenPallets";
			public const string ConsolidatedPallets = "ConsolidatedPallets";
			public const string ExpiryDate = "ExpiryDate";
			public const string FifoOption = "FifoOption";
			public const string FifoFallback = "FifoFallback";
			public const string FifoBulkOnly = "FifoBulkOnly";
			public const string FullPallets = "FullPallets";
			public const string IsPickfaceEmptyPreventPickingFromBulk = "IsPickfaceEmptyPreventPickingFromBulk";
			public const string PalletOverflow = "PalletOverflow";
			public const string PickFaces = "PickFaces";
			public const string HighPriorityLocations = "HighPriorityLocations";

			public const string Column = "Column";
			public const string Level = "Level";
			public const string Row = "Row";
		}

		#endregion

		#region GetClone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PickingSequence();
		}

		#endregion

		#region ReadElements

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			BrokenPallets = new ZByte(reader.ReadElementString(Schema.BrokenPallets));
			ConsolidatedPallets = new ZByte(reader.ReadElementString(Schema.ConsolidatedPallets));
			ExpiryDate = new ZByte(reader.ReadElementString(Schema.ExpiryDate));
			FifoFallback = new ZByte(reader.ReadElementString(Schema.FifoFallback));
			FifoBulkOnly = new ZByte(reader.ReadElementString(Schema.FifoBulkOnly));

			FullPallets = new ZByte(reader.ReadElementString(Schema.FullPallets));
			PalletOverflow = new ZByte(reader.ReadElementString(Schema.PalletOverflow));
			PickFaces = new ZByte(reader.ReadElementString(Schema.PickFaces));
			HighPriorityLocations = new ZByte(reader.ReadElementString(Schema.HighPriorityLocations));

			Column = new ZByte(reader.ReadElementString(Schema.Column));
			Level = new ZByte(reader.ReadElementString(Schema.Level));
			Row = new ZByte(reader.ReadElementString(Schema.Row));
		}

		#endregion

		#region SetCustomDefaultValuesCore

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			BrokenPallets = 0;
			ConsolidatedPallets = 0;
			ExpiryDate = 0;

			HighPriorityLocations = 1;
			FifoFallback = 2;
			FifoBulkOnly = 0;

			FullPallets = 0;
			PalletOverflow = 0;
			PickFaces = 0;

			Column = 2;
			Level = 3;
			Row = 1;
		}

		#endregion

		#region WriteElements

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.BrokenPallets, BrokenPallets.ToString());
			writer.WriteElementString(Schema.ConsolidatedPallets, ConsolidatedPallets.ToString());
			writer.WriteElementString(Schema.ExpiryDate, ExpiryDate.ToString());
			writer.WriteElementString(Schema.FifoFallback, FifoFallback.ToString());
			writer.WriteElementString(Schema.FifoBulkOnly, FifoBulkOnly.ToString());
			writer.WriteElementString(Schema.FullPallets, FullPallets.ToString());
			writer.WriteElementString(Schema.PalletOverflow, PalletOverflow.ToString());
			writer.WriteElementString(Schema.PickFaces, PickFaces.ToString());
			writer.WriteElementString(Schema.HighPriorityLocations, HighPriorityLocations.ToString());

			writer.WriteElementString(Schema.Column, Column.ToString());
			writer.WriteElementString(Schema.Level, Level.ToString());
			writer.WriteElementString(Schema.Row, Row.ToString());
		}

		#endregion

		#region Location Sort Order

		#region Column

		ZByte column;

		public ZByte Column
		{
			get { return column; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(ColumnInfo, ref column, value);
			}
		}

		public ZPropertyInfo ColumnInfo
		{
			get { return GetZPropertyInfo(Schema.Column); }
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
			}
		}

		public ZPropertyInfo LevelInfo
		{
			get { return GetZPropertyInfo(Schema.Level); }
		}

		#endregion

		#region Row

		ZByte row;

		public ZByte Row
		{
			get { return row; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(RowInfo, ref row, value);
			}
		}

		public ZPropertyInfo RowInfo
		{
			get { return GetZPropertyInfo(Schema.Row); }
		}

		#endregion

		#endregion

		#region Pick Algorithm Sequence

		#region Broken Pallets

		public ZByte BrokenPallets
		{
			get { return brokenPallets; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(BrokenPalletsInfo, ref brokenPallets, value);
			}
		}
		ZByte brokenPallets;

		public ZPropertyInfo BrokenPalletsInfo
		{
			get { return GetZPropertyInfo(Schema.BrokenPallets); }
		}

		#endregion

		#region Consolidated Pallets

		public ZByte ConsolidatedPallets
		{
			get { return consolidatedPallets; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(ConsolidatedPalletsInfo, ref consolidatedPallets, value);
			}
		}
		ZByte consolidatedPallets;

		public ZPropertyInfo ConsolidatedPalletsInfo
		{
			get { return GetZPropertyInfo(Schema.ConsolidatedPallets); }
		}

		#endregion

		#region Expiry Date

		public ZByte ExpiryDate
		{
			get { return expiryDate; }
			set
			{
				SetNonPersistentPropertyValue(ExpiryDateInfo, ref expiryDate, value);
			}
		}
		ZByte expiryDate;

		public ZPropertyInfo ExpiryDateInfo
		{
			get { return GetZPropertyInfo(Schema.ExpiryDate); }
		}

		#endregion

		#region Fifo Option

		#region FifoOption

		public ZByte FifoOption
		{
			get { return FifoFallback; }
			set
			{
				FifoFallback = value;
				FifoOptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FifoOptionInfo
		{
			get { return GetZPropertyInfo(Schema.FifoOption, "FIFO Fallback"); } // FIFO Fallback is the Name shown on the registry item.
		}

		#endregion

		#region Fifo Bulk Only

		public ZByte FifoBulkOnly
		{
			get { return fifoBulkOnly; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(FifoBulkOnlyInfo, ref fifoBulkOnly, value);
			}
		}
		ZByte fifoBulkOnly;

		public ZPropertyInfo FifoBulkOnlyInfo
		{
			get { return GetZPropertyInfo(Schema.FifoBulkOnly, "FIFO Bulk Only"); }
		}

		#endregion

		#region FIFO Fallback

		public ZByte FifoFallback
		{
			get { return fifoFallback; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(FifoFallbackInfo, ref fifoFallback, value);
			}
		}
		ZByte fifoFallback;

		public ZPropertyInfo FifoFallbackInfo
		{
			get { return GetZPropertyInfo(Schema.FifoFallback, "FIFO Fallback"); }
		}

		#endregion

		#endregion

		#region Is Pickface Empty Prevent Picking From Bulk

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member PickfaceEmptyPreventPickingFromBulkReadOnly")]
		[ReadOnlyMember("PickfaceEmptyPreventPickingFromBulkReadOnly")]
		public ZBool IsPickfaceEmptyPreventPickingFromBulk
		{
			get
			{
				return false;
			}
		}

		public ZPropertyInfo IsPickfaceEmptyPreventPickingFromBulkInfo
		{
			get { return GetZPropertyInfo(Schema.IsPickfaceEmptyPreventPickingFromBulk, "Is Pickface Empty Prevent Picking From Bulk"); }
		}

		#endregion

		#region Full Pallets

		public ZByte FullPallets
		{
			get { return fullPallets; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(FullPalletsInfo, ref fullPallets, value);
			}
		}
		ZByte fullPallets;

		public ZPropertyInfo FullPalletsInfo
		{
			get { return GetZPropertyInfo(Schema.FullPallets); }
		}

		#endregion

		#region Pallet Overflow

		public ZByte PalletOverflow
		{
			get { return palletOverflow; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(PalletOverflowInfo, ref palletOverflow, value);
			}
		}
		ZByte palletOverflow;

		public ZPropertyInfo PalletOverflowInfo
		{
			get { return GetZPropertyInfo(Schema.PalletOverflow); }
		}

		#endregion

		#region Pick Faces

		public ZByte PickFaces
		{
			get { return pickFaces; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(PickFacesInfo, ref pickFaces, value);
			}
		}
		ZByte pickFaces;

		public ZPropertyInfo PickFacesInfo
		{
			get { return GetZPropertyInfo(Schema.PickFaces); }
		}

		#endregion

		#region High Priority Locations

		public ZByte HighPriorityLocations
		{
			get { return highPriorityLocations; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(HighPriorityLocationsInfo, ref highPriorityLocations, value);
			}
		}
		ZByte highPriorityLocations;

		public ZPropertyInfo HighPriorityLocationsInfo
		{
			get { return GetZPropertyInfo(Schema.HighPriorityLocations); }
		}

		#endregion

		#endregion
	}
}
