using System.Drawing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class FontList : CodeDescriptionPairList
	{
		public FontList()
		{
			foreach (var family in FontFamily.Families)
			{
				AddPair(family.Name);
			}
		}
	}
}
