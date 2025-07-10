using System;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common.CodeMappings;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Update.CodeMappings
{
	public class CodeMappingRepository : ICodeMappingHelper, ICodeMappingRepository
	{
		public CodeMappingRepository(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public static Guid DefaultOrgPK(BusinessObjectFactory factory)
		{
			var orgHeader = OrgHeader.GetDefaultOrg(factory);
			return orgHeader == null ? Guid.Empty : orgHeader.PK.ToGuid();
		}

		public static string DefaultOrgCode(BusinessObjectFactory factory)
		{
			var orgHeader = OrgHeader.GetDefaultOrg(factory);
			return orgHeader == null ? String.Empty : orgHeader.OH_Code.ToString();
		}

		public OrgPatternMatchOverride New()
		{
			return factory.New<OrgPatternMatchOverride>();
		}

		public OrgPatternMatchOverride Load(string relationship, object foreignCode, Guid ownerPK)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, relationship);
			query.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, ownerPK);
			query.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, foreignCode);
			return factory.LoadTop1<OrgPatternMatchOverride>(query);
		}

		public void CreateCodeMapping(CodeMapping codeMapping)
		{
			var ownerPK = FindOwnerOrgPK(codeMapping.OwnerCode);
			var mapping = New();
			mapping.OO_Relationship = codeMapping.Relationship;
			mapping.OO_OH = ownerPK;
			mapping.OO_LocalCode = codeMapping.LocalCode;
			mapping.OO_ForeignCode = codeMapping.ForeignCode;
			mapping.OO_LocalGuid = codeMapping.LocalGuid;
		}

		public string MapLocalCode(string relationship, object foreignCode, string ownerCode)
		{
			var ownerPK = FindOwnerOrgPK(ownerCode);
			return MapLocalCode(relationship, foreignCode, ownerPK);
		}

		public string MapLocalCode(string relationship, object foreignCode, Guid ownerPK)
		{
			var match = Load(relationship, foreignCode, ownerPK);
			return MapLocalCode(match);
		}

		/// <summary>
		/// Try to find Local Code by Pattern Match Override
		/// </summary>
		/// <param name="patternMatchOverride"></param>
		/// <returns>String Empty if could not find Code</returns>
		public string MapLocalCode(OrgPatternMatchOverride patternMatchOverride)
		{
			if (patternMatchOverride != null)
			{
				return patternMatchOverride.GetLocalCode();
			}

			return string.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public Guid FindOwnerOrgPK(string ownerOrgCode)
		{
			var orgPK = Guid.Empty;
			var defaultOrgPK = DefaultOrgPK(factory);

			orgPK = defaultOrgPK;

			var codeMapping = EDICodeMapper.FindGlobalCodeMapping("OrgHeader", "Code");
			var relationship = codeMapping.Relationship;

			var localOrgCode = MapLocalCode(relationship, ownerOrgCode, orgPK);
			if (localOrgCode.IsEmpty())
			{
				localOrgCode = ownerOrgCode;
			}

			orgPK = FindOrgPKByCode(localOrgCode);

			return orgPK == Guid.Empty ? defaultOrgPK : orgPK;
		}

		Guid FindOrgPKByCode(string code)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgHeaderSchema.OH_Code, code);
			var orgHeader = factory.LoadTop1<OrgHeader>(query);
			return orgHeader == null ? Guid.Empty : orgHeader.PK.ToGuid();
		}

		readonly BusinessObjectFactory factory;
	}
}
