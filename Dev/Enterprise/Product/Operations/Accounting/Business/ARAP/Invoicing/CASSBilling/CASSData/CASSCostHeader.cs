using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class CASSCostHeader : CASSData, IValueObject
	{
		public CASSCostHeader()
			: base(null)
		{
			hotFileName = ZString.Empty;
		}

		public override void Delete()
		{
			Lines?.RemoveAndDeleteAll();

			base.Delete();
		}

		[ReadOnly(true)]
		[XmlIgnore]
		public ZDateTime DatePeriodStart
		{
			get { return datePeriodStart; }
			set { SetNonPersistentPropertyValue(DatePeriodStartInfo, ref datePeriodStart, value); }
		}
		ZDateTime datePeriodStart;

		[XmlIgnore]
		public ZPropertyInfo DatePeriodStartInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DatePeriodStart));
			}
		}

		[ReadOnly(true)]
		[XmlIgnore]
		public ZDateTime DatePeriodEnd
		{
			get { return datePeriodEnd; }
			set { SetNonPersistentPropertyValue(DatePeriodEndInfo, ref datePeriodEnd, value); }
		}
		ZDateTime datePeriodEnd;

		[XmlIgnore]
		public ZPropertyInfo DatePeriodEndInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DatePeriodEnd));
			}
		}

		[ReadOnly(true)]
		[XmlIgnore]
		public ZDateTime DateOfBilling
		{
			get { return dateOfBilling; }
			set { SetNonPersistentPropertyValue(DateOfBillingInfo, ref dateOfBilling, value); }
		}
		ZDateTime dateOfBilling;

		[XmlIgnore]
		public ZPropertyInfo DateOfBillingInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DateOfBilling));
			}
		}

		[ReadOnly(true)]
		[XmlIgnore]
		public ZString BillingCurrency
		{
			get { return billingCurrency; }
			set { SetNonPersistentPropertyValue(BillingCurrencyInfo, ref billingCurrency, value); }
		}
		ZString billingCurrency;

		[XmlIgnore]
		public ZPropertyInfo BillingCurrencyInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(BillingCurrency));
			}
		}

		[ReadOnly(true)]
		[XmlIgnore]
		public ZString HOTFileName
		{
			get { return hotFileName; }
			set { SetNonPersistentPropertyValue(HOTFileNameInfo, ref hotFileName, value); }
		}
		ZString hotFileName;

		[XmlIgnore]
		public ZPropertyInfo HOTFileNameInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(HOTFileName));
			}
		}

		#region Lines

		[ReadOnly(true)]
		[XmlIgnore]
		public BusinessObjectCollection Lines
		{
			get
			{
				return lines;
			}
		}
		BusinessObjectCollection lines;

		public void InitializeAsExportCASS()
		{
			if (Lines == null || Lines.Count == 0)
			{
				SetLines(new CASSCostExportLineCollection());
			}
			else
			{
				throw new InvalidOperationException(InvalidInitializationMessage);
			}
		}

		public void InitializeAsImportCASS()
		{
			if (Lines == null || Lines.Count == 0)
			{
				SetLines(new CASSCostImportLineCollection());
			}
			else
			{
				throw new InvalidOperationException(InvalidInitializationMessage);
			}
		}

		void SetLines<T>(T newLines)
			where T : BusinessObjectCollection
		{
			if (lines != null)
			{
				UnRegisterEditableChildObject(lines);
			}
			lines = newLines;
			RegisterEditableChildObject(lines);
		}

		[ReadOnly(true)]
		[XmlIgnore]
		public CASSCostExportLineCollection ExportLines
		{
			get
			{
				var result = Lines as CASSCostExportLineCollection ?? new CASSCostExportLineCollection();

				return result;
			}
		}

		[ReadOnly(true)]
		[XmlIgnore]
		public CASSCostImportLineCollection ImportLines
		{
			get
			{
				var result = Lines as CASSCostImportLineCollection ?? new CASSCostImportLineCollection();

				return result;
			}
		}

		#endregion

		[ReadOnly(true)]
		[XmlIgnore]
		public bool IsInitialized
		{
			get
			{
				return (Lines != null);
			}
		}

		public void UpdateOriginalAmountFields()
		{
			if (Lines != null)
			{
				Lines.Cast<CASSCostLine>().ForEach(x => x.UpdateOriginalAmounts());
			}
		}

		string InvalidInitializationMessage
		{
			get
			{
				var cassType = ZString.Empty;

				if (Lines != null && Lines is CASSCostExportLineCollection)
				{
					cassType = CASSChargeCodeLookups.CASSTypes.Export.Code;
				}
				else if (Lines != null && Lines is CASSCostImportLineCollection)
				{
					cassType = CASSChargeCodeLookups.CASSTypes.Import.Code;
				}

				return Res.GetString("bf345457-0d0b-400e-970c-95f4daaca121", "Already initialized as {0} CASS", cassType);
			}
		}

		#region IValueObject Members

		[XmlIgnore]
		public bool IsSpecified
		{
			get { return true; }
		}

		[XmlIgnore]
		public bool ShouldCreateElementForEmptyValue { get; set; }

		#endregion
	}
}
