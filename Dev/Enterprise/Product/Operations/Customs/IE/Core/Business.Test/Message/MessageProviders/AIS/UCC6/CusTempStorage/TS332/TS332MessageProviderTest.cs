using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class TS332MessageProviderTest : DataProviderTestCase<TS332MessageProvider>
	{
		protected override TS332MessageProvider GetProvider() => new TS332MessageProvider(new TemporaryStorageMessageSendingObject(header));

		public void TestITS332Header()
		{
			Assert("Should implement ITS332Header", Provider is ITS332Header);
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Should throw ArgumentException with null header.", () => new TS332MessageProvider(null));
		}

		public void TestDeclaration()
		{
			Assert("Should be Declaration09Provider", Provider.Declaration is Declaration09Provider);
		}

		public void TestRepresentative() => CombineAssertions(() =>
		{
			AssertNull("No representative added to header", Provider.Representative);
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			header.AMA_OA_Representative = orgAddress.PK;
			var representative = GetProvider().Representative;
			AssertType<RepresentativeProvider>("Representative added to header", representative);
			Assert("Should be IRepresentativeType05", representative is IRepresentativeType05);
		});

		public void TestDeclarant()
		{
			Assert("Should be DeclarantProvider", Provider.Declarant is DeclarantProvider);
		}

		public void TestPersonPresentingTheGoodsID()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORIPPG");
			var orgAddress = orgHeader.MainAddress;
			header.AMA_OA_Presenter = orgAddress.PK;
			AssertEquals("TemporaryStorageHeader.AMA_OA_Presenter - EORI", "IEEORIPPG", Provider.PersonPresentingTheGoodsID);
		}

		public void TestPresentationOffice()
		{
			header.PresentationCustomsOffice = "PCO";
			AssertEquals("TemporaryStorageHeader.PresentationCustomsOffice", "PCO", Provider.PresentationOffice);
		}

		public void TestCustomsOfficeLodgement()
		{
			header.CustomsOfficeOfLodgement = "COL";
			AssertEquals("TemporaryStorageHeader.CustomsOfficeOfLodgement", "COL", Provider.CustomsOfficeLodgement);
		}

		public void TestConsignment()
		{
			var consignment = GetProvider().Consignment;
			CombineAssertions(() =>
			{
				AssertType<ConsignmentProvider>("Consignment added to header", consignment);
				Assert("Should be IConsignment", consignment is IConsignment);
				AssertNotNull("LocationOfGoods is not null", consignment.LocationOfGoods);
			});
		}

		public void TestFallbackProcedure()
		{
			AssertType<FallbackProcedureProvider>("TS332MessageProvider.FallbackProcedure should return object of FallbackProcedure", Provider.FallbackProcedure);
		}

		[TestDate(2023, 11, 22, 08, 45, 00)]
		public void TestPreparationDateAndTime()
		{
			AssertEquals("PreparationDateAndTime", new DateTime(2023, 11, 22, 08, 45, 00), Provider.PreparationDateAndTime);
		}

		TemporaryStorageHeader header;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		}
	}
}
