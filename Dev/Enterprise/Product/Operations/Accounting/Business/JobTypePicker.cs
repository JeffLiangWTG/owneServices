using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business
{
	public interface IJobTypePickerSupporter
	{
		ZBoolDescriptionPairList JobTypeList { get; }
		IEnumerable<ZString> GetInitialSelectedJobTypeCodeList();
		ZString GetSelectedJobTypeCode(ZString jobTypeDescription);
		ZString GetSelectedJobTypeDescription(ZString jobTypeCode);
		void ValidateJobTypes();
	}

	public class JobTypePicker : NonPersistentBusinessObject
	{
		public JobTypePicker(IJobTypePickerSupporter jobTypePickerSupporter)
		{
			this.jobTypePickerSupporter = jobTypePickerSupporter;
		}

		public ZBoolDescriptionPairList JobTypeList
		{
			get { return jobTypePickerSupporter.JobTypeList; }
		}

		public ZBoolDescriptionPairList SelectedJobTypeList
		{
			get
			{
				return GetSelectedJobTypes();
			}
		}

		List<ZString> selectedJobTypeDescriptions;

		public void Reset()
		{
			//Clearing
			jobTypePickerSupporter.JobTypeList.ForEach(x => x.Value = false);
			SelectedJobTypeList.Clear();

			//ReInitializing
			SetInitialSelectedJobTypeList();
			SetSelectedJobTypeList(false);
			RefreshBinding();
		}

		public void SetInitialSelectedJobTypeList()
		{
			var initialSelectedJobTypes = this.jobTypePickerSupporter.GetInitialSelectedJobTypeCodeList();
			if (initialSelectedJobTypes != null)
			{
				JobTypeList.ForEach(x => x.Value = initialSelectedJobTypes.Contains(this.jobTypePickerSupporter.GetSelectedJobTypeCode(x.Description)));
			}
		}

		ZBoolDescriptionPairList GetSelectedJobTypes()
		{
			var result = new ZBoolDescriptionPairList();
			selectedJobTypeDescriptions = new List<ZString>();
			JobTypeList.Where(x => x.Value).ForEach
				(
					(x) =>
					{
						result.Add(new ZBoolDescriptionPair(x.Description, ZBool.True));
						selectedJobTypeDescriptions.Add(x.Description);
					}
				);
			result.OnPairChanged += result_OnPairChanged;
			return result;
		}

		void result_OnPairChanged(ZBoolDescriptionPairChangedEventArgs e)
		{
			JobTypeList.Where(x => x.Description == e.Pair.Description).ToList().ForEach(x => x.Value = e.Pair.Value);
			jobTypePickerSupporter.ValidateJobTypes();
			SetSelectedJobTypeList(false);
		}

		public void SetSelectedJobTypeList(bool refreshBinding = true)
		{
			jobTypePickerSupporter.ValidateJobTypes();
			if (refreshBinding)
			{
				RefreshBinding();
			}
		}

		public void UndoSelectedJobTypes()
		{
			JobTypeList.ForEach(x => x.Value = selectedJobTypeDescriptions.Contains(x.Description));
			RefreshBinding();
		}

		readonly IJobTypePickerSupporter jobTypePickerSupporter;
	}
}

#region Test
#if DEBUG

namespace Enterprise.Accounting.Business.Testing
{
	public class DummyJobTypePickerSupporter : IJobTypePickerSupporter
	{
		public DummyJobTypePickerSupporter(OrgHeader org, string transportMode = "ALL", string serviceDirection = "ALL")
			: this(true, org, transportMode, serviceDirection)
		{
		}

		public DummyJobTypePickerSupporter(bool runValidation, OrgHeader org, string transportMode = "ALL", string serviceDirection = "ALL")
		{
			this.Org = org;
			this.ServiceDirection = serviceDirection;
			this.TransportMode = transportMode;
			this.RunValidation = runValidation;
			HasError = null;
		}

		public ZBoolDescriptionPairList JobTypeList
		{
			get
			{
				if (jobTypeList == null)
				{
					jobTypeList = new ZBoolDescriptionPairList();
					jobTypeList.Add(new ZBoolDescriptionPair(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, false));
					jobTypeList.Add(new ZBoolDescriptionPair(JobInvoicingConsumerTypes.AgencyBooking.Code, false));
					jobTypeList.Add(new ZBoolDescriptionPair(JobInvoicingConsumerTypes.AgencyDetentionInvoice.Code, false));
					jobTypeList.Add(new ZBoolDescriptionPair(JobInvoicingConsumerTypes.Shipment.Code, false));
					jobTypeList.Add(new ZBoolDescriptionPair(JobInvoicingConsumerTypes.TransportBooking.Code, false));
					jobTypeList.Add(new ZBoolDescriptionPair(JobInvoicingConsumerTypes.LocalCartage.Code, false));
				}
				return jobTypeList;
			}
		}
		ZBoolDescriptionPairList jobTypeList;

		public ZString GetSelectedJobTypeCode(ZString selectedJobTypeDescription)
		{
			return selectedJobTypeDescription;
		}

		public ZString GetSelectedJobTypeDescription(ZString jobTypeCode)
		{
			return jobTypeCode;
		}

		public IEnumerable<ZString> GetInitialSelectedJobTypeCodeList()
		{
			return new List<ZString>();
		}

		public void ValidateJobTypes()
		{
			HasError = null;
			if (RunValidation)
			{
				var selectedTypes = Org.CompanyData.GetApplicableInvoiceTypes(TransportMode, ServiceDirection, ServiceLevel, false, SelectedJobTypes);
				HasError = !SelectedJobTypes.All(x => selectedTypes.ContainsKey(x));
			}
		}

		public ZString TransportMode { get; set; }
		public ZString ServiceDirection { get; set; }
		public ZString ServiceLevel { get; set; }
		public ZString[] SelectedJobTypes { get; set; }
		public bool? HasError { get; private set; }

		readonly OrgHeader Org;
		readonly bool RunValidation;
	}
}
#endif

#endregion
