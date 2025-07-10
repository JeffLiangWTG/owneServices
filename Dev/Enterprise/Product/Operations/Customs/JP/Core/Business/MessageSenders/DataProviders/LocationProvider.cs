using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;

namespace Enterprise.Customs.JP.Business
{
	sealed class LocationProvider : ILocation
	{
		public LocationProvider(JobDeclaration declaration, string type)
		{
			Argument.NotNull(declaration, nameof(declaration));
			this.declaration = declaration;
			this.type = type;
		}

		readonly JobDeclaration declaration;
		readonly string type;

		public string Code
		{
			get
			{
				var result = string.Empty;
				switch (type)
				{
					case nameof(CusEntryHeaderMessageProvider.PortOfLoading):
						if (declaration.IsImport || (declaration.IsExport && declaration.JE_RL_NKPortOfLoading.Equals("ZZZ")))
						{
							result = declaration.JE_RL_NKPortOfLoading;
						}
						else
						{
							result = declaration.JE_RL_NKPortOfLoading.SubstringSafe(2);
						}
						break;
					case nameof(CusEntryHeaderMessageProvider.PortOfUnloading):
						result = declaration.JE_RL_NKPortOfArrival.SubstringSafe(2);
						break;
					case nameof(CusEntryHeaderMessageProvider.FinalDestination):
						result = declaration.JE_RL_NKFinalDestination;
						break;
					case nameof(CusEntryHeaderMessageProvider.PortOfOrigin):
						result = declaration.JE_RL_NKOrigin;
						break;
					default:
						break;
				}
				return result;
			}
		}

		public string Name
		{
			get
			{
				var result = string.Empty;
				switch (type)
				{
					case nameof(CusEntryHeaderMessageProvider.PortOfLoading):
						if (declaration.IsImport)
						{
							result = declaration.JE_PortOfLoadingName;
						}
						break;
					case nameof(CusEntryHeaderMessageProvider.FinalDestination):
						result = declaration.JE_FinalDestinationName;
						break;
					case nameof(CusEntryHeaderMessageProvider.PortOfOrigin):
						result = declaration.Origin?.RL_PortName;
						break;
					default:
						break;
				}
				return result;
			}
		}
	}
}
