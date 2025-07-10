using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.FR.Business.Testing
{
	public static class JobDeclarationExtensions
	{
		public static JobDeclaration WithJobNo(this JobDeclaration declaration, string jobNo)
		{
			declaration.JE_DeclarationReference = jobNo;
			return declaration;
		}

		public static JobDeclaration WithProfile(this JobDeclaration declaration, string profile)
		{
			declaration.JE_CustomsProfile = profile;
			return declaration;
		}

		public static JobDeclaration WithPaymentMethod(this JobDeclaration declaration, string paymentMethod)
		{
			declaration.JE_PaymentMethod = paymentMethod;
			return declaration;
		}

		public static JobDeclaration WithDefermentNumber(this JobDeclaration declaration, string defermentNumber)
		{
			declaration.JE_DefermentAccountNumber = defermentNumber;
			return declaration;
		}

		public static JobDeclaration WithSiretCode(this JobDeclaration declaration, string siretCode)
		{
			return declaration.WithCusCode(OrgCusCode.FranceCodeTypes.Siret, siretCode);
		}

		public static JobDeclaration WithFlux(this JobDeclaration declaration, string flux)
		{
			declaration.JE_MessageType = flux;
			if (declaration.IsImport)
			{
				declaration.SetupImporter();
			}
			else
			{
				declaration.SetupSupplier();
			}
			return declaration;
		}

		public static JobDeclaration WithDeltaIE(this JobDeclaration declaration)
		{
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			return declaration;
		}

		public static JobDeclaration WithDeltaG(this JobDeclaration declaration)
		{
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			return declaration;
		}

		public static JobDeclaration WithDeltaType(this JobDeclaration declaration, string type)
		{
			declaration.JE_DeltaMode = type;
			declaration.DefaultDeltaAccountOrgHeader.SetupAccount(declaration.DeltaGAccountCode, type, "TEST", ZString.Empty, ZString.Empty, "06ECE6AD");
			return declaration;
		}

		public static OrgHeader SetupImporter(this JobDeclaration declaration, OrgHeader importer = null)
		{
			var newImporter = importer ?? (declaration.Importer ?? declaration.Factory.NewWithValidTestData<OrgHeader>());
			declaration.JE_OH_Importer = newImporter.PK;
			return declaration.Importer;
		}

		public static OrgHeader SetupSupplier(this JobDeclaration declaration, OrgHeader supplier = null)
		{
			var newSupplier = supplier ?? (declaration.Supplier ?? declaration.Factory.NewWithValidTestData<OrgHeader>());
			declaration.JE_OH_Supplier = newSupplier.PK;
			return declaration.Supplier;
		}

		public static OrgAddress SetupDeclarant(this JobDeclaration declaration, OrgAddress declarant = null)
		{
			var newDeclarant = declarant ?? (declaration.Declarant ?? declaration.Factory.NewWithValidTestData<OrgAddress>());
			declaration.JE_OA_DeclarantAddress = newDeclarant.PK;
			return declaration.Declarant;
		}

		public static OrgAddress SetupRepresentative(this JobDeclaration declaration, OrgAddress representative = null)
		{
			var newRepresentative = representative ?? (declaration.Representative ?? declaration.Factory.NewWithValidTestData<OrgAddress>());
			declaration.JE_OA_Representative = newRepresentative.PK;
			return declaration.Representative;
		}

		public static JobDeclaration WithCusCode(this JobDeclaration declaration, string type, string number)
		{
			var authOwner = declaration.DeltaAccountOrgHeader
				?? throw new InvalidOperationException("Set up Authorisation owner on declaration first.");
			authOwner.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(type, number, Core.Constants.CountryCodes.France);
			return declaration;
		}

		public static CusEntryHeader SetupFirstEntry(this JobDeclaration declaration)
			=> declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault()
				?? (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
	}
}
