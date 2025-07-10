using System;

namespace Enterprise.DataTransfer.Native.Utils.Models
{
	public class Edge<T>
	{
		public Edge(T from, T to)
		{
			From = from;
			To = to;
		}

		public T From { get; }

		public T To { get; }

		public bool IsMateOf(T definition)
		{
			return From.Equals(definition) || To.Equals(definition);
		}

		public virtual T Mate(T node)
		{
			if (node.Equals(From))
			{
				return To;
			}

			if (node.Equals(To))
			{
				return From;
			}
			throw new ArgumentException("Could not find the other end of " + node + " is not belongs to " + this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer information")]
		public override string ToString()
		{
			return string.Format("From: {0}, To: {1}", From, To);
		}
	}
}
