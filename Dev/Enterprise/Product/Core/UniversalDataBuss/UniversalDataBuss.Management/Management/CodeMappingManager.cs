using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management.CodeMapping
{
	public class CodeMappingManager : ICodeMappingManager
	{
		public CodeMappingManager(IXmlImportLogger logger)
		{
			this.logger = logger;
			this.codeMappings = new List<CodeMapping>();
			stringInterner = new StringInterner();
		}

		readonly IXmlImportLogger logger;
		readonly List<CodeMapping> codeMappings;
		readonly StringInterner stringInterner;
		IXmlReader reader;

		public bool QueueForCodeMappingWhereApplicable(IXmlReader xmlReader, object targetDataObject, PropertyInfo propertyInfo, string elementFullName, string input, DataFieldValidatorAndSetter setter, int currentLineNumber, bool isAttribute)
		{
			reader = reader ?? xmlReader;
			var codeMapRelationshipCode = GetCodeMapRelationshipCode(propertyInfo);
			if (!string.IsNullOrEmpty(codeMapRelationshipCode) && !string.IsNullOrEmpty(input))
			{
				codeMappings.Add(new CodeMapping(targetDataObject, propertyInfo, stringInterner.InternValue(elementFullName), input, stringInterner.InternValue(codeMapRelationshipCode), currentLineNumber, isAttribute, setter));
				return true;
			}

			return false;
		}

		public void UpdateMappedCodes(ITopLevelDataObject topLevelDataObject, BusinessObjectFactory factory)
		{
			if (codeMappings.Count > 0)
			{
				IUniversalCodeMapper codeMapper = GetCodeMapper(topLevelDataObject, factory);
				codeMappings.ForEach(mapping => mapping.MapValue(codeMapper, reader));
			}
		}

		public IOrgPatternMatchOverride CreateOrUpdateCodeMapping<T, U>(ITopLevelDataObject topLevelDataObject, BusinessObjectFactory factory, Expression<Func<T, U>> getDataObjectPropertyWithCodeMapping, string foreignCode, string localCode, ZGuid localGuid)
		{
			Argument.NotNull(topLevelDataObject, nameof(topLevelDataObject));
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(getDataObjectPropertyWithCodeMapping, nameof(getDataObjectPropertyWithCodeMapping));

			var propertyInfo = GetPropertyInfo(getDataObjectPropertyWithCodeMapping);
			var codeMapRelationshipCode = GetCodeMapRelationshipCode(propertyInfo);
			if (string.IsNullOrEmpty(codeMapRelationshipCode))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Can't get code mapping relationship code for property {0}.", propertyInfo.Name));
			}

			IUniversalCodeMapper codeMapper = GetCodeMapper(topLevelDataObject, factory);
			return codeMapper.CreateOrUpdateCodeMapping(codeMapRelationshipCode, foreignCode, localCode, localGuid);
		}

		IUniversalCodeMapper GetCodeMapper(ITopLevelDataObject topLevelDataObject, BusinessObjectFactory factory)
		{
			string dataProvider = string.Empty;
			if (topLevelDataObject.DataContext != null && !topLevelDataObject.DataContext.DataProviderForCodeMapping.IsEmpty)
			{
				dataProvider = topLevelDataObject.DataContext.DataProviderForCodeMapping;
			}

			var codeMapper = GetCodeMapperFromDataContext(topLevelDataObject, factory)
				?? ObjectFactory.New<IUniversalCodeMapper>(dataProvider, logger, factory);

			return codeMapper;
		}

		static IUniversalCodeMapper GetCodeMapperFromDataContext(ITopLevelDataObject topLevelDataObject, BusinessObjectFactory factory)
		{
			var dataTargetCollecton = topLevelDataObject?.DataContext?.DataTargetCollection;

			if (dataTargetCollecton == null)
			{
				return null;
			}

			var dataTarget = dataTargetCollecton.FirstOrDefault()?.Type.ToString();

			if (string.IsNullOrEmpty(dataTarget))
			{
				return null;
			}

			var providers = (Hashtable)ObjectFactory.Get("UniversalCodeMapperProviderList");
			var objectHandle = (ObjectHandle)providers[dataTarget];

			return objectHandle?.GetObject() is IUniversalCodeMapperProvider provider
				? provider.Create(topLevelDataObject, factory)
				: null;
		}

		static string GetCodeMapRelationshipCode(PropertyInfo propertyInfo)
		{
			var codeMapAttributes = propertyInfo.GetCustomAttributes(typeof(CodeMapAttribute), false);
			if (codeMapAttributes.Length > 0)
			{
				return (codeMapAttributes[0] as CodeMapAttribute).CodeMappingRelationshipCode;
			}

			return null;
		}

		PropertyInfo GetPropertyInfo<T, U>(Expression<Func<T, U>> lambda)
		{
			if (lambda.Body == null)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Lambda expression ({0}) Body must not be null.", lambda));
			}
			var member = lambda.Body as MemberExpression ?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Can't cast lambda expression Body ({0}) to MemberExpression.", lambda.Body));

			if (member.Member == null)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Lambda expression ({0}) Member must not be null.", member));
			}
			var propertyInfo = member.Member as PropertyInfo ?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Can't cats lambda expression Member ({0}) to PropertyInfo.", member.Member));

			return propertyInfo;
		}

		#region Test

#if DEBUG

		public IEnumerable<IEnumerable<string>> GetInternedStrings()
		{
			yield return codeMappings.Select(m => m.ElementFullName);
			yield return codeMappings.Select(m => m.IncomingValue);
			yield return codeMappings.Select(m => m.MappingRelationshipCode);
		}

#endif

		#endregion
	}
}

