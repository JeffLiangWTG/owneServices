using System;
using NUnit.Framework;

namespace Enterprise.Security.Core.Testing
{
	sealed class NoneSecurityCheckpointTest : TestCase
	{
		NoneSecurityCheckpoint checkpoint;

		NoneSecurityCheckpoint Checkpoint
		{
			get { return checkpoint ?? (checkpoint = new NoneSecurityCheckpoint()); }
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "AddChild() is not supported by NoneSecurityCheckpoint.")]
		public void TestAddChild()
		{
			Checkpoint.AddChild(null);
		}

		public void TestIsAllowed()
		{
			AssertEquals("IsAllowed", true, Checkpoint.IsAllowed);
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "ShowError() is not supported by NoneSecurityCheckpoint.")]
		public void TestShowError()
		{
			Checkpoint.ShowError();
		}

		public void TestVisible()
		{
			AssertEquals("Visible", false, Checkpoint.Visible);
		}
	}
}
