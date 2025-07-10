using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public class AttributeMultiControlColumnStyle : ZMultiControlColumnStyle
	{
		public AttributeMultiControlColumnStyle(ZMultiControlColumnStyleInfo columnInfo)
			: base(() => new AttributeMultiCombinationControl(), columnInfo)
		{
		}
	}
}
