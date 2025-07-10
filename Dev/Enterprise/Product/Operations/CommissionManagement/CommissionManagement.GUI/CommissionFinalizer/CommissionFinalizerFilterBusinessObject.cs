using CargoWise.EntityFramework;
using Enterprise.CommissionManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.GUI
{
	public class CommissionFinalizerFilterBusinessObject : CommissionManagementFilterBusinessObject
	{
		#region Filter

		public override ZQuery Filter
		{
			get
			{
				var result = base.Filter;
				result.AddToFilter(ViewCommissionLineSchema.VCL_CancelledDateTimeUtc, null);
				result.AddToFilter(ViewCommissionLineSchema.VCL_PaidDateTimeUtc, null);

				return result;
			}
		}

		#endregion

		#region Module Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();

			if (!OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.Value)
			{
				result[CommissionFinalizerFilterBusinessObject.FilterDescription.Company].Visibility = FilterVisibility.AlwaysVisible;
			}

			result[CommissionFinalizerFilterBusinessObject.FilterDescription.RecognitionDate].Visibility = FilterVisibility.AlwaysVisible;
			result[CommissionFinalizerFilterBusinessObject.FilterDescription.EntityStaff].Visibility = FilterVisibility.AlwaysVisible;
			result[CommissionFinalizerFilterBusinessObject.FilterDescription.EntityOrganisation].Visibility = FilterVisibility.AlwaysVisible;
			result[CommissionFinalizerFilterBusinessObject.FilterDescription.CommissionStatus].Visibility = FilterVisibility.AlwaysVisible;
			result[CommissionFinalizerFilterBusinessObject.FilterDescription.AgreementId].Visibility = FilterVisibility.AlwaysVisible;
			result[CommissionFinalizerFilterBusinessObject.FilterDescription.ApprovalRequest].Visibility = FilterVisibility.AlwaysVisible;

			return result;
		}

		#endregion

		#region Statuses

		public override ReadOnlyCodeDescriptionPairList CommissionStatuses
		{
			get
			{
				var result = new AccCommissionLineCommissionStatusList();
				result.RemoveCode(AccCommissionLineCommissionStatusList.Codes.Paid);

				return result;
			}
		}

		#endregion
	}
}
