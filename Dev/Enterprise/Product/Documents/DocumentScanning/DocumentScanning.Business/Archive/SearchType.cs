using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.DocumentScanning.Business
{
	public class SearchType : NonPersistentBusinessObject, IObsoleteValidation
	{
		//This constructor is needed for SearchTypeCollection's CreateNonPersistentBusinessObject() method
		public SearchType()
		{
		}

		public SearchType(IAssemblyData assemblyData, ArchiveEDocsManager manager)
		{
			AssemblyData = assemblyData;
			Name = assemblyData.HumanReadableName;
			Manager = manager;
			IsFilterOn = true;
		}

		ZBool isFilterOn;
		ArchiveEDocsManager Manager { get; set; }

		public IAssemblyData AssemblyData { get; set; }
		public ZBool IsFilterOn
		{
			get
			{
				return isFilterOn;
			}
			set
			{
				isFilterOn = value;
				IsFilterOnInfo.RefreshBinding();
				if (!Manager.IsValidationSuspended)
				{
					Manager.Validation.ValidateIsSearchTypeSelected();
				}
			}
		}

		public ZPropertyInfo IsFilterOnInfo
		{
			get { return GetZPropertyInfo(nameof(IsFilterOn)); }
		}

		public ZString Name { get; private set; }

		public ZPropertyInfo NameInfo
		{
			get { return GetZPropertyInfo(nameof(Name)); }
		}
	}
}
