using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;

namespace Enterprise.Accounting.DataTransfer
{
	public class CASSAdjustmentFileDataExporter : FlatFileDataExporter
	{
		public CASSAdjustmentFileDataExporter()
			: base(new BusinessObjectFactory())
		{
		}

		protected override IFlatFileConverter CreateConverter(INotifications notifications)
		{
			return new CASSAdjustmentFileConverter(notifications, Factory);
		}

		protected override IValueObjectDataAdapter DataAdapter
		{
			get
			{
				return new CASSAdjustmentFileAdapter();
			}
		}

		protected override BusinessObject LoadBusinessObjectForConversion(Type businessObjectType, BusinessObject bizObj)
		{
			return bizObj;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Only English Version is Expected")]
		public override ZString EnglishDescription
		{
			get { return "CASS Adjustment File Export"; }
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new CASSAdjustmentFileFormat(); }
		}
	}
}
