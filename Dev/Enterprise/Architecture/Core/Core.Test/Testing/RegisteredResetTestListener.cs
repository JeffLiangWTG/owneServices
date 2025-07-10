using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class RegisteredResetTestListener : BaseTestListener
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly RegisteredResetTestListener Instance = new RegisteredResetTestListener();

		readonly List<Action> registeredResetActions = new List<Action>();
		public void RegisterResetAction(Action reset)
		{
			registeredResetActions.Add(reset);
		}

		public override void BeforeEachTest(DateTime startTime)
		{
			registeredResetActions.Clear();
		}

		public override void AfterEachTest(DateTime endTime)
		{
			foreach (var action in registeredResetActions)
			{
				action.Invoke();
			}
		}
	}
}
