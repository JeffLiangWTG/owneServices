using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class OrientationList : CodeDescriptionPairList
	{
		public OrientationList()
		{
			foreach (var item in Enum.GetNames(typeof(BMBoardSectionOrientation)))
			{
				AddPair(item);
			}
		}
	}
}
