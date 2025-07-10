using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public class GridRowsDataObject : DataObject
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public GridRowsDataObject(ZGrid grid, ICollection<BusinessObject> selectedBusinessObjects)
		{
			Elements = GetNewSerializableBusinessObjectCollection();
			Elements.AddBaseBusinessObjects(selectedBusinessObjects);

			AsCsv = grid != null ? grid.GetSelectedRowsAsText(selectedBusinessObjects, true) : null; // Text is actually tabs separated (default delimiter in DataGrid), keep it because comma can be used in text values

			SetData(DataFormatType, Elements);
			SetData(typeof(ArrayList), null);

			if (AsCsv != null)
			{
				SetData(DataFormats.CommaSeparatedValue, null);
				SetData(DataFormats.Text, null);
			}
		}

		public WrappedBusinessObjectCollection Elements { get; private set; }

		public string AsCsv { get; private set; }

		public static string DataFormatType
		{
			get { return DataFormats.GetFormat(typeof(WrappedBusinessObjectCollection).FullName).Name; }
		}

		protected virtual WrappedBusinessObjectCollection GetNewSerializableBusinessObjectCollection()
		{
			return new WrappedBusinessObjectCollection();
		}

		public override object GetData(Type format)
		{
			if (format == typeof(ArrayList))
			{
				return Elements.AsArrayList();
			}
			else
			{
				return base.GetData(format);
			}
		}

		public override object GetData(string format)
		{
			if (format == DataFormats.CommaSeparatedValue || format == DataFormats.Text)
			{
				return AsCsv;
			}
			else
			{
				return base.GetData(format);
			}
		}
	}
}
