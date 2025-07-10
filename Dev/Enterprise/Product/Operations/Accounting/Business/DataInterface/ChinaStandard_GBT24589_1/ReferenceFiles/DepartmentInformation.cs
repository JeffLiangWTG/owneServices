using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public sealed class DepartmentInformation : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string LocID = "T107";
		public ZString DepartmentCode { get; set; }
		public ZString DepartmentName { get; set; }
		public ZString ParentDepartmentCode { get; set; }
	}

	public sealed class DepartmentInformationCollection : NonPersistentBusinessObjectCollection<DepartmentInformation>	{
		public DepartmentInformationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			AddDefaultElements();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DepartmentInformation();
		}

		void AddDefaultElements()
		{
			GlbDepartmentCollection collection = new GlbDepartmentCollection(Factory);
			collection.ApplySort(GlbDepartment.Schema.GE_Code, ListSortDirection.Ascending);
			foreach (GlbDepartment department in collection)
			{
				DepartmentInformation departmentInfo = AddNew();
				departmentInfo.DepartmentCode = department.GE_Code;
				departmentInfo.DepartmentName = department.GE_DescMultilingual;
				departmentInfo.ParentDepartmentCode = department.ParentDepartment.Count > 0 ? department.ParentDepartment[0].GE_Code : ZString.Empty;
			}
		}
	}
}

