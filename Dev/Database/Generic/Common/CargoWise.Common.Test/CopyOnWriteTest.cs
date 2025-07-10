using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	class CopyOnWriteTest : TestCase
	{
		public void TestUpdateValueThreadSafeWithInitNullReference()
		{
			var instance = new CopyOnWrite<int[]>();
			var tasks = new Task[100];
			for (int i = 0; i < tasks.Length; i++)
			{
				tasks[i] = new Task((object value) =>
				{
					instance.UpdateValue((int[] set) =>
					{
						return set == null ? new int[] { (int)value } : ArrayUtil.Append(set, (int)value);
					});
				}, i);
			}

			for (int i = 0; i < tasks.Length; i++)
			{
				tasks[i].Start();
			}

			Task.WaitAll(tasks);
			AssertContainsExactElementsInAnyOrder(Enumerable.Range(0, 100), instance.GetValue());
		}
	}
}