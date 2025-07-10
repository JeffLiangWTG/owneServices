using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine
{
	public class OverflowNote : DocumentWrapper
	{
		public OverflowNote(ZString title, ZString contents)
		{
			this.Title = title;
			this.Contents = contents;
		}

		public ZString Title
		{
			get;
			private set;
		}

		public ZString Contents
		{
			get;
			private set;
		}
	}
}
