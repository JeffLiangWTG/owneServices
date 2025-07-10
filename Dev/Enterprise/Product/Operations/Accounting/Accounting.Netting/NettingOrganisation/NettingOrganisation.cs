using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Netting
{
	public class NettingOrganisation : AutoNettingOrganisation
	{
		public NettingOrganisation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool IsFullNettingType
		{
			get { return NSO_NettingType == "FUL"; }
		}

		public bool IsHomeNettingType
		{
			get { return NSO_NettingType == "HOM"; }
		}

		public bool IsCurrencyNettingType
		{
			get { return NSO_NettingType == "CUR"; }
		}

		public bool IsGrossNettingType
		{
			get { return NSO_NettingType == "GRS"; }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			NSO_NettingType = "CUR";
		}
#endif
	}
}
