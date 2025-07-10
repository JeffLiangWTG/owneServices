using System;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class CodeMapAttribute : Attribute
	{
		/// <summary>
		/// Indicates to the Universal Data engine that code mapping should be performed on a field on the way in.
		/// </summary>
		/// <param name="codeMappingRelationshipCode">Must be one of the members from Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships</param>
		public CodeMapAttribute(string codeMappingRelationshipCode)
		{
			this.CodeMappingRelationshipCode = codeMappingRelationshipCode;
		}

		public readonly string CodeMappingRelationshipCode;
	}
}
