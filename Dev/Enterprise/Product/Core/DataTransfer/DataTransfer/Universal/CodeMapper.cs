using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Universal
{
	public class CodeMapper : IUniversalCodeMapper
	{
		public CodeMapper(string codeMapSourceForeignCode, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			this.logger = logger;
			this.factory = factory;

			this.codeMapSourcePK = Env.CurrentCompany.OrganisationPK;

			if (!string.IsNullOrEmpty(codeMapSourceForeignCode))
			{
				var localGuid = GetMappedPK(Constants.OrgPatternMatchOverrideRelationships.Organisation, codeMapSourceForeignCode);

				if (localGuid.IsValid)
				{
					this.codeMapSourcePK = localGuid;
				}
			}
		}

		readonly ZGuid codeMapSourcePK;
		readonly IXmlImportLogger logger;

		#region IUniversalCodeMapper

		public string GetMappedOrInput(string input, string codeMappingRelationshipCode, int? currentLineNumber = null)
		{
			var mappedCode = GetMappedOrEmpty(input, codeMappingRelationshipCode, currentLineNumber);
			return string.IsNullOrEmpty(mappedCode) ? input : mappedCode;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Just one not too big switch - it is not too complex to refactor")]
		public string GetMappedOrEmpty(string input, string codeMappingRelationshipCode, int? currentLineNumber = null)
		{
			switch (codeMappingRelationshipCode)
			{
				case Constants.OrgPatternMatchOverrideRelationships.Organisation:
					return Map<IOrgHeader>(input, codeMappingRelationshipCode, currentLineNumber, organisation => organisation.OH_Code);
				case Constants.OrgPatternMatchOverrideRelationships.Port:
					return Map<IRefUNLOCO>(input, codeMappingRelationshipCode, currentLineNumber, port => port.RL_Code);
				case Constants.OrgPatternMatchOverrideRelationships.Country:
					return Map<IRefCountry>(input, codeMappingRelationshipCode, currentLineNumber, country => country.RN_Code);
				case Constants.OrgPatternMatchOverrideRelationships.ContainerType:
					return Map<IRefContainer>(input, codeMappingRelationshipCode, currentLineNumber, containerType => containerType.RC_Code);
				case Constants.OrgPatternMatchOverrideRelationships.Commodities:
					return Map<IRefCommodityCode>(input, codeMappingRelationshipCode, currentLineNumber, commodity => commodity.RH_Code);
				case Constants.OrgPatternMatchOverrideRelationships.Equipment:
					return Map<IRefEquipment>(input, codeMappingRelationshipCode, currentLineNumber, equipment => equipment.RQ_ShortCode);
				case Constants.OrgPatternMatchOverrideRelationships.ServiceLevel:
					return Map<IRefServiceLevel>(input, codeMappingRelationshipCode, currentLineNumber, serviceLevel => serviceLevel.RS_Code);
				case Constants.OrgPatternMatchOverrideRelationships.DocumentType:
					return Map<IRefDocType>(input, codeMappingRelationshipCode, currentLineNumber, docType => docType.RT_DocType);
				case Constants.OrgPatternMatchOverrideRelationships.Currency:
					return Map<IRefCurrency>(input, codeMappingRelationshipCode, currentLineNumber, currency => currency.RX_Code);
				case Constants.OrgPatternMatchOverrideRelationships.Warehouse:
					return Map<IWhsWarehouse>(input, codeMappingRelationshipCode, currentLineNumber, warehouse => warehouse.WW_WarehouseCode);
				default:
					var matchedCode = GetMappedCode(codeMappingRelationshipCode, input);
					LogCodeMapping(matchedCode, input, GetCodeMapDescription(codeMappingRelationshipCode), currentLineNumber);
					return matchedCode;
			}
		}

		public IOrgPatternMatchOverride CreateOrUpdateCodeMapping(string codeMappingRelationshipCode, string foreignCode, string localCode, ZGuid localGuid)
		{
			Argument.NotNullOrEmpty(codeMappingRelationshipCode, nameof(codeMappingRelationshipCode));
			Argument.NotNullOrEmpty(foreignCode, nameof(foreignCode));

			switch (codeMappingRelationshipCode)
			{
				case Constants.OrgPatternMatchOverrideRelationships.Organisation:
				case Constants.OrgPatternMatchOverrideRelationships.Port:
				case Constants.OrgPatternMatchOverrideRelationships.Country:
				case Constants.OrgPatternMatchOverrideRelationships.ContainerType:
				case Constants.OrgPatternMatchOverrideRelationships.Commodities:
				case Constants.OrgPatternMatchOverrideRelationships.Equipment:
				case Constants.OrgPatternMatchOverrideRelationships.ServiceLevel:
				case Constants.OrgPatternMatchOverrideRelationships.DocumentType:
				case Constants.OrgPatternMatchOverrideRelationships.Currency:
				case Constants.OrgPatternMatchOverrideRelationships.Warehouse:
					return CreateOrUpdateMatchOverride(codeMappingRelationshipCode, foreignCode, localGuid);
				default:
					return CreateOrUpdateMatchOverride(codeMappingRelationshipCode, foreignCode, localCode);
			}
		}

		// remove - check with ben
		IOrgHeader IUniversalCodeMapper.SourceOrganisation
		{
			get { return factory.Load<IOrgHeader>(codeMapSourcePK); }
		}

		#endregion

		#region Implementation

		string Map<T>(string input, string codeMappingRelationshipCode, int? currentLineNumber, Func<T, string> getCode)
			where T : class
		{
			var result = string.Empty;
			var matchedBOPK = GetMappedPK(codeMappingRelationshipCode, input);
			if (matchedBOPK.IsValid)
			{
				var matchedBO = factory.Load<T>(matchedBOPK);
				var description = GetCodeMapDescription(codeMappingRelationshipCode);
				if (matchedBO != null)
				{
					result = getCode(matchedBO);
					LogCodeMapping(result, input, description, currentLineNumber);
				}
				else
				{
					LogCodeMappingInvalid(matchedBOPK, input, description, currentLineNumber);
				}
			}
			return result;
		}

		void LogCodeMapping(string matchedBOCode, string foreignCode, string description, int? currentLineNumber)
		{
			if (string.IsNullOrEmpty(matchedBOCode))
			{
				return;
			}

			AddFirstLog();
			var lineNumber = FormatLineNumber(currentLineNumber);
			logger.Log(LogType.Information, lineNumber + Res.GetString("83e9cf08-bd1d-45b4-83c6-5963cc64dac3", "Mapped {0} code '{1}' to '{2}'.", description, foreignCode, matchedBOCode));
		}

		void LogCodeMappingInvalid(ZGuid localGuid, string foreignCode, string description, int? currentLineNumber)
		{
			AddFirstLog();
			var lineNumber = FormatLineNumber(currentLineNumber);
			logger.Log(LogType.Warning, lineNumber + Res.GetString("d340afc9-a582-4764-a269-81b41c6c2419", "{0} mapping with code '{1}' is invalid [Local GUID: '{2}'].", description, foreignCode, localGuid));
		}

		void AddFirstLog()
		{
			if (isFirstLog)
			{
				isFirstLog = false;
				logger.Log(LogType.Information, Res.GetString("b6fa1e8b-3646-4ce7-ac76-0630c0bfef9e", "Used code mapping defined in Organization(Code: {0}) > Config > EDI Code Mapping.", CodeMapSourceCode));
			}
		}

		static string FormatLineNumber(int? lineNumber)
		{
			return lineNumber == null ? string.Empty : Res.GetString("2f1dacae-6b72-4ec0-be7b-d3d7ec8590bb", "Line {0}:", lineNumber) + " ";
		}

		string GetMappedCode(string relationship, string foreignCode)
		{
			var match = GetMatchOverride(relationship, foreignCode);

			return match != null ? (string)match.OO_LocalCode : string.Empty;
		}

		ZGuid GetMappedPK(string relationship, string foreignCode)
		{
			var match = GetMatchOverride(relationship, foreignCode);

			return match != null ? match.OO_LocalGuid : ZGuid.Empty;
		}

		OrgPatternMatchOverride GetMatchOverride(string relationship, string foreignCode)
		{
			var filter = new ZQuery();
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, codeMapSourcePK);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, relationship);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, foreignCode);

			return factory.LoadTop1<OrgPatternMatchOverride>(filter);
		}

		OrgPatternMatchOverride CreateOrUpdateMatchOverride(string relationship, string foreignCode, string localCode)
		{
			Argument.NotNullOrEmpty(localCode, nameof(localCode));

			var matchOverride = CreateOrGetMatchOverride(relationship, foreignCode);
			matchOverride.OO_LocalCode = localCode;

			return matchOverride;
		}

		OrgPatternMatchOverride CreateOrUpdateMatchOverride(string relationship, string foreignCode, ZGuid localGuid)
		{
			if (localGuid.IsEmpty)
			{
				throw new ArgumentException("Argument must not be empty.", nameof(localGuid));
			}

			var matchOverride = CreateOrGetMatchOverride(relationship, foreignCode);
			matchOverride.OO_LocalGuid = localGuid;

			return matchOverride;
		}

		OrgPatternMatchOverride CreateOrGetMatchOverride(string relationship, string foreignCode)
		{
			var matchOverride = GetMatchOverride(relationship, foreignCode);
			if (matchOverride == null)
			{
				matchOverride = factory.New<OrgPatternMatchOverride>();
				matchOverride.OO_OH = codeMapSourcePK;
				matchOverride.OO_Relationship = relationship;
				matchOverride.OO_ForeignCode = foreignCode;
			}

			return matchOverride;
		}

		string GetCodeMapDescription(string relationshipCode) => CodeMapDescriptions.GetDescriptionFromCode(relationshipCode) ?? string.Empty;

		CodeDescriptionPairList CodeMapDescriptions => codeMapDescriptions ?? (codeMapDescriptions = OrgPatternMatchOverrideLookups.GetRelationshipCodeDescriptionList());
		CodeDescriptionPairList codeMapDescriptions;

		string CodeMapSourceCode
		{
			get { return (!string.IsNullOrEmpty(codeMapSourceCode)) ? codeMapSourceCode : (codeMapSourceCode = factory.Load<IOrgHeader>(codeMapSourcePK).OH_Code); }
		}

		string codeMapSourceCode;

		readonly BusinessObjectFactory factory;
		bool isFirstLog = true;

		#endregion
	}
}
