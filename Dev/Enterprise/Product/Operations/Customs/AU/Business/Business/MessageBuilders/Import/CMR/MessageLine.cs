using System;
using Enterprise.Edifact.Auto;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class MessageLine
	{
		public abstract string StringValue { get; }

		public abstract SegmentGroup GetNewSegmentGroup(SegmentGroup message);

		protected internal abstract Type SegmentGroupType { get; }
	}
}
