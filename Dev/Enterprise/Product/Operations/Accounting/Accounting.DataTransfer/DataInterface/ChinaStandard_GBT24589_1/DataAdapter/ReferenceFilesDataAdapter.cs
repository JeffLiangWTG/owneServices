using System;
using System.Globalization;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.DataTransfer.DataInterface;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public class ReferenceFilesDataAdapter : BaseAccountingDataAdapter<BusinessObjectThatDoesntSaveForCN, XSDs.公共档案>
	{
		#region Data Adapter Overrides

		public override string RootCollectionElementName
		{
			get { return "ReferenceFiles"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Element name")]
		public override string RootElementName
		{
			get { return "公共档案"; }
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

		#endregion

		protected override void ExportToValueObjectCore(BusinessObjectThatDoesntSaveForCN bizObj, XSDs.公共档案 constructedValueObject, IValueObjectExportContext context)
		{
			currentFactory = bizObj.Factory;
			SetEAccountBookValue(constructedValueObject.电子账簿, bizObj.Period);//EAccountBook
			SetAccountingPeriodValue(constructedValueObject.会计期间, new PeriodManager(currentFactory), bizObj.Period);//AccountingPeriod
			SetVoucherTypeValue(constructedValueObject.记账凭证类型);//VoucherType
			SetExRateTypeValue(constructedValueObject.汇率类型);//ExRateType
			SetCurrencyValue(constructedValueObject.币种);//Currency
			SetPaymentTypeValue(constructedValueObject.结算方式);//PaymentType
			SetDepartmentInformationValue(constructedValueObject.部门档案);//DepartmentInformation
			SetStaffInformationValue(constructedValueObject.员工档案);//StaffInformation
			SetSupplierInformationValue(constructedValueObject.供应商档案);//SupplierInformation
			SetClientInformationValue(constructedValueObject.客户档案);//ClientInformation
			SetDefinedDataItemValue(constructedValueObject.自定义档案项);//DefinedDataItem
			SetDefinedDataItemValueValue(constructedValueObject.自定义档案项值);//DefinedDataItemValue
		}

		void SetEAccountBookValue(XSDs.电子账簿 eAccountBook, ZInt period)
		{
			AccountingEBook accountingEBook = new AccountingEBook();

			eAccountBook.电子账簿编号.Value = AccountingEBook.BookNumber;
			eAccountBook.电子账簿名称.Value = accountingEBook.BookName;
			eAccountBook.会计核算单位.Value = accountingEBook.CompanyName;
			eAccountBook.组织机构代码.Value = accountingEBook.CompanyRegistrationCode;
			eAccountBook.单位性质.Value = AccountingEBook.CompanyType;
			eAccountBook.行业.Value = AccountingEBook.Industry;
			eAccountBook.开发单位.Value = AccountingEBook.DevelopmentCompany;
			eAccountBook.版本号.Value = AccountingEBook.Version;
			eAccountBook.本位币.Value = accountingEBook.BaseCurrency;
			AccPeriodManagement accPeriodManagement = (new AccountingPeriodCalculator(currentFactory)).GetPeriodManagementFromDate((new AccountingPeriodCalculator(currentFactory)).GetFirstDayForPeriod(period));
			eAccountBook.会计年度.Value = accPeriodManagement == null ? ZDateTime.Today.Year.ToString(new CultureInfo("en-US")) : accPeriodManagement.AM_Year.ToString();
			eAccountBook.标准版本号.Value = AccountingEBook.ChinaStandardVersion;
		}

		void SetAccountingPeriodValue(XSDs.会计期间Collection accountingPeriods, PeriodManager periodManager, ZInt period1)
		{
			var accPeriodManagement = (new AccountingPeriodCalculator(currentFactory)).GetPeriodManagementFromDate((new AccountingPeriodCalculator(currentFactory)).GetFirstDayForPeriod(period1));
			if (accPeriodManagement != null)
			{
				periodManager.FinancialYear = accPeriodManagement.AM_Year;
			}

			foreach (Period period in periodManager.Periods)
			{
				XSDs.会计期间 accountingPeriodXSD = accountingPeriods.AddNew();
				accountingPeriodXSD.会计期间号.Value = period.AM_Period.ToString();
				accountingPeriodXSD.会计年度.Value = period.AM_Year.ToString();
				accountingPeriodXSD.会计期间起始日期.Value = period.AM_StartDate.ToString("yyyyMMdd", new CultureInfo("en-US"));
				accountingPeriodXSD.会计期间结束日期.Value = period.AM_EndDate.ToString("yyyyMMdd", new CultureInfo("en-US"));
			}
		}

		void SetVoucherTypeValue(XSDs.记账凭证类型Collection voucherTypes)
		{
			VoucherTypeCollection collection = new VoucherTypeCollection(1, currentFactory);
			foreach (VoucherType voucherType in collection)
			{
				XSDs.记账凭证类型 voucherTypeXSD = voucherTypes.AddNew();
				voucherTypeXSD.记账凭证类型编号.Value = voucherType.VoucherTypeNumber;
				voucherTypeXSD.记账凭证类型名称.Value = voucherType.VoucherTypeName;
				voucherTypeXSD.记账凭证类型简称.Value = voucherType.VoucherTypeAbbreviation;
			}
		}

		void SetExRateTypeValue(XSDs.汇率类型1Collection exRateTypes)
		{
			ExRateTypeCollection collection = new ExRateTypeCollection(currentFactory);
			foreach (ExRateType exRateType in collection)
			{
				XSDs.汇率类型1 exRateTypeXSD = exRateTypes.AddNew();
				exRateTypeXSD.汇率类型编号.Value = exRateType.ExRateTypeNumber;
				exRateTypeXSD.汇率类型名称.Value = exRateType.ExRateTypeName;
			}
		}

		void SetCurrencyValue(XSDs.币种Collection currencys)
		{
			CurrencyCollection collection = new CurrencyCollection(currentFactory);
			foreach (Currency currency in collection)
			{
				XSDs.币种 currencyXSD = currencys.AddNew();
				currencyXSD.币种编码.Value = currency.CurrencyCode;
				currencyXSD.币种名称.Value = currency.CurrencyName;
			}
		}

		void SetPaymentTypeValue(XSDs.结算方式Collection paymentTypes)
		{
			PaymentTypeCollection collection = new PaymentTypeCollection(currentFactory);
			foreach (PaymentType paymentType in collection)
			{
				XSDs.结算方式 paymentTypeXSD = paymentTypes.AddNew();
				paymentTypeXSD.结算方式编码.Value = paymentType.PaymentTypeCode;
				paymentTypeXSD.结算方式名称.Value = paymentType.PaymentTypeName;
			}
		}

		void SetDepartmentInformationValue(XSDs.部门档案Collection departmentInformations)
		{
			DepartmentInformationCollection collection = new DepartmentInformationCollection(currentFactory);
			foreach (DepartmentInformation departmentInformation in collection)
			{
				XSDs.部门档案 departmentInformationXSD = departmentInformations.AddNew();
				departmentInformationXSD.部门编码.Value = departmentInformation.DepartmentCode;
				departmentInformationXSD.部门名称.Value = departmentInformation.DepartmentName;
				departmentInformationXSD.上级部门编码.Value = departmentInformation.ParentDepartmentCode;
			}
		}

		void SetStaffInformationValue(XSDs.员工档案Collection staffInformations)
		{
			StaffInformationCollection collection = new StaffInformationCollection(currentFactory);
			foreach (StaffInformation staffInformation in collection)
			{
				XSDs.员工档案 staffInformationXSD = staffInformations.AddNew();
				staffInformationXSD.员工编码.Value = staffInformation.StaffCode; //StaffCode
				staffInformationXSD.员工姓名.Value = staffInformation.StaffName; //StaffName
				staffInformationXSD.证件类别.Value = staffInformation.IDType; //IDType
				staffInformationXSD.证件号码.Value = staffInformation.IDNumber; //IDNumber
				staffInformationXSD.性别.Value = staffInformation.Gender; //Gender
				staffInformationXSD.出生日期.Value = staffInformation.BirthDate; //BirthDate
				staffInformationXSD.部门编码.Value = staffInformation.DepartmentCode; //DepartmentCode
				staffInformationXSD.入职日期.Value = string.IsNullOrEmpty(staffInformation.EmploymentDate) ? "00000000" : staffInformation.EmploymentDate.ToString(); //EmploymentDate
				staffInformationXSD.离职日期.Value = string.IsNullOrEmpty(staffInformation.LeaveDate) ? "00000000" : staffInformation.LeaveDate.ToString();//LeaveDate
			}
		}

		void SetSupplierInformationValue(XSDs.供应商档案Collection supplierInformations)
		{
			SupplierInformationCollection collection = new SupplierInformationCollection(currentFactory);
			foreach (SupplierInformation supplierInformation in collection)
			{
				XSDs.供应商档案 supplierInformationXSD = supplierInformations.AddNew();
				supplierInformationXSD.供应商编码.Value = supplierInformation.SupplierCode;//SupplierCode
				supplierInformationXSD.供应商名称.Value = supplierInformation.SupplierName;//SupplierName
				supplierInformationXSD.供应商简称.Value = supplierInformation.SupplierAbbreviation;//SupplierAbbreviation
			}
		}

		void SetClientInformationValue(XSDs.客户档案Collection clientInformations)
		{
			ClientInformationCollection collection = new ClientInformationCollection(currentFactory);
			foreach (ClientInformation clientInformation in collection)
			{
				XSDs.客户档案 clientInformationXSD = clientInformations.AddNew();
				clientInformationXSD.客户编码.Value = clientInformation.ClientCode;
				clientInformationXSD.客户名称.Value = clientInformation.ClientName;
				clientInformationXSD.客户简称.Value = clientInformation.ClientAbbreviation;
			}
		}

		void SetDefinedDataItemValue(XSDs.自定义档案项Collection definedDataItems)
		{
			DefinedDataItemCollection collection = new DefinedDataItemCollection(currentFactory);
			foreach (DefinedDataItem definedDataItem in collection)
			{
				XSDs.自定义档案项 definedDataItemXSD = definedDataItems.AddNew();
				definedDataItemXSD.档案编码.Value = definedDataItem.DataCode;
				definedDataItemXSD.档案名称.Value = definedDataItem.DataName;
				definedDataItemXSD.档案描述.Value = definedDataItem.DataDescription;
				definedDataItemXSD.是否有层级特征.Value = definedDataItem.HasLevel;
				definedDataItemXSD.档案编码规则.Value = definedDataItem.DefinedDataCodeRule;
			}
		}

		void SetDefinedDataItemValueValue(XSDs.自定义档案项值Collection definedDataItemValues)
		{
			DefinedDataItemValueCollection collection = new DefinedDataItemValueCollection(currentFactory);
			foreach (DefinedDataItemValue definedDataItemValue in collection)
			{
				XSDs.自定义档案项值 definedDataItemValueXSD = definedDataItemValues.AddNew();
				definedDataItemValueXSD.档案编码.Value = definedDataItemValue.DefinedDataCode;
				definedDataItemValueXSD.档案值编码.Value = definedDataItemValue.DefinedDataItemValueCode;
				definedDataItemValueXSD.档案值名称.Value = definedDataItemValue.DefinedDataItemValueName;
				definedDataItemValueXSD.档案值描述.Value = definedDataItemValue.DefinedDataItemValueDescription;
				definedDataItemValueXSD.档案值父节点.Value = definedDataItemValue.ParentDefinedDataItemValueCode;
				definedDataItemValueXSD.档案值级次.Value = definedDataItemValue.DefinedDataItemValueLevel;
			}
		}

		BusinessObjectFactory currentFactory;

		protected override void ImportFromValueObjectCore(BusinessObjectThatDoesntSaveForCN bizObj, XSDs.公共档案 value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}
	}
}
