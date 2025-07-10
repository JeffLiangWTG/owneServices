using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.ZArchitecture.Schema;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class WarehouseCustomsLineDetails : DataTransfer.Universal.WarehouseCustomsLineDetails
	{
		internal WarehouseCustomsLineDetails(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, CommercialInvoiceHeader invoice, DataTransfer.Universal.WarehouseCustomsFallbackDetail fallbackDetail)
			: base(factory, invoiceLine, fallbackDetail)
		{
			this.invoice = invoice;
		}
		readonly CommercialInvoiceHeader invoice;

		DeclarationDetail declarationDetail
		{
			get { return (DeclarationDetail)FallbackDetail; }
		}

		public class DeclarationDetail : DataTransfer.Universal.WarehouseCustomsFallbackDetail
		{
			public bool IsWarehousedByExternalAgent;

			protected override DataTransfer.Universal.WarehouseCustomsFallbackDetail CloneCore()
			{
				return new DeclarationDetail()
				{
					IsExWarehouse = this.IsExWarehouse,
					IsWarehousedByExternalAgent = this.IsWarehousedByExternalAgent,
					SupplierAddress = this.SupplierAddress
				};
			}
		}

		protected override List<UniversalAddInfo> GetAddInfosApplicableForWarehousing()
		{
			var addInfos = new List<UniversalAddInfo>();
			if (InvoiceLine.AddInfoCollection != null)
			{
				addInfos.AddRange(InvoiceLine.AddInfoCollection.Where(x => x.Key.HasValue && x.Value.HasValue
					&& x.Key.Value != UniversalExtensions.TILV4Warehouse && !ConvertToZType(x).IsDefault));
			}
			if (invoice != null && invoice.AddInfoCollection != null)
			{
				foreach (var addInfo in invoice.AddInfoCollection.Where(x => x.Key.HasValue && x.Value.HasValue && !ConvertToZType(x).IsDefault))
				{
					if (GetKey(AUAddInfoSchema.Constants.ZA_HeaderREL_Hidden) == addInfo.Key.Value && addInfo.Value.Value == CMRRelatedTransaction.Yes.Code)
					{
						var rel_Hidden = addInfos.FirstOrDefault(x => GetKey(AUAddInfoSchema.Constants.ZA_REL_Hidden) == x.Key.Value);
						if (rel_Hidden == null || rel_Hidden.Value.Value == CMRRelatedTransaction.Default.Code)
						{
							if (rel_Hidden != null)
							{
								addInfos.Remove(rel_Hidden);
							}
							rel_Hidden = new UniversalAddInfo();
							rel_Hidden.Key = GetKey(AUAddInfoSchema.Constants.ZA_REL_Hidden);
							rel_Hidden.Value = CMRRelatedTransaction.Yes.Code;
							addInfos.Add(rel_Hidden);
						}
					}
					if (!addInfos.Any(x => x.Key.Value == addInfo.Key.Value))
					{
						addInfos.Add(addInfo);
					}
				}
			}
			return addInfos;
		}

		ZString GetKey(ZString schemaColumnName)
		{
			return schemaColumnName.Substring(3);
		}

		IZType ConvertToZType(UniversalAddInfo addInfo)
		{
			var schemaColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(AUAddInfoSchema.Constants.Prefix + "_" + addInfo.Key, AUAddInfoSchema.Constants.TableName);
			return BaseAddInfo.ConvertToZType(schemaColumn != null ? schemaColumn.GetEquivalentZType() : typeof(ZString), addInfo.Value.Value);
		}

		public override Country CountryOfOrigin
		{
			get
			{
				var result = base.CountryOfOrigin;
				if ((result == null || result.Code.GetValueOrDefault().IsEmpty) && invoice != null && invoice.AddInfoCollection != null)
				{
					var orgAddInfoKey = GetKey(AUAddInfoSchema.Constants.ZA_ORG);
					var orgAddInfo = invoice.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == orgAddInfoKey);
					if (orgAddInfo != null)
					{
						var countryCode = orgAddInfo.Value.GetValueOrDefault();
						if (countryCode.Length > 2)
						{
							countryCode = AUCCountryCodeToRefCountryCode.GetRefCountryCode(countryCode);
						}
						if (countryCode != "")
						{
							result = new Country() { Code = countryCode };
						}
					}
				}
				return result;
			}
		}

		public override ZDecimal? CustomsQuantity
		{
			get
			{
				if (HasWRQOverride)
				{
					return wRQAddInfoValue;
				}
				else
				{
					return base.CustomsQuantity;
				}
			}
		}

		public override CodeDescriptionPair6Char CustomsQuantityUnit
		{
			get
			{
				if (HasWRQOverride)
				{
					return new CodeDescriptionPair6Char() { Code = wRUAddInfoValue };
				}
				else
				{
					return base.CustomsQuantityUnit;
				}
			}
		}

		bool HasWRQOverride
		{
			get
			{
				if (!hasWRQOverrideCached.HasValue)
				{
					if (InvoiceLine.AddInfoCollection != null)
					{
						var wruAddInfoKey = GetKey(AUAddInfoSchema.Constants.ZA_WRU);
						var wRUAddInfo = InvoiceLine.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == wruAddInfoKey);
						wRUAddInfoValue = wRUAddInfo != null ? wRUAddInfo.Value.GetValueOrDefault() : ZString.Empty;
						var wrqAddInfoKey = GetKey(AUAddInfoSchema.Constants.ZA_WRQ);
						var wRQAddInfo = InvoiceLine.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == wrqAddInfoKey);
						wRQAddInfoValue = wRQAddInfo != null ? (ZDecimal)ConvertToZType(wRQAddInfo) : ZDecimal.Zero;
					}
					hasWRQOverrideCached = !wRUAddInfoValue.IsEmpty && wRQAddInfoValue > 0;
				}
				return hasWRQOverrideCached.Value;
			}
		}
		bool? hasWRQOverrideCached;
		ZString wRUAddInfoValue;
		ZDecimal wRQAddInfoValue;

		public override ZDecimal TILV
		{
			get
			{
				if (!fTILV.HasValue)
				{
					fTILV = ZDecimal.Zero;
					var tilvAddInfo = InvoiceLine.AddInfoCollection.GetTILV4Warehouse();
					if (tilvAddInfo != null && tilvAddInfo.Value.HasValue)
					{
						fTILV = ZDecimal.ParseSafe(tilvAddInfo.Value.Value, ZDecimal.Zero);
					}
				}
				return fTILV.Value;
			}
		}
		ZDecimal? fTILV;

		protected override ZString? GetEntryNumber()
		{
			return declarationDetail.IsWarehousedByExternalAgent ? GetAddInfoEntryNumber() : base.GetEntryNumber();
		}

		protected override ZShort? GetEntryLineNumber()
		{
			return declarationDetail.IsWarehousedByExternalAgent ? GetAddInfoEntryLineNumber() : base.GetEntryLineNumber();
		}

		protected override ZString? GetPreviousEntryNumber()
		{
			return declarationDetail.IsExWarehouse ? GetAddInfoEntryNumber() : null;
		}

		protected override ZShort? GetPreviousEntryLineNumber()
		{
			return declarationDetail.IsExWarehouse ? GetAddInfoEntryLineNumber() : null;
		}

		ZString? GetAddInfoEntryNumber()
		{
			var result = ZString.Empty;
			if (InvoiceLine.AddInfoCollection != null)
			{
				var entryNumberAddInfo = InvoiceLine.AddInfoCollection.GetEntryNumberForWarehouse();
				if (entryNumberAddInfo != null && entryNumberAddInfo.Value.HasValue)
				{
					result = entryNumberAddInfo.Value.GetValueOrDefault();
				}
			}
			return result;
		}

		ZShort? GetAddInfoEntryLineNumber()
		{
			var result = ZShort.Zero;
			if (InvoiceLine.AddInfoCollection != null)
			{
				var entryLineNumberAddInfo = InvoiceLine.AddInfoCollection.GetEntryLineNumberForWarehouse();
				if (entryLineNumberAddInfo != null && entryLineNumberAddInfo.Value.HasValue)
				{
					result = ZShort.ParseSafe(entryLineNumberAddInfo.Value.Value, ZShort.Zero);
				}
			}
			return result;
		}
	}
}
