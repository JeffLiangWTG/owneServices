using System.Collections.Generic;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentTemplatedTextGenerator : TemplatedTextGenerator<IncidentMainBase>
	{
		public IncidentTemplatedTextGenerator(IncidentMainBase businessObject) : base(businessObject) { }

		protected override string GetMacroValueCore(string macro)
		{
			switch (macro)
			{
				case MacroIncidentType:
					return BizO.IM_IncidentType;
				case MacroIM_Priority:
					return BizO.IM_Priority;
				case MacroClientCode:
					return BizO.ClientCode;
				case MacroClientName:
					return BizO.ClientName;
				case MacroIM_Description:
					return BizO.IM_Description;
				default:
					return null;
			}
		}

		protected override List<string> GetSupportedMacrosList()
		{
			List<string> result = new List<string>();

			result.Add(MacroIncidentType);
			result.Add(MacroIM_Priority);
			result.Add(MacroClientCode);
			result.Add(MacroClientName);
			result.Add(MacroIM_Description);

			return result;
		}

		const string MacroIncidentType = "<<IncidentType>>";
		const string MacroIM_Priority = "<<IM_Priority>>";
		const string MacroClientCode = "<<ClientCode>>";
		const string MacroClientName = "<<ClientName>>";
		const string MacroIM_Description = "<<IM_Description>>";
	}
}

