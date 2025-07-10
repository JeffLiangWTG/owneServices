using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class JobInvoicingDefaultDepartmentsRegistryItem : StronglyTypedRegistryItem<JobInvoicingDefaultDepartmentsCollection>
	{
		// DO NOT DELETE -- may be used by Enterprise.DbUpgrader.Transformation.DataModification.Registry.MigrateDefaultDepartmentSettingsToNewRegistries via reflection
		public JobInvoicingDefaultDepartmentsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, JobInvoicingDefaultDepartmentsCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new JobInvoicingDefaultDepartmentsRegistryDataType(), storage, defaultValue))
		{
		}

		public JobInvoicingDefaultDepartmentsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ZGuid defaultDepartment)
			: this(name, category, caption, hint, storage, new JobInvoicingDefaultDepartmentsCollection { new JobInvoicingDefaultDepartments(true) { ConsolType = "ALL", Department = defaultDepartment } })
		{
			// M.K: Removed DefaultValue.ResumeValidation() because DefaultValue is actually a clone of original default value, and though its validation is not suspended.
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.JobInvoicingDefaultDepartmentsRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class JobInvoicingDefaultDepartmentsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<JobInvoicingDefaultDepartmentsCollection>
	{
	}
}
