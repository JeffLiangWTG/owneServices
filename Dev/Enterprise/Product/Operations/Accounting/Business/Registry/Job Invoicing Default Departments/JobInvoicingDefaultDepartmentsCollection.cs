using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JobInvoicingDefaultDepartmentsCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new JobInvoicingDefaultDepartments this[int i]
		{
			get { return (JobInvoicingDefaultDepartments)Elements[i]; }
		}

		public new JobInvoicingDefaultDepartments AddNew()
		{
			return (JobInvoicingDefaultDepartments)base.AddNew();
		}

		public bool ContainsConsolType(ZString consolType)
		{
			bool result = false;

			foreach (JobInvoicingDefaultDepartments entity in this)
			{
				if (entity.ConsolType == consolType)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		public JobInvoicingDefaultDepartmentsCollection Normalized
		{
			get
			{
				JobInvoicingDefaultDepartmentsCollection result = new JobInvoicingDefaultDepartmentsCollection();

				JobInvoicingDefaultDepartments all = null;
				foreach (JobInvoicingDefaultDepartments entry in this)
				{
					if (entry.ConsolType == Constants.JobInvoicingDefaultDepartmentConsolType.All)
					{
						all = entry;
					}
					else
					{
						result.Add(entry);
					}
				}

				if (all != null)
				{
					foreach (ICodeDescription pair in all.ConsolTypes)
					{
						if (pair.Code != Constants.JobInvoicingDefaultDepartmentConsolType.All && !result.ContainsConsolType(pair.Code))
						{
							JobInvoicingDefaultDepartments entry = result.AddNew();
							using (entry.GetValidationSuspender())
							{
								entry.ConsolType = pair.Code;
								entry.Department = all.Department;
							}
						}
					}
				}

				return result;
			}
		}

		public ZGuid GetDepartment(ZString consolType)
		{
			ZGuid result = ZGuid.Empty;

			foreach (JobInvoicingDefaultDepartments entry in Normalized)
			{
				if (entry.ConsolType == consolType)
				{
					result = entry.Department;
					break;
				}
			}

			return result;
		}

		public ZGuid GetDefaultDepartment()
		{
			return GetDepartment(Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JobInvoicingDefaultDepartmentsCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new JobInvoicingDefaultDepartments();
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			RunPreSaveValidation();
		}
	}
}
