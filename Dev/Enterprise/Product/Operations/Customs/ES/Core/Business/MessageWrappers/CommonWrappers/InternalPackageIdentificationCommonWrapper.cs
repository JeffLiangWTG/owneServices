using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class InternalPackageIdentificationCommonWrapper : IInternalPackageIdentificationCommon
	{
		public InternalPackageIdentificationCommonWrapper(ZString marks, ZString type, ZLong elementsNum)
		{
			Tag = marks;
			ElementsType = type;
			NumberOfElements = elementsNum;
		}

		public ZString Tag { get; }

		public ZString ElementsType { get; }

		public ZLong NumberOfElements { get; }
	}
}
