using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using CargoWise.Common.Testing;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	interface IGridControlWithoutColumnStylesSerialization { }

	[SuppressFormDesignerAnalysis]
	[ToolboxItem(false)]
	public class ZGridWithoutColumnStylesSerialisation : ZGrid, IGridControlWithoutColumnStylesSerialization
	{
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[SuppressWeaklyTypedCollectionMessage]
		public override ArrayList ColumnStyles
		{
			get
			{
				if (Site != null && Site.DesignMode)
				{
					var host = (IDesignerHost)GetService(typeof(IDesignerHost));
					if (!(host.RootComponent is IGridControlWithoutColumnStylesSerialization))
					{
						return new ArrayList();
					}
				}
				return base.ColumnStyles;
			}
		}

		bool ShouldSerializeColumnStyles()
		{
			return Parent == null || !(Parent is ZModuleButtonGrid || Parent.Parent is ZModuleButtonGrid);
		}
	}
}
