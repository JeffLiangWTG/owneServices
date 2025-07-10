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
	[TestedType(typeof(PrincipalBrandingControl))]
	sealed class PrincipalBrandingControlTest : RegistryZUserControlTestCase
	{
		#region Implementation

		T GetControl<T>(PrincipalBrandingControl control, string name)
			where T : Control
		{
			return (T)typeof(PrincipalBrandingControl).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control);
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			PrincipalBrandingCollection collection = new PrincipalBrandingCollection(Factory, new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			collection.AddNew();
			return collection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			PrincipalBrandingControl brandingControl = (PrincipalBrandingControl)control;
			ZGrid grid = GetControl<ZGrid>(brandingControl, "BrandingGrid");
			ImageSelectionControl imageSelection = GetControl<ImageSelectionControl>(brandingControl, "ImageBoundImageSelectionControl");
			ZTextBox brandName = GetControl<ZTextBox>(brandingControl, "BrandNameBoundTextBox");
			ZTextBox brandEmail = GetControl<ZTextBox>(brandingControl, "BrandEmailAddressBoundTextBox");
			ZCheckBox useGeneric = GetControl<ZCheckBox>(brandingControl, "UseGenericBoundCheckBox");

			return grid.ReadOnly
				&& imageSelection.ReadOnly
				&& brandName.ReadOnly
				&& brandEmail.ReadOnly
				&& useGeneric.ReadOnly;
		}

		#endregion
	}
}
