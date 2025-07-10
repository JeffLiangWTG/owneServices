using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map
{
	[DefaultField("TitleText")]
	public class SyntaxAndFormattingWrapper : GenericWrapper
	{
		public SyntaxAndFormattingWrapper(string titleText, string lineText, BusinessObjectFactory factory)
			: base(null, factory)
		{
			fTitleText = titleText;
			fLineText = lineText;
		}

		public ZString TitleText
		{
			get { return fTitleText; }
		}
		readonly ZString fTitleText;

		public ZString LineText
		{
			get { return fLineText; }
		}
		readonly ZString fLineText;
	}
}
