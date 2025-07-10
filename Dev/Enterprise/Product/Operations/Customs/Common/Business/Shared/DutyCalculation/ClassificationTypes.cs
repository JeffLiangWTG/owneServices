using System.Diagnostics.CodeAnalysis;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.Common
{
	[SuppressMessage("Microsoft.Maintainability", "CA1052:StaticHolderTypesShouldBeStaticOrNotInheritable", Justification = "This class is being inherited.")]
	public class ClassificationType
	{
		public const string EXP = ClassificationTypeList.Codes.Export;
		public const string IMP = ClassificationTypeList.Codes.Import;
		public const string Both = ClassificationTypeList.Codes.Both;
	}
}
