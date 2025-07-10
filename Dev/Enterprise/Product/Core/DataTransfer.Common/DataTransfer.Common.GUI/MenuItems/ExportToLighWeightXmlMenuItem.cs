using System;
using CargoWise.EntityFramework;
using DataTransfer.Common.GUI.MenuItems;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DataTransfer.GUI.MenuItems
{
	public class ExportToLighWeightXmlMenuItem<T> : ExportToXmlMenuItem<T> where T : BusinessObject
	{
		public ExportToLighWeightXmlMenuItem(Func<IXmlDataTransferExporter> getDirector, T businessObject)
			: base(getDirector, businessObject)
		{
		}

		public ExportToLighWeightXmlMenuItem(Func<IXmlDataTransferExporter> director, params T[] businessObjects)
			: base(director, businessObjects)
		{
		}

		public override MultilingualString Text
		{
			get
			{
				return ExportXmlMenuItemHelper.LightWeightMenuItemText;
			}
		}

		public override string Key
		{
			get { return ExportXmlMenuItemHelper.LightWeightMenuItemName; }
		}

		public override EventHandler OnClick
		{
			get
			{
				return LightWeighHandler((s, e) => GetExporter().PromptUserAndExport(businessObjects));
			}
		}
	}
}