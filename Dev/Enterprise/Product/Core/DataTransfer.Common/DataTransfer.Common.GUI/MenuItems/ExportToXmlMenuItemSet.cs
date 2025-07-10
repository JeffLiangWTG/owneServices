using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using DataTransfer.Common.GUI.MenuItems;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DataTransfer.GUI.MenuItems
{
	public class ExportToXmlMenuItemSet<T> where T : BusinessObject
	{
		public ExportToXmlMenuItemSet(Func<IXmlDataTransferExporter> getDirector, T businessObject)
		{
			GetDirector = getDirector;
			businessObjects = new[] { businessObject };
		}

		public ExportToXmlMenuItemSet(Func<IXmlDataTransferExporter> director, params T[] businessObjects)
		{
			GetDirector = director;
			this.businessObjects = businessObjects;
		}
		public readonly Func<IXmlDataTransferExporter> GetDirector;
		public readonly T[] businessObjects;

		public static implicit operator List<MenuItem>(ExportToXmlMenuItemSet<T> menuItem)
		{
			var result = new List<MenuItem>();
			var xmlMenuItem = new ExportToVerboseXmlMenuItem<T>(menuItem.GetDirector, menuItem.businessObjects);
			result.Add(xmlMenuItem);
			var lightXmlMenuItem = new ExportToLighWeightXmlMenuItem<T>(menuItem.GetDirector, menuItem.businessObjects);
			result.Add(lightXmlMenuItem);
			return result;
		}
	}
}