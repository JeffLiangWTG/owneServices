using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.Testing;

public abstract class ConfigurationTestContext<TConfiguration>(BusinessObjectFactory factory) : FunctionalityTestContext, IDisposable
	where TConfiguration : class
{
	public BusinessObjectFactory Factory
	{
		get => factory;
		set
		{
			if (ReferenceEquals(factory, value))
			{
				return;
			}
			Reset();
			factory = value;
		}
	}
	BusinessObjectFactory factory = factory;

	public void AddAutoCacheResetObject(BusinessObject obj)
	{
		obj.ClearAllCachedValues();
		autoResetCacheObjects.Add(obj);
	}

	protected override void ResetCore()
	{
		autoResetCacheObjects.ForEach(x => x.ClearAllCachedValues());
		configurationOverrideHolder?.Dispose();
		configurationMock = null;
		base.ResetCore();
	}

	public void SetConfiguration<TConfigurationType>(Expression<Func<TConfiguration, TConfigurationType>> getConfigurationValueExpression, TConfigurationType value)
	{
		var expressionTailDescriptor = GetExpressionTailDescriptor(getConfigurationValueExpression);
		var argumentExpressions = expressionTailDescriptor.ArgumentTypes?.Select(object (x) => GetIsAnyExpression(x)).ToArray() ?? [];
		autoResetCacheObjects.ForEach(x => x.ClearAllCachedValues());
		_ = ConfigurationMock.Protected()
							.Setup<TConfigurationType>($"{expressionTailDescriptor.Name}Core", argumentExpressions)
							.Returns(value);
	}

	public void EnableConfiguration(Expression<Func<TConfiguration, ZBool>> getConfigurationValueExpression)
		=> SetConfiguration(getConfigurationValueExpression, true);

	public void DisableConfiguration(Expression<Func<TConfiguration, ZBool>> getConfigurationValueExpression)
		=> SetConfiguration(getConfigurationValueExpression, false);

	protected Mock<TConfiguration> ConfigurationMock => configurationMock ??= CreateConfigurationMock();

	Mock<TConfiguration> CreateConfigurationMock()
	{
		var configuration = new Mock<TConfiguration> { CallBase = true };
		configurationOverrideHolder = UpdateObjectFactory(factory, configuration);
		return configuration;
	}

	Mock<TConfiguration> configurationMock;

	protected abstract IDisposable UpdateObjectFactory(BusinessObjectFactory factory, Mock<TConfiguration> configurationMock);

	sealed record ExpressionTailDescriptor(string Name, Type[] ArgumentTypes = null);

	static ExpressionTailDescriptor GetExpressionTailDescriptor<TConfigurationType>(Expression<Func<TConfiguration, TConfigurationType>> expression)
		=> expression.Body switch
		{
			UnaryExpression { NodeType: ExpressionType.Convert, Operand: MemberExpression member } => new(member.Member.Name),
			MemberExpression member => new(member.Member.Name),
			MethodCallExpression methodCall => new(methodCall.Method.Name, methodCall.Arguments.Select(x => x.Type).ToArray()),
			_ => throw new ArgumentException((NoResString)"Expression is not a valid member expression.", nameof(expression)),
		};

	static Expression GetIsAnyExpression(Type argumentType) => Expression.Call(IsAnyMethod.MakeGenericMethod(argumentType));

	static readonly MethodInfo IsAnyMethod = typeof(It).GetMethod(nameof(It.IsAny), BindingFlags.Public | BindingFlags.Static);

	IDisposable configurationOverrideHolder;
	protected readonly List<BusinessObject> autoResetCacheObjects = [];
}
