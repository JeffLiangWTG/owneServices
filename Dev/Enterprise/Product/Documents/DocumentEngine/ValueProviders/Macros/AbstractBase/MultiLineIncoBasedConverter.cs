using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	abstract class MultiLineIncoBasedConverter : ValueProvider
	{
		protected ZString MultiLineConvertByIncoTerm(bool isImport, ZString chargeGroupsString, ZString incoTermString, ZString payAtLoad, ZString payAtDischarge)
		{
			ZString result;

			ZString[] chargeGroups = chargeGroupsString.IsEmpty ? System.Array.Empty<ZString>() : chargeGroupsString.Split('\n');
			IncoTerm inco;
			if (IncoTermRegistry.TryGetValue(incoTermString, out inco))
			{
				string[] results = new string[chargeGroups.Length];
				for (int index = 0; index < chargeGroups.Length; index++)
				{
					var exportImport = isImport
						? Directions.Import
						: Directions.Export;
					switch (inco.GetLocalClientOrAgent(exportImport, chargeGroups[index].Trim()))
					{
						case MasterFiles.Business.ChargedParty.None:
							results[index] = isImport ? payAtLoad : payAtDischarge;
							break;

						case MasterFiles.Business.ChargedParty.Agent:
						case MasterFiles.Business.ChargedParty.LocalClient:
							results[index] = isImport ? payAtDischarge : payAtLoad;
							break;

						default:
							results[index] = "?";
							ErrorReporter.ReportOnce("{936B6D51-6F89-4b47-80A3-A6AC4F417159}", "Should never fall through to default");
							break;
					}
				}

				result = string.Join("\n", results);
			}
			else
			{
				result = (NoResString)"Invalid Incoterm";
			}

			return result;
		}
	}
}
