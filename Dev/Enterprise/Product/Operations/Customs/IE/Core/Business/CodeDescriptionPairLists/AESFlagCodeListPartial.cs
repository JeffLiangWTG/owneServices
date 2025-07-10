using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	partial class AESFlagCodeList
	{
		public static string GetContainerIndicator(JobDeclaration declaration) => declaration.CusContainers.Count > 0 ? Codes.Yes : Codes.No;
	}
}
