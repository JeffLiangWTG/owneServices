using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class JobDeclarationDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCloneStrategyManuallySets_JE_ApplicationCode()
		{
			var oldDec = Factory.New<JobDeclaration>();
			oldDec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var cloneStrategy = new JobDeclarationDeepCloneStrategy(oldDec, CloneType.TemplateCopy);

			var args = new BusinessObjectCloneArgs();

			var newDec = (JobDeclaration)cloneStrategy.Clone(args);
			AssertEquals("JE_Application should be copied over", DeclarationApplicationCodeList.Codes.DeltaIE, newDec.JE_ApplicationCode);
			AssertEquals("JE_Application code should be excluded from cloning strategy and set manually instead", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_ApplicationCode));
		}

		public void TestCloneStrategy_CopiesAddInfoProperty()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var zgProperties = GetAddInfoProperties(declaration);
			SetDeclarationPropertiesValues(declaration, zgProperties);
			declaration.JE_TariffType = Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff;
			Factory.Save();

			var cloneStrategy = new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy);
			var args = new BusinessObjectCloneArgs();
			var clonedDeclaration = (JobDeclaration)cloneStrategy.Clone(args);
			var clonedZgProperties = GetAddInfoProperties(clonedDeclaration);
			clonedDeclaration.JE_DeclarationReference = Guid.NewGuid().ToString("N");
			Factory.Save();

			var existingValues = GetPropertyInfoValuesList(zgProperties, declaration);
			var clonedValues = GetPropertyInfoValuesList(clonedZgProperties, clonedDeclaration);
			AssertContainsExactElementsInAnyOrder(existingValues, clonedValues);
		}

		void SetDeclarationPropertiesValues(JobDeclaration declaration, List<PropertyInfo> properties)
		{
			var valuesDictionary = new Dictionary<Type, object>
			{
				[typeof(ZString)] = new ZString("X"),
				[typeof(ZDecimal)] = new ZDecimal(123.45),
				[typeof(ZDateTime)] = new ZDateTime(2024, 10, 02),
				[typeof(ZInt)] = new ZInt(123),
				[typeof(ZBool)] = new ZBool(true),
				[typeof(ZShort)] = new ZShort(1234)
			};

			foreach (var property in properties)
			{
				if (valuesDictionary.TryGetValue(property.PropertyType, out var value))
				{
					property.SetValue(declaration, value);
				}
				else
				{
					throw new Exception($"{property.Name}: {property.PropertyType} is not supported. Please add the missing type to the dictionary.");
				}
			}
		}

		List<PropertyInfo> GetAddInfoProperties(JobDeclaration declaration)
		{
			return declaration.AddInfoTableSchema.All.Where(p => !Schema.IsSystemColumn(p.Name)).Select(p => declaration.GetType().GetProperty(p.Name)).Where(p => p != null).ToList();
		}

		Dictionary<string, object> GetPropertyInfoValuesList(List<PropertyInfo> propertyInfoList, JobDeclaration declaration)
		{
			return propertyInfoList.ToDictionary(p => p.Name, p => p.GetValue(declaration));
		}
	}
}
