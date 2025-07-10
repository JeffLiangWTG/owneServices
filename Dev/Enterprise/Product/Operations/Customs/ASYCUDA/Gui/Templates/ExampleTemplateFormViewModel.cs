using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public class ExampleTemplateFormViewModel : NonPersistentBusinessObject, IPanelLayoutProvider
	{
		readonly PanelLayout layout1 = ExampleLayouts.CreateExampleLayout1();
		readonly PanelLayout layout2 = ExampleLayouts.CreateExampleLayout2();
		readonly PanelLayout layout3 = ExampleLayouts.CreateExampleLayout3();
		readonly PanelLayout layout4 = ExampleLayouts.CreateExampleLayout4();

		public ExampleTemplateFormViewModel(AsycudaManifestHeader manifestHeader) : base(manifestHeader.Factory)
		{
			ManifestHeader = manifestHeader;
			SelectLayout(true, layout1);
		}

		public ExampleTemplateFormViewModel() : base()
		{
			SelectLayout(true, layout1);
		}

		public AsycudaManifestHeader ManifestHeader { get; }

		PanelLayout Layout { get; set; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		public event EventHandler LayoutChanged;

		[BusinessObjectTestExclude]
		public ZBool Layout1Selected
		{
			get => Layout == layout1;
			set => SelectLayout(value, layout1);
		}

		public ZPropertyInfo Layout1SelectedInfo => GetZPropertyInfo(nameof(Layout1Selected));

		public ZBool Layout2Selected
		{
			get => Layout == layout2;
			set => SelectLayout(value, layout2);
		}

		public ZPropertyInfo Layout2SelectedInfo => GetZPropertyInfo(nameof(Layout2Selected));

		public ZBool Layout3Selected
		{
			get => Layout == layout3;
			set => SelectLayout(value, layout3);
		}

		public ZPropertyInfo Layout3SelectedInfo => GetZPropertyInfo(nameof(Layout3Selected));

		public ZBool Layout4Selected
		{
			get => Layout == layout4;
			set => SelectLayout(value, layout4);
		}

		public ZPropertyInfo Layout4SelectedInfo => GetZPropertyInfo(nameof(Layout4Selected));

		void SelectLayout(bool shouldSelect, PanelLayout newLayout)
		{
			if (!shouldSelect)
			{
				return;
			}

			if (Layout != newLayout)
			{
				Layout = newLayout;
				LayoutChanged?.Invoke(this, new EventArgs());

				Layout1SelectedInfo.RefreshBinding();
				Layout2SelectedInfo.RefreshBinding();
				Layout3SelectedInfo.RefreshBinding();
				Layout4SelectedInfo.RefreshBinding();
			}
		}
	}
}
