using System.Linq;
using System.Reflection;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public static class ParameterLookupsProvider
	{
		public static CodeDescriptionPairList GetParameterCodes()
		{
			var parametersList = new CodeDescriptionPairList();
			foreach (FieldInfo codeField in typeof(CargoWise.EventReference.Constants.EventReferenceParameters.Codes).GetFields())
			{
				var description = typeof(Constants.EventReferenceParameters.Descriptions).GetProperties().FirstOrDefault(x => x.Name == codeField.Name);
				var descriptionValue = description.GetValue(null);
				var codeFieldValue = codeField.GetValue(null).ToString().ToUpperInvariant();
				if (description != null && descriptionValue != null)
				{
					var multilingualdescriptionValue = descriptionValue as MultilingualString;

					if (descriptionValue != null)
					{
						parametersList.AddPair(codeFieldValue, multilingualdescriptionValue);
					}
					else
					{
						parametersList.AddPair(codeFieldValue, description.ToString());
					}
				}
				else
				{
					parametersList.AddPair(codeFieldValue, string.Empty);
				}
			}
			parametersList.Sort();
			return parametersList;
		}

		public static CodeDescriptionPairList GetParameterCodes(string eventCode)
		{
			var parametersList = GetParameterCodes();
			switch (eventCode)
			{
				case AutoEvents.WorkflowTemplateAppliedCode:
					parametersList.AddOverwriteIfExists(new CodeDescriptionPair("NAM", Constants.EventReferenceParameters.Descriptions.MachineNameCode));
					break;
				default:
					break;
			}
			return parametersList;
		}
	}
}
