using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DUAImportExporterWrapper : PartyWrapper, IDUACompleteImportExporterProvider
	{
		public static DUAImportExporterWrapper New(CusEntryHeader cusEntryHeader)
		{
			var suppliers = cusEntryHeader?.InvoiceHeaders?.Select(x => x.Supplier).Distinct();
			var orgHeader = suppliers?.FirstOrDefault();
			var isMultipleSuppliers = suppliers?.Skip(1)?.Any() ?? false;

			if (orgHeader == null)
			{
				return null;
			}
			else
			{
				var supplierAddress = cusEntryHeader.InvoiceHeaders?.FirstOrDefault().SupplierAddress;
				var addressToSend = supplierAddress ?? orgHeader.MainAddress;
				return new DUAImportExporterWrapper(addressToSend, cusEntryHeader, isMultipleSuppliers);
			}
		}

		DUAImportExporterWrapper(OrgAddress address, CusEntryHeader cusEntryHeader, ZBool isMultipleSuppliers)
			: base(address)
		{
			entryHeader = cusEntryHeader;
			declaration = entryHeader.Declaration;
			this.isMultipleSuppliers = isMultipleSuppliers;
		}

		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		readonly ZBool isMultipleSuppliers;

		const string SupDocCode9011 = "9011";
		const string SupDocCode1015 = "1015";
		const string SimplifiedProcedureA = "A";
		const string SimplifiedProcedureB = "B";
		const string MultipleSuppliersCode = "00200";

		public ZString SimplifiedProcedureType
		{
			get
			{
				if (simplifiedProcedureType == null && !isMultipleSuppliers)
				{
					simplifiedProcedureType = new CachedProperty<ZString>(declaration.Factory, () =>
					{
						var procedureType = ZString.Empty;

						var entryStyle = declaration.JE_MessageSubType;
						var originCountry = declaration.JE_GoodsOrigin;
						if (entryStyle == EU.Business.EntryStyleListImport.Codes.ImportFromSpecialTerritory && originCountry == Core.Constants.CountryCodes.Spain)
						{
							var supportingDocuments = GetAllSupportingDocuments();

							var has9011Code = supportingDocuments.Any(x => x.CSI_Code == SupDocCode9011);
							var has1015Code = supportingDocuments.Any(x => x.CSI_Code == SupDocCode1015);

							if (has9011Code && has1015Code)
							{
								procedureType = SimplifiedProcedureB;
							}
							else if (has9011Code && !has1015Code)
							{
								procedureType = SimplifiedProcedureA;
							}
						}

						return procedureType;
					});
				}
				return isMultipleSuppliers ? null : simplifiedProcedureType.Value;

				IEnumerable<SupportingDocument> GetAllSupportingDocuments()
				{
					var entryLinesSupportingDocuments = entryHeader.MergedLines.Cast<CusEntryLine>().SelectMany(x => x.SupportingDocuments).Cast<SupportingDocument>();
					var entryHeaderSupportingDocuments = entryHeader.SupportingDocuments.Cast<SupportingDocument>();
					return entryLinesSupportingDocuments.Concat(entryHeaderSupportingDocuments);
				}
			}
		}
		CachedProperty<ZString> simplifiedProcedureType;

		protected override ZString AddressCore => isMultipleSuppliers ? null : base.AddressCore;
		protected override ZString CityCore => isMultipleSuppliers ? null : base.CityCore;
		protected override ZString PostCodeCore => isMultipleSuppliers ? null : base.PostCodeCore;
		protected override ZString CountryCore => isMultipleSuppliers ? null : declaration.GetDefaultTerritory(base.CountryCore);
		protected override ZString IdCore => isMultipleSuppliers || !ShouldSupplierIdBeSent() ? null : base.IdCore;
		protected override ZString NameCore => isMultipleSuppliers ? (ZString)MultipleSuppliersCode : base.NameCore;

		ZBool ShouldSupplierIdBeSent() => orgAddress.OA_RN_NKCountryCode == Core.Constants.CountryCodes.Spain && declaration.JE_GoodsOrigin == Core.Constants.CountryCodes.Spain
			&& declaration.JE_GoodsDestination == Core.Constants.CountryCodes.Spain;
	}
}
