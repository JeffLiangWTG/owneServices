using QuikGraph;

namespace Enterprise.BufferManagement.Business
{
	public class EdgeWithOffset : IEdge<ScheduleNode>
	{
		public EdgeWithOffset(ScheduleNode source, ScheduleNode target)
		{
			this.source = source;
			this.target = target;
		}

		readonly ScheduleNode source;
		readonly ScheduleNode target;

		public ScheduleNode Source
		{
			get { return source; }
		}

		public ScheduleNode Target
		{
			get { return target; }
		}

		public override int GetHashCode()
		{
			return source.GetHashCode() ^ target.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var other = obj as EdgeWithOffset;
			if (other != null)
			{
				return source == other.source
					&& target == other.target
					&& Offset == other.Offset
					&& PassDirection == other.PassDirection;
			}
			else
			{
				return base.Equals(obj);
			}
		}

		public decimal Offset { get; set; }
		public PassDirection PassDirection { get; set; }
	}
}
