using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface ICodeMappingManager
	{
		bool QueueForCodeMappingWhereApplicable(IXmlReader reader, object targetDataObject, PropertyInfo propertyInfo, string elementFullName, string input, DataFieldValidatorAndSetter setter, int currentLineNumber, bool isAttribute);

		void UpdateMappedCodes(ITopLevelDataObject topLevelDataObject, BusinessObjectFactory factory);

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Such expression requires to simplify getting PropertyInfo without reflection")]
		IOrgPatternMatchOverride CreateOrUpdateCodeMapping<T, U>(ITopLevelDataObject topLevelDataObject, BusinessObjectFactory factory, Expression<Func<T, U>> getDataObjectPropertyWithCodeMapping, string foreignCode, string localCode, ZGuid localGuid);
	}

	public delegate object DataFieldValidatorAndSetter(IXmlReader reader, object targetDataObject, PropertyInfo propertyInfo, string originalValue, string mappedValue, int? lineNumber, string elementFullName, bool isAttribute);
}
