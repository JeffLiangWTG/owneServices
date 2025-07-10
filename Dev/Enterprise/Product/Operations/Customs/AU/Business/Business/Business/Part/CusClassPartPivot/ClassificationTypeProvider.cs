using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class ClassificationTypeProvider : Customs.Business.IClassificationTypeProvider
	{
		public ZString HTICode => Customs.Business.ClassificationTypeList.Codes.HTI;

		public ZString HTECode => Customs.Business.ClassificationTypeList.Codes.HTE;

		public ZString SHBCode => ZString.Empty;

		public ZString HTBCode => ZString.Empty;
	}
}
