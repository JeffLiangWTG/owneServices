namespace Enterprise.DataTransfer.Business
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public struct FixedWidthFlatFileFieldProperty
	{
		public FixedWidthFlatFileFieldProperty(int name, int position, int length)
		{
			this.Name = name;
			this.Position = position;
			this.Length = length;
		}

		public readonly int Name;
		public readonly int Position;
		public readonly int Length;
	}
}
