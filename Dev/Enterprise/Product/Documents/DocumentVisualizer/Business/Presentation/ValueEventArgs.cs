using System;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public sealed class ValueEventArgs<T> : EventArgs
	{
		public ValueEventArgs(T value)
		{
			Value = value;
		}

		public T Value { get; private set; }
	}
}