using System;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes;
using Enterprise.DataTransfer.Integration;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public class FixedAssetsDataAdapter : BaseAccountingDataAdapter<BusinessObjectThatDoesntSave, XSDs.固定资产>
	{
		#region Data Adapter Overrides

		public override string RootCollectionElementName
		{
			get { return "FixedAssets"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Element name")]
		public override string RootElementName
		{
			get { return "固定资产"; }
		}

		public override XmlSchema Schema
		{
			get { return new ZXmlSchema(); }
		}

		public override XmlSchema CollectionSchema
		{
			get { return new ZXmlSchema(); }
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{ }

		protected override void ImportFromValueObjectCore(BusinessObjectThatDoesntSave bizObj, XSDs.固定资产 value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		#endregion

		protected override void ExportToValueObjectCore(BusinessObjectThatDoesntSave bizObj, XSDs.固定资产 constructedValueObject, IValueObjectExportContext context)
		{
			SetFixedAssetsBaseInformationValue(constructedValueObject.固定资产基础信息);//FixedAssetsBaseInformation
			SetFixedAssetsTypeSettingValue(constructedValueObject.固定资产类别设置);//FixedAssetsTypeSetting
			SetFixedAssetsMovementModeValue(constructedValueObject.固定资产变动方式);//FixedAssetsMovementMode
			SetFixedAssetsDepreciationMethodValue(constructedValueObject.固定资产折旧方法);//FixedAssetsDepreciationMethod
			SetFixedAssetsUsageValue(constructedValueObject.固定资产使用状况);//FixedAssetsUsage
			SetFixedAssetsCardValue(constructedValueObject.固定资产卡片);//FixedAssetsCard
			SetFixedAssetsCardPhysicalInfoValue(constructedValueObject.固定资产卡片实物信息);//FixedAssetsCardPhysicalInfo
			SetFixedAssetsCardUsageInfoValue(constructedValueObject.固定资产卡片使用信息);//FixedAssetsCardUsageInfo
			SetFixedAssetsDecreasedDetailsValue(constructedValueObject.固定资产减少情况);//FixedAssetsDecreasedDetails
			SetFixedAssetsDecreasedPhysicalInfoValue(constructedValueObject.固定资产减少实物信息);//FixedAssetsDecreasedPhysicalInfo
			SetFixedAssetsMovementDetailsValue(constructedValueObject.固定资产变动情况);//FixedAssetsMovementDetails
		}

		void SetFixedAssetsBaseInformationValue(XSDs.固定资产基础信息 fixedAssetsBaseInformation) { }
		void SetFixedAssetsTypeSettingValue(XSDs.固定资产类别设置Collection fixedAssetsTypeSetting) { }
		void SetFixedAssetsMovementModeValue(XSDs.固定资产变动方式Collection fixedAssetsMovementMode) { }
		void SetFixedAssetsDepreciationMethodValue(XSDs.固定资产折旧方法Collection fixedAssetsDepreciationMethod) { }
		void SetFixedAssetsUsageValue(XSDs.固定资产使用状况Collection fixedAssetsUsage) { }
		void SetFixedAssetsCardValue(XSDs.固定资产卡片Collection fixedAssetsCard) { }
		void SetFixedAssetsCardPhysicalInfoValue(XSDs.固定资产卡片实物信息Collection fixedAssetsCardPhysicalInfo) { }
		void SetFixedAssetsCardUsageInfoValue(XSDs.固定资产卡片使用信息Collection fixedAssetsCardUsageInfo) { }
		void SetFixedAssetsDecreasedDetailsValue(XSDs.固定资产减少情况Collection fixedAssetsDecreasedDetails) { }
		void SetFixedAssetsDecreasedPhysicalInfoValue(XSDs.固定资产减少实物信息Collection fixedAssetsDecreasedPhysicalInfo) { }
		void SetFixedAssetsMovementDetailsValue(XSDs.固定资产变动情况Collection fixedAssetsMovementDetails) { }
	}
}
