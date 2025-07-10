namespace Enterprise.DataTransfer.Business
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public struct FlatFileFieldProperty
	{
		public FlatFileFieldProperty(int name, int length)
		{
			this.Name = name;
			this.Length = length;
		}

		public readonly int Name;
		public readonly int Length;
	}
}
