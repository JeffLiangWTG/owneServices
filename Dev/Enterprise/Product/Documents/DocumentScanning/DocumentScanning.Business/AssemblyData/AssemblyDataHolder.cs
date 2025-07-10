using CargoWise.Definitions;

namespace Enterprise.DocumentScanning.Business
{
	using System;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	/// <summary>
	/// This class holds IAssemblyData information without keeping references to assemblies, delay loading types only when required
	/// </summary>
#if DEBUG
	public
#endif
	class AssemblyDataHolder : IAssemblyData
	{
		public AssemblyDataHolder(IAssemblyDataProvider assemblyDataProvider)
		{
			Argument.NotNull(assemblyDataProvider, "assemblyDataProvider");
			this.assemblyDataProvider = assemblyDataProvider;
		}

		readonly IAssemblyDataProvider assemblyDataProvider;
		AssemblyData assemblyData;

		AssemblyData GetAssemblyData()
		{
			if (assemblyData == null)
			{
				Type type = Type.GetType(assemblyDataProvider.TypeName + ", " + assemblyDataProvider.TypeAssemblyName);
				assemblyData = (AssemblyData)Activator.CreateInstance(type);
			}
			return assemblyData;
		}

		internal Clients ClientSpecificCode => assemblyDataProvider.ClientSpecificCode;

		public IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			AssemblyData assemblyData = GetAssemblyData();
			return assemblyData.GetBusinessObjectCollection(factory);
		}

		public IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory, AssemblyDataParams assemblyDataParams)
		{
			AssemblyData assemblyData = GetAssemblyData();
			return assemblyData.GetBusinessObjectCollection(factory, assemblyDataParams);
		}

		public Type BusinessObjectType
		{
			get
			{
				AssemblyData assemblyData = GetAssemblyData();
				return assemblyData.BusinessObjectType;
			}
		}

		public string DocManagerCode
		{
			get { return assemblyDataProvider.DocManagerCode; }
		}

		public string HumanReadableName
		{
			get
			{
				AssemblyData assemblyData = GetAssemblyData();
				return assemblyData.HumanReadableName;
			}
		}

		public bool IsAllowedForUnallocatedeDocs
		{
			get
			{
				AssemblyData assemblyData = GetAssemblyData();
				return assemblyData.IsAllowedForUnallocatedeDocs;
			}
		}

		public ModuleIdentifier ModuleID
		{
			get
			{
				AssemblyData assemblyData = GetAssemblyData();
				return assemblyData.ModuleID;
			}
		}

		public string ReferenceType
		{
			get
			{
				AssemblyData assemblyData = GetAssemblyData();
				return assemblyData.ReferenceType;
			}
		}

		public ZString GetFriendlyName(BusinessObject businessObject)
		{
			var assemblyData = GetAssemblyData();
			return assemblyData.GetFriendlyName(businessObject);
		}

		public ZQuery GetQuery(AssemblyDataParams assemblyDataParams)
		{
			AssemblyData assemblyData = GetAssemblyData();
			return assemblyData.GetQuery(assemblyDataParams);
		}

		public IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport()
		{
			return GetAssemblyData().GetEDocsViaUniversalXmlSupport();
		}

		public bool AllowLookupOfBizOFromPk
		{
			get { return GetAssemblyData().AllowLookupOfBizOFromPk; }
		}
	}
}
