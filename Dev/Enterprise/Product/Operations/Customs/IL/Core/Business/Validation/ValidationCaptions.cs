using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business
{
	public static class ValidationCaptions
	{
		#region CusSeal

		public static class CusSeal
		{
			public static string TheMaximumNumberOfAdditionalSealsExceeded(int maximumAdditionalSeals) => Res.GetString("309884A3-919D-4B8E-A25D-F2BF638ABE98", "The maximum number of {0} Additional Seals has been exceeded.", maximumAdditionalSeals);
		}

		#endregion

		#region Manifest

		public static class Manifest
		{
			public static string SeaManifestNumberFormatMust6digits => Res.GetString("AF818A8C-F9C6-4BA2-B35E-E5A7A65BB8E8", "Import Ocean manifest length must be 6 starting with 2 digits of the year (Previous/current/next year).");

			public static string VATNumberIsMissingForPartner(string partner) => Res.GetString("76120F3B-DE32-4E3A-83A5-FD7CCDED5696", "VAT Number is missing for {0} - Update organization config tab.", partner);

			public static string ImportInlandManifestMust16varchars => Res.GetString("2FDFEDCA-90BD-4B12-B4E5-6F90EFD5A68D", "Import Inland manifest length must be 16 starting with \"I\".");

			public static string CustomsCarrierCodeIsMissingForShippingAgent => Res.GetString("75799366-48B9-42A1-88CB-BE3DF09AB5CB", "Customs Carrier Code is missing for Shipping Agent - Update organization Config tab");

			public static string CustomsManifestProviderCodeIsMissingForDeclarant => Res.GetString("D493AFE9-F1B7-4B35-A911-078B60FD99A7", "Customs Manifest Provider Code is missing for Declarant - Update organization Config tab");

			public static string ShippingAgentIsMissingCheckCarrierOrganizationCarrierAgenciesTab => Res.GetString("F7C74DF0-0129-4BA6-A2B2-AF5DF0008FF2", "Shipping Agent is missing, verify Carrier Organization >> Carrier >> Agencies tab.");

			public static string AtLeastOneTransportMeansRecordIsRequired => Res.GetString("D71159CA-C8FD-46B9-9D19-50117AD9273B", "At Least one Transport Means record is required in road manifest.");
		}

		#endregion

		#region Asycuda Bill

		public static class AsycudaBill
		{
			public static string SequenceMaxLength => Res.GetString("5FAB3DC3-ECE8-4403-8BC1-00AD567FDD95", "Sequence number must be up to 3 digits");

			public static string QuantityMaxLength => Res.GetString("3FCC56B0-90F2-453C-8AD6-948E47112346", "Quantity must be up to 8 digits");

			public static string TheVATNoShouldNotEmptyWhenManifestNatureIsImport => Res.GetString("83CFA604-A193-4395-99CD-3DA494CCC67D", "The VAT No. should not empty when Manifest Nature is Import.");

			public static string EachBillMustIncludeOneAdditionalInfoRecordWithStatementTypeIs2AndConsigneeVAT => Res.GetString("46A7D05B-26D2-4AF6-B45F-BFDBB6399545", "Each Bill must include one additional info record with Statement Type = 2 and Consignee 'VAT'");

			public static string AtLeastOnPackageIsRequired => Res.GetString("16851F42-EB75-4A4A-A975-221FD444152D", "At least one package record is required.");

			public static string AtLeastOnItemIsRequired => Res.GetString("195CEAFC-E5D7-46ED-B7B7-E8D22CB385F1", "At least one item record is required.");

			public static string IL3ShouldExistWhenRoad => Res.GetString("8AC34659-230E-4608-996A-C9D1B0C70FAE", "A Transport Document record of type IL3 must be provided in road Manifest.");

			public static string MandatoryConsigorRegNo => Res.GetString("16228B22-AA55-4A51-B3A0-42E5AC2E10E7", "Consignor's Reg # is mandatory for road manifest, please update organization’s config tab");

			public static string PackageFeeTypeShouldExistWhenRoad => Res.GetString("B4BF5274-EAFD-4CE5-B4FC-F1ED0BA3A32F", "In ROA Manifest an additional info record 18 must be provided.");
		}

		#endregion Asycuda Bill

		#region Asycuda Packed Item

		public static class AsycudaPackedItem
		{
			public static string DGSubstanceMustBeProvidedWhenCargoStatusIsDangerous => Res.GetString("CF1B51D2-F5AA-409B-897D-2799A408D6B6", "DG Substance must be provided when cargo status = 'D'");

			public static string UNDGContactMustBeProvidedWhenCargoStatusIsDangerous => Res.GetString("D4AA2284-EBE1-4F87-8B91-0087FD91E28E", "UNDG Contact must be provided when cargo status = 'D'");

			public static string TheEnteredValueMustBeGreaterThanZero => Res.GetString("F352748E-1EC9-4DBC-AFC1-ABD4240B7A14", "The entered value must be greater than 0.");
			public static string AllBillItemsLinkedToPackage => Res.GetString("ECA5C419-FC4C-446A-9E0E-969BD923350A", "All bill items must be linked to a package.");

			public static string HarmonizedCodeLessThan4Digits => Res.GetString("F64AB18B-9FD5-43D5-80A3-978B30EEE457", "Harmonized Code is less than 4 digits.");

			public static string TheCodeYouSelectedIsNotInTheList => Res.GetString("6CA6C1AD-E8D0-430A-824F-AA4B959840A4", "The code you have selected is not in the list.");
		}

		#endregion

		public static class AsycudaTransportDocumentInfo
		{
			public static string RuleCC_BR1_WCO_090_IL1 => Res.GetString("787AFCA2-28AE-4B06-BAD8-3B6C1B7DEF7A", "[CC_BR1_WCO_090] You must enter additional record of type \"IL1\" in case of using \"IL2\"");
			public static string RuleCC_BR1_WCO_091_IL1IL2 => Res.GetString("923BC0B4-0F3B-4508-989C-8BAFDD917212", "[CC_BR1_WCO_091] Ocean Deal Number format is invalid, must start with \"I\" and following 9 chars");
			public static string RuleCC_BR1_WCO_091_IL3 => Res.GetString("D574C005-D839-4126-BCF4-966B906D26D4", "[CC_BR1_WCO_091] Ocean Deal Number format is invalid, must start with \"I\" and following 15 chars");
			public static string RuleTypeDuplication => Res.GetString("6A3B0344-BAC6-4495-A5A8-1B9D16732EB6", "Record with the same reference type already exists");
			public static string UniqueIL1ForwarderDealReferenceNumber => Res.GetString("54B5FC6A-578E-46A7-B797-65EF2EF192C2", "There is another Global Manifest with the same Manifest Number and Forwarder Deal Number – Review Manifest");
			public static string First3digitsDifferentIL1IL2 => Res.GetString("4527EEF6-710E-48AF-94F2-7D010EEA6A26", "Shipping Agent in Forwarder Deal Number (IL1) do not match to Parent Deal Number (IL2)");
		}

		public static class SupportingDocumentMetaData
		{
			public static string ValueIsMandatory => Res.GetString("FBBE2E23-9049-412C-8E9E-28293BB11964", "A meta data value is mandatory for this type of supporting document.");
		}

		public static class GlbILExternalPasswordCollection
		{
			public static string OnlyOneSignatureEntryIsAllowed => Res.GetString("C94AB23C-2650-4761-9D89-E7A350558E85", "Only one Signature entry is allowed.");
		}

		public static class GlbILExternalPassword
		{
			public static ResourceString IncorrectUser => ResString.GetMultilingualString("5972CF31-0AEF-4C84-BAB8-CE8BD6C81197", "The selected user either does not exist or does not have certificate details in the staff record.");
		}

		public static class Partners
		{
			public static string Consignee => Res.GetString("B8108761-9E39-42CE-8A58-92C6BEE3A272", "Consignee");
			public static string Consignor => Res.GetString("07B6C10A-3EEF-4829-8A6E-500384674B1F", "Consignor");
			public static string Carrier => Res.GetString("7A06E7C1-D140-44D3-B3FC-7F71956E89F8", "Carrier");
		}

		public static class SupportingDocument
		{
			public static string EDocIsMissing => Res.GetString("AF724118-A317-41E9-ADFF-AADC43C6B0E1", "There is no eDoc attached to the supporting document.");
		}

		public static class SupportingDocumentWrapper
		{
			public static string IsSelectedEDocMissing => Res.GetString("53731824-B8E5-4226-B365-53C25A3FD5EF", "The selected record is not related to an eDoc, please update the record from the Supporting Documents tab.");
		}

		public static class AsycudaContainer
		{
			public static string ContainerTypeWithoutISOType => Res.GetString("326A226C-120D-4AA4-A1AC-24F7E9F98389", "Container Type is not related to an ISO type – review container type definition");
		}

		public static class JobComInvoiceHeader
		{
			public static string SupplierCustomsNumberIsMissing => Res.GetString("33E69226-7211-4ED6-94C3-C204AB2DF48E", "Supplier Customs Number (CSC) is missing, please review Organization Config Tab");
		}
	}
}
