using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.LogWalker
{
	/// <summary>
	/// This class is just for hygiene
	/// </summary>
	class ProcessableLogGroupStack : Disposable
	{
		readonly Stack<ProcessableLogGroup> stack = new Stack<ProcessableLogGroup>();

		#region Public API

		public void Push(ProcessableLogGroup group) => stack.Push(group);

		public ProcessableLogGroup Pop() => stack.Pop();

		public ProcessableLogGroup Peek() => stack.Peek();

		public int Count => stack.Count;

		#endregion

		#region Disposable

		protected override void Dispose(bool isDisposing)
		{
			foreach (var group in stack)
			{
				group.FreeLocks();
			}
		}

		#endregion
	}
}
