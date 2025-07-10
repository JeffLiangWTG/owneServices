using System;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI
{
	public class ExcelExportCollectionColumn : ExcelExportColumnBase
	{
		public ExcelExportCollectionColumn(string description, ZString bindToCollection, ZString bindToField)
		{
			this.description = description;
			this.bindToCollection = bindToCollection;
			this.bindToField = bindToField;
		}

		#region Implementation

		readonly string description;
		readonly ZString bindToCollection;
		readonly ZString bindToField;

		protected override IZType GetValueForExportCore(BusinessObject bizObj)
		{
			try
			{
				PropertyInfo propertyInfo = bizObj.GetType().GetProperty(bindToCollection);
				if (propertyInfo != null)
				{
					BusinessObjectCollection collection = propertyInfo.GetValue(bizObj, null) as BusinessObjectCollection;
					if (collection != null)
					{
						return ArrayToTextConverter.ConvertToCommaSeparatedMultilineText(collection, bindToField);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("ExcelExportCollectionColumn - " + description,
					string.Format("Failed to get value from collection={0}, field ={1}", bindToCollection, bindToField),
					ex);
			}
			return ZString.Empty;
		}

		protected override string GetDescription()
		{
			return description;
		}

		public override CellFormat GetFormat(IZType value)
		{
			return new CellFormat();
		}

		#endregion
	}
}
