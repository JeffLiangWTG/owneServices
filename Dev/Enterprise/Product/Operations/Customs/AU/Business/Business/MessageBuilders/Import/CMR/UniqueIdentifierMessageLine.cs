using System;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.Auto;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class UniqueIdentifierMessageLine : MessageLine
	{
		public override sealed string StringValue
		{
			get
			{
				SegmentGroup tempGroup = (SegmentGroup)Activator.CreateInstance(SegmentGroupType);
				Populate(tempGroup, null);
				return tempGroup.ToString(new UNOCCMRCharacterSet());
			}
		}

		public abstract void Populate(SegmentGroup segmentGroup, string lineActionCode);

		public abstract string UniqueIdentifier
		{
			get;
		}

		public virtual ZDateTime LastMessageDate
		{
			get { return ZDateTime.Empty; }
		}
	}
}
