namespace Enterprise.DocumentScanning.Business
{
	public class DbMergeInfo
	{
		public DbMergeInfo(int number, string name, int size, bool isReadOnly)
		{
			this.number = number;
			this.name = name;
			Size = size;
			this.isReadOnly = isReadOnly;
		}

		public int Number
		{
			get { return number; }
		}
		readonly int number;

		public string Name
		{
			get { return name; }
		}
		readonly string name;

		public int Size { get; set; }

		public bool IsReadOnly
		{
			get { return isReadOnly; }
		}
		readonly bool isReadOnly;
	}
}
