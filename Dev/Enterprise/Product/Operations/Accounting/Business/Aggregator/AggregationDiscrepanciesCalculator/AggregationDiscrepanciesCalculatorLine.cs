using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class AggregationDiscrepanciesCalculatorLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AggregationDiscrepanciesCalculatorLine()
			: base()
		{
		}

		#region Properties

		[ReadOnly(true)]
		ZString fLineType;

		[MaxLength(15)]
		public ZString LineType
		{
			get { return fLineType; }
			set { SetNonPersistentPropertyValue(LineTypeInfo, ref fLineType, value); }
		}
		public ZPropertyInfo LineTypeInfo
		{
			get { return GetZPropertyInfo(nameof(LineType)); }
		}

		[ReadOnly(true)]
		ZDecimal fPeriod1;
		public ZDecimal Period1
		{
			get { return fPeriod1; }
			set { SetNonPersistentPropertyValue(Period1Info, ref fPeriod1, value); }
		}
		public ZPropertyInfo Period1Info
		{
			get { return GetZPropertyInfo(nameof(Period1)); }
		}

		[ReadOnly(true)]
		ZDecimal fPeriod2;
		public ZDecimal Period2
		{
			get { return fPeriod2; }
			set { SetNonPersistentPropertyValue(Period2Info, ref fPeriod2, value); }
		}
		public ZPropertyInfo Period2Info
		{
			get { return GetZPropertyInfo(nameof(Period2)); }
		}

		[ReadOnly(true)]
		ZDecimal fPeriod3;
		public ZDecimal Period3
		{
			get { return fPeriod3; }
			set { SetNonPersistentPropertyValue(Period3Info, ref fPeriod3, value); }
		}
		public ZPropertyInfo Period3Info
		{
			get { return GetZPropertyInfo(nameof(Period3)); }
		}

		[ReadOnly(true)]
		ZDecimal fPeriod4;
		public ZDecimal Period4
		{
			get { return fPeriod4; }
			set { SetNonPersistentPropertyValue(Period4Info, ref fPeriod4, value); }
		}
		public ZPropertyInfo Period4Info
		{
			get { return GetZPropertyInfo(nameof(Period4)); }
		}

		[ReadOnly(true)]
		ZDecimal fPeriod5;
		public ZDecimal Period5
		{
			get { return fPeriod5; }
			set { SetNonPersistentPropertyValue(Period5Info, ref fPeriod5, value); }
		}
		public ZPropertyInfo Period5Info
		{
			get { return GetZPropertyInfo(nameof(Period5)); }
		}

		[ReadOnly(true)]
		ZDecimal fPeriod6;
		public ZDecimal Period6
		{
			get { return fPeriod6; }
			set { SetNonPersistentPropertyValue(Period6Info, ref fPeriod6, value); }
		}
		public ZPropertyInfo Period6Info
		{
			get { return GetZPropertyInfo(nameof(Period6)); }
		}

		[ReadOnly(true)]
		ZDecimal fPeriod7;
		public ZDecimal Period7
		{
			get { return fPeriod7; }
			set { SetNonPersistentPropertyValue(Period7Info, ref fPeriod7, value); }
		}
		public ZPropertyInfo Period7Info
		{
			get { return GetZPropertyInfo(nameof(Period7)); }
		}

		[ReadOnly(true)]
		ZDecimal fPeriod8;
		public ZDecimal Period8
		{
			get { return fPeriod8; }
			set { SetNonPersistentPropertyValue(Period8Info, ref fPeriod8, value); }
		}
		public ZPropertyInfo Period8Info
		{
			get { return GetZPropertyInfo(nameof(Period8)); }
		}

		[ReadOnly(true)]
		ZDecimal fPeriod9;
		public ZDecimal Period9
		{
			get { return fPeriod9; }
			set { SetNonPersistentPropertyValue(Period9Info, ref fPeriod9, value); }
		}
		public ZPropertyInfo Period9Info
		{
			get { return GetZPropertyInfo(nameof(Period9)); }
		}

		[ReadOnly(true)]
		ZDecimal fPeriod10;
		public ZDecimal Period10
		{
			get { return fPeriod10; }
			set { SetNonPersistentPropertyValue(Period10Info, ref fPeriod10, value); }
		}
		public ZPropertyInfo Period10Info
		{
			get { return GetZPropertyInfo(nameof(Period10)); }
		}

		[ReadOnly(true)]
		ZDecimal fPeriod11;
		public ZDecimal Period11
		{
			get { return fPeriod11; }
			set { SetNonPersistentPropertyValue(Period11Info, ref fPeriod11, value); }
		}
		public ZPropertyInfo Period11Info
		{
			get { return GetZPropertyInfo(nameof(Period11)); }
		}

		[ReadOnly(true)]
		ZDecimal fPeriod12;
		public ZDecimal Period12
		{
			get { return fPeriod12; }
			set { SetNonPersistentPropertyValue(Period12Info, ref fPeriod12, value); }
		}
		public ZPropertyInfo Period12Info
		{
			get { return GetZPropertyInfo(nameof(Period12)); }
		}

		#endregion

	}
}