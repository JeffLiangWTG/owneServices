using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace DataTransfer.Common.GUI.MenuItems
{
	public class ExportToVerboseXmlMenuItem<T> : ExportToXmlMenuItem<T> where T : BusinessObject
	{
		public ExportToVerboseXmlMenuItem(Func<IXmlDataTransferExporter> getDirector, T businessObject)
			: base(getDirector, businessObject)
		{
		}

		public ExportToVerboseXmlMenuItem(Func<IXmlDataTransferExporter> director, params T[] businessObjects)
			: base(director, businessObjects)
		{
		}

		public override MultilingualString Text
		{
			get
			{
				return ExportXmlMenuItemHelper.VerboseMenuItemText;
			}
		}

		public override string Key
		{
			get { return ExportXmlMenuItemHelper.VerboseMenuItemName; }
		}

		public override EventHandler OnClick
		{
			get
			{
				return VerboseHandler((s, e) => GetExporter().PromptUserAndExport(businessObjects));
			}
		}
	}
}