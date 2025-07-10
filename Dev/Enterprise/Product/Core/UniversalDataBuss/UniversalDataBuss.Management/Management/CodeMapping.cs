using System.Reflection;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management.CodeMapping
{
	class CodeMapping
	{
		internal CodeMapping(object dataTargetObject, PropertyInfo propertyInfo, string elementFullName, string incomingValue, string mappingRelationshipCode, int lineNumber, bool isAttribute, DataFieldValidatorAndSetter valueSetter)
		{
			this.propertyInfo = propertyInfo;
			this.dataTargetObject = dataTargetObject;
			this.isAttribute = isAttribute;
			this.lineNumber = lineNumber;
			this.validateAndSetValue = valueSetter;
			ElementFullName = elementFullName;
			IncomingValue = incomingValue;
			MappingRelationshipCode = mappingRelationshipCode;
		}

		readonly PropertyInfo propertyInfo;
		readonly object dataTargetObject;
		readonly int lineNumber;
		readonly bool isAttribute;
		readonly DataFieldValidatorAndSetter validateAndSetValue;

		internal string ElementFullName { get; }
		internal string IncomingValue { get; }
		internal string MappingRelationshipCode { get; }

		internal void MapValue(IUniversalCodeMapper codeMapper, IXmlReader reader)
		{
			var mappedValue = codeMapper.GetMappedOrEmpty(IncomingValue, MappingRelationshipCode, lineNumber);
			validateAndSetValue(reader, dataTargetObject, propertyInfo, IncomingValue, mappedValue, lineNumber, ElementFullName, isAttribute);
		}
	}
}

