using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class ZModuleCountryTreeView : ZModuleTreeView
	{
		public ZModuleCountryTreeView()
		{
			InitializeComponent();
		}

		protected override TreeNode GetModuleCategoryNode(Enterprise.Core.Modules.ModuleCategory category)
		{
			return new ZModulePointNode(category.Name, category.DisplayTextWithoutAmpersand, null, ZString.Empty);
		}

		protected override TreeNode GetModuleSectionNode(Enterprise.Core.Modules.ModuleSection section)
		{
			fCurrentSection = section;
			return new ZModulePointNode(section.Name, section.DisplayTextWithoutAmpersand, null, ZString.Empty);
		}
		ModuleSection fCurrentSection;

		protected override TreeNode GetModuleNode(INamedModule module)
		{
			return new ZModulePointNode(module.Description, module.Description, null, ZString.Empty);
		}

		protected override void PopulateModuleItems(TreeNode newModuleNode, INamedModule module)
		{
			if (countriesForAllCompanies != null)
			{
				ZModulePointNode allCountriesPoint = new ZModulePointNode(module.ModuleID + "ALL", Res.GetString("e60fc301-7540-486d-a5d0-1b5393ff714b", "All Countries/Regions"), module.ModuleID, "ALL");
				newModuleNode.Nodes.Add(allCountriesPoint);
				if (fCurrentSection != null &&
					fCurrentSection.Subcategory.Name == Enterprise.ZArchitecture.Modules.ModuleTreeLoaderConstant.Subcategory.Customs.Name)
				{
					foreach (RefCountry country in countriesForAllCompanies)
					{
						allCountriesPoint.Nodes.Add(new ZModulePointNode(module.ModuleID + country.RN_Code, country.RN_DescMultilingual, module.ModuleID, country.RN_Code));
					}
				}
			}
		}

		IBusinessObjectCollection countriesForAllCompanies;

		public void Populate(IBusinessObjectCollection countryList)
		{
			countriesForAllCompanies = countryList;
			base.Populate();
		}
	}

	public class ZModulePointNode : TreeNode
	{
		public ZModulePointNode(ZString itemName, ZString displayText, Enterprise.ZArchitecture.Modules.ModuleIdentifier moduleId, ZString countryCode)
		{
			this.CountryCode = countryCode;
			this.ModuleID = moduleId;
			this.Name = itemName;
			this.Text = displayText;
		}

		public readonly ZString CountryCode;
		public readonly Enterprise.ZArchitecture.Modules.ModuleIdentifier ModuleID;
	}
}
