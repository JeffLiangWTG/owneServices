using System.Collections.Generic;

using Enterprise.Client.EDI.IncidentManager.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class OrgTemplatedTextGenerator : TemplatedTextGenerator<EDIOrgHeader>
	{
		public OrgTemplatedTextGenerator(EDIOrgHeader businessObject) : base(businessObject) { }

		protected override string GetMacroValueCore(string macro)
		{
			switch (macro)
			{
				case MacroOH_FullName:
					return BizO.OH_FullNameTruncated;
				case MacroOH_FullNameOriginalValue:
					return BizO.OH_FullNameInfo.OriginalValue.ToString();
				case MacroOH_Code:
					return BizO.OH_Code;
				case MacroOH_CodeOriginalValue:
					return BizO.OH_CodeInfo.OriginalValue.ToString();
				case MacroUNLOCO:
					return BizO.OH_RL_NKClosestPort;
				default:
					return null;
			}
		}

		protected override List<string> GetSupportedMacrosList()
		{
			List<string> result = new List<string>();

			result.Add(MacroOH_FullName);
			result.Add(MacroOH_FullNameOriginalValue);
			result.Add(MacroOH_Code);
			result.Add(MacroOH_CodeOriginalValue);
			result.Add(MacroUNLOCO);

			return result;
		}

		const string MacroOH_FullName = "<<OH_FullName>>";
		const string MacroOH_FullNameOriginalValue = "<<OH_FullNameOriginalValue>>";
		const string MacroOH_Code = "<<OH_Code>>";
		const string MacroOH_CodeOriginalValue = "<<OH_CodeOriginalValue>>";
		const string MacroUNLOCO = "<<UNLOCO>>";
	}
}

