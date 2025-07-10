using System;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class PartsDataToSave : PartsDataToLoad
	{
		public ZString UC1_QtyParent
		{
			get { return GetUnitConversionQuantityInParent(0); }
		}

		public ZString UC2_QtyParent
		{
			get { return GetUnitConversionQuantityInParent(1); }
		}

		public ZString UC3_QtyParent
		{
			get { return GetUnitConversionQuantityInParent(2); }
		}

		public ZString UC4_QtyParent
		{
			get { return GetUnitConversionQuantityInParent(3); }
		}

		public ZString UC5_QtyParent
		{
			get { return GetUnitConversionQuantityInParent(4); }
		}

		public ZString UC1_Package
		{
			get { return GetUnitConversionPackageType(0); }
		}

		public ZString UC2_Package
		{
			get { return GetUnitConversionPackageType(1); }
		}

		public ZString UC3_Package
		{
			get { return GetUnitConversionPackageType(2); }
		}

		public ZString UC4_Package
		{
			get { return GetUnitConversionPackageType(3); }
		}

		public ZString UC5_Package
		{
			get { return GetUnitConversionPackageType(4); }
		}

		public ZString UC1_ParentPackage
		{
			get { return GetUnitConversionParentPackageType(0); }
		}

		public ZString UC2_ParentPackage
		{
			get { return GetUnitConversionParentPackageType(1); }
		}

		public ZString UC3_ParentPackage
		{
			get { return GetUnitConversionParentPackageType(2); }
		}

		public ZString UC4_ParentPackage
		{
			get { return GetUnitConversionParentPackageType(3); }
		}

		public ZString UC5_ParentPackage
		{
			get { return GetUnitConversionParentPackageType(4); }
		}

		ZString GetUnitConversionQuantityInParent(int index)
		{
			ZString result = ZString.Empty;
			try
			{
				result =  UnitConversions[index].QuantityInParent.ToString("0.000");
			}
			catch (Exception e) when (!e.IsCriticalException()) { }
			return result;
		}

		ZString GetUnitConversionParentPackageType(int index)
		{
			ZString result = ZString.Empty;
			try
			{
				result = UnitConversions[index].ParentPackageType;
			}
			catch (Exception e) when (!e.IsCriticalException()) { }
			return result;
		}

		ZString GetUnitConversionPackageType(int index)
		{
			ZString result = ZString.Empty;
			try
			{
				result = UnitConversions[index].PackageType;
			}
			catch (Exception e) when (!e.IsCriticalException()) { }
			return result;
		}
	}
}
