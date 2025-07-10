using System.Data;

namespace Enterprise.ZArchitecture.Environment
{
	public struct AWBDocumentTitleItem
	{
#if DEBUG
		public AWBDocumentTitleItem(string name, string title, bool printed)
		{
			Name = name;
			Title = title;
			Printed = printed;
		}
#endif

		internal AWBDocumentTitleItem(DataRow rowWithDocumentTitleData)
		{
			Name = (string)rowWithDocumentTitleData["Name"];
			Title = (string)rowWithDocumentTitleData["Title"];
			Printed = bool.Parse((string)rowWithDocumentTitleData["Printed"]);
		}

		public readonly string Name;
		public readonly string Title;
		public readonly bool Printed;
	}
}
