using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class Declaration09ProviderTest : DataProviderTestCase<Declaration09Provider>
	{
		public void TestIDeclaration09()
		{
			Assert("Should implement IDeclaration09", Provider is IDeclaration09);
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("TemporaryStorageHeader missing", () => GenerateProvider(null));
			AssertNoExceptionThrown(() => GenerateProvider(header));
		}

		public void TestLRN()
		{
			header.LRN = "123";
			AssertEquals("LRN", "123", Provider.LRN);
		}

		public void TestMRN()
		{
			header.MRN = "123456789";
			AssertEquals("MRN", "123456789", Provider.MRN);
		}

		[TestDate(2023, 12, 07, 16, 45, 03)]
		public void TestDeclarationDateValue()
		{
			header.DeclarationDate = ZDateTime.Now;
			AssertEquals("DeclarationDate", ZDateTime.Now, Provider.DeclarationDateValue);
		}

		[TestDate(2023, 12, 07, 16, 45, 03)]
		public void TestPreviousDocuments()
		{
			header.AMA_DateAtCustomsOffice = ZDateTime.Now;
			CombineAssertions(() =>
			{
				Assert("PreviousDocument should be a ReadOnlyCollection<DateTime>", Provider.PreviousDocuments is ReadOnlyCollection<DateTime>);
				AssertEquals("Only 1 entry in PreviousDocument", 1, Provider.PreviousDocuments.Count);
				AssertEquals("PreviousDocument", ZDateTime.Now, Provider.PreviousDocuments.FirstOrDefault());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<TemporaryStorageHeader>();
		}
		TemporaryStorageHeader header;

		Declaration09Provider GenerateProvider(TemporaryStorageHeader header) => new Declaration09Provider(header);

		protected override Declaration09Provider GetProvider() => GenerateProvider(header);
	}
}
