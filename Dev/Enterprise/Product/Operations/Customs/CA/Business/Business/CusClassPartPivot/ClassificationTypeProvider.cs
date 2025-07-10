using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	class ClassificationTypeProvider : Customs.Business.IClassificationTypeProvider
	{
		public ZString HTICode => ClassificationTypeList.Codes.HTI;

		public ZString HTECode => ClassificationTypeList.Codes.HTE;

		public ZString SHBCode => ClassificationTypeList.Codes.SHB;

		public ZString HTBCode => ZString.Empty;
	}
}
