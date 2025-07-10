using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;

namespace Enterprise.VisualBoards.Business
{
	public static class SectionDescriptorProvider
	{
		public static IBoardSectionDescriptor Get(string sectionType)
		{
			return GetAllDescriptors().SingleOrDefault(d => d.Type == sectionType);
		}

		public static IEnumerable<string> GetSupportedSectionTypes()
		{
			return GetAllDescriptors().Select(d => d.Type);
		}

		internal static IEnumerable<IBoardSectionDescriptor> GetAllDescriptors()
		{
			return ObjectFactory.Get<IEnumerable>("VisualBoardSectionDescriptors").Cast<IBoardSectionDescriptor>();
		}
	}
}
