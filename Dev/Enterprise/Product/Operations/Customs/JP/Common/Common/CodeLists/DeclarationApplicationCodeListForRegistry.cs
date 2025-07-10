using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Common
{
	public class DeclarationApplicationCodeListForRegistry : DeclarationApplicationCodeList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Codes : DeclarationApplicationCodeList.Codes
		{
			public const string BothBuiltInDefaulted = "BTH";
			public const string BothInterfaceDefaulted = "BIT";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Descriptions : DeclarationApplicationCodeList.Descriptions
		{
			public static MultilingualString BothBuiltInDefaulted { get { return ResString.GetMultilingualString("B3BB5699-318A-4726-AF46-2F343C9B6D9E", "Allow choosing entry submission method on Declaration. Built In ({0}) will default.", Codes.Builtin); } }
			public static MultilingualString BothInterfaceDefaulted { get { return ResString.GetMultilingualString("2192DAAB-8BD7-4B05-A7C4-065139CE18FC", "Allow choosing entry submission method on Declaration. Interfaced ({0}) will default.", Codes.Interfaced); } }
		}

		public DeclarationApplicationCodeListForRegistry()
			: base()
		{
			AddPair(Codes.BothBuiltInDefaulted, Descriptions.BothBuiltInDefaulted);
			AddPair(Codes.BothInterfaceDefaulted, Descriptions.BothInterfaceDefaulted);
		}
	}
}
