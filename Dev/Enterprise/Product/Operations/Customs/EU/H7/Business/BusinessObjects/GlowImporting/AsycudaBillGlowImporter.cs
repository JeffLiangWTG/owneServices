using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaBillGlowImporter : IGlowCustomImporter
	{
		public void ConvertCustomLine(BusinessObject parent, string code, INotifications logger, int rowIndex, string propertyName)
		{
			var bill = parent as AsycudaBill ?? throw new ArgumentException("Expected a AsycudaBill", nameof(parent));
			var property = propertyName.Split('_');
			try
			{
				var columnName = property[property.Length - 2] + "_" + property[property.Length - 1];
				if (property[0] == "CusGoodsLocation")
				{
					var propertyInfo = bill.CusGoodsLocation.FindPropertyInfo(columnName);
					propertyInfo.SetValueFromString(code);
				}
			}
			catch
			{
				logger.AddError($"{propertyName} is not a valid property.");
			}
		}

		public bool ImportChildlessChildren(BusinessObject parent, INotifications logger, int rowIndex, string[] childHeaders, CargoWise.DataTransfer.ImportPreviewLineDetails[] childValues) => false;

		public bool IsEmptyValueAllowed(string propertyName) => false;

		bool IGlowCustomImporter.ShouldCustomizeChildrenImport => false;
	}
}
