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
	public class PayrollsDataAdapter : BaseAccountingDataAdapter<BusinessObjectThatDoesntSave, XSDs.员工薪酬>
	{
		#region Data Adapter Overrides

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Element name")]
		public override string RootCollectionElementName
		{
			get { return "Payrolls"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Element name")]
		public override string RootElementName
		{
			get { return "员工薪酬"; }
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

		protected override void ImportFromValueObjectCore(BusinessObjectThatDoesntSave bizObj, XSDs.员工薪酬 value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		#endregion

		protected override void ExportToValueObjectCore(BusinessObjectThatDoesntSave bizObj, XSDs.员工薪酬 constructedValueObject, IValueObjectExportContext context)
		{
			SetPayrollPeriodsValue(constructedValueObject.薪酬期间);//PayrollPeriods
			SetPayrollItemsValue(constructedValueObject.薪酬项目);//PayrollItems
			SetStaffPayrollListValue(constructedValueObject.员工薪酬记录);//StaffPayrollList
			SetStaffPayrollDetailsValue(constructedValueObject.员工薪酬记录明细);//StaffPayrollDetails
		}

		void SetPayrollPeriodsValue(XSDs.薪酬期间Collection payrollPeriods) { }
		void SetPayrollItemsValue(XSDs.薪酬项目Collection payrollItems) { }
		void SetStaffPayrollListValue(XSDs.员工薪酬记录Collection staffPayrollList) { }
		void SetStaffPayrollDetailsValue(XSDs.员工薪酬记录明细Collection staffPayrollDetails) { }
	}
}
