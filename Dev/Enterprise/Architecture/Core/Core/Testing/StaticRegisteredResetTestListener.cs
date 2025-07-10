#if DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core
{
	public class StaticRegisteredResetTestListener : BaseTestListener
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly StaticRegisteredResetTestListener Instance = new StaticRegisteredResetTestListener();

		StaticRegisteredResetTestListener()
		{
		}

		public void Register(Action reset)
		{
			registeredActions.Add(reset);
		}

		readonly List<Action> registeredActions = new List<Action>();

		public override void StartAllTests(DateTime startTime)
		{
			foreach (var registeredAction in registeredActions)
			{
				registeredAction();
			}
		}

		public override void AfterEachTest(DateTime endTime)
		{
			foreach (var registeredAction in registeredActions)
			{
				registeredAction();
			}
		}
	}
}
#endif
