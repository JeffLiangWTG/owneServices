using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	public class DLMDetail : MessageBlock
	{
		public DLMDetail()
			: base("D")
		{
		}

		//[MessageBlockString(2, 2)]
		//public ZString NotUsed1;

		[MessageBlockString(4, 20)]
		public ZString CountryOfOrigin;

		//[MessageBlockString(24, 2)]
		//public ZString NotUsed2;

		[MessageBlockString(26, 30)]
		public ZString ProvinceOfOrigin;

		[MessageBlockString(56, 10)]
		public ZString HarmonizedSystemCode;

		[MessageBlockString(66, 255)]
		public ZString ProductDescription;

		[MessageBlockString(321, 30)]
		public ZString ConveyanceIdentificationNumber;

		//[MessageBlockString(351, 14)]
		//public ZString NotUsed3;

		//[MessageBlockString(365, 3)]
		//public ZString NotUsed4;

		//[MessageBlockString(368, 50)]
		//public ZString NotUsed5;

		[MessageBlockDecimal(418, 14)]
		public ZDecimal Quantity;

		//[MessageBlockString(432, 3)]
		//public ZString NotUsed6;

		[MessageBlockString(435, 50)]
		public ZString UnitOfMeasure;

		[MessageBlockDecimal(485, 16)]
		public ZDecimal ValueFOBPointOfExit;
	}
}
