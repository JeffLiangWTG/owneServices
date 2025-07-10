using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IUniversalCodeMapper
	{
		/// <summary>
		/// Return mapped code from the input string or input string if not mapped
		/// </summary>
		/// <param name="codeMappingRelationshipCode">Must be one of the members from Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships</param>
		string GetMappedOrInput(string input, string codeMappingRelationshipCode, int? currentLineNumber = null);

		/// <summary>
		/// Return mapped code from the input string or empty string if not mapped
		/// </summary>
		/// <param name="codeMappingRelationshipCode">Must be one of the members from Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships</param>
		string GetMappedOrEmpty(string input, string codeMappingRelationshipCode, int? currentLineNumber = null);

		/// <summary>
		/// Create or update code mapping for foreignCode using localCode or localGuid depending on codeMappingRelationshipCode.
		/// </summary>
		/// <param name="codeMappingRelationshipCode">Must be one of the members from Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships</param>
		IOrgPatternMatchOverride CreateOrUpdateCodeMapping(string codeMappingRelationshipCode, string foreignCode, string localCode, ZGuid localGuid);

		/// <summary>
		/// Return the Source OrgHeader for successfully mapped Foreign Code, otherwise return the Current Company Org Proxy.
		/// </summary>
		IOrgHeader SourceOrganisation { get; }
	}
}
