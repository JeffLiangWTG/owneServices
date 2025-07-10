using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.Testing
{
	sealed class SecurityVectorTest : TestCaseWithFactory
	{
		public void TestNodesAreTheSameWhenPopulatedMultipleTimes_UserUninteractive()
		{
			var vector = new SecurityVector();
			vector.Initialise(Env.Security);
			var checkpoints1 = vector.Select(info => info.Checkpoint).ToArray();

			ISecurityCheckpoint[] checkpoints2;
			var originalValue = Globals.IsUserInteractive;
			Globals.IsUserInteractive = false;
			try
			{
				vector.Initialise(Env.Security);
				checkpoints2 = vector.Select(info => info.Checkpoint).ToArray();
			}
			finally
			{
				Globals.IsUserInteractive = originalValue;
			}

			var maxLength = Math.Max(checkpoints1.Length, checkpoints2.Length);
			for (int i = 0; i < maxLength; i++)
			{
				if (i >= checkpoints1.Length)
				{
					Fail(checkpoints1[i].DisplayTextPathToSecurityRight + "," + checkpoints1[i].LookupKey + " was unmatched in second collection");
				}
				if (i >= checkpoints2.Length)
				{
					Fail(checkpoints2[i].DisplayTextPathToSecurityRight + "," + checkpoints2[i].LookupKey + " was unmatched in first collection");
				}
				AssertEquals(checkpoints1[i].DisplayTextPathToSecurityRight + " != " + checkpoints2[i].DisplayTextPathToSecurityRight,
					checkpoints1[i].LookupKey, checkpoints2[i].LookupKey);
			}
		}

		public void TestNodesAreTheSameWhenPopulatedMultipleTimes()
		{
			var vector = new SecurityVector();
			vector.Initialise(Env.Security);
			var checkpoints1 = vector.Select(info => info.Checkpoint).ToArray();
			vector.Initialise(Env.Security);
			var checkpoints2 = vector.Select(info => info.Checkpoint).ToArray();

			var maxLength = Math.Max(checkpoints1.Length, checkpoints2.Length);
			for (var i = 0; i < maxLength; i++)
			{
				if (i >= checkpoints1.Length)
				{
					Fail(checkpoints1[i].DisplayTextPathToSecurityRight + "," + checkpoints1[i].LookupKey + " was unmatched in second collection");
				}
				if (i >= checkpoints2.Length)
				{
					Fail(checkpoints2[i].DisplayTextPathToSecurityRight + "," + checkpoints2[i].LookupKey + " was unmatched in first collection");
				}
				AssertEquals(checkpoints1[i].DisplayTextPathToSecurityRight + " != " + checkpoints2[i].DisplayTextPathToSecurityRight,
					checkpoints1[i].LookupKey, checkpoints2[i].LookupKey);
			}
		}

		public void TestNoNewSecurityCheckpointsWithOverriddenNames()
		{
			var vector = new SecurityVector();
			SecurityCore security = new SecurityCore(new GlbSecurityCollection(Factory), Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			vector.Initialise(security);
			var checkpoints1 = vector.Select(info => info.Checkpoint).ToArray();

			string[] exceptions = { "ASYCUDA Manifest", "Global Manifest", "Global Manifest Bill", "Transaction Approval", "Intercompany Transaction Approval", "Rates Service", "Barcode Validation", "Service Tasks Legacy", "Service Tasks" };
			for (var i = 0; i < checkpoints1.Length; ++i)
			{
				var checkpoint = (SecurityCheckpoint)checkpoints1[i];
				if (checkpoint.fDisplayTextOverride.Value != null
					&& checkpoint.fDisplayTextOverride.Value.GetUnresolvedString() != checkpoint.fDisplayText.GetUnresolvedString()
					&& !exceptions.Contains(checkpoint.fDisplayTextOverride.Value.GetUnresolvedString()))
				{
					Fail(string.Format(CultureInfo.InvariantCulture,
						@"A security checkpoint should have exactly one name no matter what. Have you considered changing the original name or making a new security checkpoint instead?
If you know for a fact only one module or the other can show up at a time (for example Transaction Approval case in WI00134032), then it's fine (add it to exceptions list of this unit test).
Original name: {0} New name: {1}", checkpoint.fDisplayText.GetUnresolvedString(), checkpoint.fDisplayTextOverride.Value.GetUnresolvedString()));
				}
			}
			Assert(true); //Passed!
		}

		public void TestNodeCheckpointsNotNull()
		{
			var vector = new SecurityVector();
			SecurityCore security = new SecurityCore(new GlbSecurityCollection(Factory), Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			vector.Initialise(security);

			foreach (ISecurityInfo info in vector.Nodes)
			{
				AssertCheckpointNotNull(info, 0);
			}
		}

		public void AssertCheckpointNotNull(ISecurityInfo info, int level)
		{
			Debug.Print(new String(' ', level * 2) + info.Name);
			AssertNotNull(info.Name, info.Checkpoint);

			foreach (ISecurityInfo child in info.Nodes)
			{
				if (child.IsGrouped)
				{
					AssertNotEquals(string.Format("The true parent of {0} should not be it's Grouping Parent.", child.Name), info.Checkpoint, child.Checkpoint.Parent);
				}
				else
				{
					if (child.Name != "Sales & Marketing" && child.Name != "DocManager" && child.Name != "Workflow Manager")
					{
						if (info.Checkpoint != child.Checkpoint.Parent)
						{
							AssertEquals(child.Name, info.Checkpoint, child.Checkpoint.Parent.Parent);
						}
						else
						{
							AssertEquals(child.Name, info.Checkpoint, child.Checkpoint.Parent);
						}
					}
				}

				Assert(child.IsChildOf(info));
				AssertCheckpointNotNull(child, level + 1);
			}
		}
	}
}
