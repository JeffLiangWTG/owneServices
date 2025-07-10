using System;
using System.Reflection;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	[AttributeUsage(AttributeTargets.Class)]
	public class CustomIndexerListAttribute : Attribute
	{
		public CustomIndexerListAttribute(Type typeOfList)
		{
			if (typeOfList == null)
			{
				throw new ArgumentNullException(nameof(typeOfList));
			}
			if (!typeof(CodeDescriptionPairList).IsAssignableFrom(typeOfList))
			{
				throw new ArgumentOutOfRangeException(nameof(typeOfList), "Type of List passed in must be a subclass of CodeDescriptionPairList.");
			}
			TypeOfList = typeOfList;
		}
		readonly Type TypeOfList;

		CodeDescriptionPairList GetListOfValidIndexerValues()
		{
			ConstructorInfo constructorInfo = TypeOfList.GetConstructor(Type.EmptyTypes) ?? throw new InvalidCodeDescriptionPairException("Every CodeDescriptionPairList should have a parameterless constructor.");
			return (CodeDescriptionPairList)constructorInfo.Invoke(null);
		}

		public static CodeDescriptionPairList GetListForIndexer(Type typeWithStringBasedIndexer)
		{
			object[] customIndexerListAttributes = typeWithStringBasedIndexer.GetCustomAttributes(typeof(CustomIndexerListAttribute), true);
			if (customIndexerListAttributes.Length > 0)
			{
				CustomIndexerListAttribute customIndexerListAttribute = (CustomIndexerListAttribute)customIndexerListAttributes[0];
				return customIndexerListAttribute.GetListOfValidIndexerValues();
			}
			return null;
		}
	}
}
