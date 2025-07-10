using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.KR.Business
{
	public sealed class KRCustomsRegistry : RegistryItemSet
	{
		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_KoreaSouth { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("D7E3C867-6D5C-4422-9B75-9BE9DEED294C", "South Korea")); } }
			public static MultilingualString Customs_KoreaSouth_Export { get { return CombineCategories(Customs_KoreaSouth, ResString.GetMultilingualString("BB33530A-B8CD-4F16-8C32-C2392AE8F25E", "Export")); } }
			public static MultilingualString Customs_KoreaSouth_Import { get { return CombineCategories(Customs_KoreaSouth, ResString.GetMultilingualString("640EBBB5-7D84-494A-B094-B333E46D68A6", "Import")); } }
			public static MultilingualString Customs_KoreaSouth_ImportStatement { get { return CombineCategories(Customs_KoreaSouth, ResString.GetMultilingualString("6a315c0c-d6dd-42d2-a00d-cd5b1f3001a8", "Import Statement")); } }
			public static MultilingualString Customs_KoreaSouth_LocalExport { get { return CombineCategories(Customs_KoreaSouth, ResString.GetMultilingualString("DDB96200-AE1B-4DA0-B457-A46C58D76858", "Local Export")); } }
			public static MultilingualString Customs_KoreaSouth_Declarant { get { return CombineCategories(Customs_KoreaSouth, ResString.GetMultilingualString("594DEE8B-DFDB-4D3E-8826-C445116DC5C8", "Declarant Details in English")); } }
		}

		public static KRCustomsRegistry Instance => instance ?? (instance = new KRCustomsRegistry());
		[ThreadStatic]
		static KRCustomsRegistry instance;

		readonly MultilingualString emailGroupCaption = ResString.GetMultilingualString("44CD8DCD-CC4D-4870-9989-B6366FA42E9C", "Email Recipients Group");
		readonly MultilingualString emailGroupHint = ResString.GetMultilingualString("75660155-6141-4D87-A221-7E16D8AC7ECC", "The staff group that will be notified when no original sender has been found while processing the response message.");
		const string customsWebAddress = "https://gsg.customs.go.kr:38120/mediate/gsg/usw/getResponse/message?code=X509";
		#region Common

		public StringRegistryItem UNIPASSDeclarantID
		{
			get
			{
				return GetItem("UNIPASSDeclarantID", delegate
				{
					var result = new StringRegistryItem(
						"UNIPASSDeclarantID",
						Categories.Customs_KoreaSouth,
						ResString.GetMultilingualString("C45A2599-76EF-48B2-87D3-03A96BB5FE06", "UNIPASS Declarant ID"),
						ResString.GetMultilingualString("3DE908D8-4451-4874-88AB-364F353F5CD1", "5-character UNIPASS Declarant ID"),
						RegistryStorageFlags.Company,
						RegistryOptions.CannotCallParameterlessValueGetter);
					result.CountryFilterPKs = new Guid[] { Core.CountryGuids.Instance.KoreaRepublicof };
					return result;
				});
			}
		}

		public StringRegistryItem CompanyName
		{
			get
			{
				return GetItem("CompanyName", delegate
				{
					var result = new StringRegistryItem(
						"CompanyName",
						Categories.Customs_KoreaSouth_Declarant,
						ResString.GetMultilingualString("E99F906C-2040-496C-A22C-044C7DEECD08", "Company Name"),
						ResString.GetMultilingualString("AB8C89C9-615E-4B19-A171-C45BE74BC478", "Company Name"),
						RegistryStorageFlags.Company,
						RegistryOptions.CannotCallParameterlessValueGetter);
					result.CountryFilterPKs = new Guid[] { Core.CountryGuids.Instance.KoreaRepublicof };
					return result;
				});
			}
		}

		public StringRegistryItem RepresentativeName
		{
			get
			{
				return GetItem("RepresentativeName", delegate
				{
					var result = new StringRegistryItem(
						"RepresentativeName",
						Categories.Customs_KoreaSouth_Declarant,
						ResString.GetMultilingualString("1B90C645-57FD-4102-B440-486D546E1115", "Representative Name"),
						ResString.GetMultilingualString("B6808CE4-6B42-454A-852D-70E7ED6950BA", "Representative Name"),
						RegistryStorageFlags.Company,
						RegistryOptions.CannotCallParameterlessValueGetter);
					result.CountryFilterPKs = new Guid[] { Core.CountryGuids.Instance.KoreaRepublicof };
					return result;
				});
			}
		}

		public StringRegistryItem CustomsWebAddress
		{
			get
			{
				return GetItem("KRCustomsPublicKeyWebAddress", delegate
				{
					var result = new StringRegistryItem(
						"KRCustomsPublicKeyWebAddress",
						Categories.Customs_KoreaSouth,
						ResString.GetMultilingualString("AAEA9671-7404-4312-9267-26539E3C655C", "Customs Certificate Address"),
						ResString.GetMultilingualString("5B737B9D-8645-4909-8C4B-B1328B3BDFDE", "Web address to get Customs certificate"),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory,
						customsWebAddress);
					return result;
				});
			}
		}

		public BinaryRegistryItem CustomsCertificate
		{
			get
			{
				return GetItem("KRCustomsPublicKeyCertificate", delegate
				{
					var result = new BinaryRegistryItem(
						"KRCustomsPublicKeyCertificate",
						Categories.Customs_KoreaSouth,
						ResString.GetMultilingualString("60E424B2-A7A5-4007-B981-C58E92B34BF0", "Customs Public Certificate"),
						ResString.GetMultilingualString("60E424B2-A7A5-4007-B981-C58E92B34BF0", "Customs Public Certificate"),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.IsOnlyForSupport,
						Array.Empty<byte>());
					return result;
				});
			}
		}

		#endregion

		#region Export
		public GuidRegistryItem ExportEmailGroup
		{
			get
			{
				return GetItem("ExportEmailGroup", delegate
				{
					var result = new GuidRegistryItem(
						"ExportEmailGroup",
						Categories.Customs_KoreaSouth_Export,
						emailGroupCaption,
						emailGroupHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter,
						RegistryConstants.GroupPKs.Notification);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = new Guid[] { Core.CountryGuids.Instance.KoreaRepublicof };
					return result;
				});
			}
		}

		#endregion

		#region Import
		public GuidRegistryItem ImportEmailGroup
		{
			get
			{
				return GetItem("ImportEmailGroup", delegate
				{
					var result = new GuidRegistryItem(
						"ImportEmailGroup",
						Categories.Customs_KoreaSouth_Import,
						emailGroupCaption,
						emailGroupHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter,
						RegistryConstants.GroupPKs.Notification);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = new Guid[] { Core.CountryGuids.Instance.KoreaRepublicof };
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableToSaveImportDeclaration
		{
			get
			{
				return GetItem((NoResString)"Enable to save Import declaration", delegate
				{
					var result = new BooleanRegistryItem(
						(NoResString)"Enable to save Import declaration",
						Categories.Customs_KoreaSouth_Import,
						ResString.GetMultilingualString("ED942309-170B-4803-A2F2-A07437D3543F", "Enable to save Import declaration"),
						ResString.GetMultilingualString("D64A61C8-CB9F-4D3B-8272-908E15157433", "As development is in progress, Import cannot be saved if it is not CW1 Support"),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
					result.CountryFilterPKs = new Guid[] { Core.CountryGuids.Instance.KoreaRepublicof };
					return result;
				});
			}
		}

		public FamilyRelationRegistryItem FamilyRelations
		{
			get
			{
				return GetItem("FamilyRelations", delegate
				{
					var result =  new FamilyRelationRegistryItem(
						"FamilyRelations",
						Categories.Customs_KoreaSouth_Import,
						ResString.GetMultilingualString("25441FC7-6FAA-4CB6-8BE1-EA73B0ED2866", "Family Relations"),
						ResString.GetMultilingualString("DAEEEC69-7101-4B71-8EB1-D6E864CB18FC", "This list indicates the relationships to the declarant who moves to Korea. Users can add items except codes 00 to 10."),
						RegistryStorageFlags.Company,
						FamilyRelationCollection.GetDefaultFamilyRelationCollection());
					result.CountryFilterPKs = new Guid[] { Core.CountryGuids.Instance.KoreaRepublicof };
					return result;
				});
			}
		}
		#endregion

		#region ImportStatement
		public GuidRegistryItem ImportStatementEmailGroup
		{
			get
			{
				return GetItem("ImportStatementEmailGroup", delegate
				{
					var result = new GuidRegistryItem(
						"ImportStatementEmailGroup",
						Categories.Customs_KoreaSouth_ImportStatement,
						emailGroupCaption,
						emailGroupHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter,
						RegistryConstants.GroupPKs.Notification);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = new Guid[] { Core.CountryGuids.Instance.KoreaRepublicof };
					return result;
				});
			}
		}
		#endregion

		#region Local Export
		public GuidRegistryItem LocalExportEmailGroup
		{
			get
			{
				return GetItem("LocalExportEmailGroup", delegate
				{
					var result = new GuidRegistryItem(
						"LocalExportEmailGroup",
						Categories.Customs_KoreaSouth_LocalExport,
						emailGroupCaption,
						emailGroupHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter,
						RegistryConstants.GroupPKs.Notification);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = new Guid[] { Core.CountryGuids.Instance.KoreaRepublicof };
					return result;
				});
			}
		}
		#endregion

		public override bool IsForProductivityWise => false;
	}
}
