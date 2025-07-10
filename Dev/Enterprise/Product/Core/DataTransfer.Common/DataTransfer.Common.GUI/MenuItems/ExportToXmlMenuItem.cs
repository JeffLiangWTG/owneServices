using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace DataTransfer.Common.GUI.MenuItems
{
	public abstract class ExportToXmlMenuItem
	{
		// TODO: Should move to a helper class
		#region Handler
		public static EventHandler VerboseHandler(EventHandler handler)
		{
			return (s, e) =>
			{
				var originValue = SystemDataRegistry.Instance.SimpleXMLExportFormat.Value;
				SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				handler(s, e);
				SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originValue);
			};
		}

		public static EventHandler LightWeighHandler(EventHandler handler)
		{
			return (s, e) =>
			{
				var originValue = SystemDataRegistry.Instance.SimpleXMLExportFormat.Value;
				SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				handler(s, e);
				SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originValue);
			};
		}
		#endregion

		public abstract MultilingualString Text { get; }
		public abstract string Key { get; }
		public abstract EventHandler OnClick { get; }

		public static implicit operator System.Windows.Forms.MenuItem(ExportToXmlMenuItem menuItem)
		{
			var xmlMenuItem = new ZMenuItem(menuItem.Text, menuItem.OnClick);
			xmlMenuItem.Name = menuItem.Key;
			return xmlMenuItem;
		}
	}

	public abstract class ExportToXmlMenuItem<T> : ExportToXmlMenuItem where T : BusinessObject
	{
		public ExportToXmlMenuItem(Func<IXmlDataTransferExporter> getExporter, T businessObject)
		{
			GetExporter = getExporter;
			businessObjects = new[] { businessObject };
		}

		public ExportToXmlMenuItem(Func<IXmlDataTransferExporter> exporter, params T[] businessObjects)
		{
			this.GetExporter = exporter;
			this.businessObjects = businessObjects;
		}
		public readonly Func<IXmlDataTransferExporter> GetExporter;
		public readonly T[] businessObjects;
	}
}