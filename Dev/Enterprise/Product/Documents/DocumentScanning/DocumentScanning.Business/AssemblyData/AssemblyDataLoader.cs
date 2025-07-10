using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using CargoWise.Definitions;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.DocumentScanning.Business
{
#if DEBUG
	public
#endif
	class AssemblyDataLoader
	{
		public AssemblyDataLoader(IGlbCompany company)
		{
			Argument.NotNull(company, "company");
			Company = company;
		}

		public AssemblyDataLoader()
		{
		}

		public IGlbCompany Company { get; }

		public DocManagerCodesCodeDescriptionPairList Load()
		{
			var allAssemblyData = new DocManagerCodesCodeDescriptionPairList(Company);
			foreach (var provider in GetAssemblyDataProviders())
			{
				if ((string.IsNullOrEmpty(provider.Country) || provider.Country == allAssemblyData.CountryCode)
					&& (provider.ClientSpecificCode == Clients.None || provider.ClientSpecificCode == ClientHookLoader.Instance.Client))
				{
					var code = provider.DocManagerCode;
					var existing = (AssemblyDataHolder)allAssemblyData.GetAssemblyDataFromDocManagerCode(code);

					if (existing != null)
					{
						// Client specific can replace generic. existing.ClientSpecificCode can either be Clients.None or same as provider.ClientSpecificCode
						if (existing.ClientSpecificCode != provider.ClientSpecificCode && provider.ClientSpecificCode != Clients.None)
						{
							allAssemblyData.Replace(provider.DocManagerCode, new AssemblyDataHolder(provider));
						}
						else
						{
							var messageBuilder = new StringBuilder();
							messageBuilder.Append($"Both assembly '{existing.BusinessObjectType.Assembly.FullName}' and assembly '{provider.GetType().Assembly.FullName}' have same DocManagerCode '{code}'.");

							if (Globals.IsDebugMode)
							{
								messageBuilder.AppendLine().Append((NoResString)"If you are in debug mode, please try to perform a full build with QGL. If the issue persists, please send the error report.");
							}

							var message = messageBuilder.ToString();

							ErrorReporter.ReportOnce("DuplicateDocManagerCode", message, new Exception(message));
						}
					}
					else
					{
						allAssemblyData.Add(provider.DocManagerCode, new AssemblyDataHolder(provider));
					}
				}
			}
			return allAssemblyData;
		}

		public DocManagerCodesCodeDescriptionPairList LoadRegardlessOfCompany()
		{
			var allAssemblyData = new DocManagerCodesCodeDescriptionPairList();
			var allAssemblyDataList = new List<string>();
			foreach (var provider in GetAssemblyDataProviders())
			{
				var code = provider.DocManagerCode;
				if (!allAssemblyDataList.Contains(code))
				{
					allAssemblyDataList.Add(code);
					allAssemblyData.Add(provider.DocManagerCode, new AssemblyDataHolder(provider));
				}
			}
			return allAssemblyData;
		}

#if DEBUG
		public
#else
		internal
#endif
		static IEnumerable<IAssemblyDataProvider> GetAssemblyDataProviders()
		{
			return AssemblyMetaDataReader.GetAttributes<AssemblyDataProviderAttribute>();
		}
	}
}
