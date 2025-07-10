using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class CommercialInvoiceHeaderDataObjectWriter : Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter
	{
		public CommercialInvoiceHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, ILandedCostDataWriter landedCostDataWriter = null, Customs.Business.CusEntryHeader relatedEntry = null)
			: base(manager, helper, landedCostDataWriter, relatedEntry)
		{
		}

		protected override void PopulateCountrySpecificLineData(CommercialInvoiceLine invoiceLineData, Customs.Business.BaseJobComInvoiceLine invoiceLineBO)
		{
			if (invoiceLineBO is JobComInvoiceLine line && line.Declaration is JobDeclaration declaration && declaration.Configuration.InvoiceLineConfiguration.OrganizationsSupport(declaration))
			{
				var result = invoiceLineData.CustomsReferenceCollection ?? new List<CustomsReference>();
				UniversalCustomsDataObjectProvider.PopulateCustomsReference(result, line.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>(), helper, writeManager);
				invoiceLineData.SetCustomsReferenceCollection(() => { return result; });
			}
		}

		protected new UniversalDataObjectWriterHelper helper => (UniversalDataObjectWriterHelper)base.helper;

		protected override List<AddInfoGroup> GetInvoiceAddInfoGroupCollection(Customs.Business.BaseJobComInvoiceHeader invoiceBO)
		{
			return DeclarationDataObjectWriter.CreateOldSupportingInfoDataIfNeeded(base.GetInvoiceAddInfoGroupCollection(invoiceBO), invoiceBO as Integration.Customs.ICusSupportingInfoTypeSupporter, helper);
		}

		protected override List<AddInfoGroup> GetInvoiceLineAddInfoGroupCollection(Customs.Business.BaseJobComInvoiceLine invoiceLineBO)
		{
			return CreateOldTaxDataIfNeeded(DeclarationDataObjectWriter.CreateOldSupportingInfoDataIfNeeded(base.GetInvoiceLineAddInfoGroupCollection(invoiceLineBO), invoiceLineBO as Integration.Customs.ICusSupportingInfoTypeSupporter, helper), invoiceLineBO);
		}

		List<AddInfoGroup> CreateOldTaxDataIfNeeded(List<AddInfoGroup> list, Customs.Business.BaseJobComInvoiceLine invoiceLineBO)
		{
			if (Registry.EUCustomsDataRegistry.Instance.AllowExportAndImportOfOldStyleCusAddInfoRowsInUniversalAsWellAsNewStyleTaxOrFee.Value)
			{
				// Additionally write to AddInfo
				var query = new ZQuery(JobComInvoiceLineTaxSchema.JLT_JI, invoiceLineBO.PK);
				var bizObjs = helper.Load<JobComInvoiceLineTax>(query);
				if (bizObjs.Any())
				{
					list = list ?? new List<AddInfoGroup>();
					foreach (var taxBO in bizObjs)
					{
						var addInfoGroup = new AddInfoGroup() { Type = new CodeDescriptionPair() { Code = Constants.OldCusAddInfoTypes.Tax.TaxAddInfoTypeCodeGTX }, AddInfoCollection = new List<AddInfo>() };
						foreach (var field in new[]
													{
													Tuple.Create<IZType, string>(taxBO.JLT_Amount, Constants.OldCusAddInfoTypes.Tax.Fields.Amount),
													Tuple.Create<IZType, string>(taxBO.JLT_BaseQuantity, Constants.OldCusAddInfoTypes.Tax.Fields.BaseQuantity),
													Tuple.Create<IZType, string>(taxBO.JLT_BaseValue, Constants.OldCusAddInfoTypes.Tax.Fields.BaseAmount),
													Tuple.Create<IZType, string>(taxBO.JLT_MethodOfPayment, Constants.OldCusAddInfoTypes.Tax.Fields.MethodOfPayment),
													Tuple.Create<IZType, string>(taxBO.JLT_RateOverrideReasonCode, Constants.OldCusAddInfoTypes.Tax.Fields.RateOverride),
													Tuple.Create<IZType, string>(taxBO.JLT_Type, Constants.OldCusAddInfoTypes.Tax.Fields.Tty),
													Tuple.Create<IZType, string>(taxBO.JLT_MethodOfCalculation.PadRight(4, ' ').Left(3).Trim(), Constants.OldCusAddInfoTypes.Tax.Fields.RateDuty),
													Tuple.Create<IZType, string>(taxBO.JLT_MethodOfCalculation.PadRight(4, ' ').Right(1).Trim(), Constants.OldCusAddInfoTypes.Tax.Fields.RateSuspension)
												})
						{
							var addInfo = new AddInfo() { Key = field.Item2, Value = field.Item1.ToString() };
							addInfoGroup.AddInfoCollection.Add(addInfo);
						}
						list.Add(addInfoGroup);
					}
				}
			}
			return list;
		}
	}
}
