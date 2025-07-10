using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BarcodeParsing.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.BarcodeParsing.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
namespace Enterprise.BarcodeParsing.Module
{
	public class BarcodeParsingModule : ZFilterGridModule
	{
		#region Controller

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.BarcodeParsing);
		}

		#endregion

		#region Module Filter

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new BarcodeRuleFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new BarcodeRuleFilterControl((BarcodeRuleCollection)GridCollection, (BarcodeRuleFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new BarcodeRuleCollection(Factory);
		}

		#endregion

		#region ID

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.BarcodeParsing; }
		}

		#endregion

		#region Licence

		//#warning Not sure what Licence we will be using
		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.BarcodeParsing; }
		}

		#endregion

		#region Menuitems

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = base.GetNewStandardMenuItems().ToList();

			NewMenuItem.MenuItems.Add((NewMenuItem.CloneMenu()));
			NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("a283a9d0-60a1-494b-964c-b357ef3e3c88", "New Warehouse GS1 Rule Set with System Default Rules"),
				(s, e) => new BarcodeParsingController(copyFromSystemTemplate: true).ShowNewForm()));

			return menuItems.ToArray();
		}

		#endregion

		#region ShowNewForm

		protected override IZForm ShowNewForm()
		{
			var form = base.ShowNewForm();

			var barcodeRuleSet = (BarcodeRuleSet)form.BusinessEntityForPersistingForm;

			var filter = (ModuleTextFilter)FilterBusinessObject[BarcodeRuleFilterBusinessObject.FilterNames.Module];

			using (barcodeRuleSet.GetValidationSuspender()) // don't want to show an error when setting to an empty value
			{
				if (filter.IsActive && !filter.HasWarnings && !filter.HasErrors)
				{
					barcodeRuleSet.BRS_Module = filter.Property;
				}
				else
				{
					barcodeRuleSet.BRS_Module = ""; // clear out Warehouse DB default
				}
			}

			return form;
		}

		#endregion
	}
}
