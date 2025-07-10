using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Testing;

public class Preq<T>
	where T : BusinessObject
{
	public Preq(Action<T> validAction, Action<T> failingAction) : this(new[] { validAction }, new[] { failingAction } ) { }

	protected Preq() { }

	Preq(Action<T> action) : this(new[] { action }, Array.Empty<Action<T>>() ) { }

	Preq(IEnumerable<Action<T>> satisfactoryActions, IEnumerable<Action<T>> failingActions)
	{
		this.satisfactoryActions = satisfactoryActions;
		this.failingActions = failingActions;
	}

	readonly IEnumerable<Action<T>> satisfactoryActions;
	readonly IEnumerable<Action<T>> failingActions;

	public IEnumerable<Action<T>> GetSatisfactoryActions() => GetSatisfactoryActionsCore();
	protected virtual IEnumerable<Action<T>> GetSatisfactoryActionsCore() => satisfactoryActions;

	public IEnumerable<Action<T>> GetFailingActions() => GetFailingActionsCore();
	protected virtual IEnumerable<Action<T>> GetFailingActionsCore() => failingActions ?? Array.Empty<Action<T>>();

	#region Convertion

	public static implicit operator Preq<T>(Action<T> action) => new Preq<T>(action);

	#endregion

	#region Operator Overloads

	public static bool operator true(Preq<T> p) => false;

	public static bool operator false(Preq<T> p) => false;

	public static Preq<T> operator &(Preq<T> left, Preq<T> right)
	{
		var mergedSatisfactoryActions = left.GetSatisfactoryActions().SelectMany(x => right.GetSatisfactoryActions().Select(y => x += y));
		var mergedFailingActions = left.GetFailingActions().Concat(right.GetFailingActions());

		return new Preq<T>(mergedSatisfactoryActions, mergedFailingActions);
	}

	public static Preq<T> operator |(Preq<T> left, Preq<T> right)
	{
		var mergedSatisfactoryActions = left.GetSatisfactoryActions().Concat(right.GetSatisfactoryActions());
		var mergedFailingActions = left.GetFailingActions().SelectMany(x => right.GetFailingActions().Select(y => x += y));

		return new Preq<T>(mergedSatisfactoryActions, mergedFailingActions);
	}

	#endregion
}
