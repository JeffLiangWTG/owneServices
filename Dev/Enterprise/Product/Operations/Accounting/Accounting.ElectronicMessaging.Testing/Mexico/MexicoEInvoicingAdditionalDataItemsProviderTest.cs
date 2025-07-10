using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Moq;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	public class MexicoEInvoicingAdditionalDataItemsProviderTest : TestCaseWithFactory
	{
		public void TestAdditionaDataItem_When_InvoiceAndStatementLogo_ExceedsMaximumSize()
		{
			(GlbBranch branch, UniversalTransactionBatch batch, Mock<ICountryEInvoicingObjectFactory> countryFactoryMock, AccEInvoicingBatch accEInvoiceBatch) = SetDataForTest();

			using (var image = new Bitmap(2000, 5000))
			using (var stream = new MemoryStream())
			{
				image.Save(stream, ImageFormat.Jpeg);
				var invoiceAndStatementLogo = new Bitmap(stream);
				SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, invoiceAndStatementLogo);

				var warnings = new Common.Logger();
				var additionalDataItemProvider = new MexicoEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
				var additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);
				AssertEquals(nameof(additionalDataItem.Count), 0, additionalDataItem.Count);
				AssertEquals(nameof(warnings), "Invoice logo file size has exceeded the maximum size", warnings.ToString());
			}
		}

		public void TestAdditionaDataItem_When_InvoiceAndStatementLogo_IsNull()
		{
			(GlbBranch branch, UniversalTransactionBatch batch, Mock<ICountryEInvoicingObjectFactory> countryFactoryMock, AccEInvoicingBatch accEInvoiceBatch) = SetDataForTest();

			SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);

			var warnings = new Common.Logger();
			var additionalDataItemProvider = new MexicoEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);
			AssertEquals(nameof(additionalDataItem.Count), 0, additionalDataItem.Count);
			AssertNullOrEmpty(warnings.ToString());
		}

		public void TestAdditionaDataItem_Contains_InvoiceAndStatementLogo_AtCompanyLevel()
		{
			(GlbBranch branch, UniversalTransactionBatch batch, Mock<ICountryEInvoicingObjectFactory> countryFactoryMock, AccEInvoicingBatch accEInvoiceBatch) = SetDataForTest();

			SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new Bitmap(2, 2));

			var warnings = new Common.Logger();
			var additionalDataItemProvider = new MexicoEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);
			AssertEquals(nameof(additionalDataItem.Count), 1, additionalDataItem.Count);
			AssertEquals("AdditionalDataItem.Key", expectedInvoiceAndStatementLogoKey, additionalDataItem[0].Key);
			AssertEquals("AdditionalDataItem.Value", expectedInvoiceAndStatementLogo, additionalDataItem[0].Value);
			AssertNullOrEmpty(warnings.ToString());
		}

		public void TestAdditionaDataItem_Contains_InvoiceAndStatementLogo_AtBranchLevel()
		{
			(GlbBranch branch, UniversalTransactionBatch batch, Mock<ICountryEInvoicingObjectFactory> countryFactoryMock, AccEInvoicingBatch accEInvoiceBatch) = SetDataForTest();

			SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, new Bitmap(2, 2));

			var warnings = new Common.Logger();
			var additionalDataItemProvider = new MexicoEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);
			AssertEquals(nameof(additionalDataItem.Count), 1, additionalDataItem.Count);
			AssertEquals("AdditionalDataItem.Key", expectedInvoiceAndStatementLogoKey, additionalDataItem[0].Key);
			AssertEquals("AdditionalDataItem.Value", expectedInvoiceAndStatementLogo, additionalDataItem[0].Value);
			AssertNullOrEmpty(warnings.ToString());
		}

		public void TestAdditionaDataItem_Contains_InvoiceAndStatementLogo_AtDepartmentLevel()
		{
			(GlbBranch branch, UniversalTransactionBatch batch, Mock<ICountryEInvoicingObjectFactory> countryFactoryMock, AccEInvoicingBatch accEInvoiceBatch) = SetDataForTest();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "XXX";
			batch.TransactionCollection[0].Department = new Department() { Code = department.GE_Code };

			Factory.Save();

			SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(Guid.Empty, Guid.Empty, department.PK.ToGuid(), new Bitmap(2, 2));

			var warnings = new Common.Logger();
			var additionalDataItemProvider = new MexicoEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItem = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactoryMock.Object, warnings);
			AssertEquals(nameof(additionalDataItem.Count), 1, additionalDataItem.Count);
			AssertEquals("AdditionalDataItem.Key", expectedInvoiceAndStatementLogoKey, additionalDataItem[0].Key);
			AssertEquals("AdditionalDataItem.Value", expectedInvoiceAndStatementLogo, additionalDataItem[0].Value);
			AssertNullOrEmpty(warnings.ToString());
		}

		const string expectedInvoiceAndStatementLogoKey = "InvoiceLogo";
		const string expectedInvoiceAndStatementLogo = "/9j/4AAQSkZJRgABAQEAYABgAAD/4QBaRXhpZgAATU0AKgAAAAgABQMBAAUAAAABAAAASgMDAAEAAAABAAAAAFEQAAEAAAABAQAAAFERAAQAAAABAAAOw1ESAAQAAAABAAAOwwAAAAAAAYagAACxj//bAEMACAYGBwYFCAcHBwkJCAoMFA0MCwsMGRITDxQdGh8eHRocHCAkLicgIiwjHBwoNyksMDE0NDQfJzk9ODI8LjM0Mv/bAEMBCQkJDAsMGA0NGDIhHCEyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMv/AABEIAAIAAgMBIgACEQEDEQH/xAAfAAABBQEBAQEBAQAAAAAAAAAAAQIDBAUGBwgJCgv/xAC1EAACAQMDAgQDBQUEBAAAAX0BAgMABBEFEiExQQYTUWEHInEUMoGRoQgjQrHBFVLR8CQzYnKCCQoWFxgZGiUmJygpKjQ1Njc4OTpDREVGR0hJSlNUVVZXWFlaY2RlZmdoaWpzdHV2d3h5eoOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4eLj5OXm5+jp6vHy8/T19vf4+fr/xAAfAQADAQEBAQEBAQEBAAAAAAAAAQIDBAUGBwgJCgv/xAC1EQACAQIEBAMEBwUEBAABAncAAQIDEQQFITEGEkFRB2FxEyIygQgUQpGhscEJIzNS8BVictEKFiQ04SXxFxgZGiYnKCkqNTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqCg4SFhoeIiYqSk5SVlpeYmZqio6Slpqeoqaqys7S1tre4ubrCw8TFxsfIycrS09TV1tfY2dri4+Tl5ufo6ery8/T19vf4+fr/2gAMAwEAAhEDEQA/APn+iiigD//Z";

		(GlbBranch branch, UniversalTransactionBatch batch, Mock<ICountryEInvoicingObjectFactory> countryFactoryMock, AccEInvoicingBatch accEInvoiceBatch) SetDataForTest()
		{
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();

			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;

			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			batch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch() { Code = branch.GB_Code }
			});

			return (branch, batch, countryFactoryMock, accBatch);
		}
	}
}
