using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentWrappers.Customs.EU.EMCS.Testing
{
	public static class EMCSDeclarationWrapperTestHelper
	{
		public static OrgHeader AddOrganisation(EMCSJobDeclaration declaration, DocAddressType docAddressType, PartyAddressTestData party)
		{
			var factory = declaration.Factory;
			var org = factory.NewWithValidTestData<OrgHeader>();
			_ = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, party.ExciseCode, party.CountryCode);
			_ = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, party.EORINumber, party.CountryCode);
			org.OH_FullName = party.Name;
			org.MainAddress.OA_Language = party.Language;
			org.MainAddress.OA_Address1 = party.Address1;
			org.MainAddress.OA_Address2 = party.Address2;
			org.MainAddress.OA_PostCode = party.PostCode;
			org.MainAddress.OA_City = party.City;
			org.MainAddress.OA_Phone = party.Phone;

			AddJobDocAddress(declaration, org, docAddressType, new PartyAddressTestData());
			return org;
		}

		public static void AddJobDocAddress(EMCSJobDeclaration declaration, OrgHeader organisation, DocAddressType docAddressType, PartyAddressTestData party)
		{
			var address = default(JobDocAddress);
			switch (docAddressType)
			{
				case DocAddressType.SupplierDocumentaryAddress:
					address = declaration.SupplierDocumentaryAddress.Cast<JobDocAddress>().FirstOrDefault() ?? declaration.DocAddresses.AddNew(docAddressType);
					break;
				case DocAddressType.ImporterDocumentaryAddress:
					address = declaration.ImporterDocumentaryAddress.Cast<JobDocAddress>().FirstOrDefault() ?? declaration.DocAddresses.AddNew(docAddressType);
					break;
				case DocAddressType.DestinationWarehouse:
					address = declaration.DestinationWarehouseDocumentaryAddress.Cast<JobDocAddress>().FirstOrDefault() ?? declaration.DocAddresses.AddNew(docAddressType);
					break;
				case DocAddressType.Transporter:
					address = declaration.TransporterDocumentaryAddress.Cast<JobDocAddress>().FirstOrDefault() ?? declaration.DocAddresses.AddNew(docAddressType);
					break;
				case DocAddressType.OwnerOfGoods:
					address = declaration.OwnerDocumentaryAddress.Cast<JobDocAddress>().FirstOrDefault() ?? declaration.DocAddresses.AddNew(docAddressType);
					break;
				case DocAddressType.DispatchWarehouse:
					address = declaration.DispatchWarehouseDocumentaryAddress.Cast<JobDocAddress>().FirstOrDefault() ?? declaration.DocAddresses.AddNew(docAddressType);
					break;
			}

			if (address != null)
			{
				address.OrganisationPK = organisation.PK;
				address.E2_AddressOverride = party.OverrideAddress;
				address.E2_Address1 = party.Address1;
				address.E2_Address2 = party.Address2;
				address.E2_City = party.City;
				address.E2_Postcode = party.PostCode;
				address.StateCode = party.State;
				address.CompanyName = party.Name;
				address.E2_GovRegNumType = docAddressType == DocAddressType.DispatchWarehouse ? OrgCusCode.EuropeanUnionSharedCodeTypes.Eori : OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;
				address.E2_GovRegNum = docAddressType == DocAddressType.DispatchWarehouse ? party.EORINumber : party.ExciseCodeOverride;
			}
		}

		public static void AddCustomsOffice(EMCSJobDeclaration declaration, CustomsOfficeTestData office)
		{
			var factory = declaration.Factory;
			var cusCodeList = factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_Code = office.Code;
			cusCodeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Portugal;
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddDays(2);
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddDays(-2);
			AddRefCusCustomsOfficeAttribute(cusCodeList, "Street", office.Street);
			AddRefCusCustomsOfficeAttribute(cusCodeList, "PostCode", office.PostCode);
			AddRefCusCustomsOfficeAttribute(cusCodeList, "CITY", office.City);
			factory.Save();

			var customsOffice = declaration.CustomsOffices.Cast<OfficeCode>().FirstOrDefault(o => o.CY_Code == office.CodeType) ?? declaration.CustomsOffices.AddNew();
			customsOffice.CY_Code = office.CodeType;
			customsOffice.CY_Data = office.Code;
			customsOffice.CY_Date = ZDateTime.Today;
		}

		static void AddRefCusCustomsOfficeAttribute(ZZRefCusCodeListCombined cusCodeList, string attributeName, string attributeValue)
		{
			var officeAttribute = cusCodeList.Attributes.AddNew();
			officeAttribute.ZZE_ZXE_NKName = attributeName;
			officeAttribute.ZZE_Value = attributeValue;
		}

		public static void AddContainer(EMCSJobDeclaration declaration, string unitCode, string containerNumber, string seal, string sealDetails)
		{
			var transport = declaration.CusContainers.AddNew();
			transport.ZG_UnitCode = unitCode;
			transport.CO_ContainerNumber = containerNumber;
			transport.CO_Seal = seal;
			transport.SealDetails = sealDetails;
			transport.Comment = string.Empty;
		}

		public static void AddInvoiceLine(EMCSJobDeclaration declaration, decimal sizeOfProducer)
		{
			var line = declaration.InvoiceHeader.InvoiceLines.AddNew();
			line.ZG_SizeOfProducer = sizeOfProducer;
		}

		public static EMCSJobComInvoiceLine AddInvoiceLine(EMCSJobDeclaration declaration, bool isMainPack, string description, decimal alcoholicStrength, decimal density, string tariff, string origin)
		{
			var line = declaration.InvoiceHeader.InvoiceLines.AddNew();
			line.ZG_IsMainPack = isMainPack;
			line.JI_NDescription = description;
			line.ZG_AlcoholicStrength = alcoholicStrength;
			line.ZG_Density = density;
			line.JI_Tariff = tariff;
			line.ZG_Origin = origin;
			return line;
		}

		public static GlbStaff AddStaff(BusinessObjectFactory factory, string code, string name)
		{
			var staff = factory.New<GlbStaff>();
			staff.GS_Code = code;
			staff.GS_FullName = name;
			return staff;
		}

		public static void AddSADNumbers(EMCSJobDeclaration declaration, string[] descriptions)
		{
			foreach (var description in descriptions.Where(s => !string.IsNullOrEmpty(s)))
			{
				var sad = declaration.ImportSADNumbers.AddNew();
				sad.CSI_Description = description;
			}
		}

		public static void AddPackages(EMCSJobComInvoiceLine invoiceLine, PackageTestData[] packages)
		{
			packages.ForEach(p => AddPackage(invoiceLine, p));
		}

		public static void AddPackage(EMCSJobComInvoiceLine invoiceLine, PackageTestData packageData)
		{
			var package = invoiceLine.Declaration.EMCSPackages.AddNew();
			package.B5_UnitCount = new ZLong(packageData.NumberOfPackages);
			package.B5_UnitType = packageData.KindOfPackages;
			package.B5_MarksAndNumbers = packageData.ShippingMarks;

			var pivot = invoiceLine.EMCSPackagePivots[invoiceLine.EMCSPackagePivots.Count - 1];
			pivot.IsForInvoiceLine = true;
		}

		public static void SetupReferenceTestData(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Portugal, parent: eunZZZ);
			_ = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office", dataGrouping.ZZZ_DataGrouping);
			_ = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "Exise Movement Control System (EMCS) Pack Types", dataGrouping.ZZZ_DataGrouping);

			var codeList = helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "BX", "Box", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			var attribute = "COUNTABLE";
			_ = helper.CreateNewOrGetExistingCusCodeListAttribute(codeList.PK, attribute, attribute);
			_ = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(attribute, attribute, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, Core.Constants.CountryCodes.Portugal);
			factory.Save();
		}
	}

	public class CustomsOfficeTestData
	{
		public CustomsOfficeTestData(string codeType)
		{
			var i = new Random().Next(1, 999);
			CodeType = codeType;
			Code = $"{Core.Constants.CountryCodes.Portugal}{codeType}";
			Street = $"Street {i}";
			PostCode = $"{i}";
			City = $"City {i}";
		}

		public string CodeType;
		public string Code;
		public string Street;
		public string PostCode;
		public string City;
		public string ExpectedCustomsOffice => $"{Code}{System.Environment.NewLine}{Street}\n{PostCode} {City}";
	}

	public class PartyAddressTestData
	{
		public PartyAddressTestData()
		{
			var i = new Random().Next(1, 999);
			ExciseCode = $"AR1{i}";
			ExciseCodeOverride = $"AR2{i}";
			EORINumber = $"EORI{1}";
			Name = $"Organisation {i}";
			Address1 = $"Address 1 - {i}";
			Address2 = $"Address 2 - {i}";
			PostCode = $"{i}";
			City = $"City {i}";
		}

		public string ExciseCode;
		public string ExciseCodeOverride;
		public string EORINumber;
		public string CountryCode => Core.Constants.CountryCodes.Portugal;
		public string Language => "EN-GB";
		public string Name;
		public string Address1;
		public string Address2;
		public string PostCode;
		public string City;
		public string State => "HAM";
		public string Address => Address1 + Address2;
		public string Country => string.Empty;
		public bool OverrideAddress;
		public string Phone => "+44 1234 567890";

		public string ExpectedDocumentaryAddress => $"{Name}{System.Environment.NewLine}{Address}{System.Environment.NewLine}{PostCode}{System.Environment.NewLine}{City}";
		public string ExpectedImporterDocumentaryAddress => $"{Name}{System.Environment.NewLine}{Address}{System.Environment.NewLine}{PostCode}{System.Environment.NewLine}{City}{System.Environment.NewLine}{(OverrideAddress ? ExciseCodeOverride : ExciseCode)}";
	}

	public class PackageTestData
	{
		public PackageTestData(string description, long unitCount, string unitType, string shippingMarks, decimal strength, decimal density, bool countable)
		{
			Description = description;
			if (countable)
			{
				NumberOfPackages = unitCount;
				ShippingMarks = shippingMarks;
			}
			else
			{
				NumberOfPackages = null;
				ShippingMarks = string.Empty;
			}

			KindOfPackages = unitType;
			AlcoholStrength = strength;
			Density = density;
		}

		public string GetExpected()
		{
			var expected = new ZStringBuilder(Description);
			if (NumberOfPackages != null)
			{
				_ = expected.AppendIfNotEmpty(NumberOfPackages.ToString());
			}
			_ = expected.AppendIfNotEmpty(KindOfPackages)
				.AppendIfNotEmpty(ShippingMarks);

			if (AlcoholStrength > 0)
			{
				_ = expected.AppendIfNotEmpty(AlcoholStrength.ToString());
			}

			if (Density > 0)
			{
				_ = expected.AppendIfNotEmpty(Density.ToString());
			}

			return expected.ToStringWithDelimiterBetweenAppends(EMCSDeclarationWrapperHelper.Delimiter);
		}

		public string Description;
		public long? NumberOfPackages;
		public string KindOfPackages;
		public string ShippingMarks;
		public decimal AlcoholStrength;
		public decimal Density;
	}
}
