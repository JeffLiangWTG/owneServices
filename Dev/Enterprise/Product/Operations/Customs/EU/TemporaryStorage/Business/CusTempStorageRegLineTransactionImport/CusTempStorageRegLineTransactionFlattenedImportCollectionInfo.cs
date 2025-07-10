using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeaderImport
{
	public class CusTempStorageRegLineTransactionFlattenedImportCollectionInfo : ImportCollectionInfoImpl
	{
		public CusTempStorageRegLineTransactionFlattenedImportCollectionInfo(IBusinessObjectCollection collection) : base(collection)
		{
			var orgCasing = Env.Registry.OrgAllowMixedCase ? ZCharacterCasing.Normal : ZCharacterCasing.Upper;

			AddProperty(new ImportPropertyInfoImpl<CusTempStorageRegLineTransactionFlattened>(AutoCusTempStorageRegLineTransactionFlattened.Schema.SRP_CustomsLocation, isMandatory: true) { HeaderText = Res.GetString("47236162-5C59-4C26-AC4F-50696828DF36", "Customs Location"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(new ImportPropertyInfoImpl<CusTempStorageRegLineTransactionFlattened>(AutoCusTempStorageRegLineTransactionFlattened.Schema.SRH_Reference, isMandatory: true) { HeaderText = Res.GetString("A91FA093-DC6A-43FD-BBB7-205B99FBC7DF", "TSD Number"), CharacterCasing = orgCasing });
			AddProperty(new ImportPropertyInfoImpl<CusTempStorageRegLineTransactionFlattened>(AutoCusTempStorageRegLineTransactionFlattened.Schema.SRI_GoodsItemNumber, isMandatory: true) { HeaderText = Res.GetString("DB95D01E-CB30-4AAB-A47B-79121F839923", "TSD Item Number") });
			AddProperty(new ImportPropertyInfoImpl<CusTempStorageRegLineTransactionFlattened>(AutoCusTempStorageRegLineTransactionFlattened.Schema.SRL_PackageType, isMandatory: true) { HeaderText = Res.GetString("00D85A6A-1F1F-45B7-96B7-EFD8D7E7558E", "Package Type"), CharacterCasing = ZCharacterCasing.Upper });
			AddProperty(new ImportPropertyInfoImpl<CusTempStorageRegLineTransactionFlattened>(AutoCusTempStorageRegLineTransactionFlattened.Schema.SRL_PackageMarks, isMandatory: true) { HeaderText = Res.GetString("BF3AD33E-BF81-42A5-BCBA-740849103FF2", "Package Marks"), CharacterCasing = orgCasing });
			AddProperty(new ImportPropertyInfoImpl<CusTempStorageRegLineTransactionFlattened>(AutoCusTempStorageRegLineTransactionFlattened.Schema.SRT_PhysicalInOutDate, isMandatory: true) { HeaderText = Res.GetString("4C2AA2D7-E2BC-497D-AACA-B4840E6E57CA", "Physical In/Out Date") });
			AddProperty(new ImportPropertyInfoImpl<CusTempStorageRegLineTransactionFlattened>(AutoCusTempStorageRegLineTransactionFlattened.Schema.SRT_TransactionDate, isMandatory: true) { HeaderText = Res.GetString("42655D8D-4D45-464E-8300-6B983B509B75", "Transaction Date") });
			AddProperty(new ImportPropertyInfoImpl<CusTempStorageRegLineTransactionFlattened>(AutoCusTempStorageRegLineTransactionFlattened.Schema.SRT_GrossWeight, isMandatory: true) { HeaderText = Res.GetString("12C8B1CC-B9B2-4F2F-934B-A5FA17600432", "Gross Weight in KGs") });
			AddProperty(new ImportPropertyInfoImpl<CusTempStorageRegLineTransactionFlattened>(AutoCusTempStorageRegLineTransactionFlattened.Schema.SRT_PackageQty, isMandatory: true) { HeaderText = Res.GetString("D51AC242-5130-454D-928D-5655999C9E76", "Package Quantity") });
			AddProperty(new ImportPropertyInfoImpl<CusTempStorageRegLineTransactionFlattened>(AutoCusTempStorageRegLineTransactionFlattened.Schema.SRT_InternalReferenceType, isMandatory: true) { HeaderText = Res.GetString("463FB0FB-324F-4E5A-8CC2-E1BAB7E6F262", "Internal Ref. Type", orgCasing) });
			AddProperty(new ImportPropertyInfoImpl<CusTempStorageRegLineTransactionFlattened>(AutoCusTempStorageRegLineTransactionFlattened.Schema.SRT_InternalReferenceNumber, isMandatory: true) { HeaderText = Res.GetString("F862DD9F-9D16-40FB-841E-A26B8EEA8724", "Internal Ref. Number", orgCasing) });
			AddProperty(new ImportPropertyInfoImpl<CusTempStorageRegLineTransactionFlattened>(AutoCusTempStorageRegLineTransactionFlattened.Schema.SRT_ReferenceType, isMandatory: true) { HeaderText = Res.GetString("D96ED99C-259B-4B25-9DFF-FDAF1B817E43", "Reference Type", orgCasing) });
			AddProperty(new ImportPropertyInfoImpl<CusTempStorageRegLineTransactionFlattened>(AutoCusTempStorageRegLineTransactionFlattened.Schema.SRT_Reference, isMandatory: true) { HeaderText = Res.GetString("C14D8BA7-C359-4647-A32E-EAADD466E246", "Reference Number", orgCasing) });
			AddProperty(new ImportPropertyInfoImpl<CusTempStorageRegLineTransactionFlattened>(AutoCusTempStorageRegLineTransactionFlattened.Schema.SRT_Comments) { HeaderText = Res.GetString("B51EC179-EC77-4EF6-94C0-24672FC95A8F", "Comments", orgCasing) });

			void AddProperty(IImportPropertyInfo property)
			{
				headerProperties.Add(property);

				Add(property);
			}
		}
		readonly IList<IImportPropertyInfo> headerProperties = new List<IImportPropertyInfo>();

		public IEnumerable<IImportPropertyInfo> HeaderProperties => headerProperties;
	}
}
