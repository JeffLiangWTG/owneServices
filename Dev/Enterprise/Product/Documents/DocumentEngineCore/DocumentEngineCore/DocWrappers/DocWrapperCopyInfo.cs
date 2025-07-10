using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	public abstract class DocWrapperCopyInfo
	{
		/// <summary>
		/// ReadOnly - this object is constructed from ReplacementTitle and number of CopyCount
		/// </summary>
		public TitleCopyCountPair TitleCopyCountPair
		{
			get { return new TitleCopyCountPair(Name, CopyCount); }
		}

		public MultilingualString Name
		{
			get { return fName; }
			set { fName = value; }
		}

		MultilingualString fName = (NoResString)"Document";

		public short CopyCount
		{
			get { return fCopyCount; }
			set { fCopyCount = value; }
		}

		short fCopyCount = 1;

		public PrintCopyType DeliveryMethod
		{
			get { return fDeliveryMethod; }
			set { fDeliveryMethod = value; }
		}

		PrintCopyType fDeliveryMethod = PrintCopyType.ALL;
	}
}
