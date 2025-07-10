using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.MasterFiles.Business.WorkflowMacroDataContextManager;
using MapElement = Enterprise.DocumentEngineCore.DocumentSupport.DataContextMapList.MapElement;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap
{
	public class DocDataProviderReflector : DocDataReflector
	{
		public DocDataProviderReflector(MapElement mapType)
			: base((mapType.TopLevelDataSourceType, null), new DocDataReflectorFilter(), MemberDescription.MacroTagTypes.Document)
		{
			Type type = mapType.TopLevelDataSourceType;
			string assemblyShortName = type.Assembly.FullName.Split(new char[] { ',' }, 2)[0];
			topLevelDataSourceInformation = Res.GetString("48f379e9-b782-4d5d-9ca7-c6dc8d770c02"
				, @"DataContext={0}

Data Source Type: {1}
Namespace: {2}
Assembly: {3}", mapType.DataContextIdentifier, type.Name, type.Namespace, assemblyShortName);
		}

		public DocDataProviderReflector(Type type)
			: this(type, MemberDescription.MacroTagTypes.Document)
		{
		}

		public DocDataProviderReflector(Type type, MemberDescription.MacroTagTypes macroTagType)
			: this(type, new DocDataReflectorFilter(), macroTagType)
		{
		}

		public DocDataProviderReflector(Type type, IDataReflectorFilter filter, MemberDescription.MacroTagTypes macroTagType)
			: base((type, null), filter, macroTagType)
		{
			if (type.GetInterfaces()
					.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBusinessObjectEventDataModel<,>)))
			{
				type = type.BaseType;
			}
			topLevelDataSourceInformation = Res.GetString("651bf79b-1a2b-40ec-b52e-d2ac0ca17b31"
				, @"Data Source Type: {0}
Namespace: {1}", type.Name, type.Namespace);
		}

		public ZString TopLevelDataSourceInformation
		{
			get { return topLevelDataSourceInformation; }
		}
		readonly ZString topLevelDataSourceInformation;

		public ZPropertyInfo TopLevelDataSourceInformationInfo
		{
			get { return GetZPropertyInfo(nameof(TopLevelDataSourceInformation)); }
		}
	}
}
