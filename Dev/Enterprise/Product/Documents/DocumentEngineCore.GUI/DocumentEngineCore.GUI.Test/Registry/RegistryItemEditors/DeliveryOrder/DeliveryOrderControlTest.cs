using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	[TestedType(typeof(DeliveryOrderUserControl))]
	sealed class DeliveryOrderControlTest : RegistryZUserControlTestCase
	{
		#region Implementation

		T GetControl<T>(DeliveryOrderUserControl control, string name)
			where T : Control
		{
			return (T)typeof(DeliveryOrderUserControl).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control);
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			DeliveryOrderCollection collection = new DeliveryOrderCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			collection.AddNew();
			return collection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			DeliveryOrderUserControl deliveryOrderControl = (DeliveryOrderUserControl)control;
			ZGrid grid = GetControl<ZGrid>(deliveryOrderControl, "PrincipalGrid");
			ImageSelectionControl imageSelection = GetControl<ImageSelectionControl>(deliveryOrderControl, "ImageBoundImageSelectionControl");

			return grid.ReadOnly && imageSelection.ReadOnly;
		}

		#endregion
	}
}
