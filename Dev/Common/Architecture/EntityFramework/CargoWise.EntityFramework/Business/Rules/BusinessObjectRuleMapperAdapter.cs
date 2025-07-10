using System;
using System.Linq.Expressions;
using WTG.Rules.Engine.Impl;

namespace CargoWise.EntityFramework.Business.Rules
{
	class BusinessObjectRuleMapperAdapter : IRuleMapperAdapter
	{
		public Expression Convert(Expression expression, Type type)
		{
			return Expression.Convert(expression, type);
		}

		public Type GetObjectType(Type type)
		{
			return type;
		}

		public Type GetDependencyType(Type type, string dependencyPath)
		{
			return GetForType(type, dependencyPath).ResultType;
		}

		public Expression GetDependency(Type type, string dependencyPath)
		{
			return Expression.Constant(GetForType(type, dependencyPath));
		}

		IBusinessObjectRuleDependancy GetForType(Type t, string propertyPath)
		{
			return (IBusinessObjectRuleDependancy)Activator.CreateInstance(typeof(BusinessObjectRuleDependancy<>).MakeGenericType(t), propertyPath);
		}
	}
}
