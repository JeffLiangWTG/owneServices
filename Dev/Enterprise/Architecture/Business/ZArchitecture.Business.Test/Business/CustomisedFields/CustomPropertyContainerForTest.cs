using System.Collections.Generic;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class CustomPropertyContainerForTest : ICustomPropertyContainer
	{
		public IEnumerable<ICustomProperty> CustomProperties
		{
			get { return InnerCustomPropertyList; }
		}

		public readonly List<ICustomProperty> InnerCustomPropertyList = new List<ICustomProperty>();
	}
}
