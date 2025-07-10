using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	class ClassificationTypeProvider : Customs.Business.IClassificationTypeProvider
	{
		public ZString HTICode => ClassificationType.IMP;

		public ZString HTECode => ClassificationType.EXP;

		public ZString SHBCode => ZString.Empty;

		public ZString HTBCode => ClassificationType.Both;
	}
}
