using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BarcodeParsing.Business
{
	public interface IBarcodeRuleWithLength
	{
		public ZString LengthType { get; }

		public ZShort MinLength { get; set; }

		public ZShort MaxLength { get; set; }

		public ZPropertyInfo MinLengthInfo { get; }

		public ZPropertyInfo MaxLengthInfo { get; }
	}
}
