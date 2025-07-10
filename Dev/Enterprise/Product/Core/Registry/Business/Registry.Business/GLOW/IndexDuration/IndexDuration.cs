using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Glow.Model.CW1.Resources;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class IndexDuration : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string Table = "Table";
			public const string DurationInMonths = "DurationInMonths";
		}

		#endregion

		public IndexDuration() { }

		#region Properties

		#region Caption

		public ZString Table
		{
			get => table;
			set
			{
				SetNonPersistentPropertyValue<ZString>(TableInfo, ref table, value);
				if (!IsValidationSuspended)
				{
					ValidateTable();
				}
			}
		}

		public ZPropertyInfo TableInfo => GetZPropertyInfo(IndexDuration.Schema.Table);

		ZString table;

		#endregion

		#region DurationInMonths

		public ZInt DurationInMonths
		{
			get => durationInMonths;
			set
			{
				SetNonPersistentPropertyValue<ZInt>(DurationInMonthsInfo, ref durationInMonths, value);
				if (!IsValidationSuspended)
				{
					ValidateDurationInMonths();
				}
			}
		}

		public CodeDescriptionPairList LowWatermarkTables
		{
			get
			{
				var tables = LowWatermarkTableList.TableList.Select(ele => new CodeDescriptionPair(ele, ele)).ToList();
				var result = new CodeDescriptionPairList();
				result.AddRange(tables);
				return result;
			}
		}

		public ZPropertyInfo DurationInMonthsInfo => GetZPropertyInfo(IndexDuration.Schema.DurationInMonths);

		ZInt durationInMonths;

		#endregion

		#endregion

		#region Validation
		public static ZString NegativeDuration => Res.GetString("4AB28E43-F5DD-4EC1-AF9F-AF54DA734BFD", "Please enter a non-negative duration.");
		public static ZString InvalidTable => Res.GetString("9979C30C-79A4-4090-BBB2-C882D37220DA", "Please select a table from the list.");

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateTable();
			ValidateDurationInMonths();
		}

		protected void ValidateTable()
		{
			TableInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TableInfo);
			if (!LowWatermarkTableList.TableList.Contains(Table.ToString()))
			{
				TableInfo.AddError(InvalidTable);
			}

			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(TableInfo);
		}

		protected void ValidateDurationInMonths()
		{
			DurationInMonthsInfo.ClearAllNotifications();
			if (DurationInMonths < 0)
			{
				DurationInMonthsInfo.AddError(NegativeDuration);
			}
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new IndexDuration();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var mapEntry = (IndexDuration)clone;
			mapEntry.DurationInMonths = DurationInMonths;
			mapEntry.Table = Table;
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(IndexDuration.Schema.Table, Table);
			writer.WriteElementString(IndexDuration.Schema.DurationInMonths, DurationInMonths.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Table = reader.ReadElementString(IndexDuration.Schema.Table);
			DurationInMonths = int.Parse(reader.ReadElementString(IndexDuration.Schema.DurationInMonths));
		}

		#endregion
	}
}
