using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	[PropertyDescriptorCollection(typeof(BusinessObjectPropertyDescriptorCollection))]
	public interface IBusiness : IBusinessObjectState, IBindingList, INotificationProvider, IIdentified
	{
		BusinessObjectFactory Factory { get; }
		void SuspendValidation();
		void ResumeValidation();
		bool IgnoreValidationSuspended { get; set; }
		bool IsValidationSuspended { get; }
		void Delete();
		string TableName { get; }
		ZString HumanReadableName { get; }
		bool CanContinueWithSave { get; }
		void RunPreSaveValidation();
		void RunPreSaveValidationFetch(bool executeHints);
		void MarkAsNeedingValidationIncludingChildren();
		void ValidateIfQuickAndImprovesPreSaveValidationPerformance();
		IBusiness[] Children { get; }
		void NotifyRegisteredChildEditable();
		void DeleteForDataRefresh();
		bool CanDeleteForDataRefresh { get; }
	}
}
