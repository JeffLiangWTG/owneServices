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
	[TestedType(typeof(LocalTransportCompanyBrandingControl))]
	sealed class LocalTransportCompanyBrandingControlTest : RegistryZUserControlTestCase
	{
		#region Implementation

		T GetControl<T>(LocalTransportCompanyBrandingControl control, string name)
			where T : Control
		{
			return (T)typeof(LocalTransportCompanyBrandingControl).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control);
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new LocalTransportCompanyBrandingCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			collection.AddNew();
			return collection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var brandingControl = (LocalTransportCompanyBrandingControl)control;
			var grid = GetControl<ZGrid>(brandingControl, "BrandingGrid");
			var imageSelection = GetControl<ImageSelectionControl>(brandingControl, "ImageSelectionControl");

			return grid.ReadOnly && imageSelection.ReadOnly;
		}

		#endregion
	}
}
