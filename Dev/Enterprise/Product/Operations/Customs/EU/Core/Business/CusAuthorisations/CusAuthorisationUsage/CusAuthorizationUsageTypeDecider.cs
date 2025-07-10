using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business
{
	public class CusAuthorizationUsageTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ICusAuthorizationUsage>),
			new CountrySpecificType(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusAuthorizationUsage>),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusAuthorizationUsage);

		protected override Type DefaultTypeForEuCountry => typeof(CusAuthorizationUsage);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetCountryCode(row, factory));

		protected ZString GetCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			ZString? result = null;
			if (row != null)
			{
				var tableCode = new ZString(row[CusAuthorizationUsage.Schema.AGC_ParentTableCode]);
				var parentPK = new ZGuid(row[CusAuthorizationUsage.Schema.AGC_ParentID]);

				switch (tableCode)
				{
					case CusEntryInstructionSchema.Constants.Prefix:
						result = GetCountryCodeFromInstruction(parentPK, factory);
						break;
					case JobComInvoiceLineSchema.Constants.Prefix:
						result = GetCountryCodeFromInvoiceLine(parentPK, factory);
						break;
					case AsycudaManifestHeaderSchema.Constants.Prefix:
						result = GetCountryCodeFromTemporaryStorage(parentPK, factory);
						break;
				}
			}
			return result ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		ZString? GetCountryCodeFromInvoiceLine(ZGuid invoiceLinePK, BusinessObjectFactory factory)
		{
			var invoiceLine = factory.Load<Customs.Business.BaseJobComInvoiceLine>(invoiceLinePK);
			return invoiceLine?.InvoiceHeader?.CountryCode;
		}

		ZString? GetCountryCodeFromInstruction(ZGuid instructionPK, BusinessObjectFactory factory)
		{
			var instruction = factory.Load<Customs.Business.CusEntryInstruction>(instructionPK);
			return instruction?.CountryCode;
		}

		ZString? GetCountryCodeFromTemporaryStorage(ZGuid temporaryStoragePK, BusinessObjectFactory factory)
		{
			var temporaryStorage = factory.Load<TemporaryStorageHeader>(temporaryStoragePK);
			return temporaryStorage?.AMA_RN_NKCountry;
		}
	}
}
