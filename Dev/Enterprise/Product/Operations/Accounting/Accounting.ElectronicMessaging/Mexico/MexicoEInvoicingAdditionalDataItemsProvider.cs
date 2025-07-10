using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Universal;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public class MexicoEInvoicingAdditionalDataItemsProvider : IAdditionalDataItemsProvider
	{
		public GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection GetAdditionalHeaderDataItems(AccEInvoicingBatch batch, GlbBranch branch, UniversalTransactionBatch universalTransactionBatch, ICountryEInvoicingObjectFactory countryFactory, INotifications warnings)
		{
			var items = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection();
			var additionalItem = AddItemInvoiceLogo(branch, universalTransactionBatch, warnings);
			if (additionalItem != null)
			{
				items.Add(additionalItem);
			}

			return items;
		}

		#region Invoice Logo

		GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem AddItemInvoiceLogo(GlbBranch branch, UniversalTransactionBatch batch, INotifications warnings)
		{
			var logoAsString = GetInvoiceLogoAsString(branch, batch, warnings);

			if (!string.IsNullOrEmpty(logoAsString))
			{
				return new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem()
				{
					Key = "InvoiceLogo",
					Value = logoAsString
				};
			}

			return null;
		}

		string GetInvoiceLogoAsString(GlbBranch branch, UniversalTransactionBatch batch, INotifications warnings)
		{
			// Pack provider information
			// Customization of your CFDI's logo (.jpg)
			// - Recommendation: 592px wide by 471px high.
			// - Resolution: 72px / inch.
			// - Maximum file size allowed: 150Kb.
			// Must be sent in Base64 as a String.

			var logoAsString = string.Empty;
			var departmentId = GetDepartmentId(batch);
			var logoAsImage = SystemDataRegistry.Instance.InvoceAndStatementLogo.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), departmentId);
			if (logoAsImage != null)
			{
				var logoAsBytes = ConvertFromImageToBytes(logoAsImage, warnings);
				if (logoAsBytes.Length > 0)
				{
					logoAsString = Convert.ToBase64String(logoAsBytes);
				}
			}

			return logoAsString ;
		}

		byte[] ConvertFromImageToBytes(Image image, INotifications warnings)
		{
			var imageToBytes = Array.Empty<byte>();

			if (image != null)
			{
				using (var stream = new MemoryStream())
				{
					image.Save(stream, ImageFormat.Jpeg);

					if (stream.Length <= MaxSizeInBytes)
					{
						imageToBytes = stream.CopyToByteArray();
					}
					else
					{
						warnings.AddWarning(Res.GetString("ED651AFC-68FE-4CB9-8E23-F4744D815D0A", "Invoice logo file size has exceeded the maximum size"));
					}
				}
			}

			return imageToBytes;
		}

		Guid GetDepartmentId(UniversalTransactionBatch batch)
		{
			var dataLoader = new TransactionBatchDataLoader();

			var (department, _) = dataLoader.LoadDepartment(batch);

			return department == null ? Guid.Empty : department.PK.ToGuid();
		}

		const int MaxSizeInBytes = 8 * 1024;

		#endregion
	}
}
