using System;
using System.ComponentModel;
using System.Drawing;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngineIntegration;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Excel
{
	[Serializable]
	public class CouldNotGetValueForExportException : Exception
	{
		public readonly string ColumnName;
		public readonly Type BusinessObjectType;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message not shown to user")]
		public CouldNotGetValueForExportException(Type businessObjectType, string columnName, Exception inner = null)
			: base("Could not retrieve the value for " + columnName, inner)
		{
			ColumnName = columnName;
			BusinessObjectType = businessObjectType;
		}

#if NETFRAMEWORK
		protected CouldNotGetValueForExportException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	public abstract class ExcelExportColumnBase
	{
		#region Construction

		protected ExcelExportColumnBase()
		{
		}

		protected ExcelExportColumnBase(IExcelExportCellComment commentSupport, IExcelExportCellColor colorSupport)
		{
			this.CommentSupport = commentSupport;
			this.ColorSupport = colorSupport;
		}

		#endregion Construction

		#region Description

		public string Description
		{
			get { return fDescription ?? (fDescription = GetDescription()); }
			set { fDescription = value; }
		}

		string fDescription;

		protected abstract string GetDescription();

		#endregion

		#region Value

		public IZType GetValueForExport(BusinessObject bizObj, ZGridColumnStyle columnStyle = null)
		{
			try
			{
				return GetValueForExportCore(bizObj);
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				if (columnStyle == null || PropertyDescriptor == null)
				{
					throw new CouldNotGetValueForExportException(bizObj.GetType(), GetDescription(), ex);
				}

				try
				{
					var value = PropertyDescriptor.GetValue(bizObj);
					return PropertyDescriptor.PropertyType == typeof(ZDecimal) ? new ZDecimal(value) : new ZString(columnStyle.FormatValueObject(bizObj, value));
				}
				catch (InvalidCastException exception)
				{
					throw new CouldNotGetValueForExportException(bizObj.GetType(), GetDescription(), exception);
				}
			}
		}

		protected abstract IZType GetValueForExportCore(BusinessObject bizObj);

		internal PropertyDescriptor PropertyDescriptor { get; set; }

		#endregion

		#region Format

		public abstract CellFormat GetFormat(IZType value);

		#endregion

		#region Color

		protected readonly IExcelExportCellColor ColorSupport;

		public virtual Color? GetColor(BusinessObject bizObj)
		{
			Color? result = null;
			if (ColorSupport != null)
			{
				result = ColorSupport.GetCustomColor(bizObj);
			}
			return result;
		}

		#endregion

		public int Width;

		#region Comment

		protected readonly IExcelExportCellComment CommentSupport;

		public virtual ZString GetComment(BusinessObject bizObj)
		{
			var result = ZString.Empty;
			if (CommentSupport != null)
			{
				result = CommentSupport.GetComment(bizObj);
			}
			return result;
		}

		#endregion

		protected string GetUnableToFormatValueErrorMessage(IZType value)
		{
			return Res.GetString("312d387e-a6fb-4af0-865e-4c1ae759f226", "The value '{0}' is not a valid type for formatting by an {1}.", value != null ? value.ToString() : "null", this.GetType().Name);
		}
	}
}
