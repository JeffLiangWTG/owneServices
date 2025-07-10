using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing;

public abstract class ValidationDeciderTestContext<TValidationDecider, TConfiguration>(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
	: ConfigurationTestContext<TConfiguration>(factory), IDisposable
	where TValidationDecider : class
	where TConfiguration : class
{
	public List<Type> RuleDeciderInterfaceTypes { get; } = [..ruleDeciderInterfaceTypes];

	/// <remarks>Will reset all rules</remarks>>
	public void InstallAdditionalRuleDecider<TRuleDecider>() where TRuleDecider : class
	{
		if (RuleDeciderInterfaceTypes.Contains(typeof(TRuleDecider)))
		{
			return;
		}

		RuleDeciderInterfaceTypes.Add(typeof(TRuleDecider));
		ResetCore();
	}

	public void EnableRuleDecider<TRuleDecider>(Expression<Func<TRuleDecider, bool>> isActive) where TRuleDecider : class
	{
		if (!RuleDeciderInterfaceTypes.Contains(typeof(TRuleDecider)))
		{
			Assertion.Fail($"You need to call {nameof(InstallAdditionalRuleDecider)}<{typeof(TRuleDecider)}>() before Enable/Disable it!");
			return;
		}

		autoResetCacheObjects.ForEach(x => x.ClearAllCachedValues());
		_ = ValidationDeciderMock.As<TRuleDecider>().SetupGet(isActive).Returns(true);
	}

	public void DisableRuleDecider<TRuleDecider>(Expression<Func<TRuleDecider, bool>> isActive) where TRuleDecider : class
	{
		if (!RuleDeciderInterfaceTypes.Contains(typeof(TRuleDecider)))
		{
			Assertion.Fail($"You need to call {nameof(InstallAdditionalRuleDecider)}<{typeof(TRuleDecider)}>() before Enable/Disable it!");
			return;
		}

		autoResetCacheObjects.ForEach(x => x.ClearAllCachedValues());
		_ = ValidationDeciderMock.As<TRuleDecider>().SetupGet(isActive).Returns(false);
	}

	public void EnableRule(Expression<Func<TValidationDecider, bool>> rule)
		=> SetRuleStatus(rule, true);

	public void DisableRule(Expression<Func<TValidationDecider, bool>> rule)
		=> SetRuleStatus(rule, false);

	public void SetRuleStatus(Expression<Func<TValidationDecider, bool>> rule, bool value)
	{
		autoResetCacheObjects.ForEach(x => x.ClearAllCachedValues());
		_ = ValidationDeciderMock.SetupGet(rule).Returns(value);
	}

	public void VerifyRule(Expression<Func<TValidationDecider, bool>> rule, string messagePrefix = null)
	{
		ValidationDeciderMock.VerifyGet(rule, Times.AtLeastOnce, (messagePrefix != null ? messagePrefix + " " : string.Empty) + "Expected IsRuleXXXXXActive property to be checked at least once per test case.");
		ValidationDeciderMock.Invocations.Clear();
	}

	[ExpectNoExceptions]
	public void AssertNoNotifications(ZPropertyInfo propertyInfo, string message, Expression<Func<TValidationDecider, bool>> rule, bool active, params Action[] testCases)
	{
		if (active)
		{
			EnableRule(rule);
		}
		else
		{
			DisableRule(rule);
		}

		foreach (var testCase in testCases)
		{
			testCase.Invoke();
			Assert.That(propertyInfo.Notifications.Select(e => e.Message), Has.None.Matches<string>(x => x.Contains(message)), $"Expected notifications would not contain {message}");
			VerifyRule(rule);
		}
	}

	protected override void SetFunctionalityCore(ZString code, ZString grouping, ZDateTime effectiveDate, bool enabled)
	{
		base.SetFunctionalityCore(code, grouping, effectiveDate, enabled);
		autoResetCacheObjects.ForEach(x => x.ClearAllCachedValues());
	}

	protected override void SetFunctionalityAttributeCore(ZString code, ZString grouping, ZDateTime effectiveDate, ZString attribute, ZString value)
	{
		base.SetFunctionalityAttributeCore(code, grouping, effectiveDate, attribute, value);
		autoResetCacheObjects.ForEach(x => x.ClearAllCachedValues());
	}

	protected override void ResetCore()
	{
		validationDeciderMock = null;
		base.ResetCore();
	}

	protected abstract void SetupConfiguration(Mock<TConfiguration> configurationMock, TValidationDecider validationDecider);

	protected void InitializeConfiguration() => _ = ValidationDeciderMock;

	Mock<TValidationDecider> ValidationDeciderMock => validationDeciderMock ??= CreateValidationDeciderMock();

	Mock<TValidationDecider> CreateValidationDeciderMock()
	{
		var result = new Mock<TValidationDecider> { CallBase = true };
		SetupRuleDeciderInterfaceTypes();
		SetupConfiguration(ConfigurationMock, result.Object);
		return result;

		void SetupRuleDeciderInterfaceTypes()
		{
			var asMethod = result.GetType().GetMethod("As");
			foreach (var ruleDeciderInterfaceType in RuleDeciderInterfaceTypes)
			{
				asMethod!.MakeGenericMethod(ruleDeciderInterfaceType).Invoke(result, null);
			}
		}
	}

	Mock<TValidationDecider> validationDeciderMock;
}
