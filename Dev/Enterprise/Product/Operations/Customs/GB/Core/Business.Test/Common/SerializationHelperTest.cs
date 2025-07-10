using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing.Common
{
	sealed class SerializationHelperTest : TestCase
	{
		public void TestSerialize()
		{
			AssertNoExceptionThrown(() =>
			{
				var result = SerializationHelper.Serialize(declaration);
			});
		}

		CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration declaration => new()
		{
			GoodsShipment = new DeclarationGoodsShipment()
			{
				Consignee = new DeclarationGoodsShipmentConsignee()
				{
					Name = new ConsigneeNameTextType()
					{
						Value = string.Format("{0}Antonio", (char)0xD860)
					}
				}
			}
		};
	}
}
