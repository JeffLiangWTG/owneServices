using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions
{
	public class EntitySetDefinitionTest : TransactionedTestCase
	{
		public void TestCreateAssociation()
		{
			var dummyBizo = info.Entities.FindDefinition("DummyBizo");
			AssertEquals(4, dummyBizo.Children.Count());

			foreach (EntityDefinition child in dummyBizo.Children)
			{
				AssertEquals(dummyBizo, child.Parent);
			}
		}

		public void TestEntityDefinitions_MultiThreadAccess()
		{
			var entitySetDefinition = info;
			var threads = new List<Thread>();
			var exceptions = new ConcurrentBag<Exception>();
			var set = new HashSet<string>();
			foreach (var entity in entitySetDefinition.Entities)
			{
				set.Add(entity.EntityName);
			}

			for (var i = 0; i < 20; i++)
			{
				var thread = new Thread(() =>
				{
					try
					{
						for (var j = 0; j < 1000000; j++)
						{
							foreach (var entity in entitySetDefinition.Entities)
							{
								Assert(set.Contains(entity.EntityName));
							}
						}
					}
					catch (Exception e)
					{
						exceptions.Add(e);
					}
				});

				threads.Add(thread);
			}

			foreach (var thread in threads)
			{
				thread.Start();
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}

			AssertEquals(0, exceptions.Count);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestUtil.AlterDummyTable();

			info = TestUtil.GetEntitySetDefinition("Dummy");
		}

		#endregion

		EntitySetDefinition info;
	}
}
