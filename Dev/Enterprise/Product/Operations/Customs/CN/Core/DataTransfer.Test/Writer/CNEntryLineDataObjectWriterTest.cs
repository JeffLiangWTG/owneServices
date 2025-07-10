using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.CN.DataTransfer.Universal.Testing
{
	class CNEntryLineDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestNameOfGoodsAndGoodsSpecModel()
		{
			var entryLine = Factory.New<Business.CusEntryLine>();

			var writer = new CNEntryLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new CNDataObjectWriterHelper(Factory.BOFactory));
			var result = writer.GetDataObject(entryLine);

			var addInfos = result.AddInfoCollection;
			Assert(addInfos.Count >= 2);
			Assert(addInfos.Any(addinfo => addinfo.Key.GetValueOrDefault() == Constants.AddInfoKeys.EntryLine.NameOfGoods));
			Assert(addInfos.Any(addinfo => addinfo.Key.GetValueOrDefault() == Constants.AddInfoKeys.EntryLine.GoodsSpecModel));
			Assert(addInfos.Any(addinfo => addinfo.Key.GetValueOrDefault() == Constants.AddInfoKeys.EntryLine.UnitPrice));
			Assert(addInfos.Any(addinfo => addinfo.Key.GetValueOrDefault() == Constants.AddInfoKeys.EntryLine.TotalPrice));
			Assert(addInfos.Any(addinfo => addinfo.Key.GetValueOrDefault() == Constants.AddInfoKeys.EntryLine.CurrencyCode));
		}
	}
}
