using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class Page : IPage
	{
		public Page(string name, int number)
		{
			Name = name;
			Number = number;
		}

		public string Name { get; }
		public int Number { get; }
	}
}